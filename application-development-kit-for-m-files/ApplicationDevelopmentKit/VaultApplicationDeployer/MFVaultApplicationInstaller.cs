using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

using Microsoft.Win32;
using MFilesAPI;

namespace ApplicationDevelopmentKit
{
    public class MFVaultApplicationInstaller : IDisposable
    {
        public MFilesSettings MFilesSettings { get; set; }
        public static string USER = null;
        public static string PASSWD = null;
        private static Vault VAULT { get; set; }
        private static VaultsOnServer Vaults { get; set; }
        private static MFilesServerApplication MFilesServer { get; set; }
        private static AdminServerConnectionProperties SERVER_CONN { get; set; }
        public bool NoPromptExit { get; set; }
        public static void RunTests(Vault vault) { }

        public MFVaultApplicationInstaller(MFilesSettings mfilesSettings)
        {
            // Go through command prompt if M-Files settings option is empty
            //Initialize();
            MFilesSettings = mfilesSettings;

        }
        public bool Run()
        {
            Console.WriteLine("\n+--------------+");
            Console.WriteLine("| VAF Deployer |");
            Console.WriteLine("+--------------+\n");

            DirectoryInfo vafDirectoryInfo = ALFilesWriter.GetMFTargetDirectoryInfo();
            string vafProjectFullPath = vafDirectoryInfo.FullName;
            string vafProjectName = vafDirectoryInfo.Name;

            ALFilesWriter alFilesWriter = FilesWriterFactory.GetFilesWriter(GeneratorArtifacts.VAULT_APPLICATION);
            alFilesWriter.WriteFiles(null);
            BuildUtils.IncludeFilesToProjectBuild(alFilesWriter.GeneratedFiles);

#if DEBUG
			string appPathStr = Path.Combine(new string[]{ vafProjectFullPath, "bin", "Debug"});
#else
            string appPathStr = Path.Combine(new string[] { vafProjectFullPath, "bin", "Release" });
#endif
            try {
                Console.WriteLine($"[INFO] Building VAF project...");
                BuildErrorConsoleLogger errorLogger = new BuildErrorConsoleLogger();
                BuildUtils.BuildProject(errorLogger);
                Console.WriteLine($"[INFO] Sucessfully built VAF project...");
                DirectoryInfo applicationPath = new DirectoryInfo(appPathStr);
                FileInfo appDefFile = new FileInfo(Path.Combine(applicationPath.FullName, "appdef.xml"));
                Version version = new Version("1.0.0.0");
                //Version version = typeof(VAF.VaultApplication).Assembly.GetName().Version;
                File.WriteAllText(appDefFile.FullName, GenerateXmlFile(version.ToString()));

                string vault = MFilesSettings.VaultGUID;
                if (string.IsNullOrWhiteSpace(vault))
                    vault = MFilesSettings.VaultName;

                Console.WriteLine($"[INFO] Creating vault application package for <{vault}>...");
                VAEBuilder vaeBuilder = new VAEBuilder();
                string applicationGuid = GetApplicationGuid(appDefFile);
                FileInfo applicationPackage = vaeBuilder.CreateApplicationPackage(applicationGuid, applicationPath);
                Console.WriteLine($"[INFO] Application package for {vault} created: " + applicationPackage);

                if (MFilesSettings.Server != null && !string.IsNullOrWhiteSpace(vault)) {
                    ConnectAndReinstall(applicationGuid, applicationPackage);
                    Console.WriteLine($"[INFO] Completed deployment for vault <{MFilesSettings.VaultName} ({MFilesSettings.VaultGUID})>...\n");
                    return true;
                } else
                    Console.WriteLine("[INFO] Skipping installation:");
            } catch (Exception e) {
                Console.WriteLine($"[ERROR] {e.Message}\n");
            }
            if (!NoPromptExit) {
                Console.WriteLine($"Press any key to exit...");
                Console.ReadKey();
            }
            return false;
        }
        public void Dispose()
        {
            if (VAULT != null)
                VAULT.LogOutSilent();
            if (MFilesServer != null)
                MFilesServer.Disconnect();
        }
        private static void Initialize()
        {
            InputServerName();
            InputCredentials();
            InputVaultName();
        }
        private static void InputServerName()
        {
            List<AdminServerConnectionProperties> adminServerConnPropsList = GetAdminServerNames(new MFilesServerApplication());
            int numLabel = 1;

            Console.WriteLine("===Vault Application Deployer===");
            while (true) {
                Console.WriteLine("Select server:");
                adminServerConnPropsList.ForEach(serverConnProps => Console.WriteLine($"{numLabel++}. {serverConnProps.Alias}"));

                string selectedServer = Console.ReadLine();
                SERVER_CONN = adminServerConnPropsList[Int32.Parse(selectedServer) - 1];
                Console.WriteLine($"You have selected: {SERVER_CONN.Name}\n");
                return;
            }
        }
        private static void InputCredentials()
        {
            bool isValidCredentials = false;

            Console.WriteLine("Select Authentication Type:");
            Console.WriteLine("1. M-Files user");
            Console.WriteLine("2. Windows user");

            string selectedAuthType = Console.ReadLine();
            switch (selectedAuthType) {
            case "2":
                Console.WriteLine($"You have selected: Windows user\n");
                SERVER_CONN.AuthType = MFAuthType.MFAuthTypeSpecificWindowsUser;
                while (!isValidCredentials) {
                    Console.WriteLine("Enter Domain:");
                    SERVER_CONN.Domain = Console.ReadLine();
                    if (SERVER_CONN.Domain == null)
                        return;

                    Console.WriteLine("Enter Username:");
                    SERVER_CONN.Username = Console.ReadLine();
                    if (SERVER_CONN.Username == null)
                        return;

                    Console.WriteLine("Enter Password:");
                    SERVER_CONN.Password = GetConsolePassword();
                    if (SERVER_CONN.Password == null)
                        return;

                    isValidCredentials = TryConnect();
                }
                break;
            case "1":
            default:
                Console.WriteLine($"You have selected: M-Files user\n");
                SERVER_CONN.AuthType = MFAuthType.MFAuthTypeSpecificMFilesUser;
                while (!isValidCredentials) {
                    Console.WriteLine("Enter Username:");
                    SERVER_CONN.Username = Console.ReadLine();
                    if (SERVER_CONN.Username == null)
                        return;

                    Console.WriteLine("Enter Password:");
                    SERVER_CONN.Password = GetConsolePassword();
                    if (SERVER_CONN.Password == null)
                        return;

                    isValidCredentials = TryConnect();
                }
                break;
            }
        }
        private static void InputVaultName()
        {
            bool isValidVaultName = false;

            Console.WriteLine();
            while (!isValidVaultName) {
                Console.WriteLine($"Select Vault: \n{GetVaultNames()}");

                string inputVault = Console.ReadLine();
                if (int.TryParse(inputVault, out int j) && j >= 1 && j <= Vaults.Count) {
                    try {
                        VAULT = Vaults[j].LogIn();
                    } catch {
                        Console.WriteLine("\nAccess denied. You do not have a user account in this document vault.");
                        Console.WriteLine("\nPlease press 'Enter' key to exit...");
                        Console.ReadLine();
                        Environment.Exit(0);
                    }
                    Console.Clear();
                    Console.WriteLine($"You have selected {Vaults[j].Name}...");
                    isValidVaultName = true;
                } else {
                    Console.WriteLine("Selected Vault Name is invalid.\n");
                }
            }
        }
        private static bool TryConnect()
        {
            MFilesServer = new MFilesServerApplication();
            MFServerConnection serverConnection =
                MFilesServer.Connect(SERVER_CONN.AuthType, SERVER_CONN.Username, SERVER_CONN.Password, SERVER_CONN.Domain, null, SERVER_CONN.Name, null, "", true);

            if (MFServerConnection.MFServerConnectionAnonymous.Equals(serverConnection)) {
                Console.WriteLine($"[ERROR] There was a problem connecting to {SERVER_CONN.Name}.");
                Console.WriteLine("[ERROR] Incorrect username or password.\n");
                return false;
            }
            return true;
        }
        private static string GetVaultNames()
        {
            StringBuilder sb = new StringBuilder();

            Vaults = MFilesServer.GetOnlineVaults();
            for (int i = 1; i < Vaults.Count + 1; i++)
                sb.AppendLine($"{i}. {Vaults[i].Name}");

            return sb?.Remove(sb.Length - 2, 1).ToString();
        }
        private static string GetConsolePassword()
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
        private static List<AdminServerConnectionProperties> GetAdminServerNames(MFilesServerApplication serverApp)
        {
            List<AdminServerConnectionProperties> serverNamesList = new List<AdminServerConnectionProperties>();
            string name = string.Format("Software\\Motive\\M-Files\\{0}\\ServerTools\\MFAdmin\\Servers", (object)serverApp.GetAPIVersion().Display);
            RegistryKey registryKey1 = Registry.CurrentUser.OpenSubKey(name, false);

            foreach (string subKeyName in registryKey1.GetSubKeyNames()) {
                RegistryKey registryKey2 = registryKey1.OpenSubKey(subKeyName);
                serverNamesList.Add(new AdminServerConnectionProperties() {
                    Alias = subKeyName,
                    Name = (string)registryKey2.GetValue("NetworkAddress")
                });
            }

            return serverNamesList;
        }
        private void ConnectAndReinstall(string applicationGuid, FileInfo applicationPackageFile)
        {
            MFilesServerApplication server = new MFilesServerApplication();
            Console.WriteLine($"[INFO] Connecting to <{MFilesSettings.Server}>...");
            server.Connect(MFilesSettings.AuthType, MFilesSettings.Username, MFilesSettings.Password, MFilesSettings.Domain, null, MFilesSettings.Server, null, "", true);
            Console.WriteLine($"[INFO] Successully connected to <{MFilesSettings.Server}>...");
            Vaults = server.GetOnlineVaults();
            VaultOnServer VaultOnServer = null;
            bool isUsingVaultGuid = !string.IsNullOrWhiteSpace(MFilesSettings.VaultGUID);
            string vaultGuidName = isUsingVaultGuid ? MFilesSettings.VaultGUID : MFilesSettings.VaultName;

            if (isUsingVaultGuid) {
                VaultOnServer = Vaults.GetVaultByGUID(vaultGuidName);
                MFilesSettings.VaultName = VaultOnServer.Name;
            } else {
                VaultOnServer = Vaults.GetVaultByName(vaultGuidName);
            }
            Console.WriteLine($"[INFO] Connecting to <{VaultOnServer.Name}>...");
            RetryingVaultConnection vaultConnection = new RetryingVaultConnection(VaultOnServer, vos => vos.LogIn());

            ReinstallApplication(applicationPackageFile, server, VaultOnServer, vaultConnection, applicationGuid);
        }
        private static string GetApplicationGuid(FileInfo appDefFile)
        {
            return XDocument.Load(appDefFile.FullName).Root.Element("guid").Value;
        }
        private static VaultOnServer GetVaultOnServer(MFilesServerApplication server, string vaultName = null)
        {
            try {
                return server.GetOnlineVaults().GetVaultByName(vaultName);
            } catch (Exception ex) {
                Console.WriteLine("[ERROR] Problem connecting to the vault: " + vaultName + "\n" + ex.Message);
            }
            return null;
        }
        private static void ReinstallApplication(FileInfo appFile, MFilesServerApplication server, VaultOnServer vaultOnServer, RetryingVaultConnection vaultConnection, string applicationGuid)
        {
            Console.WriteLine("[INFO] Checking for previous installation of the application...");
            if (vaultConnection.DoWithReconnect(vault => IsApplicationInstalled(vault, applicationGuid))) {
                Console.WriteLine("[INFO] Found previous installation of the application...");
                Console.WriteLine("[INFO] Uninstalling the previous installation of the application...");
                UninstallVAE(server, vaultOnServer, vaultConnection, applicationGuid);
            }
            Console.WriteLine("[INFO] Installing the application...");
            vaultConnection.DoWithReconnect(vault => vault.CustomApplicationManagementOperations.InstallCustomApplication(appFile.FullName));
            Console.WriteLine("[INFO] Completed installation of the application...");
            Console.WriteLine($"[INFO] Restarting the vault <{vaultOnServer.Name}>...");
            server.VaultManagementOperations.TakeVaultOffline(vaultOnServer.GUID, true);
            server.VaultManagementOperations.BringVaultOnline(vaultOnServer.GUID);
            Console.WriteLine($"[INFO] Completed restarting the vault <{vaultOnServer.Name}>...");
        }
        private static void UninstallVAE(MFilesServerApplication server, VaultOnServer vaultOnServer, RetryingVaultConnection vaultConnection, string applicationGuid)
        {
            if (!vaultConnection.DoWithReconnect(vault => TryUninstallCustomApplication(vault, applicationGuid)))
                return;
            Console.WriteLine("[INFO] Completed uninstallation...");
        }
        private static bool IsApplicationInstalled(Vault vault, string applicationGuid)
        {
            try {
                vault.CustomApplicationManagementOperations.GetCustomApplication(applicationGuid);
            } catch (COMException ex) {
                if (IsMFilesNotFoundError(ex))
                    return false;
                throw;
            }
            return true;
        }
        private static bool TryUninstallCustomApplication(Vault vault, string appID)
        {
            try {
                vault.CustomApplicationManagementOperations.UninstallCustomApplication(appID);
            } catch (COMException ex) {
                if (IsMFilesNotFoundError(ex))
                    return false;
                throw;
            }
            return true;
        }
        public static bool IsMFilesNotFoundError(Exception exception)
        {
            return exception is COMException && (exception.Message.IndexOf("(0x8004000B)") != -1 || exception.Message.IndexOf("(0x800408A4)") != -1);
        }
        private static string GenerateXmlFile(string v)
        {
            string projectName = ALFilesWriter.GetMFTargetDirectoryInfo().Name;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            sb.AppendLine("<application");
            sb.AppendLine("			type=\"server-application\"");
            sb.AppendLine("			xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
            sb.AppendLine("			xsi:noNamespaceSchemaLocation=\"http://www.m-files.com/schemas/appdef-server-v1.xsd\">");
            sb.AppendLine("  <guid>ee54cadd-fd56-4092-ac43-007f844558ef</guid>");
            sb.AppendLine($"  <name>{projectName}</name>");
            sb.AppendLine("  <description>VAF Vault Application</description>");
            sb.AppendLine("  <publisher></publisher>");
            sb.AppendLine("  <version>1.0.0</version>");
            sb.AppendLine("  <copyright></copyright>");
            sb.AppendLine("  <extension-objects>");
            sb.AppendLine("    <extension-object>");
            sb.AppendLine($"      <name>{projectName}</name>");
            sb.AppendLine($"      <assembly>{projectName}.dll</assembly>");
            sb.AppendLine($"      <class>{projectName}.VaultApplication</class>");
            sb.AppendLine("      <installation-method>Install</installation-method>");
            sb.AppendLine("      <uninstallation-method>Uninstall</uninstallation-method>");
            sb.AppendLine("      <initialization-method>Initialize</initialization-method>");
            sb.AppendLine("      <uninitialization-method>Uninitialize</uninitialization-method>");
            sb.AppendLine("      <start-operations-method>StartOperations</start-operations-method>");
            sb.AppendLine("    </extension-object>");
            sb.AppendLine("  </extension-objects>");
            sb.AppendLine("</application>");

            return sb.ToString();
        }
    }

    public class AdminServerConnectionProperties
    {
        public string Alias { get; set; }
        public string Name { get; set; }
        public MFAuthType AuthType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
    }
}