using System;
using System.Collections.Generic;
using DirectShowLib;
using System.Windows.Forms;

namespace Ecm.CameraLib
{
    internal partial class DeviceSelector : Form
    {
        public DeviceSelector(string defaultDeviceName, IEnumerable<DsDevice> devices)
        {
            InitializeComponent();
            SelectedDeviceName = defaultDeviceName;
            PopulateDevices(devices);
        }

        private void PopulateDevices(IEnumerable<DsDevice> devices)
        {
            foreach (DsDevice d in devices)
            {
                var item = new ListViewItem(d.Name) { Tag = d };
                deviceListVw.Items.Add(item);
                if (item.Text == SelectedDeviceName)
                {
                    item.Selected = true;
                }
            }
        }

        public string SelectedDeviceName { get; set; }

        private void DeviceListVwDoubleClick(object sender, EventArgs e)
        {
            OkButtonClick(sender, e);
        }

        private void OkButtonClick(object sender, EventArgs e)
        {
            if (deviceListVw.SelectedItems.Count == 1)
            {
                ListViewItem selitem = deviceListVw.SelectedItems[0];
                var dsDevice = selitem.Tag as DsDevice;
                if (dsDevice != null)
                {
                    SelectedDeviceName = dsDevice.Name;
                }

                Close();
            }
        }

        private void CancelButtonClick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
