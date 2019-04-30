using System.Reflection;
using Telegram.Bot.Types.InputFiles;

namespace Summoner.Helpers
{
	static class Images
	{
		private static readonly Assembly _assembly = typeof(Bot).GetTypeInfo().Assembly;
		internal static InputOnlineFile Coffee => new InputOnlineFile(_assembly.GetManifestResourceStream("Summoner.Resources.coffee_512x512.png"));
		internal static InputOnlineFile Smoke => new InputOnlineFile(_assembly.GetManifestResourceStream("Summoner.Resources.smoke_512x512.png"));
	}
}
