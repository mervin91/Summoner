using System;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading;
using Telegram.Bot.Types.Enums;
using Medallion;

namespace Summoner.Games
{
	internal sealed class PidorOfTheDay
	{
		private static PidorOfTheDay instance;

		private static readonly object padlock = new object();
		private static readonly object gamelock = new object();

		private static bool gameInProgress;

		internal static PidorOfTheDay Instance
		{
			get
			{
				lock (padlock)
				{
					if (instance == null)
					{
						instance = new PidorOfTheDay();
					}
					return instance;
				}
			}
		}

		internal void Start(TelegramBotClient bot, Message message, BotDatabaseContext databaseContext)
		{
			if (gameInProgress)
			{
				return;
			}

			databaseContext.GetLatestPidorLaunchDate(out DateTime lastFaggotLaunch);

			var dateDiff = DateTime.Now - lastFaggotLaunch;
			if (dateDiff.TotalHours < 24)
			{
				bot.SendTextMessageAsync(message.Chat.Id, $"PIDOR OF THE DAY was already chosen. Please, try again in {23 - dateDiff.Hours} hour(s) {59 - dateDiff.Minutes} minute(s).");
				return;
			}

			databaseContext.SetLastPidorLaunchDate();

			new Thread(PidorOfTheDayLaunch)
				.Start(new GameVariables(bot, message));
		}

		private class GameVariables
		{
			internal TelegramBotClient Bot { get; }
			internal Message Message { get; }

			internal GameVariables(TelegramBotClient bot, Message message)
			{
				Bot = bot;
				Message = message;
			}
		}

		private void PidorOfTheDayLaunch(object gameVariables)
		{
			lock (gamelock)
			{
				var gameVars = gameVariables as GameVariables;
				var msg = gameVars?.Message;
				var botClient = gameVars?.Bot;

				if (msg == null || botClient == null)
				{
					return;
				}

				gameInProgress = true;

				var chatId = msg.Chat.Id;

				var phrases = new[]
				{
					"Researching PIDOR OF THE DAY detection algorithms...",
					"Compiling...",
					"Running searching sequence...",
					"PIDOR OF THE DAY was found!",
					"Are you ready?!",
					"Drumroll..."
				};

				foreach (var phrase in phrases)
				{
					botClient.SendTextMessageAsync(chatId, phrase);
					Thread.Sleep(1000 * Rand.Next(3, 10));
				}

				var admins = botClient.GetChatAdministratorsAsync(chatId).Result.Where(admin => !admin.User.IsBot).ToArray();

				int faggotIndex = 0;
				// Randomize the qty of randomizations.
				int faggotResearchPhaseQty = Rand.Next(1, admins.Length);
				for (int i = 0; i < faggotResearchPhaseQty; i++)
				{
					faggotIndex = Rand.Next(0, admins.Length);
				}

				var faggot = admins[faggotIndex].User;

				var message = botClient.SendTextMessageAsync(
					chatId,
					$"PIDOR OF THE DAY IS [{string.Join(" ", faggot.FirstName, faggot.LastName)}](tg://user?id={faggot.Id})",
					ParseMode.Markdown).Result;

				botClient.PinChatMessageAsync(chatId, message.MessageId);

				gameInProgress = false;
			}
		}
	}
}
