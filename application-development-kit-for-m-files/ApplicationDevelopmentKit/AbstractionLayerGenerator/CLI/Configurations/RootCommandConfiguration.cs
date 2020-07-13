using Microsoft.Extensions.CommandLineUtils;

namespace ApplicationDevelopmentKit
{
	public static class RootCommandConfiguration
	{
		public static void Configure(CommandLineApplication commandLineApp, CommandLineOptions commandLineOptions)
		{
			commandLineApp.Name = "mfcli";
			commandLineApp.Description = "M-Files CLI";
			commandLineApp.HelpOption("-?|-h|--help");

			commandLineApp.Command("new", c => NewCommandConfiguration.Configure(c, commandLineOptions));
			commandLineApp.Command("update", c => UpdateCommandConfiguration.Configure(c, commandLineOptions));
			commandLineApp.Command("deploy", c => DeployCommandConfiguration.Configure(c, commandLineOptions));

			commandLineApp.OnExecute(() => {
				commandLineOptions.Command = new RootCommand(commandLineApp);
				return 0;
			});
		}
	}
}
