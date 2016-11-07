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
    class Client
    {
        private static string ROOT_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Platinium");
        private static string TEMP_PATH = Path.GetTempPath();
        private static string PLUGINS_PATH = Path.Combine(ROOT_PATH, "plugins");
        private static string CONFIG_PATH = Path.Combine(ROOT_PATH, "config");
        private static string COREASSEMBLY_PATH = Path.Combine(ROOT_PATH, "core");
        private static string COREASSEMBLYFILE_PATH = Path.Combine(COREASSEMBLY_PATH, "Platinium.dll");
        private Assembly assembly;
        private Type type;
        public Client()
        {
            InitializeEnvironment();
            Initialize();
        }
        private void Initialize()
        {
            type = assembly.GetType("Platinium.Shared.Entities.Client");
            object client = Activator.CreateInstance(type);
        }
        private void InitializeEnvironment()
        {
            Directory.CreateDirectory(PLUGINS_PATH);
            Directory.CreateDirectory(CONFIG_PATH);
            Directory.CreateDirectory(COREASSEMBLY_PATH);
            assembly = Assembly.LoadFrom(COREASSEMBLYFILE_PATH);
        }
    }
}
