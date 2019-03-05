using NiceHashMiner.Algorithms;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Windows.Forms;
using NiceHashMiner;
using NiceHashMinerLegacy.Common.Device;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MinerSmokeTest
{
    public partial class PluginForm : Form
    {
        public PluginForm()
        {
            InitializeComponent();
            this.dgv_pluginAlgo.Rows.Clear();
            this.dgv_pluginDevices.Rows.Clear();

            dgv_pluginAlgo.CellContentClick += dgv_pluginAlgo_CellContentClick;
            dgv_pluginDevices.CellContentClick += dgv_pluginDevices_CellContentClick;

            this.Shown += new EventHandler(this.PluginFormShown);
            //CUDADevice.INSTALLED_NVIDIA_DRIVERS = new Version(416, 34);
        }

        public static object[] GetDeviceRowData(ComputeDevice d)
        {
            object[] rowData = { d.Enabled, d.GetFullName() };
            return rowData;
        }

        public static object[] GetAlgorithmRowData(NiceHashMinerLegacy.Common.Algorithm.Algorithm a)
        {
            object[] rowData = { a.Enabled, a.FirstAlgorithmType, a.MinerID };
            return rowData;
        }

        private async void PluginFormShown(object sender, EventArgs e)
        {
            MinerPaths.InitializePackages();
            ConfigManager.GeneralConfig.Use3rdPartyMiners = Use3rdPartyMiners.YES;
            await ComputeDeviceManager.QueryDevicesAsync();
            MinerPluginsManager.LoadMinerPlugins();
            var devices = AvailableDevices.Devices;

            foreach (var device in devices)
            {
                dgv_pluginDevices.Rows.Add(GetDeviceRowData(device));

                var newRow = dgv_pluginDevices.Rows[dgv_pluginDevices.Rows.Count - 1];
                newRow.Tag = device;
            }
            // disable/enable all by default 
            foreach (var device in devices)
            {
                foreach (var algo in device.GetAlgorithmSettings())
                {
                    algo.Enabled = true;
                }
            }
        }

        private void dgv_pluginDevices_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dgv_pluginAlgo.Rows.Clear();
            if (!(e.RowIndex >= 0)) return;

            var senderGrid = (DataGridView)sender;
            var row = senderGrid.Rows[e.RowIndex];
            ComputeDevice device;
            if (row.Tag is ComputeDevice dev)
            {
                device = dev;
            }
            else
            {
                // TAG is not device type
                return;
            }

            var cellItem = row.Cells[e.ColumnIndex];
            if (cellItem is DataGridViewCheckBoxCell checkbox)
            {
                var deviceEnabled = checkbox.Value != null && (bool)checkbox.Value;
                checkbox.Value = !deviceEnabled;
                device.Enabled = !deviceEnabled;
            }

            var algorithms = device.GetAlgorithmSettings().Where(algo => algo is PluginAlgorithm);
            
            foreach (var algo in algorithms)
            {
                dgv_pluginAlgo.Rows.Add(GetAlgorithmRowData(algo));

                var newRow = dgv_pluginAlgo.Rows[dgv_pluginAlgo.Rows.Count - 1];
                newRow.Tag = algo;
            }
        }

        private void dgv_pluginAlgo_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!(e.RowIndex >= 0)) return;

            var senderGrid = (DataGridView)sender;
            var row = senderGrid.Rows[e.RowIndex];

            NiceHashMinerLegacy.Common.Algorithm.Algorithm algo;
            if (row.Tag is NiceHashMinerLegacy.Common.Algorithm.Algorithm a)
            {
                algo = a;
            }
            else
            {
                // TAG is not algo type
                return;
            }

            var cellItem = row.Cells[e.ColumnIndex];
            if (cellItem is DataGridViewCheckBoxCell checkbox)
            {
                var deviceEnabled = checkbox.Value != null && (bool)checkbox.Value;
                checkbox.Value = !deviceEnabled;
                algo.Enabled = !deviceEnabled;
            }
        }


        private async void btn_pluginStart_Click(object sender, EventArgs e)
        {
            int plugMinM, plugMinS, plugMinMS;
            int.TryParse(tbx_pluginMinTimeM.Text, out plugMinM);
            int.TryParse(tbx_pluginMinTimeS.Text, out plugMinS);
            int.TryParse(tbx_pluginMinTimeMS.Text, out plugMinMS);
            int minTime = (60 * plugMinM * 1000) + (1000 * plugMinS) + plugMinMS;
            var miningTime = TimeSpan.FromMilliseconds(minTime);

            int plugDelayM, plugDelayS, plugDelayMS;
            int.TryParse(tbx_pluginStopDelayM.Text, out plugDelayM);
            int.TryParse(tbx_pluginStopDelayS.Text, out plugDelayS);
            int.TryParse(tbx_pluginStopDelayMS.Text, out plugDelayMS);
            int delayTime = (60 * plugDelayM * 1000) + (1000 * plugDelayS) + plugDelayMS;
            var stopDelayTime = TimeSpan.FromMilliseconds(delayTime); //TimeSpan.FromSeconds(1);
            var enabledDevs = AvailableDevices.Devices.Where(dev => dev.Enabled);

            var testSteps = enabledDevs.Select(dev => dev.GetAlgorithmSettings().Where(algo => algo.Enabled).Count()).Sum();
            var step = 0;

            foreach(var device in enabledDevs)
            {
                foreach(var kvp in MinerPluginLoader.MinerPluginHost.MinerPlugin)
                {
                    try
                    {
                        var uuid = kvp.Key;
                        MinerPlugin.IMinerPlugin plugin = kvp.Value;

                        var supported = plugin.GetSupportedAlgorithms(new List<BaseDevice>{
                    new CUDADevice(new BaseDevice(device.PluginDevice), device.BusID, device.GpuRam, 6,1)});
                        foreach (var dev in supported)
                        {
                            var enabledAlgorithms = dev.Value.Where(algo => algo.Enabled);

                            var miner = plugin.CreateMiner();
                            miner.InitMiningLocationAndUsername("eu", Globals.DemoUser);
                            foreach (var algo in enabledAlgorithms)
                            {
                                step++;
                                miner.InitMiningPairs(new List<(BaseDevice, NiceHashMinerLegacy.Common.Algorithm.Algorithm)> { (dev.Key, algo) });
                                
                                lbl_pluginSteps.Text = $"{step} / {testSteps}";
                                tbx_pluginInfo.Text += $"Starting miner running for {miningTime.ToString()}" + Environment.NewLine;
                                miner.StartMining();
                                await Task.Delay(miningTime);

                                tbx_pluginInfo.Text += "Stopping" + Environment.NewLine;
                                miner.StopMining();

                                tbx_pluginInfo.Text += $"Delay after stop {stopDelayTime.ToString()}" + Environment.NewLine;
                                await Task.Delay(stopDelayTime);
                                tbx_pluginInfo.Text += $"DONE" + Environment.NewLine + Environment.NewLine;

                            }
                        }
                    } catch(Exception ex)
                    {
                        tbx_pluginInfo.Text += $"Exception {ex}" + Environment.NewLine;
                    }
                }
            }
        }

    }
}
