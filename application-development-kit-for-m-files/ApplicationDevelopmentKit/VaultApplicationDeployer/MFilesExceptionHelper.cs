using System;
using System.Globalization;

namespace ApplicationDevelopmentKit
{
    internal class MFilesExceptionHelper
    {
        private const uint SEVERITY_ERROR = 1;
        private const uint FACILITY_ITF = 4;
        public const uint E_MFILES_FILE_NOT_COMMITTED = 181;
        public const uint E_MFILES_INVALID_SESSION = 27;

        public static uint ExtractMFilesErrorCode(string errorMessage)
        {
            int startIndex1 = errorMessage.IndexOf(".cpp, ");
            if (startIndex1 == -1)
                throw new FormatException("Can't parse. Wrong error message format: " + errorMessage);
            int num1 = errorMessage.IndexOf(" (0x", startIndex1);
            int num2 = -1;
            if (num1 == num2)
                throw new FormatException("Can't parse. Wrong error message format: " + errorMessage);
            int num3 = 2;
            int startIndex2 = num1 + num3;
            int num4 = errorMessage.IndexOf(")", startIndex2);
            if (num4 == -1)
                throw new FormatException("Can't parse. Wrong error message format: " + errorMessage);
            return GetMFilesErrorCode(errorMessage.Substring(startIndex2, num4 - startIndex2));
        }

        public static uint GetMFilesErrorCode(string sCode)
        {
            return GetMFilesErrorCode(uint.Parse(sCode.Substring(2), NumberStyles.AllowHexSpecifier));
        }

        public static uint GetMFilesErrorCode(uint hr)
        {
            hr ^= 2147745792U;
            return hr;
        }
    }
}