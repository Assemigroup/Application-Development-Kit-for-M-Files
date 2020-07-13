using System;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

using MFilesAPI;

namespace ApplicationDevelopmentKit
{
	public abstract class AbstractMFDevelopmentKit : MFilesServerConnection
	{
		public VaultsOnServer Vaults;
		public abstract void Run(string[] args);

		protected void ConnectUsingCurrentSettings()
		{
			Console.WriteLine($"\n[INFO] Connecting to <{MFilesSettings.Server}> server...");
			bool isValidCredentials = TryConnect();

			if (!isValidCredentials) {
				Console.WriteLine($"[ERROR] Something wrong connecting to <{MFilesSettings.Server}> server...\n");
				return;
			}
			Console.WriteLine($"[INFO] Successfully connected to <{MFilesSettings.Server}> server...");

			Vaults = MFilesServerApp.GetOnlineVaults();
			VaultOnServer vaultOnServer = null;
			bool isUsingVaultGuid = !string.IsNullOrWhiteSpace(MFilesSettings.VaultGUID);
			string vaultGuidName = isUsingVaultGuid ? MFilesSettings.VaultGUID : MFilesSettings.VaultName;

			if (isUsingVaultGuid) {
				vaultOnServer = Vaults.GetVaultByGUID(vaultGuidName);
				MFilesSettings.VaultName = vaultOnServer.Name;
			} else {
				vaultOnServer = Vaults.GetVaultByName(vaultGuidName);
			}

			Console.WriteLine($"[INFO] Connecting to <{MFilesSettings.VaultName}> vault...");
			Vault = vaultOnServer.LogIn();
			Console.WriteLine($"[INFO] Successfully connected to <{MFilesSettings.VaultName}> vault...");
		}

		protected void PromptUserForMFilesettings()
		{
			InputServerName();
			InputAuthentication();
			InputVaultName();
		}

		private void InputServerName()
		{
			bool isSelectedServerOptionValid = false;
			MFilesSettings selectedMFilesSettings = null;
			List<MFilesSettings> adminServerConnPropsList = GetAdminServerNames(new MFilesServerApplication());
			int numLabel = 1;

			while (!isSelectedServerOptionValid) {
				Console.WriteLine("Select M-Files server:");
				adminServerConnPropsList.ForEach(serverConnProps => Console.WriteLine($"({numLabel++}) {serverConnProps.Alias}"));
				string selectedServerOption = Console.ReadLine();

				bool isSelectedServerOptionInt = int.TryParse(selectedServerOption, out int selectedServerOptionInInt);
				isSelectedServerOptionValid = isSelectedServerOptionInt && selectedServerOptionInInt <= adminServerConnPropsList.Count;

				if (isSelectedServerOptionValid) {
					selectedMFilesSettings = adminServerConnPropsList[selectedServerOptionInInt - 1];
					MFilesSettings.Server = selectedMFilesSettings.Server;
					MFilesSettings.Alias = selectedMFilesSettings.Alias;
					Console.WriteLine($"[INFO] Selected M-Files Server:  {MFilesSettings.Alias} ({MFilesSettings.Server})\n");
				} else {
					Console.WriteLine($"[ERROR] Selected server <{selectedServerOption}> is invalid. Please try again...\n");
					numLabel = 1;
				}
			}
		}

		private void InputAuthentication()
		{
			bool isSelectedAuthTypeValid = false;
			var authTypes = Enum.GetValues(typeof(CommandPromptAuthType));

			while (!isSelectedAuthTypeValid) {
				Console.WriteLine("Select Authentication Type:");
				foreach (CommandPromptAuthType authType in authTypes) {
					Console.WriteLine($"({(int)authType}) {GetEnumDescription((CommandPromptAuthType)authType)}");
				}
				string selectedAuthType = Console.ReadLine();

				bool isSelectedCommandInt = int.TryParse(selectedAuthType, out int selectedAuthTypeInInt);
				isSelectedAuthTypeValid = isSelectedCommandInt
					&& selectedAuthTypeInInt <= Enum.GetNames(typeof(CommandPromptAuthType)).Length;

				if (isSelectedAuthTypeValid) {
					string selectedAuthTypeStr = GetEnumDescription((CommandPromptAuthType)selectedAuthTypeInInt);
					Console.WriteLine($"[INFO] Selected Authentication Type: {selectedAuthTypeStr}\n");

					switch (selectedAuthTypeInInt) {
					case 2:
						MFilesSettings.AuthType = MFAuthType.MFAuthTypeSpecificWindowsUser;
						InputCredentials(false);
						break;
					case 1:
					default:
						MFilesSettings.AuthType = MFAuthType.MFAuthTypeSpecificMFilesUser;
						InputCredentials();
						break;
					}
				} else {
					Console.WriteLine($"Selected authentication type <{selectedAuthType}> is invalid. Please try again...\n");
				}
			}
		}

		protected void InputCredentials(bool isMFilesCredential = true)
		{
			bool isValidCredentials = false;
			while (!isValidCredentials) {
				if (!isMFilesCredential) {
					Console.Write("Domain: ");
					MFilesSettings.Domain = Console.ReadLine();
					if (MFilesSettings.Domain == null)
						return;
				}

				Console.Write("Username: ");
				MFilesSettings.Username = Console.ReadLine();
				if (MFilesSettings.Username == null)
					return;

				Console.Write("Password: ");
				MFilesSettings.Password = GetConsolePassword();
				if (MFilesSettings.Password == null)
					return;

				isValidCredentials = TryConnect();
			}
		}

		private void InputVaultName()
		{
			bool isSelectedVaultNameValid = false;

			Console.WriteLine();
			while (!isSelectedVaultNameValid) {
				Console.WriteLine($"Select Vault: \n{GetVaultNames()}");
				string selectedVaultOption = Console.ReadLine();

				bool isSelectedVaultOptionInt = int.TryParse(selectedVaultOption, out int selectedVaultOptionIntInt);
				isSelectedVaultNameValid = isSelectedVaultOptionInt && selectedVaultOptionIntInt >= 1 && selectedVaultOptionIntInt <= Vaults.Count;

				if (isSelectedVaultNameValid) {
					try {
						Vault = Vaults[selectedVaultOptionIntInt].LogIn();
						MFilesSettings.VaultGUID = Vault.GetGUID();
						MFilesSettings.VaultName = Vault.Name;
						Console.WriteLine($"[INFO] Selected Vault: {Vault.Name}\n");
					} catch {
						Console.WriteLine("\n[ERROR] Access denied. You do not have a user account in this document vault.");
						Console.WriteLine("\nPlease press 'Enter' key to exit...");
						Console.ReadLine();
						Environment.Exit(0);
					}
					isSelectedVaultNameValid = true;
				} else {
					Console.WriteLine($"[ERROR] Selected Vault <{selectedVaultOption}> is invalid...\n");
				}
			}
		}

		private string GetVaultNames()
		{
			StringBuilder sb = new StringBuilder();

			Vaults = MFilesServerApp.GetOnlineVaults();
			for (int i = 1; i < Vaults.Count + 1; i++)
				sb.AppendLine($"({i}) {Vaults[i].Name}");

			return sb?.Remove(sb.Length - 2, 1).ToString();
		}

		private string GetConsolePassword()
		{
			StringBuilder sb = new StringBuilder();
			while (true) {
				ConsoleKeyInfo cki = Console.ReadKey(true);
				if (cki.Key == ConsoleKey.Enter) {
					Console.WriteLine();
					break;
				}
				if (cki.Key == ConsoleKey.Backspace) {
					if (sb.Length > 0) {
						Console.Write("\b\0\b");
						sb.Length--;
					}
					continue;
				}
				Console.Write('*');
				sb.Append(cki.KeyChar);
			}

			return sb.ToString();
		}

		private List<MFilesSettings> GetAdminServerNames(MFilesServerApplication serverApp)
		{
			List<MFilesSettings> serverNamesList = new List<MFilesSettings>();
			string name = string.Format("Software\\Motive\\M-Files\\{0}\\ServerTools\\MFAdmin\\Servers", (object)serverApp.GetAPIVersion().Display);
			RegistryKey registryKey1 = Registry.CurrentUser.OpenSubKey(name, false);

			foreach (string subKeyName in registryKey1.GetSubKeyNames()) {
				RegistryKey registryKey2 = registryKey1.OpenSubKey(subKeyName);
				serverNamesList.Add(new MFilesSettings() {
					Alias = subKeyName,
					Server = (string)registryKey2.GetValue("NetworkAddress")
				});
			}

			return serverNamesList;
		}

		protected static string GetEnumDescription(Enum value)
		{
			try {
				FieldInfo fi = value.GetType().GetField(value.ToString());

				DescriptionAttribute[] attributes =
					(DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

				if (attributes != null && attributes.Length > 0)
					return attributes[0].Description;
				else
					return value.ToString();
			} catch {
				return null;
			}
		}
	}

	public class CommandPromptDevelopmentKit : AbstractMFDevelopmentKit
	{
		public override void Run(string[] args)
		{
			Console.WriteLine("\n+-----------------------------------------+");
			Console.WriteLine("| Application Development Kit for M-Files |");
			Console.WriteLine("+-----------------------------------------+\n");
			MFilesSettings = new MFilesSettings();
			MFilesSettings.ReadSettingsFile();
			MFilesSettings.Display();

			InputCommand();

			switch (MFilesSettings.MFDevToolCommand) {
			case MFDevelopmentKitCommand.Update:
			case MFDevelopmentKitCommand.Deploy:
				if (!MFilesSettings.HasSettingsXmlFile) {
					Console.WriteLine("[INFO] No existing M-Files settings file...\n");
					PromptUserForMFilesettings();
					MFilesSettings.SaveToXmlFile();
				} else {
					Console.WriteLine("[INFO] M-Files settings file exists...\n");
					ConnectUsingCurrentSettings();
				}
				break;
			case MFDevelopmentKitCommand.New: // New command
				PromptUserForMFilesettings();
				MFilesSettings.SaveToXmlFile();
				break;
			}
		}

		private void InputCommand()
		{
			bool isSelectedCommandValid = false;
			var commands = Enum.GetValues(typeof(MFDevelopmentKitCommand));

			while (!isSelectedCommandValid) {
				Console.WriteLine("Select Command: ");
				foreach (MFDevelopmentKitCommand command in commands) {
					Console.WriteLine($"({(int)command}) {GetEnumDescription(command)}");
				}
				string selectedCommand = Console.ReadLine();

				bool isSelectedCommandInt = int.TryParse(selectedCommand, out int selectedCommandInInt);
				isSelectedCommandValid = isSelectedCommandInt
					&& selectedCommandInInt <= Enum.GetNames(typeof(MFDevelopmentKitCommand)).Length;
				 
				if (isSelectedCommandValid) {
					MFilesSettings.MFDevToolCommand = (MFDevelopmentKitCommand)selectedCommandInInt;
					if (MFDevelopmentKitCommand.Exit.Equals(MFilesSettings.MFDevToolCommand))
						Environment.Exit(0);

					Console.WriteLine($"[INFO] Selected Command: {MFilesSettings.MFDevToolCommand.ToString()}\n");
				} else {
					Console.WriteLine($"[ERROR] Selected command <{selectedCommand}> is invalid. Please try again...\n");
				}
			}
		}
	}
}
