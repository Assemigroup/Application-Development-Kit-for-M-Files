using System;
using System.IO;
using System.Xml;

namespace ApplicationDevelopmentKit
{
	class SettingsFilesWriter : ALFilesWriter
	{
		public override void WriteFiles(Api api)
		{
			string settingsTargetDirectory = GetTargetPath("");
			InitializeDirectory(settingsTargetDirectory);
			GenerateSettingsFile(api);
		}

		private void GenerateSettingsFile(Api api)
		{
			// Check if M-Files properties file exists, if not create one
			if (File.Exists(MFilesSettings.MFILES_SETTINGS_XML_FILE) && api.MFilesSettings.MFDevToolCommand == MFDevelopmentKitCommand.New) {
				throw new Exception("The current directory already has an existing VAF project." +
					" Please try using the \'update\' cli command...\n");
			} else if (!File.Exists(MFilesSettings.MFILES_SETTINGS_XML_FILE) && api.MFilesSettings.MFDevToolCommand == MFDevelopmentKitCommand.Update) {
				throw new Exception("The current directory doesn't have an existing VAF project." +
					" Please try creating a VAF project first by using the \'new\' cli command...\n");
			}

			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = true;
			xmlWriterSettings.NewLineOnAttributes = true;
			XmlWriter xmlWriter = XmlWriter.Create(MFilesSettings.MFILES_SETTINGS_XML_FILE, xmlWriterSettings);

			if (string.IsNullOrWhiteSpace(api.MFilesSettings.ProjectName))
				api.MFilesSettings.ProjectName = "";
			if (string.IsNullOrWhiteSpace(api.MFilesSettings.Domain))
				api.MFilesSettings.Domain = "";

			Console.WriteLine("[INFO] Creating/Updating M-Files settings...");
			xmlWriter.WriteStartElement("mfiles");
			xmlWriter.WriteElementString("projectName", api.MFilesSettings.ProjectName);
            xmlWriter.WriteElementString("serverName", api.MFilesSettings.Server);
			xmlWriter.WriteElementString("port", string.IsNullOrWhiteSpace(api.MFilesSettings.Port) ? "2266" : api.MFilesSettings.Port);
            xmlWriter.WriteElementString("vaultName", api.MFilesSettings.VaultName);
            xmlWriter.WriteElementString("vaultGUID", api.MFilesSettings.VaultGUID);
			xmlWriter.WriteElementString("authType", ((int)api.MFilesSettings.AuthType).ToString());
            xmlWriter.WriteElementString("domain", api.MFilesSettings.Domain);
			xmlWriter.WriteElementString("username", api.MFilesSettings.Username);
			xmlWriter.WriteElementString("password", SecurityUtils.Encrypt(api.MFilesSettings.Password));
            
			xmlWriter.WriteEndElement();
			xmlWriter.Flush();
			Console.WriteLine("[INFO] Completed creating M-Files settings...");
		}
	}
}
