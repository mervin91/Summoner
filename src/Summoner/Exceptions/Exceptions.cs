using System;

namespace Summoner.Exceptions
{
	internal class ConfigurationException : Exception
	{
		internal ConfigurationException() : base (Constants.ConfigurationException)
		{
		}

		internal ConfigurationException(string message) : base (message)
		{
		}
	}
}
