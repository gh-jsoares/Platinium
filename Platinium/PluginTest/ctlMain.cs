using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Platinium.Shared.Data.Packages;
using Platinium.Shared.Content;
using Platinium.Shared.Info;
using Platinium.Shared.Data.Network;
using Platinium.Entities;

namespace PluginTest
{
    public partial class ctlMain : UserControl
    {
        public static ctlMain Instance { get; set; }

        public Plugin Plugin { get; set; }

        public ctlMain(Plugin plugin)
        {
            InitializeComponent();
            label1.Text = plugin.TEST;
            Plugin = plugin;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var s = (PlatiniumMaster)Plugin.Interface;
            NetworkManagement.WriteData(s.masterSocket, new Package("TEST|Action", null, PackageType.PluginCommand, new ClientInfo("7AAA-7FEC-C5F6-D623-462C-DCD6-DA5A-070A"), null));
        }
        public void SetLabelValue(string value)
        {
            if (label1.InvokeRequired)
            {
                label1.Invoke(new MethodInvoker(delegate
                {
                    label1.Text = value;
                }));
            }
        }
    }
}
