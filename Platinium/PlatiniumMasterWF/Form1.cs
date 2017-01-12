using Platinium.Shared.Data.Structures;
using PlatiniumMasterWPF;
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
    public partial class Form1 : Form
    {
        private static MasterController master;
        private static int ReadLinesCount = 0;
        public Form1()
        {
            InitializeComponent();
            Watch();
            InitializeMaster();
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

                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(delegate
                {
                    richTextBox.AppendText(item + "\u2028");
                    richTextBox.SelectionStart = richTextBox.Text.Length;
                    richTextBox.ScrollToCaret();
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
            foreach (var item in DataStructure.PluginDictionary.ToList())
            {
                listboxPlugins.Items.Add(item.Key.Name);
            }
            //listboxPlugins.DataSource = DataStructure.PluginDictionary.Select(x => x.Key.Name);
        }
        private void GetClients()
        {
            master.GetClientList();
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
