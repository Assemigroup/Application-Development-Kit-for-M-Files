using System.IO;
using System.IO.Compression;

namespace ApplicationDevelopmentKit
{
    public class VAEBuilder
    {
#if DEBUG
		private DirectoryInfo workingDir = new DirectoryInfo(
            Path.Combine(new string[] { ALFilesWriter.GetMFTargetDirectoryInfo().FullName, "bin", "Debug", "MFVaultApplicationInstallerWorkingDir" }));
#else
		private DirectoryInfo workingDir = new DirectoryInfo(
            Path.Combine(new string[] { ALFilesWriter.GetMFTargetDirectoryInfo().FullName, "bin", "Release", "MFVaultApplicationInstallerWorkingDir" }));
#endif
		public const string WorkingDirName = "MFVaultApplicationInstallerWorkingDir";

        public VAEBuilder(string baseDir = null)
        {
            if (baseDir != null)
                workingDir = new DirectoryInfo(Path.Combine(baseDir, "MFVaultApplicationInstallerWorkingDir"));
            if (!workingDir.Exists)
                workingDir.Create();
            EnsureEmptyDir(workingDir);
        }

        public FileInfo CreateApplicationPackage(string applicationID, DirectoryInfo applicationPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(workingDir.FullName, applicationID));
            EnsureEmptyDir(directoryInfo);
            DeepCopy(applicationPath, directoryInfo, 0);
            FileInfo fileInfo = new FileInfo(Path.Combine(workingDir.FullName, applicationID + ".zip"));
			ZipFile.CreateFromDirectory(directoryInfo.FullName, fileInfo.FullName);
            return fileInfo;
        }

        private void EnsureEmptyDir(DirectoryInfo dir)
        {
            if (dir.Exists)
                dir.Delete(true);
            dir.Create();
        }

        private void CopyFiles(string sourceDir, string destinationDir)
        {
            destinationDir = EnsureTrailingPathSeparator(destinationDir);
            foreach (string file in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
                File.Copy(file, file.Replace(sourceDir, destinationDir), true);
        }

        public static string EnsureTrailingPathSeparator(string path)
        {
            string str1 = Path.DirectorySeparatorChar.ToString();
            string str2 = Path.AltDirectorySeparatorChar.ToString();
            path = path.Trim();
            if (path.EndsWith(str1) || path.EndsWith(str2))
                return path;
            if (path.Contains(str2))
                return path + str2;
            return path + str1;
        }

        public static void DeepCopy(DirectoryInfo source, DirectoryInfo target, int recursionLevel = 0)
        {
            foreach (DirectoryInfo directory in source.GetDirectories())
            {
                if (recursionLevel != 0 || !(directory.Name == "MFVaultApplicationInstallerWorkingDir"))
                    DeepCopy(directory, target.CreateSubdirectory(directory.Name), recursionLevel + 1);
            }
            foreach (FileInfo file in source.GetFiles())
            {
                if (recursionLevel != 0 || !file.Name.StartsWith("MFVaultApplicationInstaller"))
                    file.CopyTo(Path.Combine(target.FullName, file.Name));
            }
        }
    }
}