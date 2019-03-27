using System;
using System.IO;
using Summoner.Enumerations;

namespace Summoner
{
	internal static class Log
	{
		private static readonly object padlock = new object();

		internal static void Write(string input)
		{
			var logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "summoner.log");
			lock (padlock)
			{
				using (var streamWriter = new StreamWriter(logFilePath, true))
				{
					streamWriter.WriteLine($"[{DateTime.Now}] - {input}".TrimEnd('\r', '\n'));
				}
			}
		}

		internal static void WriteAndQuit(string logMessage, ExitCodes exitCode)
		{
			Console.WriteLine("Application failed with error. See log for details.");
			Write($"FAIL: {logMessage}");
			Environment.Exit((int)exitCode);
		}
	}
}
