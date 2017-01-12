using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginInterface;
using System.IO;
using System.Reflection;

namespace PluginHost
{
    public class PluginServices : IPluginHost
    {
        public PluginServices()
        {

        }
        private Types.AvailablePlugins colAvailablePlugins = new Types.AvailablePlugins();

        public Types.AvailablePlugins AvailablePlugins
        {
            get { return colAvailablePlugins; }
            set { colAvailablePlugins = value; }
        }
        public void FindPlugins()
        {
            FindPlugins(AppDomain.CurrentDomain.BaseDirectory);
        }
        public void FindPlugins(string Path)
        {
            colAvailablePlugins.Clear();
            foreach (string fileOn in Directory.GetFiles(Path))
            {
                FileInfo file = new FileInfo(fileOn);
                if (file.Extension.Equals(".dll"))
                {
                    this.AddPlugin(fileOn);
                }
            }
        }
        public void ClosePlugins()
        {
            foreach (Types.AvailablePlugin pluginOn in colAvailablePlugins)
            {
                pluginOn.Instance.Dispose();
                pluginOn.Instance = null;
            }
            colAvailablePlugins.Clear();
        }
        private void AddPlugin(string FileName)
        {
            Assembly pluginAssembly = Assembly.LoadFrom(FileName);
            foreach (Type pluginType in pluginAssembly.GetTypes())
            {
                if (pluginType.IsPublic)
                {
                    if (!pluginType.IsAbstract)
                    {
                        Type typeInterface = pluginType.GetInterface("PluginInterface.IPluginG", true);
                        if (typeInterface != null)
                        {
                            Types.AvailablePlugin newPlugin = new Types.AvailablePlugin();
                            newPlugin.AssemblyPath = FileName;
                            newPlugin.Instance = (IPluginG)Activator.CreateInstance(pluginAssembly.GetType(pluginType.ToString()));
                            newPlugin.Instance.Host = this;
                            newPlugin.Instance.Initialize();
                            this.colAvailablePlugins.Add(newPlugin);
                            newPlugin = null;
                        }
                        typeInterface = null;
                    }
                }
            }
            pluginAssembly = null;
        }
        public void Feedback(string Feedback, IPluginG Plugin)
        {
            throw new NotImplementedException();
        }
    }
}
namespace Types
{
    public class AvailablePlugins : System.Collections.CollectionBase
    {
        public void Add(Types.AvailablePlugin pluginToAdd)
        {
            this.List.Add(pluginToAdd);
        }
        public void Remove(Types.AvailablePlugin pluginToRemove)
        {
            this.List.Remove(pluginToRemove);
        }
        public Types.AvailablePlugin Find(string pluginNameOrPath)
        {
            Types.AvailablePlugin toReturn = null;
            foreach (Types.AvailablePlugin pluginOn in this.List)
            {
                if ((pluginOn.Instance.Name.Equals(pluginNameOrPath)) || pluginOn.AssemblyPath.Equals(pluginNameOrPath))
                {
                    toReturn = pluginOn;
                    break;
                }
            }
            return toReturn;
        }
    }
    public class AvailablePlugin
    {
        private IPluginG myInstance = null;
        private string myAssemblyPath = "";

        public IPluginG Instance
        {
            get { return myInstance; }
            set { myInstance = value; }
        }
        public string AssemblyPath
        {
            get { return myAssemblyPath; }
            set { myAssemblyPath = value; }
        }
    }
}