using System;

using MFilesAPI;

namespace ApplicationDevelopmentKit
{
	public class MFilesConnection
	{
		public Vault Vault { get; set; }
	}
	public class MFilesServerConnection : MFilesConnection, IDisposable
	{
		public MFilesServerApplication MFilesServerApp { get; set; }
		public MFilesSettings MFilesSettings { get; set; }
		public bool TryConnect()
		{
			MFilesServerApp = new MFilesServerApplication();

			string domain = "null";
			if (MFilesSettings.AuthType == MFAuthType.MFAuthTypeSpecificWindowsUser)
				domain = MFilesSettings.Domain;

			MFServerConnection serverConnection = MFilesServerApp.Connect(
				MFilesSettings.AuthType
				, MFilesSettings.Username
				, MFilesSettings.Password
				, domain
				, null
				, MFilesSettings.Server
				, null
				, ""
				, true);

			if (MFServerConnection.MFServerConnectionAnonymous.Equals(serverConnection)) {
				Console.WriteLine($"[ERROR] There was a problem connecting to {MFilesSettings.Server}...");
				Console.WriteLine("[ERROR] Incorrect username or password...");
				Console.WriteLine("[ERROR] Please try again...\n");
				return false;
			}
			return true;
		}
		public void Dispose()
		{
			if (Vault.LoggedIn) {
				Vault.LogOutSilent();
				Vault = null;
			}
			MFilesServerApp.Disconnect();
			MFilesServerApp = null;
		}
	}
}