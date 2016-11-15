using Platinium.Shared.Data.Serialization;
using Platinium.Shared.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

[Metadata(Name = "TEST")]
public class Plugin : IPlugin
{
    public string TEST = "OLA";
    public IPluginClientSide ClientSide { get; set; }
    public IPluginServerSide ServerSide { get; set; }
    public IPluginMasterSide MasterSide { get; set; }
    public Plugin()
    {

    }
    public void Action()
    {

    }
    public void InstantiateClient()
    {
        ClientSide = new PluginClientSide();
    }
    public void InstantiateServer()
    {
        ServerSide = new PluginServerSide();
    }
    public void InstantiateMaster()
    {
        MasterSide = new PluginMasterSide();
    }
}
public class PluginClientSide : IPluginClientSide
{
    public string TESTEST = "OLAOLA";
    public PluginClientSide()
    {

    }
    public void Action()
    {
        throw new NotImplementedException();
    }
}
public class PluginServerSide : IPluginServerSide
{
    public string TT = "00001";
    public PluginServerSide()
    {

    }
    public void Action()
    {
        throw new NotImplementedException();
    }
}
public class PluginMasterSide : IPluginMasterSide
{
    public string OO = "00003";
    public void Action()
    {
        throw new NotImplementedException();
    }
}
