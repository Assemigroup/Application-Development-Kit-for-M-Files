namespace ApplicationDevelopmentKit
{
	public enum GeneratorArtifacts
	{
		INTERFACES,
		UTILITIES,
		MODELS,
		EVENT_HANDLERS,
		VAULT_APPLICATION,
		SETTINGS
	}
	public static class FilesWriterFactory
	{
		public static ALFilesWriter GetFilesWriter(GeneratorArtifacts filesDirectory)
		{
			ALFilesWriter alFilesWriter = null;

			switch (filesDirectory) {
			case GeneratorArtifacts.UTILITIES:
				alFilesWriter = new UtilitiesFilesWriter();
				break;
			case GeneratorArtifacts.INTERFACES:
				alFilesWriter = new InterfacesFilesWriter();
				break;
			case GeneratorArtifacts.EVENT_HANDLERS:
				alFilesWriter = new EventHandlersFilesWriter();
				break;
			case GeneratorArtifacts.VAULT_APPLICATION:
				alFilesWriter = new VaultApplicationFilesWriter();
				break;
			case GeneratorArtifacts.SETTINGS:
				alFilesWriter = new SettingsFilesWriter();
				break;
			case GeneratorArtifacts.MODELS:
			default:
				alFilesWriter = new ModelsFilesWriter();
				break;
			}

			return alFilesWriter;
		}
	}
}
