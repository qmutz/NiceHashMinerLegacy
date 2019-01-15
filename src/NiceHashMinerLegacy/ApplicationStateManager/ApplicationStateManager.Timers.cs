using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using SystemTimer = System.Timers.Timer;

namespace NiceHashMiner
{
    static partial class ApplicationStateManager
    {
        #region MinerStatsCheck
        private static SystemTimer _minerStatsCheck;

        private static void StartMinerStatsCheckTimer()
        {
            _minerStatsCheck = new SystemTimer();
            _minerStatsCheck.Elapsed += async (object sender, ElapsedEventArgs e) => {
                await MinersManager.MinerStatsCheck();
            };
            _minerStatsCheck.Interval = ConfigManager.GeneralConfig.MinerAPIQueryInterval * 1000;
            _minerStatsCheck.Start();
        }

        private static void StopMinerStatsCheckTimer()
        {
            _minerStatsCheck?.Stop();
            _minerStatsCheck = null;
        }
        #endregion

        #region ComputeDevicesCheck
        private static SystemTimer _computeDevicesCheckTimer;

        private static void StartComputeDevicesCheckTimer()
        {
            // return if we don't want to check devices
            if (!ConfigManager.GeneralConfig.RunScriptOnCUDA_GPU_Lost) return;

            _computeDevicesCheckTimer = new SystemTimer();
            _computeDevicesCheckTimer.Elapsed += (object sender, ElapsedEventArgs e) => {
                if (ComputeDeviceManager.Query.CheckVideoControllersCountMismath())
                {
                    // less GPUs than before, ACT!
                    try
                    {
                        var onGpusLost = new ProcessStartInfo(Directory.GetCurrentDirectory() + "\\OnGPUsLost.bat")
                        {
                            WindowStyle = ProcessWindowStyle.Minimized
                        };
                        Process.Start(onGpusLost);
                    }
                    catch (Exception ex)
                    {
                        Helpers.ConsolePrint("NICEHASH", "OnGPUsMismatch.bat error: " + ex.Message);
                    }
                }
            };
            _computeDevicesCheckTimer.Interval = 60000;
            _computeDevicesCheckTimer.Start();
        }

        private static void StopComputeDevicesCheckTimer()
        {
            _computeDevicesCheckTimer?.Stop();
            _computeDevicesCheckTimer = null;
        }
        #endregion
    }
}
