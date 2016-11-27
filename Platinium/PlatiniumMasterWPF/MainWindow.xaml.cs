using Platinium.Shared.Data.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlatiniumMasterWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static MasterController master;
        private static int ReadLinesCount = 0;
        public MainWindow()
        {
            InitializeComponent();
            InitializeMaster();
            Watch();
        }
        public void InitializeMaster()
        {
            master = new MasterController();
            Connect();
        }
        public void Watch()
        {
            var watch = new FileSystemWatcher();
            watch.Path = MasterController.LOG_PATH;
            watch.Filter = System.IO.Path.GetFileName(MasterController.FILE_LOG_PATH);
            watch.NotifyFilter = NotifyFilters.LastWrite;
            watch.Changed += new FileSystemEventHandler(OnChanged);
            watch.EnableRaisingEvents = true;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (e.FullPath == MasterController.FILE_LOG_PATH)
            {
                int totalLines = File.ReadAllLines(MasterController.FILE_LOG_PATH).Count();
                int newLinesCount = totalLines - ReadLinesCount;
                string[] data = File.ReadAllLines(MasterController.FILE_LOG_PATH).Skip(ReadLinesCount).Take(newLinesCount).ToArray();
                UpdateTextBox(data);
                ReadLinesCount = totalLines;
            }
        }
        private void UpdateTextBox(string[] data)
        {
            foreach (var item in data)
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    textBox.Text += "\n" + item;
                }));
            }
        }
        private void Connect()
        {
            master.Connect();
        }
        private void GetPlugins()
        {
            master.GetPlugins();
            Thread.Sleep(50);
            listboxPlugins.ItemsSource = DataStructure.PluginDictionary;
        }
        private void GetClients()
        {
            master.GetClientList();
            Thread.Sleep(50);
            datagridClients.ItemsSource = DataStructure.ClientList;
        }

        private void buttonPlugin_Click(object sender, RoutedEventArgs e)
        {
            GetPlugins();
        }

        private void buttonClientList_Click(object sender, RoutedEventArgs e)
        {
            GetClients();
        }
    }
}
