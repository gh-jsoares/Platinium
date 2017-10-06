using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlatiniumMasterWF
{
    public class MasterController
    {
        private static string DLL_URL = "http://repositorio123.esy.es/Platinium.css";
        private static string ROOT_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Platinium");
        public static string LOG_PATH = Path.Combine(ROOT_PATH, "log");
        public static string FILE_LOG_PATH = Path.Combine(LOG_PATH, "log_master_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".log");
        private byte[] raw_assembly;
        private Assembly assembly;
        private Type type;
        private object instance;
        public MasterController()
        {
            InitializeEnvironment();
            Initialize();
        }
        private void Initialize()
        {
            type = assembly.GetType("Platinium.Entities.PlatiniumMaster");
            FieldInfo field = type.GetField("FILE_LOG_PATH", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            instance = Activator.CreateInstance(type);
            field.SetValue(instance, FILE_LOG_PATH);
        }
        private void InitializeEnvironment()
        {
            Directory.CreateDirectory(ROOT_PATH);
            Directory.CreateDirectory(LOG_PATH);
            var logFile = File.Create(FILE_LOG_PATH);
            logFile.Close();
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
        public string ExecuteMethod(string method)
        {
            MethodInfo methodInfo = type.GetMethod(method);
            return (string)methodInfo.Invoke(instance, null);
        }
    }
    class Crypt
    {

    }
}
