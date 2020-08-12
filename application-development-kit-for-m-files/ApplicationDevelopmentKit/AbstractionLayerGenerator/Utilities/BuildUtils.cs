using System;
using System.IO;
using System.Linq;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;

namespace ApplicationDevelopmentKit
{
	class BuildUtils
	{
		public static Project VAFProject;
		public static void IncludeFilesToProjectBuild(string[] alFiles)
		{
			DirectoryInfo vafDirectoryInfo = ALFilesWriter.GetMFTargetDirectoryInfo();
			if (!vafDirectoryInfo.Exists)
				return;

			FileInfo projectBuildFile = new FileInfo($@"{vafDirectoryInfo.FullName}\{vafDirectoryInfo.Name}.csproj");

			if (!projectBuildFile.Exists)
				return;

			VAFProject = new Project($@"{projectBuildFile.FullName}");

			var prjItems = VAFProject.Items;
			string fileToInclude = string.Empty;
			foreach (string alFile in alFiles) {
				fileToInclude = $@"{alFile.Replace($"{vafDirectoryInfo.FullName}\\", "")}";
				if (prjItems.Any(prjItem => "Compile".Equals(prjItem.ItemType) && fileToInclude.Equals(prjItem.UnevaluatedInclude)))
					continue;

				VAFProject.AddItem("Compile", fileToInclude);
			}

			VAFProject.Save();
		}

		public static void BuildProject(ILogger logger = null)
		{
			DirectoryInfo vafDirectoryInfo = ALFilesWriter.GetMFTargetDirectoryInfo();
			if (!vafDirectoryInfo.Exists)
				return;

			if (VAFProject == null)
				VAFProject = new Project($@"{vafDirectoryInfo.FullName}\{vafDirectoryInfo.Name}.csproj");
#if !DEBUG
			BuildUtils.VAFProject.SetProperty("Configuration", "Release");
#endif
			bool isBuildSuccessfull = logger != null ? VAFProject.Build(logger) : VAFProject.Build();
			if (!isBuildSuccessfull) {
				throw new Exception($"Compile issue(s) found in VAF project...");
			}
		}
	}
}
