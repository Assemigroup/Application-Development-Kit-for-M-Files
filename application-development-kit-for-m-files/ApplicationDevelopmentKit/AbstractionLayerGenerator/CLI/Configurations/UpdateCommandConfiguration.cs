using Microsoft.Extensions.CommandLineUtils;

namespace ApplicationDevelopmentKit
{
	public static class UpdateCommandConfiguration
	{
		public static void Configure(CommandLineApplication commandLineApp, CommandLineOptions commandLineOptions)
		{
			commandLineApp.Description = "Update the class files of the existing M-Files VAF Vault Application solution/project.";
			commandLineApp.HelpOption("-?|-h|--help");

			var projectNameOption = commandLineApp.Option("-n|--name", "Name of the solution/project", CommandOptionType.SingleValue);
			var serverNameOption = commandLineApp.Option("-s|--server", "Name of the M-Files server", CommandOptionType.SingleValue);
			var portOption = commandLineApp.Option("--port", "Port", CommandOptionType.SingleValue);
            var vaultNameOption = commandLineApp.Option("--vault", "Name of the M-Files vault", CommandOptionType.SingleValue);
            var vaultGUIDOption = commandLineApp.Option("--vguid", "GUID of the M-Files vault", CommandOptionType.SingleValue);
			var authTypeOption = commandLineApp.Option("--auth", "Authentication type (2: Windows, 3: M-Files)", CommandOptionType.SingleValue);
			var domainOption = commandLineApp.Option("--domain", "Windows domain, if selected authentication type is 'Windows'", CommandOptionType.SingleValue);
			var userNameOption = commandLineApp.Option("-u|--username", "Username", CommandOptionType.SingleValue);
			var passwordOption = commandLineApp.Option("-p|--password", "Password", CommandOptionType.SingleValue);
			var silentexit = commandLineApp.Option("-se", "Silent(No Prompt) Exit", CommandOptionType.NoValue);
			commandLineApp.OnExecute(() => {
				commandLineOptions.Command = new UpdateCommand(commandLineOptions, projectNameOption.Value(), serverNameOption.Value(), portOption.Value(),
					vaultNameOption.Value(), vaultGUIDOption.Value(), authTypeOption.Value(), domainOption.Value(), userNameOption.Value(), passwordOption.Value(), silentexit.Value() == "on");
				return 0;
			});
		}
	}
}
