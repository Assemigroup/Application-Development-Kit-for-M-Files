using System;
using System.Runtime.InteropServices;

using MFilesAPI;

namespace ApplicationDevelopmentKit
{
    public class RetryingVaultConnection
    {
        private Vault privateVault;
        private VaultOnServer vaultOnServer;
        private Func<VaultOnServer, Vault> connectFunction;
        public RetryingVaultConnection(VaultOnServer vaultOnServer, Func<VaultOnServer, Vault> connectFunction)
        {
            this.vaultOnServer = vaultOnServer;
            this.connectFunction = connectFunction;
        }
        public T DoWithReconnect<T>(Func<Vault, T> action)
        {
            if (privateVault == null)
                privateVault = connectFunction(vaultOnServer);
            int num = 0;
            while (true)
            {
                try
                {
                    ++num;
                    return action(privateVault);
                } catch (COMException ex)
                {
                    if (num >= 3)
                        throw;
                    else if ((int)MFilesExceptionHelper.ExtractMFilesErrorCode(ex.Message) == 27)
                        privateVault = connectFunction(vaultOnServer);
                    else if (!IsRetryableMFilesLockError(ex))
                        throw;
                }
            }
        }
        public void DoWithReconnect(Action<Vault> action)
        {
            DoWithReconnect(vault => {
                action(vault);
                return 0;
            });
        }
        private static bool IsRetryableMFilesLockError(Exception ex)
        {
            if (ex is COMException)
            {
                string errors = ex.ToString();
                if (errors.Contains("(0x800403F9)")
                    || errors.Contains("(0x80040454)")
                    || (errors.Contains("update conflicts with concurrent update") || errors.Contains("IBCODE=isc_no_dup"))
                    || (errors.Contains("lock conflict on no wait transaction") || errors.Contains("attempt to store duplicate value") && errors.Contains("IX_PV_"))
                    || (errors.IndexOf("(0x80040041)") != -1
                        || RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 3960)
                        || (RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 3961) || RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 1205))
                        || (RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 1203) || RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 1204) || (RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 1221) || RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 1222))))
                    || ((RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 2601) && errors.Contains("IX_PV_"))
                        || (RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 40197) || RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 40501))
                        || (RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 40549) || RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 40613) || (RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 64) || RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 233)))
                        || (RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 258)
                        || RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 10053)
                        || (RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 10054)
                        || RetryingVaultConnection.IsMSSQLServerErrorCode(errors, 10060))
                        || errors.Contains("SMux") && errors.Contains("xFFFFFFFF") && errors.Contains("SQLSTATE: 08S02")))
                    || (errors.Contains("Communication link failure") || errors.Contains("Physical connection is not usable")))
                    return true;
            }
            return false;
        }
        private static bool IsMSSQLServerErrorCode(string errors, int codeToTest)
        {
            return errors.Contains(string.Format("ERROR: {0}", (object)codeToTest)) || errors.Contains(string.Format("ERROR {0}", (object)codeToTest));
        }
    }
}