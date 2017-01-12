using Platinium;
using Platinium.Shared.Data.Packages;
using Platinium.Shared.Data.Serialization;
using Platinium.Shared.Info;
using Platinium.Shared.Plugin;
using PluginTest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

[Metadata(Name = "TEST", Version = "1", Description = "Just a Test Plugin")]
public class Plugin : IPlugin
{
    public string TEST = "OLA";
    public IPluginClientController ClientController { get; set; }
    public IPluginMasterController MasterController { get; set; }

    private UserControl myInterface = new ctlMain();

    public UserControl PluginInterface { get { return myInterface; } }

    public Plugin()
    {

    }
    public void Action()
    {

    }
    public void InstantiateClient()
    {
        ClientController = new PluginClientSide();
    }
    public void InstantiateMaster()
    {
        MasterController = new PluginMasterSide();
    }
}
public class PluginClientSide : IPluginClientController
{
    public string TESTEST = "OLAOLA";
    public Package Action(Package inPackage)
    {
        Package returnPackage = inPackage;
        returnPackage = new Package("TEST|Action", "PLUGIN ACTION WORKING", Platinium.Shared.Content.PackageType.PluginCommand, inPackage.To, inPackage.From);
        return returnPackage;
    }
}
public class PluginMasterSide : IPluginMasterController
{
    public Package Action(Package inPackage)
    {
        Package returnPackage = new Package("TEST|Action", "PLUGIN ACTION WORKING (PARSED AT MASTER)", Platinium.Shared.Content.PackageType.PluginCommand, inPackage.To, inPackage.From);
        return returnPackage;
    }
}
