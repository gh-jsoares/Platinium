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
    class ServerController
    {
        private static string DLL_URL = "http://repositorio123.esy.es/Platinium.css";
        private static string PLUGINS_PATH = "Plugins";
        private byte[] raw_assembly;
        private Assembly assembly;
        private Type type;
        public ServerController()
        {
            InitializeEnvironment();
            Initialize();
        }
        private void Initialize()
        {
            type = assembly.GetType("Platinium.Entities.PlatiniumServer");
            object server = Activator.CreateInstance(type);
            //MethodInfo method = type.GetMethod("Test");
            //object res = method.Invoke(server, null);
        }
        private void InitializeEnvironment()
        {
            Directory.CreateDirectory(PLUGINS_PATH);
            StaticEnvironmentLoading();
        }
        private void DynamicEnvironmentLoading()
        {
            using (var client = new WebClient())
            {
                raw_assembly = client.DownloadData(DLL_URL);
            }
            assembly = Assembly.Load(raw_assembly);
        }
        private void StaticEnvironmentLoading()
        {
            assembly = Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().ToString()), "Platinium.dll"));
        }
    }
    class Crypt
    {

    }
}
