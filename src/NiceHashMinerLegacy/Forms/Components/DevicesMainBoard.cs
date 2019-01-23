using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NiceHashMiner.Interfaces.DataVisualizer;
using NiceHashMiner.Interfaces.StateSetters;
using NiceHashMiner.Devices;

namespace NiceHashMiner.Forms.Components
{
    public partial class DevicesMainBoard : UserControl, IEnabledDeviceStateSetter, IDevicesStateDisplayer
    {
        private enum Column : int
        {
            Enabled = 0,
            Name,
            Status,
            Temperature,
            Load,
            RPM,
            StartStop,
            //PowerModeDropdown // disable for now
        }

        //public class RowData
        //{
        //    public bool Enabled;
        //    public string Name;
        //    public string Status;
        //    public string Temperature;
        //    public string Load;
        //    public string RPM;
        //    public string StartStop;

        //    public string TagID;
        //}

        public event EventHandler<(string uuid, bool enabled)> SetDeviceEnabledState;

        public static object[] GetRowData()
        {
            object[] row0 = { true, "Name", "Status", "Temperature", "Load", "RPM", "Start/Stop" };
            return row0;
        }

        //public static object[] GetRowData(RowData rd)
        //{
        //    object[] rowData = { rd.Enabled, rd.Name, rd.Status, rd.Temperature, rd.Load, "RPM", "Start/Stop" };
        //    return rowData;
        //}

        // TODO enable this when combobox is working
        //private enum PowerMode : int
        //{
        //    Low = 0,
        //    Medium,
        //    High
        //}

        public DevicesMainBoard()
        {
            InitializeComponent();
            devicesDataGridView.CellContentClick += DevicesDataGridView_CellContentClick;
        }

        private void SetRowColumnItemValue(DataGridViewRow row, Column col, object value)
        {
            var cellItem = row.Cells[(int)col];
            cellItem.Value = value;
        }

        private void DevicesDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            Console.WriteLine($"RowIndex {e.RowIndex} ColumnIndex {e.ColumnIndex}");

            if (!(e.RowIndex >= 0)) return;

            //var columnItem = senderGrid.Columns[e.ColumnIndex];
            var row = senderGrid.Rows[e.RowIndex];
            var deviceUUID = (string)row.Tag;
            Console.WriteLine($"Row TAG {row.Tag}");
            var cellItem = row.Cells[e.ColumnIndex];
            switch (cellItem)
            {
                case DataGridViewButtonCell button:
                    button.Value = "CLICKED";
                    Console.WriteLine("DataGridViewButtonCell button");
                    break;
                case DataGridViewCheckBoxCell checkbox:
                    var deviceEnabled = checkbox.Value != null && (bool)checkbox.Value;
                    checkbox.Value = !deviceEnabled;
                    SetDeviceEnabledState?.Invoke(null, (deviceUUID, !deviceEnabled));
                    break;
                // TODO not working
                //case DataGridViewComboBoxCell comboBox:
                //    Console.WriteLine($"DataGridViewComboBoxCell comboBox {comboBox.Value}");
                //    break;

            }
        }

        // TODO this one does everything for now
        void IDevicesStateDisplayer.RefreshDeviceListView(object sender, EventArgs _)
        {
            FormHelpers.SafeInvoke(this, () => {
                // see what devices to 
                // iterate each row
                var devicesToAddUuids = new List<string>();
                var allDevs = ComputeDeviceManager.Available.Devices;
                foreach (var dev in allDevs)
                {
                    bool found = false;
                    // can't LINQ Where on rows??
                    foreach (DataGridViewRow row in devicesDataGridView.Rows)
                    {
                        var tagUUID = (string)row.Tag;
                        if (tagUUID == dev.Uuid)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found) devicesToAddUuids.Add(dev.Uuid);
                }

                // filter what to add if any
                var devsToAdd = ComputeDeviceManager.Available.Devices.Where(dev => devicesToAddUuids.Contains(dev.Uuid));
                foreach (var dev in devsToAdd)
                {
                    // add dummy data
                    devicesDataGridView.Rows.Add(GetRowData());
                    // add tag
                    var newRow = devicesDataGridView.Rows[devicesDataGridView.Rows.Count - 1];
                    newRow.Tag = dev.Uuid;
                }
                // update or init states
                foreach (DataGridViewRow row in devicesDataGridView.Rows)
                {
                    var tagUUID = (string)row.Tag;
                    var dev = ComputeDeviceManager.Available.Devices.FirstOrDefault(d => d.Uuid == tagUUID);
                    SetRowColumnItemValue(row, Column.Enabled, dev.Enabled);
                    SetRowColumnItemValue(row, Column.Name, dev.GetFullName());
                    SetRowColumnItemValue(row, Column.Status, dev.State.ToString());
                    SetRowColumnItemValue(row, Column.Temperature, ((int)dev.Temp).ToString());
                    SetRowColumnItemValue(row, Column.Load, ((int)dev.Load).ToString());
                    SetRowColumnItemValue(row, Column.RPM, dev.FanSpeed.ToString());
                    SetRowColumnItemValue(row, Column.StartStop, !dev.Enabled ? "Start" : "Stop");
                }
            });
        }
    }
}
