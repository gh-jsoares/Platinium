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

[Metadata(Name = "TEST", Version = "1", Description = "Just a Test Plugin")]
public class Plugin : PluginImplementation
{
    public string TEST = "OLA";
    public override UserControl PluginInterface { get; set; }

    public dynamic Interface { get; set; }

    public Plugin(dynamic obj)
    {
        Interface = obj;
        PluginInterface = new ctlMain(this);
    }
}
public class PluginClientSide : PluginClientController
{
    public string TESTEST = "OLAOLA";
    public override Package Action(Package inPackage)
    {
        Package returnPackage = new Package("TEST|Action", "PLUGIN ACTION WORKING", Platinium.Shared.Content.PackageType.PluginCommand, inPackage.To, inPackage.From);
        return returnPackage;
    }
}
public class PluginMasterSide : PluginMasterController
{
    public override Package Action(Package inPackage)
    {
        Package returnPackage = new Package("TEST|Action", "PLUGIN ACTION WORKING (PARSED AT MASTER)", Platinium.Shared.Content.PackageType.PluginCommand, inPackage.To, inPackage.From);
        return returnPackage;
    }
}
