using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ApplicationDevelopmentKit
{
	public abstract class ALFilesWriter
	{
		public abstract void WriteFiles(Api api);
		public string[] GeneratedFiles { get; set; }
		private string TargetLocation = @"{0}\{1}";


		public static DirectoryInfo GetMFDevelopmentToolProjectDirectoryInfo()
		{
			return Directory.GetParent(Environment.CurrentDirectory).Parent;
		}

		public static DirectoryInfo GetMFTargetDirectoryInfo()
		{
			List<DirectoryInfo> projectDirectories = new List<DirectoryInfo>();
			string[] excludedDirectory = new string[] { ".git", ".vs", "packages", GetMFDevelopmentToolProjectDirectoryInfo().Name };

			foreach (DirectoryInfo dirInfo in GetSolutionDirectory().GetDirectories()) {
				if (!excludedDirectory.Any(dirName => dirName.ToLower().Equals(dirInfo.Name.ToLower())) && dirInfo.GetFiles("*.csproj").Any())
					projectDirectories.Add(dirInfo);
			}

			return projectDirectories.Find(d => d.Name == "VAF");
		}

		public static DirectoryInfo GetSolutionDirectory()
		{
			DirectoryInfo currentDirectory = GetMFDevelopmentToolProjectDirectoryInfo();

			while (currentDirectory != null && !currentDirectory.GetFiles("*.sln").Any())
				currentDirectory = currentDirectory.Parent;

			return currentDirectory;
		}

		protected string GetTargetPath(string targetContainer)
		{
			string targetLoc = string.IsNullOrWhiteSpace(targetContainer) ? "" : $@"{targetContainer}\";
			return string.Format(TargetLocation, GetMFTargetDirectoryInfo().FullName, targetLoc);
		}

		protected void InitializeDirectory(string target_loc)
		{
			if (!Directory.Exists(target_loc))
				Directory.CreateDirectory(target_loc);
		}

		protected void CopyFiles(string sourceDir, string destinationDir)
		{
			string[] sourceFiles = Directory.GetFiles(sourceDir);

			foreach (string sourceFile in sourceFiles) {
				// Remove path from the filename
				string fileName = sourceFile.Substring(sourceDir.Length);
				string targetFileName = destinationDir + fileName;
				File.WriteAllText(targetFileName.Replace(".txt", ".cs"), File.ReadAllText(sourceFile));
			}
		}

		protected void CopyFiles(string sourceDir, string destinationDir, Dictionary<string, string> strReplacements)
		{
			string[] sourceFiles = Directory.GetFiles(sourceDir);
			StringBuilder strBuilder = null;
			
			foreach (string sourceFile in sourceFiles) {
				// Remove path from the filename
				string fileName = sourceFile.Substring(sourceDir.Length);
				string targetFileName = destinationDir + fileName;

				strBuilder = new StringBuilder(File.ReadAllText(sourceFile));
				foreach (KeyValuePair<string, string> strKeyValPair in strReplacements)
					strBuilder.Replace(strKeyValPair.Key, strKeyValPair.Value);

				File.WriteAllText(targetFileName.Replace(".txt", ".cs"), strBuilder.ToString());
			}
		}
	}
}