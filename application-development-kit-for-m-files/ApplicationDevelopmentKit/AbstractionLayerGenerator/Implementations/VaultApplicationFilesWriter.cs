using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ApplicationDevelopmentKit
{
	class VaultApplicationFilesWriter : ALFilesWriter
	{
		private static string SourceLocation = $@"{GetMFDevelopmentToolProjectDirectoryInfo().FullName}\AbstractionLayerGenerator\Files\VaultApplication\";
		public override void WriteFiles(Api api)
		{
			Console.WriteLine("[INFO] Generating <VaultApplication> file...");
			string projectTargetDirectory = GetTargetPath("");
			InitializeDirectory(projectTargetDirectory);

			Dictionary<string, string> settingsReplacements = new Dictionary<string, string>() {
				{ "?namespace", GetMFTargetDirectoryInfo().Name }
				, { "?mfEventHandlerTypes", GetMFEventTypeHandlerAttributes() }
			};
			CopyFiles(SourceLocation, projectTargetDirectory, settingsReplacements);
			GeneratedFiles = new string[] { "VaultApplication.cs" };
			Console.WriteLine($"[INFO] Writing <VaultApplication> file to <{projectTargetDirectory}>...");
			Console.WriteLine("[INFO] Done generating <VaultApplication> file...");
		}

		private string GetMFEventTypeHandlerAttributes()
		{
			string vafDirectory = ALFilesWriter.GetMFTargetDirectoryInfo().FullName;
			List<string> parsedCustomEventHandlerAttributes = new List<string>();
			SyntaxTree vaultAppTree = CSharpSyntaxTree.ParseText(File.ReadAllText($"{vafDirectory}\\Interfaces\\IMFEvent.cs"));

			#region get event handlers
			foreach (string cs_file in Directory.GetFiles(vafDirectory, "*.cs", SearchOption.AllDirectories)) {
				SyntaxTree tree = CSharpSyntaxTree.ParseText(File.ReadAllText(cs_file));
				var root = (CompilationUnitSyntax)tree.GetCompilationUnitRoot();
				var compilation = CSharpCompilation.Create("test_compile").AddSyntaxTrees(vaultAppTree, tree);
				SemanticModel model = compilation.GetSemanticModel(tree);
				foreach (var cls in root.DescendantNodes().OfType<ClassDeclarationSyntax>().Where(o => (o.BaseList + "").Contains("OT_"))) {
					foreach (var method in cls.Members.OfType<MethodDeclarationSyntax>()) {
						foreach (var attrl in method.AttributeLists) {
							foreach (AttributeSyntax attr in attrl.Attributes) {
								var typeInfo = model.GetTypeInfo(attr).Type;
								if (typeInfo == null || !typeInfo.AllInterfaces.Any(o => o.Name + "" == "IMFEvent"))
									continue;
								string eventHandlerName = typeInfo.Name.ToString();
								if (!parsedCustomEventHandlerAttributes.Contains(eventHandlerName))
									parsedCustomEventHandlerAttributes.Add(eventHandlerName);
							}
						}
					}
				}
			}
			#endregion get event handlers

			List<string> eventHandlerTypesToInsert = 
				parsedCustomEventHandlerAttributes.Select(eventHandler => $"[EventHandler(MFEventHandlerType.{eventHandler})]").ToList();

			return string.Join("\n\t\t", eventHandlerTypesToInsert);
		}

	}
}