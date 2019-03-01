using System;

namespace NiceHashMiner.Utils
{
    public static class MinersDownloadManager
    {
        public static readonly DownloadSetup StandardDlSetup = new DownloadSetup(
            "https://github.com/nicehash/NiceHashMinerLegacy/releases/download/1.9.0.15/bin_1_9_0_15.zip",
            Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\nhml\bins.zip"),
            Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\nhml\bin\"));

        public static readonly DownloadSetup ThirdPartyDlSetup = new DownloadSetup(
            "https://github.com/nicehash/NiceHashMinerLegacy/releases/download/1.9.0.15/bin_3rdparty_1_9_0_15.zip",
            Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\nhml\bins_3rdparty.zip"),
            Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\nhml\bin_3rdparty\"));
    }
}
