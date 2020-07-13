using Microsoft.Build.Locator;

namespace ApplicationDevelopmentKit
{
	public class Program
	{
		public static void Main(string[] args)
		{
			MSBuildLocator.RegisterDefaults();

			AbstractMFDevelopmentKit developmentKit = null;

			// Interactive command prompt
			if (args.Length == 0)
				developmentKit = new CommandPromptDevelopmentKit();
			// CLI
			else
				developmentKit = new CLIDevelopmentKit();

			developmentKit?.Run(args);

			AbstractCommandHandler commandHandler = CommandHandlerFactory.GetCommandHandler(developmentKit);
			if (commandHandler != null)
				commandHandler.HandleCommand();
		}
	}
}
 