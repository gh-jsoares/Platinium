using Platinium;
using Platinium.Shared.Data.Packages;
using Platinium.Shared.Data.Serialization;
using Platinium.Shared.Info;
using Platinium.Shared.Plugin;
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
    public UserControlModule PluginInterfaceControl { get; set; }
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
}
public class PluginClientSide : IPluginClientController
{
    public string TESTEST = "OLAOLA";
    public PluginClientSide()
    {

    }
    public Package Action(Package inPackage)
    {
        Package returnPackage = inPackage;
        returnPackage = new Package("TEST|Action", "PLUGIN ACTION WORKING", Platinium.Shared.Content.PackageType.PluginCommand, inPackage.From, inPackage.To);
        return returnPackage;
    }
}
