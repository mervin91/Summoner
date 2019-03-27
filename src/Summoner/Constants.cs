namespace Summoner
{
	internal static class Constants
	{
		#region Misc
		internal const string ConfigFileName = "config.xml";
		internal const int TelegramUpdatesOffset = 1;
		internal const int LongPollingTimeout = 30;
		internal const int MessageTtl = 10;
		internal const string BotMenuCommandPattern = @"^(?<cmd>\/[\w]+)(?<args>.*)$";
		#endregion

		#region XPaths
		internal const string XPathAuthenticationToken = "Config/AuthenticationToken/@value";
		internal const string XPathDatabasePath = "Config/Database/@path";
		#endregion

		#region Exceptions
		internal const string MissingTokenException = "Authentication token was not specified.";
		internal const string ConfigurationException = "Error while obtaining configuration. Check XML structure.";
		internal const string DatabasePathNotSpecifiedException = "Database path was not specified.";
		#endregion
	}
}
