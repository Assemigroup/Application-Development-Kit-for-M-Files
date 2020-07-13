using System;
using System.IO;
using System.Text;

using MFilesAPI;

namespace ApplicationDevelopmentKit
{
	class InterfacesFilesWriter : ALFilesWriter
	{
		public override void WriteFiles(Api api)
		{
			Console.WriteLine("[INFO] Generating <Interfaces> files...");
			string interfacesTargetDirectory = GetTargetPath("Interfaces");
			InitializeDirectory(interfacesTargetDirectory);
			Console.WriteLine($"[INFO] Writing <Interfaces> files to <{interfacesTargetDirectory}>...");

			File.WriteAllText(interfacesTargetDirectory + "MFilesVault.cs", GenerateMFilesVaults(api));
			GeneratedFiles = Directory.GetFiles(interfacesTargetDirectory);
			Console.WriteLine("[INFO] Done generating <Interfaces> files...");
		}

		private string GenerateMFilesVaults(Api api)
		{
			string authType = MFAuthType.MFAuthTypeSpecificMFilesUser.Equals(api.MFilesSettings.AuthType) ?
				"MFAuthType.MFAuthTypeSpecificMFilesUser" : "MFAuthType.MFAuthTypeSpecificWindowsUser";
			string domain = string.IsNullOrWhiteSpace(api.MFilesSettings.Domain) ? "null" : api.MFilesSettings.Domain;

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("using System.Collections.Generic;\n");
			sb.AppendLine("using MFilesAPI;\n");
			sb.AppendLine($"namespace {GetMFTargetDirectoryInfo().Name}");
			sb.AppendLine("{");
			sb.AppendLine("\tpublic abstract class MFilesVault");
			sb.AppendLine("\t{");
			sb.AppendLine("\t\tprotected Vault Vault;");
			sb.AppendLine("\t\tpublic string GUID { get; set; }");
			sb.AppendLine("\t\tpublic string Name { get; set; }");

			sb.AppendLine($"\t\tpublic bool Connect(string host = \"{api.MFilesSettings.Server}\")");
			sb.AppendLine("\t\t{");
			sb.AppendLine("\t\t\tMFilesServerApplication server = new MFilesServerApplication();");
			sb.AppendLine($"\t\t\tserver.Connect({authType}, \"{api.MFilesSettings.Username}\", \"{api.MFilesSettings.Password}\", " +
				$"\"{domain}\", null, host, null, \"\", true);");
			sb.AppendLine("\t\t\tVault = server.LogInToVault(GUID);");
			sb.AppendLine("\t\t\treturn true;");
			sb.AppendLine("\t\t}");

			sb.AppendLine("\t\tpublic void Disconnect()");
			sb.AppendLine("\t\t{");
			sb.AppendLine("\t\t\tif (Vault == null)");
			sb.AppendLine("\t\t\t\treturn;");
			sb.AppendLine("\t\t\tVault.LogOutSilent();");
			sb.AppendLine("\t\t}");

			sb.AppendLine("\t\tpublic bool HasDataSetByName(string dataSetName)");
			sb.AppendLine("\t\t{");
			sb.AppendLine("\t\t\tif (string.IsNullOrWhiteSpace(dataSetName))");
			sb.AppendLine("\t\t\t\treturn false;");
			sb.AppendLine("\t\t\tDataSets dataSets = Vault.DataSetOperations.GetDataSets();");
			sb.AppendLine("\t\t\tforeach (DataSet dSet in dataSets) {");
			sb.AppendLine("\t\t\t\tif (dSet.Name.Equals(dataSetName))");
			sb.AppendLine("\t\t\t\t\treturn true;");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t\treturn false;");
			sb.AppendLine("\t\t}");

			sb.AppendLine("\t\tpublic static List<MFilesVault> GetVaultInstancesOnServer()");
			sb.AppendLine("\t\t{");
			sb.AppendLine("\t\t\tList <MFilesVault> vaultInstances = new List<MFilesVault>();");
			for (int i = 1; i <= api.Vaults.Count; i++) {
				sb.AppendLine($"\t\t\tvaultInstances.Add(new V_{api.Vaults[i].Name.Normalize_to_cs_type()}());");
			}
			sb.AppendLine("\t\t\treturn vaultInstances;");
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t}");
			sb.AppendLine("}");

			return sb.ToString();
		}
	}
}
