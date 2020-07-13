using System.ComponentModel;

namespace ApplicationDevelopmentKit
{
	public interface ICommand
	{
		MFilesSettings GetMFilesSettings();
	}

	public enum MFDevelopmentKitCommand
	{
		[Description("New AbstractionLayer")] New = 1,
		[Description("Update AbstractionLayer")] Update = 2,
		[Description("Deploy VAF Vault Application")] Deploy = 3,
		[Description("Exit")] Exit = 4
	}
}
