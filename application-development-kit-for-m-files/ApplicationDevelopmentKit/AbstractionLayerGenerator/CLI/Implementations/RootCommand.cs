using Microsoft.Extensions.CommandLineUtils;

namespace ApplicationDevelopmentKit
{
	public class RootCommand : ICommand
	{
		private readonly CommandLineApplication _commandLineApp;

		public RootCommand(CommandLineApplication commandLineApp)
		{
			_commandLineApp = commandLineApp;
		}

		public MFilesSettings GetMFilesSettings()
		{
			_commandLineApp.ShowHelp();
			return null;
		}
	}
}
