using System;

namespace NiceHashMiner.Configs.ConfigJsonFile
{
    public static class Folders
    {
        public static readonly string Config = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\nhml\configs\");
        public static readonly string Internals = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\nhml\internals\");
    }
}
