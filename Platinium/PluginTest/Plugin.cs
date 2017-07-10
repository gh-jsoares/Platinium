using Platinium;
using Platinium.Shared.Data.Packages;
using Platinium.Shared.Data.Serialization;
using Platinium.Shared.Info;
using Platinium.Shared.Plugin;
using PluginTest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Platinium.Shared.Data.Network;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Platinium.Shared.Data.Structures;

[Metadata(Name = "TEST", Version = "1", Description = "Just a Test Plugin")]
public class Plugin : IPluginImplementation
{
    public string TEST = "OLA";
    public UserControl PluginInterface { get; set; }

    public dynamic Interface { get; set; }
    public PluginClientController ClientController { get; set; }
    public PluginMasterController MasterController { get; set; }

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
    public string TESTEST = "OLAOLA";
    public Package Action(Package inPackage)
    {
        Package returnPackage = new Package("TEST|Action", DataStructure.Info.ComputerName, Platinium.Shared.Content.PackageType.PluginCommand, inPackage.To, inPackage.From, null);
        
        return returnPackage;
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
        interfaceMain.SetLabelValue((string)inPackage.Content);
        Package returnPackage = new Package("TEST|Action", "PLUGIN ACTION WORKING (PARSED AT MASTER)", Platinium.Shared.Content.PackageType.PluginCommand, inPackage.To, inPackage.From, null);
        return returnPackage;
    }
}
