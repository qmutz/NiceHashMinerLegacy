using MinerPluginToolkitV1.ClaymoreCommon;
using NiceHashMinerLegacy.Common;
using System.IO;

namespace ClaymoreHub
{
    public class Claymore : ClaymoreBase
    {
        public Claymore(string uuid) : base(uuid)
        {
        }

        protected override (string binPath, string binCwd) GetBinAndCwdPaths() //TODO why is this also here (already in claymore base)
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "EthDcrMiner64.exe");
            var binCwd = pluginRootBins;
            return (binPath, binCwd);
        }
    }
}
