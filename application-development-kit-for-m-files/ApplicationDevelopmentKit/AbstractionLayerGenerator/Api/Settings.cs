using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using MFilesAPI;

namespace ApplicationDevelopmentKit
{
	public class MFilesSettings
	{
		public static string MFILES_SETTINGS_XML_FILE = $"{ALFilesWriter.GetMFTargetDirectoryInfo().FullName}\\vault.settings.xml";

		private string __Port;
		private MFAuthType __AuthType;
		public string Alias { get; set; }
		public string ProjectName { get; set; }
		public string Server { get; set; }
		public string Port
		{
			get { return string.IsNullOrWhiteSpace(__Port) ? "2266" : __Port; }
			set { __Port = value; }
		}
		public MFAuthType AuthType
		{
			get { return (int)__AuthType == -1 ? MFAuthType.MFAuthTypeSpecificMFilesUser : __AuthType; }
			set { __AuthType = value; }
		}
		public string VaultName { get; set; }
		public string VaultGUID { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Domain { get; set; }
		public MFDevelopmentKitCommand MFDevToolCommand { get; set; }
		public bool HasSettingsXmlFile { get; set; }
		public bool IsValidSettings { get; set; }

		public MFilesSettings()
		{
			HasSettingsXmlFile = false;
			IsValidSettings = false;
		}

		public void Validate()
		{
			List<string> invalidSettings = GetMissingMFilesSettings();
			IsValidSettings = invalidSettings.Count == 0 && !IsEmpty();
		}

		public void SaveToXmlFile()
		{
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = true;
			xmlWriterSettings.NewLineOnAttributes = true;
			XmlWriter xmlWriter = null;

			try {
				if (File.Exists(MFILES_SETTINGS_XML_FILE))
					File.Delete(MFILES_SETTINGS_XML_FILE);

				xmlWriter = XmlWriter.Create(MFILES_SETTINGS_XML_FILE, xmlWriterSettings);

				if (string.IsNullOrWhiteSpace(ProjectName))
					ProjectName = ALFilesWriter.GetMFTargetDirectoryInfo().Name;
				if (string.IsNullOrWhiteSpace(Domain))
					Domain = "";

				Console.WriteLine("[INFO] Creating/Updating M-Files settings...");
				xmlWriter.WriteStartElement("mfiles");
				xmlWriter.WriteElementString("projectName", ProjectName);
				xmlWriter.WriteElementString("serverName", Server);
				xmlWriter.WriteElementString("port", string.IsNullOrWhiteSpace(Port) ? "2266" : Port);
				xmlWriter.WriteElementString("vaultName", VaultName);
				xmlWriter.WriteElementString("vaultGUID", VaultGUID);
				xmlWriter.WriteElementString("authType", ((int)AuthType).ToString());
				xmlWriter.WriteElementString("domain", Domain);
				xmlWriter.WriteElementString("username", Username);
				xmlWriter.WriteElementString("password", SecurityUtils.Encrypt(Password));

				xmlWriter.WriteEndElement();
				File.SetAttributes(MFILES_SETTINGS_XML_FILE, FileAttributes.Hidden);
			} catch (Exception ex) {
				Console.WriteLine($"[ERROR] {ex.Message}");
			} finally {
				xmlWriter?.Flush();
			}
			
			Console.WriteLine("[INFO] Completed creating/updating M-Files settings...");
		}

		public void ReadSettingsFile()
		{
			try {
				if (!File.Exists(MFILES_SETTINGS_XML_FILE)) {
					HasSettingsXmlFile = false;
					return;
				}
					
				XmlDocument mfilesSettingsXmlDoc = new XmlDocument();
				mfilesSettingsXmlDoc.Load(MFILES_SETTINGS_XML_FILE);
				XmlNode mfilesSettingsNodes = mfilesSettingsXmlDoc.SelectNodes("/mfiles")[0];

				ProjectName = mfilesSettingsNodes["projectName"].InnerText;
				ProjectName = !string.IsNullOrWhiteSpace(ProjectName) ? ProjectName : ALFilesWriter.GetMFTargetDirectoryInfo().Name;
				Server = mfilesSettingsNodes["serverName"].InnerText;
				Port = mfilesSettingsNodes["port"].InnerText;
				VaultName = mfilesSettingsNodes["vaultName"].InnerText;
				VaultGUID = mfilesSettingsNodes["vaultGUID"].InnerText;
				AuthType = (MFAuthType)Int32.Parse(mfilesSettingsNodes["authType"].InnerText);
				Domain = mfilesSettingsNodes["domain"].InnerText;
				Username = mfilesSettingsNodes["username"].InnerText;
				Password = SecurityUtils.Decrypt(mfilesSettingsNodes["password"].InnerText);
				HasSettingsXmlFile = true;
			} catch {
				Console.WriteLine("[ERROR] Something wrong during reading of M-Files Settings file...");
			}
		}

		public List<string> GetMissingMFilesSettings()
		{
			List<string> invalidSettings = new List<string>();

			if (MFDevToolCommand == MFDevelopmentKitCommand.New && string.IsNullOrWhiteSpace(ProjectName))
				invalidSettings.Add("Project Name");

			if (string.IsNullOrWhiteSpace(Server))
				invalidSettings.Add("Server");

			if (string.IsNullOrWhiteSpace(VaultName) && string.IsNullOrWhiteSpace(VaultGUID))
				invalidSettings.Add("Vault Name or Vault GUID");

			if (AuthType == MFAuthType.MFAuthTypeSpecificWindowsUser && string.IsNullOrWhiteSpace(Domain))
				invalidSettings.Add("Domain");

			if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
				invalidSettings.Add("Username or Password");

			return invalidSettings;
		}

		public bool IsEmpty()
		{
			return string.IsNullOrWhiteSpace(Alias)
				&& string.IsNullOrWhiteSpace(ProjectName)
				&& string.IsNullOrWhiteSpace(Server)
				&& "2266".Equals(Port) // using default value
				&& MFAuthType.MFAuthTypeSpecificMFilesUser.Equals(AuthType) // using default value
				&& string.IsNullOrWhiteSpace(VaultName)
				&& string.IsNullOrWhiteSpace(VaultGUID)
				&& string.IsNullOrWhiteSpace(Username)
				&& string.IsNullOrWhiteSpace(Password)
				&& string.IsNullOrWhiteSpace(Domain);
		}

		public void Display()
		{
			if (HasSettingsXmlFile) {
				Console.WriteLine("[Current M-Files Settings]");
				Console.WriteLine($"Project Name\t: {ProjectName}");
				Console.WriteLine($"Server\t\t: {Server}");
				Console.WriteLine($"Port\t\t: {Port}");
				Console.WriteLine($"Vault Name\t: {VaultName}");
				Console.WriteLine($"Vault GUID\t: {VaultGUID}\n");
			}
		}
	}
}