using Platinium.Shared.Data.Structures;
using System;
using System.Collections.Generic;
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
        public MainWindow()
        {
            InitializeComponent();
            InitializeMaster();
        }
        public void InitializeMaster()
        {
            Thread.Sleep(5000);
            master = new MasterController();
        }
        private void GetPlugins()
        {
            master.GetPlugins();
            listboxPlugins.ItemsSource = DataStructure.PluginDictionary;
        }
    }
}
