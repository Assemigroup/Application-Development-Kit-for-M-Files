using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ApplicationDevelopmentKit
{
	class EventHandlersFilesWriter : ALFilesWriter
	{
		private static string SourceLocation = $@"{GetMFDevelopmentToolProjectDirectoryInfo().FullName}\AbstractionLayerGenerator\Files\EventHandlers\";

		public override void WriteFiles(Api api)
		{
			string eventHandlersTargetLocation = GetTargetPath("EventHandlers");

			string[] allCsFiles = Directory.EnumerateFiles(eventHandlersTargetLocation, "*.cs", SearchOption.AllDirectories).ToArray();
			if (Directory.Exists(eventHandlersTargetLocation)
				&& allCsFiles.Length != 0) {
				Console.WriteLine("[INFO] <Eventhandlers> directory and files already exist...");
				Console.WriteLine("[INFO] Skip generating <Eventhandlers> files...");
				GeneratedFiles = allCsFiles;
				return;
			}

			Console.WriteLine("[INFO] Generating <Eventhandlers> files...");
			InitializeDirectory(eventHandlersTargetLocation);
			CopyFiles(SourceLocation, eventHandlersTargetLocation, new Dictionary<string, string>() { { "?namespace", GetMFTargetDirectoryInfo().Name } });
			GeneratedFiles = Directory.GetFiles(eventHandlersTargetLocation, "*.cs");
			Console.WriteLine($"[INFO] Writing <Eventhandlers> files to <{eventHandlersTargetLocation}>...");
			Console.WriteLine("[INFO] Done generating <Eventhandlers> files...");
		}
	}
}
