using NiceHashMiner.Devices;
using System;
using System.Collections.Generic;

namespace NiceHashMiner.Stats.Models
{
    public class DeviceUpdateEventArgs : EventArgs
    {
        public IEnumerable<ComputeDevice> Devices { get; }

        public DeviceUpdateEventArgs(IEnumerable<ComputeDevice> devs)
        {
            Devices = devs;
        }
    }
}
