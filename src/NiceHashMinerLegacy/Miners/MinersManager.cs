using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using NiceHashMiner.Stats;
using NiceHashMiner.Switching;

namespace NiceHashMiner.Miners
{
    public static class MinersManager
    {
        private static MiningSession _curMiningSession;

        public static void StopAllMiners(bool headless)
        {
            _curMiningSession?.StopAllMiners(headless);
            Ethlargement.Stop();
            _curMiningSession = null;
            NiceHashStats.StateChanged();
        }

        public static void StopAllMinersNonProfitable()
        {
            _curMiningSession?.StopAllMinersNonProfitable();
        }

        public static string GetActiveMinersGroup()
        {
            // if no session it is idle
            return _curMiningSession != null ? _curMiningSession.GetActiveMinersGroup() : "IDLE";
        }

        public static List<int> GetActiveMinersIndexes()
        {
            return _curMiningSession != null ? _curMiningSession.ActiveDeviceIndexes : new List<int>();
        }

        //public static double GetTotalRate()
        //{
        //    return _curMiningSession?.GetTotalRate() ?? 0;
        //}

        // TODO worker and btc are now username remove or replace
        public static bool StartInitialize(string username)
        {
            _curMiningSession = new MiningSession(ComputeDeviceManager.Available.Devices, username);

            NiceHashStats.StateChanged();

            return _curMiningSession.IsMiningEnabled;
        }

        public static void EnsureMiningSession(string username)
        {
            if (_curMiningSession == null)
            {
                _curMiningSession = new MiningSession(new List<ComputeDevice>(), username);
            }
        }

        public static bool IsMiningEnabled()
        {
            return _curMiningSession != null && _curMiningSession.IsMiningEnabled;
        }

        public static void UpdateUsedDevices(IEnumerable<ComputeDevice> devices)
        {
            _curMiningSession?.UpdateUsedDevices(devices);
        }

        public static void RestartMiners()
        {
            _curMiningSession?.RestartMiners();
        }


        /// <summary>
        /// SwichMostProfitable should check the best combination for most profit.
        /// Calculate profit for each supported algorithm per device and group.
        /// </summary>
        /// <param name="niceHashData"></param>
        //[Obsolete("Deprecated in favour of AlgorithmSwitchingManager timer")]
        //public static async Task SwichMostProfitableGroupUpMethod()
        //{
        //    if (_curMiningSession != null) await _curMiningSession.SwichMostProfitableGroupUpMethod();
        //}

        public static async Task MinerStatsCheck()
        {
            if (_curMiningSession != null) await _curMiningSession.MinerStatsCheck();
        }
    }
}
