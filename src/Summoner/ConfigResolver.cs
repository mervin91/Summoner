using System;
using System.Xml;
using System.IO;
using Summoner.Exceptions;

namespace Summoner
{
	internal sealed class ConfigResolver
	{
		private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.ConfigFileName);

		private static ConfigResolver _resolverInstance;
		private static XmlDocument _configXml;

		private static readonly object Padlock = new object();

		internal string DatabaseConnectionString { get; }
		internal string AuthenticationToken { get; }

		internal static ConfigResolver Instance
		{
			get
			{
				lock (Padlock)
				{
					return _resolverInstance ?? (_resolverInstance = new ConfigResolver());
				}
			}
		}

		private ConfigResolver()
		{
			_configXml = GetConfigFile();

			DatabaseConnectionString = GetDatabaseConnectionString();
			AuthenticationToken = GetAuthenticationToken();
		}

		private string GetDatabaseConnectionString()
		{
			CheckConfigPresence();

			string dbPath = _configXml.SelectSingleNode(Constants.XPathDatabasePath).Value;

			if (string.IsNullOrEmpty(dbPath))
			{
				throw new ConfigurationException(Constants.DatabasePathNotSpecifiedException);
			}

			string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbPath);
			if (!Directory.Exists(absolutePath))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
			}

			var connectionStringBuilder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
			{
				DataSource = absolutePath
			};

			return connectionStringBuilder.ConnectionString;
		}

		private string GetAuthenticationToken()
		{
			CheckConfigPresence();

			string token = _configXml.SelectSingleNode(Constants.XPathAuthenticationToken).Value;

			if (string.IsNullOrEmpty(token))
			{
				throw new ConfigurationException(Constants.MissingTokenException);
			}

			return token;
		}

		private XmlDocument GetConfigFile()
		{
			var xDocument = new XmlDocument();
			xDocument.Load(ConfigFilePath);

			return xDocument;
		}

		private void CheckConfigPresence()
		{
			if (_configXml == null)
			{
				throw new ConfigurationException();
			}
		}
	}
}
