using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Platinium.Entities;
using Platinium.Shared.Data.Network;
using Platinium.Shared.Data.Packages;
using Platinium.Shared.Content;
using Platinium.Shared.Info;
using System.IO;
using System.Drawing.Imaging;

namespace PluginScreenshot
{
    public partial class ctlMain : UserControl
    {
        public static ctlMain Innstance { get; set; }
        public Plugin Plugin { get; set; }
        public ctlMain(Plugin plugin)
        {
            InitializeComponent();
            Plugin = plugin;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var s = (PlatiniumMaster)Plugin.Interface;
            NetworkManagement.WriteData(s.networkStream, new Package("Screenshot|Action", null, PackageType.PluginCommand, new ClientInfo("7AAA-7FEC-C5F6-D623-462C-DCD6-DA5A-070A"), null));
        }
        public void SetPictureboxImage(byte[] imgData)
        {
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new MethodInvoker(delegate
                {
                    Bitmap bmp;
                    using (var ms = new MemoryStream(imgData))
                    {
                        bmp = new Bitmap(ms);
                    }

                    bmp.Save(Path.GetRandomFileName());
                    pictureBox1.Image = bmp;
                    pictureBox1.Refresh();
                }));
            }
        }

        private void buttonSaveScreenshot_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Save($"{DateTime.Now.ToString("yyyyMMddHHmmssffff")}.png", ImageFormat.Png);
        }
    }
}
