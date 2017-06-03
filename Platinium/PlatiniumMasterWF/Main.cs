using Platinium.Shared.Data.Structures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Threading;

namespace PlatiniumMasterWF
{
    public partial class Main : Form
    {
        private static MasterController master;
        private static int ReadLinesCount = 0;
        public Main()
        {
            InitializeComponent();
            InitializeMaster();
            Watch();
            GetClients();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

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
            watch.Filter = Path.GetFileName(MasterController.FILE_LOG_PATH);
            watch.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            watch.Changed += new FileSystemEventHandler(OnChanged);
            watch.EnableRaisingEvents = true;
        }
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (File.Exists(MasterController.FILE_LOG_PATH))
            {
                if (e.FullPath == MasterController.FILE_LOG_PATH)
                {
                    try
                    {
                        int totalLines = File.ReadAllLines(MasterController.FILE_LOG_PATH).Count();
                        int newLinesCount = totalLines - ReadLinesCount;
                        string[] data = File.ReadAllLines(MasterController.FILE_LOG_PATH).Skip(ReadLinesCount).Take(newLinesCount).ToArray();
                        UpdateTextBox(data);
                        ReadLinesCount = totalLines;
                    }
                    catch (Exception) { }
                }
            }
        }
        private void UpdateTextBox(string[] data)
        {

            foreach (var item in data)
            {

                textBox1.Invoke((MethodInvoker)delegate
                {
                    textBox1.AppendText(item + "\u2028" + Environment.NewLine);
                    textBox1.SelectionStart = textBox1.Text.Length;
                    textBox1.ScrollToCaret();
                });
                //Dispatcher.CurrentDispatcher.BeginInvoke(new Action(delegate
                //{
                //  textBox1.AppendText(item + "\u2028");
                //textBox1.SelectionStart = textBox1.Text.Length;
                // textBox1.ScrollToCaret();
                //}));
            }
        }
        private void Connect()
        {
            master.ExecuteMethod("Initialize");
        }
        private void GetPlugins()
        {
            master.ExecuteMethod("GetPlugins");
            Thread.Sleep(50);
            listboxPlugins.Items.Clear();
            foreach (var item in DataStructure.PluginDictionary.ToList())
            {
                listboxPlugins.Items.Add(item.Key.Name);
            }
            //listboxPlugins.DataSource = DataStructure.PluginDictionary.Select(x => x.Key.Name);
        }
        private void GetClients()
        {
            master.ExecuteMethod("GetClients");
            Thread.Sleep(50);
            datagridClients.DataSource = DataStructure.ClientList;
            //datagridClients.Columns[]
        }
        private void buttonGetClients_Click(object sender, EventArgs e)
        {
            GetClients();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetPlugins();
        }

        private void listboxPlugins_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listboxPlugins.SelectedItem != null)
            {
                string pluginKey = listboxPlugins.SelectedItem.ToString();

                var plugin = DataStructure.PluginDictionary.Where(x => x.Key.Name == pluginKey).First();

                if (!panel1.Controls.Contains(plugin.Value.PluginInterface))
                {
                    panel1.Controls.Add(plugin.Value.PluginInterface);
                    plugin.Value.PluginInterface.Dock = DockStyle.Fill;
                    plugin.Value.PluginInterface.BringToFront();
                }
                else
                {
                    plugin.Value.PluginInterface.BringToFront();
                }
            }
            
        }
    }
    //public static class RichTextBoxExtensions
    //{
    //    public static void AppendText(this RichTextBox rtb, string text, System.Drawing.Color color)
    //    {
    //        TextRange tr = new TextRange(rtb.Document.ContentEnd, rtb.Document.ContentEnd);
    //        tr.Text = text;
    //        tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);
    //    }

    //}
}
