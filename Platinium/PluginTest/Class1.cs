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

[PluginMetadata("PluginGeoIP", "1.0.0.0", "Plugin that returns XML data of the GEO-Location")]
public class Plugin : PluginClass
{
    public Plugin() : base(new Dictionary<dynamic, dynamic>(), new Dictionary<dynamic, dynamic>())
    {
    }
    protected override Dictionary<dynamic, dynamic> InternalProperties { get; set; }
    protected override Dictionary<dynamic, dynamic> Properties { get; set; }
    protected override byte[] ClientSideData { get; set; }
    protected override byte[] MasterSideData { get; set; }
    protected override byte[] ServerSideData { get; set; }

    [Description("Initialize")]
    public override void Initialize()
    {
        InternalProperties.Add("url", "http://freegeoip.net/xml/");
        ClientSideData = Serializer.SerializeToByte(new ClientSide());
    }

    [Description("Load Plugin")]
    public override void LoadPlugin()
    {
        GetGeoIPData();
    }
    [Description("Get Geo-IP Data")]
    private void GetGeoIPData()
    {
        WebClient wc = new WebClient
        {
            Encoding = Encoding.UTF8,
            Proxy = null
        };
        MemoryStream ms = new MemoryStream(wc.DownloadData(InternalProperties["url"]));
        XmlTextReader rdr = new XmlTextReader(InternalProperties["url"]);
        XmlDocument doc = new XmlDocument();
        ms.Position = 0;
        doc.Load(ms);
        ms.Dispose();
        foreach (XmlElement el in doc.ChildNodes[0].ChildNodes)
        {
            Properties[el.Name] = el.InnerText;
        }
    }
    class ClientSide
    {
        public ClientSide()
        {

        }
    }
}
