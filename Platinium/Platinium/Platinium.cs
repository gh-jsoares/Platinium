using Platinium.Shared.Data.Packages;
using Platinium.Shared.Data.Serialization;
using Platinium.Shared.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Net;
using System.Runtime.Serialization;
using Platinium.Shared.Plugin;
using Platinium.Shared.Core;
using Platinium.Shared.Data.Structures;
using System.Threading;
using Platinium.Shared.Content;
using Platinium.Connection;
using System.Reflection;

namespace Platinium
{
    namespace Shared
    {
        namespace Core
        {
            public class Converter
            {
                public static Dictionary<string, object> ClassToDictionary(object objectToConvert)
                {
                    return objectToConvert.GetType()
                         .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                         .ToDictionary(prop => prop.Name, prop => prop.GetValue(objectToConvert, null));
                }
                public static List<Dictionary<string, object>> ClassToDictionaryList(List<object> objectListToConvert)
                {
                    List<Dictionary<string, object>> tempList = new List<Dictionary<string, object>>();
                    foreach (object objectToConvert in objectListToConvert)
                    {
                        tempList.Add(objectToConvert.GetType()
                         .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                         .ToDictionary(prop => prop.Name, prop => prop.GetValue(objectToConvert, null)));
                    }
                    return tempList;
                }
            }
        }

        namespace Content
        {
            public enum CommandType
            {
                Base,
                PluginTransfer
            }
            public interface IContent
            {
                object Data { get; set; }
                Type DataType { get; set; }
                CommandType EnumCommandType { get; set; }
            }
            [Serializable]
            public class BaseCommand : IContent
            {
                public object Data { get; set; }
                public Type DataType { get; set; }
                public CommandType EnumCommandType { get; set; }
                public BaseCommand()
                {

                }
                public BaseCommand(object data, Type datatype, CommandType enumcommandtype)
                {
                    Data = data;
                    DataType = datatype;
                    EnumCommandType = enumcommandtype;
                }
            }
        }
        namespace Plugin
        {
            /// <summary>
            /// The IPlugin Interface.
            /// </summary>
            public interface IPlugin
            {
                void Initialize();
                void LoadPlugin();
                void SetInternalProperties(Dictionary<dynamic, dynamic> properties);
                void SetProperties(Dictionary<dynamic, dynamic> properties);
                void AddProperties(List<dynamic> properties);
                void AddProperties(Dictionary<dynamic, dynamic> properties);
                void RemoveProperties(List<dynamic> properties);
                Dictionary<dynamic, dynamic> GetInternalDictionary();
                Dictionary<dynamic, dynamic> GetDictionary();
                IEnumerable<dynamic> GetDictionaryKeys();
                IEnumerable<dynamic> GetDictionaryValues();
            }
            /// <summary>
            /// The IPluginMetadata interface.
            /// </summary>
            public interface IPluginMetadata
            {
                string Name { get; }
                string Version { get; }
                string Description { get; }
            }
            /// <summary>
            /// The PluginMetadataAttribute Class
            /// </summary>
            [MetadataAttribute]
            [AttributeUsage(AttributeTargets.Class)]
            public class PluginMetadataAttribute : ExportAttribute, IPluginMetadata
            {
                /// <summary>
                /// The Plugin Metadata.
                /// </summary>
                /// <param name="name">Name of the plugin.</param>
                /// <param name="version">Version of the plugin.</param>
                /// <param name="description">Description of the plugin.</param>
                public PluginMetadataAttribute(string name, string version, string description)
                        : base(typeof(PluginClass))
                {
                    Name = name;
                    Version = version;
                    Description = description;
                }
                /// <summary>
                /// Name of the plugin.
                /// </summary>
                public string Name { get; set; }
                /// <summary>
                /// Version of the plugin.
                /// </summary>
                public string Version { get; set; }
                /// <summary>
                /// Description of the plugin.
                /// </summary>
                public string Description { get; set; }
            }
            /// <summary>
            /// The Plugin Class is derived from the IPlugin Interface. This is the base class for all the plugins developed.
            /// All Plugins must follow this class guidelines.
            /// </summary>
            public abstract class PluginClass : IPlugin
            {
                /// <summary>
                /// The property that will store the parameter Key and Value respectively.
                /// </summary>
                protected abstract Dictionary<dynamic, dynamic> Properties { get; set; }
                protected abstract Dictionary<dynamic, dynamic> InternalProperties { get; set; }
                /// <summary>
                /// The Plugin Constructor that sets the Properties when the Plugin is called.
                /// </summary>
                /// <param name="properties">The Dictionary containing the Key and Value that will be stored.</param>
                protected PluginClass(Dictionary<dynamic, dynamic> properties, Dictionary<dynamic, dynamic> internalProperties)
                {
                    Properties = properties;
                    InternalProperties = internalProperties;
                }

                public virtual void SetInternalProperties(Dictionary<dynamic, dynamic> properties)
                {
                    foreach (var property in properties)
                    {
                        InternalProperties[property.Key] = property.Value;
                    }
                }
                /// <summary>
                /// Sets new values for the specified Keys.
                /// </summary>
                /// <param name="properties">Dictionary Key and Value.</param>
                public virtual void SetProperties(Dictionary<dynamic, dynamic> properties)
                {
                    foreach (var property in properties)
                    {
                        Properties[property.Key] = property.Value;
                    }
                }
                /// <summary>
                /// Adds new properties with the specified keys, and value default Empty.
                /// </summary>
                /// <param name="properties">The List of Keys.</param>
                public void AddProperties(List<dynamic> properties)
                {
                    foreach (var property in properties)
                    {
                        Properties.Add(property, "");
                    }
                }
                /// <summary>
                /// Adds new properties with the specified keys, and specified values.
                /// </summary>
                /// <param name="properties">The Dictionary containing the Key and Value that will be added.</param>
                public void AddProperties(Dictionary<dynamic, dynamic> properties)
                {
                    foreach (var property in properties)
                    {
                        Properties.Add(property.Key, property.Value);
                    }
                }
                /// <summary>
                /// Removes a specified List of Keys from Properties.
                /// </summary>
                /// <param name="properties">The List of Keys that will be removed.</param>
                public void RemoveProperties(List<dynamic> properties)
                {
                    foreach (var property in properties)
                    {
                        Properties.Remove(property);
                    }
                }

                public abstract void Initialize();
                /// <summary>
                /// Loads the Plugin. Here is where the developer will place his plugin code.
                /// </summary>
                public abstract void LoadPlugin();

                public virtual Dictionary<dynamic, dynamic> GetInternalDictionary()
                {
                    return InternalProperties;
                }
                /// <summary>
                /// Returns the Properties with the Key and Value in a Dictionary.
                /// </summary>
                /// <returns>Returns a Dictionary.</returns>
                public virtual Dictionary<dynamic, dynamic> GetDictionary()
                {
                    return Properties;
                }
                /// <summary>
                /// Gets the Keys that Properties contains.
                /// </summary>
                /// <returns>Returns an IEnumerable that can be converted to any Enumerable type object.</returns>
                public virtual IEnumerable<dynamic> GetDictionaryKeys()
                {
                    return Properties.Keys;
                }
                /// <summary>
                /// Gets the Values that Properties contains.
                /// </summary>
                /// <returns>Returns an IEnumerable that can be converted to any Enumerable type object.</returns>
                public virtual IEnumerable<dynamic> GetDictionaryValues()
                {
                    return Properties.Values;
                }
            }
            public class PluginHelper
            {
                /// <summary>
                /// The object data containing the object Plugin and the interface IPluginMetadata.
                /// </summary>
                [ImportMany(typeof(PluginClass), AllowRecomposition = true)]
                public IEnumerable<Lazy<KeyValuePair<PluginClass, List<MethodInfo>>, IPluginMetadata>> data { get; set; }
                //IEnumerable<Lazy<IEnumerable<Plugin
                /// <summary>
                /// The Plugin object.
                /// </summary>
                public PluginClass Plugin { get; private set; }
                /// <summary>
                /// The Plugin object metadata.
                /// </summary>
                public IPluginMetadata PluginMetadata { get; private set; }
                /// <summary>
                /// Plugins folder.
                /// </summary>
                public string Folder { get; set; }
                /// <summary>
                /// Extensions that will be loaded.
                /// </summary>
                public string Extension { get; set; }
                /// <summary>
                /// Sets if the class shall output values in the console. Should be false if it's not a Console project. Default is false.
                /// </summary>
                public bool OutputConsole { get; set; } = false;
                /// <summary>
                /// Default constructor.
                /// </summary>
                public PluginHelper()
                {
                    OutputConsole = false;
                }
                /// <summary>
                /// PluginHelper constructor. This will initialize and load the plugins.
                /// </summary>
                /// <param name="folder">Plugins folder.</param>
                /// <param name="extension">Extensions that will be loaded.</param>
                /// <param name="outputConsole">(Optional) Sets if the class shall output values in the console. Should be false if it's not a Console project. Default is false.</param>
                public PluginHelper(string folder, string extension, bool outputConsole = false)
                {
                    Folder = folder;
                    Extension = extension;
                    OutputConsole = outputConsole;
                    this.Initialize();
                }
                /// <summary>
                /// Initializes.
                /// </summary>
                void Initialize()
                {
                    LoadPlugins();
                }
                /// <summary>
                /// Loads the plugins.
                /// </summary>
                public List<IDictionary<string, object>> LoadPlugins()
                {
                    try
                    {
                        List<IDictionary<string, object>> list = new List<IDictionary<string, object>>();
                        DirectoryCatalog catalog = new DirectoryCatalog(Folder, Extension);
                        if (catalog.LoadedFiles.Count != 0)
                        {
                            if (OutputConsole)
                            {
                                Console.WriteLine("*** {0} PLUGINS FOUND! ***", catalog.LoadedFiles.Count);
                            }
                            foreach (var plugin in catalog)
                            {
                                var exportDefinitions = plugin.ExportDefinitions.ToArray();
                                IDictionary<string, object> metadata = exportDefinitions[0].Metadata;
                                list.Add(metadata);
                                if (OutputConsole)
                                {
                                    Console.WriteLine("\nName: {0}\nVersion: {1}\nDescription: {2}", metadata["Name"], metadata["Version"], metadata["Description"]);
                                }
                            }
                            Compose(catalog);
                            return list;
                        }
                        else
                        {
                            if (OutputConsole)
                            {
                                Console.WriteLine("*** NO PLUGINS FOUND! ***");
                            }
                            return list;
                        }
                    }
                    catch (ArgumentNullException ex)
                    {
                        throw;
                    }
                }
                /// <summary>
                /// Composes the CompositionContainer from the specified catalog.
                /// </summary>
                /// <param name="catalog">The catalog that will be composed</param>
                private void Compose(DirectoryCatalog catalog)
                {
                    AggregateCatalog agg = new AggregateCatalog();
                    agg.Catalogs.Add(catalog);
                    CompositionContainer container = new CompositionContainer(agg);
                    container.ComposeParts(this);
                }
                /// <summary>
                /// Selects which plugin will be used, by specifying the IPluginMetadata property Name that is set in the plugin.
                /// </summary>
                /// <param name="name">The plugin name.</param>
                public void SelectPlugin(string name)
                {
                    Plugin = GetPluginInfo(name).Key;
                    PluginMetadata = GetPluginMetadata(name);
                }
                /// <summary>
                /// Finds the plugin that matches the IPluginMetaData property Name, and sets the Plugin object.
                /// </summary>
                /// <param name="name">The Plugin name.</param>
                /// <returns>Returns the Plugin object.</returns>
                private KeyValuePair<PluginClass, List<MethodInfo>> GetPluginInfo(string name)
                {
                    return data.Where(l => l.Metadata.Name.Equals(name)).Select(l => l.Value).FirstOrDefault();
                }
                private KeyValuePair<PluginClass, List<MethodInfo>> GetPlugin(string name)
                {
                    return data.Where(l => l.Metadata.Name.Equals(name)).Select(l => l.Value).FirstOrDefault();
                }
                /// <summary>
                /// Finds the plugin metadata that matches the IPluginMetadata property Name, and sets the IPluginMetadata object.
                /// </summary>
                /// <param name="name">The Plugin name.</param>
                /// <returns>Returns the IPluginMetadata object</returns>
                private IPluginMetadata GetPluginMetadata(string name)
                {
                    return data.Where(l => l.Metadata.Name.Equals(name)).Select(l => l.Metadata).FirstOrDefault();
                }
            }
        }
        namespace Data
        {
            namespace Packages
            {
                public class PackageFactory
                {

                }
                [Serializable]
                public class Package
                {
                    public BaseInfo To { get; private set; }
                    public BaseInfo From { get; private set; }
                    public object Content { get; private set; }
                    public Type ContentType { get; private set; }
                    public Package(object obj, BaseInfo to, BaseInfo from)
                    {
                        To = to;
                        From = from;
                        Content = obj;
                        ContentType = obj.GetType();
                    }
                    public Package(object obj, BaseInfo from)
                    {
                        From = from;
                        Content = obj;
                        ContentType = obj.GetType();
                    }
                }
                [Serializable]
                public class TransportPackage
                {
                    public byte[] Data = new byte[65536];
                    public TransportPackage()
                    {

                    }
                    public TransportPackage(byte[] data)
                    {
                        Data = data;
                    }
                }
            }
            namespace Serialization
            {
                public class Binder : SerializationBinder
                {
                    public override Type BindToType(string i_AssemblyName, string i_TypeName)
                    {
                        Type typeToDeserialize = Type.GetType(i_TypeName);
                        return typeToDeserialize;
                    }
                }
                public class Serializer
                {
                    public static TransportPackage Serialize(object objToSerialize)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            (new BinaryFormatter()).Serialize(memoryStream, objToSerialize);
                            return new TransportPackage(memoryStream.ToArray());
                        }
                    }
                    public static object Deserialize(TransportPackage package)
                    {
                        using (var memoryStream = new MemoryStream(package.Data))
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            bf.Binder = new Binder();
                            return (bf).Deserialize(memoryStream);
                        }
                    }
                }
            }
            namespace Structures
            {
                [Serializable]
                public static class DataStructure
                {
                    public static List<BaseInfo> MasterList = new List<BaseInfo>();
                    public static List<BaseInfo> ClientList = new List<BaseInfo>();
                    public static Dictionary<IPluginMetadata, Plugin.PluginClass> PluginDictionary = new Dictionary<IPluginMetadata, Plugin.PluginClass>();
                }
            }
        }
        namespace Info
        {
            public enum BaseInfoType
            {
                Client,
                Master,
                Server
            }
            public interface IInfo : IEnumerable<object>
            {
                string Name { get; set; }
                string IP { get; set; }
                string MACAddress { get; set; }
                string UID { get; set; }
                BaseInfoType Type { get; set; }
                Connector Connector { get; set; }
            }
            [Serializable]
            public class BaseInfo : IInfo
            {
                public string IP { get; set; }
                public string MACAddress { get; set; }
                public string Name { get; set; }
                public string UID { get; set; }
                public BaseInfoType Type { get; set; }
                public Connector Connector { get; set; }
                public BaseInfo()
                {

                }
                public BaseInfo(BaseInfoType type)
                {
                    Type = type;
                }
                public BaseInfo(string uid)
                {
                    UID = uid;
                    Type = BaseInfoType.Client;
                }
                private IEnumerable<object> Info()
                {
                    yield return IP;
                    yield return MACAddress;
                    yield return Name;
                    yield return UID;
                }
                public IEnumerator<object> GetEnumerator()
                {
                    return Info().GetEnumerator();
                }
                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }
        }
    }
}
