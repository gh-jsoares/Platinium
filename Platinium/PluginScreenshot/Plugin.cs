using Platinium.Shared.Data.Packages;
using Platinium.Shared.Plugin;
using PluginScreenshot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

[Metadata(Name = "Screenshot", Version = "1", Description = "Takes screenshots")]
public class Plugin : IPluginImplementation
{
    public dynamic Interface { get; set; }

    public PluginClientController ClientController { get; set; }
    public PluginMasterController MasterController { get; set; }
    public UserControl PluginInterface { get; set; }

    public Plugin(dynamic obj)
    {
        Interface = obj;
        PluginInterface = new ctlMain(this);
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
        byte[] imageBytes = null;
        Rectangle bounds = Screen.GetBounds(Point.Empty);
        using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            }
            ImageConverter converter = new ImageConverter();
            imageBytes = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
        }
        return new Package("Screenshot|Action", imageBytes, Platinium.Shared.Content.PackageType.PluginCommand, inPackage.To, inPackage.From, null);
    }
}
public class PluginMasterSide : PluginMasterController
{
    public PluginMasterSide(IPluginImplementation plugin) : base(plugin)
    {
        PluginInstance = plugin;
    }

    public Package Action(Package inPackage)
    {
        var interfaceMain = (ctlMain)PluginInstance.PluginInterface;
        interfaceMain.SetPictureboxImage((byte[])inPackage.Content);
        Package returnPackage = new Package("Screenshot|Action", null, Platinium.Shared.Content.PackageType.PluginCommand, inPackage.To, inPackage.From, null);
        return returnPackage;
    }
}
