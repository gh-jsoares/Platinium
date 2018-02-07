using Platinium.Shared.Plugin;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PluginScreenshot2
{
    public class PictureForm : Form
    {
        public PictureBox PictureBox { get; private set; }
        private PluginMasterSide _pluginMaster { get; set; }
        public PictureForm(PluginMasterSide pluginMaster)
        {
            _pluginMaster = pluginMaster;
            this.FormClosed += PictureForm_FormClosed;
            PictureBox = new PictureBox();
            PictureBox.Dock = DockStyle.Fill;
            PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            this.Controls.Add(PictureBox);
        }

        private void PictureForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _pluginMaster.IsInUse = false;
        }

        public void RefreshImage(Image image)
        {
            if (PictureBox.InvokeRequired)
            {
                Invoke(new RefreshImageCallback(RefreshImage), new object[] { image });
            } else
            {
                PictureBox.Image = image;
                PictureBox.Refresh();
            }
        }
        delegate void RefreshImageCallback(Image image);
    }
}
