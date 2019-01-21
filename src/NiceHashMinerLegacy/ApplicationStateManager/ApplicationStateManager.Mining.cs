using NiceHashMiner.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMiner.Benchmarking;
using NiceHashMiner.Miners;
using NiceHashMiner.Stats;

namespace NiceHashMiner
{
    static partial class ApplicationStateManager
    {
        public static bool IsDemoMining { get; set; } = false;

        public static string GetUsername()
        {
            if (IsDemoMining) {
                return Globals.GetDemoUsername();
            }

            return Globals.GetUsername();
        }

        public static (bool started, string failReason) StartAllAvailableDevices()
        {
            var allDevs = ComputeDeviceManager.Available.Devices;
            var devicesToStart = allDevs.Where(dev => dev.State == DeviceState.Stopped);
            if (devicesToStart.Count() == 0) {
                return (false, "there are no new devices to start");
            }
            // TODO for now no partial success so if one fails send back that everything fails
            var started = true;
            var failReason = "";

            var devicesToBenchmark = devicesToStart.Where(dev => BenchmarkChecker.IsDeviceWithAllEnabledAlgorithmsWithoutBenchmarks(dev));
            foreach (var dev in devicesToBenchmark) {
                dev.State = DeviceState.Benchmarking;
                BenchmarkManager.StartBenchmarForDevice(dev);
            }
            
            // TODO check count
            var devicesToMine = devicesToStart.Where(dev => !BenchmarkChecker.IsDeviceWithAllEnabledAlgorithmsWithoutBenchmarks(dev)).ToList();
            foreach (var dev in devicesToMine) {
                dev.State = DeviceState.Mining;
            }
            MinersManager.EnsureMiningSession(GetUsername());
            MinersManager.UpdateUsedDevices(devicesToMine);

            foreach (var dev in devicesToStart) {
                var (ok, err) = StartDevice(dev);
                if (!ok) {
                    started = false;
                    failReason = err;
                }
            }

            NiceHashStats.StateChanged();

            return (started, "");
        }

        public static (bool started, string failReason) StartDevice(ComputeDevice device)
        {
            // we can only start a device it is already stopped
            if (device.State != DeviceState.Stopped)
            {
                return (false, "TODO redundant or error");
            }

            // check if device has any benchmakrs
            var needsBenchmark = BenchmarkChecker.IsDeviceWithAllEnabledAlgorithmsWithoutBenchmarks(device);
            if (needsBenchmark)
            {
                device.State = DeviceState.Benchmarking;
                BenchmarkManager.StartBenchmarForDevice(device);
            }
            else
            {
                device.State = DeviceState.Mining;
            }

            NiceHashStats.StateChanged();

            return (true, "");
        }
    }
}
