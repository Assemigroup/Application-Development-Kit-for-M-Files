using System;

namespace ApplicationDevelopmentKit
{
	public abstract class AbstractCommandHandler
	{
		public AbstractMFDevelopmentKit MFDevelopmentKit { get; set; }
		public abstract void HandleCommand();
	}

	public class CommandHandlerFactory
	{
		public static AbstractCommandHandler GetCommandHandler(AbstractMFDevelopmentKit developmentKit)
		{
			AbstractCommandHandler commandHandler = null;

			if (developmentKit.MFilesSettings == null)
				return null;

			switch (developmentKit.MFilesSettings.MFDevToolCommand) {
			case MFDevelopmentKitCommand.New:
			case MFDevelopmentKitCommand.Update:
				commandHandler = new GeneratorHandler();
				break;
			default: // default
				commandHandler = new DeployerHandler();
				break;
			}
			commandHandler.MFDevelopmentKit = developmentKit;

			return commandHandler;
		}
	}

	public class GeneratorHandler : AbstractCommandHandler
	{
		public override void HandleCommand()
		{
			Api api = new Api(MFDevelopmentKit);
			AbstractionLayerGenerator alGenerator = new AbstractionLayerGenerator() { Api = api };
			Environment.ExitCode = Convert.ToInt32(alGenerator.Generate());
		}
	}

	public class DeployerHandler : AbstractCommandHandler
	{
		public override void HandleCommand()
		{
			MFVaultApplicationInstaller mfVaultAppInstaller = new MFVaultApplicationInstaller(MFDevelopmentKit.MFilesSettings);
			Environment.ExitCode = Convert.ToInt32(mfVaultAppInstaller.Run());
		}
	}
}