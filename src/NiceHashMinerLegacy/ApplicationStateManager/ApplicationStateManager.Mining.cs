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

        private static void UpdateDevicesToMine()
        {
            var allDevs = ComputeDeviceManager.Available.Devices;
            var devicesToMine = allDevs.Where(dev => dev.State == DeviceState.Mining).ToList();
            if (devicesToMine.Count > 0) {
                StartMining();
                MinersManager.EnsureMiningSession(GetUsername());
                MinersManager.UpdateUsedDevices(devicesToMine);
            } else {
                StopMining(false);
                MinersManager.StopAllMiners(true);
            }
            
        }

        // TODO add check for any enabled algorithms
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
                BenchmarkManager.StartBenchmarForDevice(dev, true);
            }
            
            // TODO check count
            var devicesToMine = devicesToStart.Where(dev => !BenchmarkChecker.IsDeviceWithAllEnabledAlgorithmsWithoutBenchmarks(dev)).ToList();
            foreach (var dev in devicesToMine) {
                dev.State = DeviceState.Mining;
            }
            UpdateDevicesToMine();
            NiceHashStats.StateChanged();
            RefreshDeviceListView?.Invoke(null, null);

            return (started, "");
        }

        public static (bool started, string failReason) StartDevice(ComputeDevice device, bool skipBenhcmakrk = false)
        {
            // we can only start a device it is already stopped
            if (device.State != DeviceState.Stopped && !skipBenhcmakrk)
            {
                return (false, "Device already started");
            }

            // check if device has any benchmakrs
            var needsBenchmark = BenchmarkChecker.IsDeviceWithAllEnabledAlgorithmsWithoutBenchmarks(device);
            if (needsBenchmark && !skipBenhcmakrk)
            {
                device.State = DeviceState.Benchmarking;
                BenchmarkManager.StartBenchmarForDevice(device, true);
            }
            else
            {
                device.State = DeviceState.Mining;
                UpdateDevicesToMine();
            }

            NiceHashStats.StateChanged();

            return (true, "");
        }

        public static (bool stopped, string failReason) StopAllDevice() {
            var allDevs = ComputeDeviceManager.Available.Devices;
            // TODO when starting and stopping we are not taking Pending and Error states into account
            var devicesToStop = allDevs.Where(dev => dev.State == DeviceState.Mining || dev.State == DeviceState.Benchmarking);
            if (devicesToStop.Count() == 0) {
                return (false, "No new devices to stop");
            }

            // TODO for now no partial success so if one fails send back that everything fails
            var stopped = true;
            var failReason = "";
            // try to stop all
            foreach (var dev in devicesToStop) {
                var (success, msg) = StopDevice(dev, false);
                if (!success) {
                    stopped = false;
                    failReason = msg;
                }
            }
            NiceHashStats.StateChanged();
            StopMining(true);
            return (stopped, failReason);
        }

        public static (bool stopped, string failReason) StopDevice(ComputeDevice device, bool refreshStateChange = true)
        {
            // we can only start a device it is already stopped
            switch (device.State)
            {
                case DeviceState.Stopped:
                    return (false, $"Device {device.Uuid} already stopped");
                case DeviceState.Benchmarking:
                    device.State = DeviceState.Stopped;
                    BenchmarkManager.StopBenchmarForDevice(device);
                    if (refreshStateChange) NiceHashStats.StateChanged();
                    return (true, "");
                case DeviceState.Mining:
                    device.State = DeviceState.Stopped;
                    UpdateDevicesToMine();
                    if (refreshStateChange) NiceHashStats.StateChanged();
                    return (true, "");
                default:
                    return (false, $"Cannot handle state {device.State.ToString()} for device {device.Uuid}");
            }
        }
    }
}
