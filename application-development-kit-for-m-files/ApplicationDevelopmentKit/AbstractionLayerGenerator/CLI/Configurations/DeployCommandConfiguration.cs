using Microsoft.Extensions.CommandLineUtils;

namespace ApplicationDevelopmentKit
{
	public static class DeployCommandConfiguration
	{
		public static void Configure(CommandLineApplication commandLineApp, CommandLineOptions commandLineOptions)
		{
			commandLineApp.Description = "Deploy the VAF Vault Application to the vault.";
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

            commandLineApp.OnExecute(() => {
                commandLineOptions.Command = new DeployCommand(commandLineOptions, projectNameOption.Value(), serverNameOption.Value(), portOption.Value(),
                    vaultNameOption.Value(), vaultGUIDOption.Value(), authTypeOption.Value(), domainOption.Value(), userNameOption.Value(), passwordOption.Value());
                return 0;
			});
		}
	}
}
