using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NiceHashMiner.Stats;
using NiceHashMiner.Stats.Models;

namespace NiceHashMiner.Forms.Components
{
    /// <summary>
    /// ListView to show algorithm information and enable/disable checkboxes
    /// </summary>
    public partial class DevicesListViewEnableControl : UserControl
    {
        private const int ENABLED = 0;

        protected AlgorithmsListView _algorithmsListView;

        public string FirstColumnText
        {
            get => listViewDevices.Columns[ENABLED].Text;
            set
            {
                if (value != null) listViewDevices.Columns[ENABLED].Text = value;
            }
        }


        public bool SaveToGeneralConfig { get; set; }

        public DevicesListViewEnableControl()
        {
            InitializeComponent();

            SaveToGeneralConfig = false;
            // intialize ListView callbacks
            listViewDevices.ItemChecked += ListViewDevicesItemChecked;
            //listViewDevices.CheckBoxes = false;
            NiceHashStats.OnDeviceUpdate += UpdateDevices;
        }

        public void SetAlgorithmsListView(AlgorithmsListView algorithmsListView)
        {
            _algorithmsListView = algorithmsListView;
        }

        public virtual void SetComputeDevices(IEnumerable<ComputeDevice> computeDevices)
        {
            // to not run callbacks when setting new
            var tmpSaveToGeneralConfig = SaveToGeneralConfig;
            SaveToGeneralConfig = false;
            listViewDevices.BeginUpdate();
            listViewDevices.Items.Clear();
            // set devices
            foreach (var computeDevice in computeDevices)
            {
                var lvi = new ListViewItem
                {
                    Checked = computeDevice.Enabled,
                    Text = computeDevice.GetFullName(),
                    Tag = computeDevice
                };
                listViewDevices.Items.Add(lvi);
                SetLvi(lvi, computeDevice.Index);
            }
            listViewDevices.EndUpdate();
            listViewDevices.Invalidate(true);
            // reset properties
            SaveToGeneralConfig = tmpSaveToGeneralConfig;
        }

        protected virtual void SetLvi(ListViewItem lvi, int index)
        { }

        public void ResetComputeDevices(IEnumerable<ComputeDevice> computeDevices)
        {
            SetComputeDevices(computeDevices);
        }

        private void UpdateDevices(object sender, DeviceUpdateEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke((Action) delegate
                {
                    SetComputeDevices(e.Devices);
                });
            }
            else
            {
                SetComputeDevices(e.Devices);
            }
        }
        
        public virtual void InitLocale()
        {
            devicesHeader.Text = Translations.Tr("Device");
        }

        protected virtual void ListViewDevicesItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Tag is ComputeDevice cDevice)
            {
                ComputeDeviceManager.Available.UpdateDeviceStatus(e.Item.Checked, cDevice.Index);

                if (SaveToGeneralConfig)
                {
                    ConfigManager.GeneralConfigFileCommit();
                }

                // TODO this should be done in callback from call above
                _algorithmsListView?.RepaintStatus(cDevice.Enabled, cDevice.Uuid);
            }
        }

        public void SetDeviceSelectionChangedCallback(ListViewItemSelectionChangedEventHandler callback)
        {
            listViewDevices.ItemSelectionChanged += callback;
        }

        protected virtual void DevicesListViewEnableControl_Resize(object sender, EventArgs e)
        {
            // only one 
            foreach (ColumnHeader ch in listViewDevices.Columns)
            {
                ch.Width = Width - 10;
            }
        }

        protected virtual void ListViewDevices_MouseClick(object sender, MouseEventArgs e)
        { }
    }
}
