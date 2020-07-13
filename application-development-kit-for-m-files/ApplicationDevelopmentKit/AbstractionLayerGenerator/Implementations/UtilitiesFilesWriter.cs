using System;
using System.Collections.Generic;
using System.IO;

using MFilesAPI;

namespace ApplicationDevelopmentKit
{
	class UtilitiesFilesWriter : ALFilesWriter
	{
		private static string SourceLocation = $@"{GetMFDevelopmentToolProjectDirectoryInfo().FullName}\AbstractionLayerGenerator\Files\Utilities\";
		public override void WriteFiles(Api api)
		{
			// TODO: Generate MFilesApiConnection file for api and test configurations only
			Console.WriteLine("[INFO] Generating <Utilities> files...");
			string utilitiesTargetDirectory = GetTargetPath("Utilities");
			InitializeDirectory(utilitiesTargetDirectory);

			string authType = MFAuthType.MFAuthTypeSpecificMFilesUser.Equals(api.MFilesSettings.AuthType) ?
				"MFAuthType.MFAuthTypeSpecificMFilesUser" : "MFAuthType.MFAuthTypeSpecificWindowsUser";
			Dictionary<string, string> settingsReplacements = new Dictionary<string, string>() {
				{ "?namespace", GetMFTargetDirectoryInfo().Name },
				{  "?authType", authType },
				{ "?user", api.MFilesSettings.Username },
				{ "?passwd", api.MFilesSettings.Password },
				{ "?domain", api.MFilesSettings.Domain != null ? api.MFilesSettings.Domain : "null" },
				{ "?host", api.MFilesSettings.Server },
				{ "?guid", api.MFilesSettings.VaultGUID }
			};
			CopyFiles(SourceLocation, utilitiesTargetDirectory, settingsReplacements);
			GeneratedFiles = Directory.GetFiles(utilitiesTargetDirectory);
			Console.WriteLine($"[INFO] Writing <Utilities> files to <{utilitiesTargetDirectory}>...");
			Console.WriteLine("[INFO] Done generating <Utilities> files...");
		}
	}
}
