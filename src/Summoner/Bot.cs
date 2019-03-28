using Summoner.Enumerations;
using Summoner.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Summoner
{
	internal delegate void TelegramUpdatesCallback(Update[] telegramUpdates);

	internal class Bot
	{
		private class BotMenuAction
		{
			internal Action<Message> Handler { get; }
			internal string ActionDescription { get; }

			internal BotMenuAction(Action<Message> messageHandlingAction, string description)
			{
				Handler = messageHandlingAction;
				ActionDescription = description;
			}
		}

		private TelegramBotClient botClient;
		private readonly TelegramUpdatesCallback callback;
		private BotDatabaseContext _dbContext;

		Dictionary<string, BotMenuAction> botMenu;

		public Bot()
		{
			try
			{
				_dbContext = new BotDatabaseContext();
				botClient = new TelegramBotClient(ConfigResolver.Instance.AuthenticationToken);

				botMenu = CreateGenericMenu();

				callback += ResolveUpdates;

				new Thread(StartPolling).Start(callback);
			}
			catch (Exception exception)
			{
				Log.Write(exception.Message + Environment.NewLine + exception.StackTrace);
			}
		}

		internal void StartPolling(object callback)
		{
			try
			{
				while (true)
				{
					_dbContext.GetLatestUpdateId(out int latestUpdateId);

					Update[] updates = botClient.GetUpdatesAsync(
						latestUpdateId + Constants.TelegramUpdatesOffset,
						timeout: Constants.LongPollingTimeout,
						allowedUpdates: new[] { UpdateType.Message, UpdateType.EditedMessage })
						.Result;

					if (updates.Length == 0)
					{
						Thread.Sleep(100);
						continue;
					}

					_dbContext.SetLatestUpdateId(updates.Last().Id);

					((TelegramUpdatesCallback)callback).Invoke(updates);
				}
			}
			catch (Exception exception)
			{
				Log.Write(exception.Message + Environment.NewLine + exception.StackTrace);
			}
		}

		private void ResolveUpdates(Update[] updates)
		{
			foreach (var update in updates)
			{
				var message = update.Message ?? update.EditedMessage;
				if (message == null || (DateTime.UtcNow - message.Date).TotalMinutes > Constants.MessageTtl)
				{
					return;
				}

				try
				{
					TryGenericMenuAction(message);
				}
				catch (Exception exception)
				{
					Log.Write(exception.Message + Environment.NewLine + exception.StackTrace);
				}
			}
		}

		private Dictionary<string, BotMenuAction> CreateGenericMenu() => new Dictionary<string, BotMenuAction>
		{
			["/summon"] = new BotMenuAction(SummonForSmoke, "Summon for smoke!"),
			["/help"] = new BotMenuAction(ShowHelp, "Show this manual."),
			["/hi"] = new BotMenuAction(ShowActivity, "Check bots' activity."),
			["/pidor"] = new BotMenuAction(LaunchPidor, "YAY! PIDOR OF THE DAY!")
		};

		private void ShowHelp(Message message)
		{
			StringBuilder help = new StringBuilder();

			foreach (var botMenuItem in botMenu)
			{
				help.Append(botMenuItem.Key).Append(" - ").AppendLine(botMenuItem.Value.ActionDescription);
			}

			botClient.SendTextMessageAsync(message.Chat.Id, help.ToString());
		}

		private void ShowActivity(Message message)
		{
			botClient.SendTextMessageAsync(
				message.Chat.Id,
				$"Hi, [{string.Join(" ", message.From.FirstName, message.From.LastName)}](tg://user?id={message.From.Id})!",
				ParseMode.Markdown);
		}

		private void SummonForSmoke(Message message)
		{
			if (NotAppropriateChat(message))
			{
				botClient.SendTextMessageAsync(message.Chat.Id, "Not available in this chat.");
				return;
			}

			// Probably, logging every messages userId
			// is better than making everyone admins.
			// No API exists to get all group members.

			// Fucking wheelchair code.
			var otherAdmins = botClient.GetChatAdministratorsAsync(message.Chat.Id).Result
				.Where(admin => !admin.User.IsBot && admin.User.Id != message.From.Id);

			string reply = $"[{string.Join(" ", message.From.FirstName, message.From.LastName)}](tg://user?id={message.From.Id}) summons ALL smokers:";

			foreach (var admin in otherAdmins)
			{
				reply = string.Join(
					Environment.NewLine,
					reply,
					$"[{string.Join(" ", admin.User.FirstName, admin.User.LastName)}](tg://user?id={admin.User.Id})");
			}

			botClient.SendTextMessageAsync(message.Chat.Id, reply, ParseMode.Markdown);
		}

		private void LaunchPidor(Message message)
		{
			if (NotAppropriateChat(message))
			{
				botClient.SendTextMessageAsync(message.Chat.Id, "There's only ONE PIDOR in this chat and that's definitely not me.");
				return;
			}

			PidorOfTheDay.Instance.Start(botClient, message, _dbContext);
		}

		private void TryGenericMenuAction(Message message)
		{
			var menuCommandMatch = new Regex(Constants.BotMenuCommandPattern, RegexOptions.Multiline)
				.Match(message.Text ?? string.Empty);

			if (menuCommandMatch.Success && botMenu.TryGetValue(menuCommandMatch.Groups["cmd"].Value, out BotMenuAction botAction))
			{
				botAction.Handler.Invoke(message);
			}
		}

		private static bool NotAppropriateChat(Message message)
		{
			return message.Chat.Type == ChatType.Private || message.Chat.Type == ChatType.Channel;
		}
	}
}