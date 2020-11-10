using System;

namespace ApplicationDevelopmentKit
{
	public class CLIDevelopmentKit : AbstractMFDevelopmentKit
	{
		public override void Run(string[] args)
		{
			CommandLineOptions commandLineOpts = CommandLineOptions.Parse(args);
			if (commandLineOpts?.Command == null)
				return;

			// Getting current m-files settings from xml file
			MFilesSettings = new MFilesSettings();
			MFilesSettings.ReadSettingsFile();

			// Set m-files settings from command
			MFilesSettings cliMfSettings = commandLineOpts.Command.GetMFilesSettings();
			cliMfSettings.Validate();

			if (cliMfSettings.IsValidSettings) {
				switch (cliMfSettings.MFDevToolCommand) {
				case MFDevelopmentKitCommand.Update:
				case MFDevelopmentKitCommand.Deploy:
					if (!MFilesSettings.HasSettingsXmlFile) {
						Console.WriteLine("[INFO] No existing M-Files settings file...\n");
						MFilesSettings = cliMfSettings;
						MFilesSettings.SaveToXmlFile();
					} else {
						Console.WriteLine("[INFO] M-Files settings file exists...\n");
						ConnectUsingCurrentSettings();
					}
					break;
				default: // New command
					MFilesSettings = cliMfSettings;
					MFilesSettings.SaveToXmlFile();
					ConnectUsingCurrentSettings();
					break;
				}
			} else {
				MFilesSettings.MFDevToolCommand = cliMfSettings.MFDevToolCommand;
				switch (cliMfSettings.MFDevToolCommand) {
				case MFDevelopmentKitCommand.Update:
				case MFDevelopmentKitCommand.Deploy:
					Console.WriteLine("[WARN] M-Files settings from cli is lacking...");
					Console.WriteLine("[INFO] M-Files settings file exists...\n");
					ConnectUsingCurrentSettings();
					break;
				default: // New command
					PromptUserForMFilesettings();
					MFilesSettings.SaveToXmlFile();
					break;
				}
			}
		}
	}
}