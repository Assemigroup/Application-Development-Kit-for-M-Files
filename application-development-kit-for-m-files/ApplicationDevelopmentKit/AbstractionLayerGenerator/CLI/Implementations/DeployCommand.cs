using System;

using MFilesAPI;

namespace ApplicationDevelopmentKit
{
	public class DeployCommand : ICommand
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
		public bool _SilentExit;

        public DeployCommand(
            CommandLineOptions commandLineOpts,
			string projectName,
			string serverName,
            string port,
            string vaultName,
            string vaultGUID,
            string authType,
            string domain,
            string userName,
            string password,
			bool silentExit)
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
			_SilentExit = silentExit;
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
			MFilesSettings mfSettings = new MFilesSettings() { MFDevToolCommand = MFDevelopmentKitCommand.Deploy};

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
					, MFDevToolCommand = MFDevelopmentKitCommand.Deploy
					,SilentExit = _SilentExit
				};
			}

			return mfSettings;
		}
	}
}