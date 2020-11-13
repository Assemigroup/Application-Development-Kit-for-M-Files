using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ApplicationDevelopmentKit
{
	public class AbstractionLayerGenerator
	{
		public Api Api { get; set; }
		public bool NoPromptExit { get; set; }
		public bool Generate()
		{
			try {
				if (Api == null)
					return false;
				Console.WriteLine("\n+----------------------------+");
				Console.WriteLine("| AbstractionLayer Generator |");
				Console.WriteLine("+----------------------------+\n");
				Console.WriteLine($"[INFO] Generating AbstractionLayer for vault <{Api.MFilesSettings.VaultName} ({Api.MFilesSettings.VaultGUID})>...");
				// Proceed with abstraction
				Api.InitObjectTypes();
				Api.InitClassTypes();
				Api.InitPropertyDefinitions();
				Api.InitValueLists();
				Api.InitWorkflowWorkflowStates();

				// Generating AL class files
				List<GeneratorArtifacts> generatorArtifactsList = new List<GeneratorArtifacts>() {
					GeneratorArtifacts.UTILITIES
					, GeneratorArtifacts.MODELS
					, GeneratorArtifacts.INTERFACES
					, GeneratorArtifacts.EVENT_HANDLERS
					, GeneratorArtifacts.VAULT_APPLICATION
				};

				ALFilesWriter alFilesWriter = null;
				string[] generatedALFiles = { };

				generatorArtifactsList.ForEach(generatorArtifact => {
					alFilesWriter = FilesWriterFactory.GetFilesWriter(generatorArtifact);
					alFilesWriter.WriteFiles(Api);
					generatedALFiles = generatedALFiles.Union(alFilesWriter.GeneratedFiles).ToArray();
				});
				Console.WriteLine($"[INFO] Successfully generated AbstractionLayer for vault <{Api.MFilesSettings.VaultName} ({Api.MFilesSettings.VaultGUID})>...");

				DirectoryInfo mfTargetDirectoryInfo = ALFilesWriter.GetMFTargetDirectoryInfo();
				Console.WriteLine($"[INFO] Adding generated AbstractionLayer files to <{mfTargetDirectoryInfo.Name}> project build...");
				BuildUtils.IncludeFilesToProjectBuild(generatedALFiles);
				Console.WriteLine($"[INFO] Successfully added generated AbstractionLayer files to <{mfTargetDirectoryInfo.Name}> project build...\n");
				return true;
			} catch (Exception ex) {
				Console.WriteLine($"[ERROR] Something went wrong during execution of ALGenerator. " + ex.Message);
			} finally {
				Api?.Dispose();
			}
			Console.WriteLine("[END]");
			if (!NoPromptExit) {
				Console.WriteLine($"Press any key to exit...");
				Console.ReadKey();
			}
			return false;
		}
	}
}