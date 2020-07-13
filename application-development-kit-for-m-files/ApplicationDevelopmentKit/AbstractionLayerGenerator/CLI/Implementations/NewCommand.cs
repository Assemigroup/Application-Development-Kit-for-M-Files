using System;

using MFilesAPI;

namespace ApplicationDevelopmentKit
{
	public class NewCommand : ICommand
	{
		private readonly CommandLineOptions _options;

		public string _ProjectName;
		public string _ServerName;
		public string _Port;
		public string _VaultGUID;
		public string _VaultName;
		public string _AuthType;
		public string _Domain;
		public string _UserName;
		public string _Password;

		public NewCommand(
			CommandLineOptions commandLineOpts,
			string projectName,
			string serverName,
			string port,
            string vaultName,
            string vaultGUID,
			string authType,
			string domain,
			string userName,
			string password)
		{
			_options = commandLineOpts;
			_ProjectName = projectName;
			_ServerName = serverName;
			_Port = port;
            _VaultName = vaultName;
            _VaultGUID = vaultGUID;
			_AuthType = authType;
			_Domain = domain;
			_UserName = userName;
			_Password = password;
		}

		private bool IsEmpty()
		{
			return string.IsNullOrWhiteSpace(_ProjectName)
				&& string.IsNullOrWhiteSpace(_ServerName)
				&& string.IsNullOrWhiteSpace(_Port)
				&& string.IsNullOrWhiteSpace(_VaultName)
				&& string.IsNullOrWhiteSpace(_VaultGUID)
				&& string.IsNullOrWhiteSpace(_AuthType)
				&& string.IsNullOrWhiteSpace(_Domain)
				&& string.IsNullOrWhiteSpace(_UserName)
				&& string.IsNullOrWhiteSpace(_Password);
		}

		public MFilesSettings GetMFilesSettings()
		{
			MFilesSettings mfSettings = new MFilesSettings() { MFDevToolCommand = MFDevelopmentKitCommand.New };

			if (!IsEmpty()) {
				mfSettings = new MFilesSettings() {
					ProjectName = _ProjectName
					, Server = _ServerName
					, Port = _Port
					, VaultName = _VaultName
					, VaultGUID = _VaultGUID
					, AuthType = (MFAuthType)Int32.Parse(_AuthType)
					, Domain = _Domain
					, Username = _UserName
					, Password = _Password
					, MFDevToolCommand = MFDevelopmentKitCommand.New
				};
			}

			return mfSettings;

			//AbstractMFDeveloperTool devTool = new CLIDeveloperTool();
			//devTool.MFilesSettings = mfilesSettings;
			//Api api = new Api(devTool);
			//AbstractionLayerGenerator alGenerator = new AbstractionLayerGenerator() { Api = api };
			//alGenerator.Generate();
		}
	}
}
