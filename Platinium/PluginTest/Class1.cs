using Platinium.Shared.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PluginTest
{
    [PluginMetadata("PluginGeoIP", "1.0.0.0", "Plugin that returns XML data of the GEO-Location")]
    public class PluginTest : PluginClass
    {
        public PluginTest() : base(new Dictionary<dynamic, dynamic>(), new Dictionary<dynamic, dynamic>())
        {
        }
        protected override Dictionary<dynamic, dynamic> InternalProperties { get; set; }
        protected override Dictionary<dynamic, dynamic> Properties { get; set; }

        public override void Initialize()
        {
            InternalProperties.Add("url", "http://freegeoip.net/xml/");
        }

        public override void LoadPlugin()
        {
            GetGeoIPData();
        }
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
    }
}
