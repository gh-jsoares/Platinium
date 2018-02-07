using Platinium.Shared.Data.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlatiniumMaster
{
    class Program
    {
        private static MasterController master;
        private static int ReadLinesCount = 0;
        private static int Timeout = 60;
        static void Main(string[] args)
        {
            InitializeMaster();
            Watch();
            new Thread(() => RefreshData()).Start();
            while (true)
            {
                string command = Console.ReadLine();
                //var id = master.ExecuteMethod(command);
                master.SendCommand("7AAA-7FEC-C5F6-D623-462C-DCD6-DA5A-070A plugin screenshot action");
                foreach (var item in DataStructure.PluginDictionary.ToList())
                {
                    Console.WriteLine($"{item.Key} - {item.Value}");
                }
                foreach (var item in DataStructure.ClientList.ToList())
                {
                    Console.WriteLine($"{item.UID} - {item.Type}");
                }
            }
        }
        public static void InitializeMaster()
        {
            master = new MasterController();
            Connect();
        }
        private static void Connect()
        {
            var id = master.ExecuteMethod("Initialize");
        }
        private static void RefreshData()
        {
            while (true)
            {
                GetClients();
                GetPlugins();
                Thread.Sleep(1000);
            }
        }
        private static void GetPlugins()
        {
            var id = master.ExecuteMethod("GetPlugins");
        }
        private static void GetClients()
        {
            var id = master.ExecuteMethod("GetClients");
            //var task = Task.Run(() => checkForMessageIdStatus(id));
            //if (task.Wait(TimeSpan.FromSeconds(Timeout)))
            //{
            //    datagridClients.DataSource = DataStructure.ClientList;
            //}
        }
        public static void Watch()
        {
            var watch = new FileSystemWatcher();
            watch.Path = MasterController.LOG_PATH;
            watch.Filter = Path.GetFileName(MasterController.LOG_PATH);
            watch.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            watch.Changed += new FileSystemEventHandler(OnChanged);
            watch.EnableRaisingEvents = true;
        }
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            if (File.Exists(MasterController.LOG_PATH))
            {
                if (e.FullPath == MasterController.LOG_PATH)
                {
                    try
                    {
                        //int totalLines = File.ReadAllLines(MasterController.LOG_PATH).Count();
                        //int newLinesCount = totalLines - ReadLinesCount;
                        //string[] data = File.ReadAllLines(MasterController.LOG_PATH).Skip(ReadLinesCount).Take(newLinesCount).ToArray();
                        //UpdateTextBox(data);
                        //ReadLinesCount = totalLines;
                    }
                    catch (Exception) { }
                }
            }
        }
    }
}
