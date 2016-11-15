using Platinium.Shared.Data.Packages;
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
            public enum PackageType
            {
                Base,
                Plugin,
                PluginCommand,
                Status,
                NoResponse,
                ClientResponse,
                ServerResponse
            }
            public interface IContent
            {
                object Data { get; set; }
            }
            [Serializable]
            public class Command : IContent
            {
                public object Data { get; set; }
                public Command()
                {

                }
                public Command(object data)
                {
                    Data = data;
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
                void Action();
                void InstantiateClient();
                void InstantiateServer();
                void InstantiateMaster();
                IPluginClientSide ClientSide { get; set; }
                IPluginServerSide ServerSide { get; set; }
                IPluginMasterSide MasterSide { get; set; }
            }
            public interface IPluginClientSide
            {
                void Action();
            }
            public interface IPluginServerSide
            {
                void Action();
            }
            public interface IPluginMasterSide
            {
                void Action();
            }
            public class Metadata : Attribute
            {
                public string Name { get; set; }
            }
            public class PluginFactory
            { }
        }
        namespace Data
        {
            namespace Packages
            {
                public class PackageFactory
                {
                    public static Package HandleServerPackages(Package inPackage)
                    {
                        Package returnPackage = inPackage;
                        switch (inPackage.PackageType)
                        {
                            case PackageType.Base:
                                returnPackage = inPackage;
                                break;
                            case PackageType.Plugin:
                                switch (inPackage.Content.ToString())
                                {
                                    case "LOAD_PLUGINS":
                                        returnPackage = new Package(DataStructure.AssemblyList, PackageType.Plugin, new BaseInfo(BaseInfoType.Server));
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                returnPackage = inPackage;
                                break;
                        }
                        return returnPackage;
                    }
                    public static Package HandleClientPackages(Package inPackage)
                    {
                        Package returnPackage = inPackage;
                        switch (inPackage.PackageType)
                        {
                            case PackageType.Base:
                                returnPackage = inPackage;
                                break;
                            case PackageType.Plugin:
                                DataStructure.AssemblyList = (List<byte[]>)inPackage.Content;
                                Console.WriteLine("LOADED ASSEMBLIES");
                                foreach (var assemblyData in DataStructure.AssemblyList)
                                {
                                    Assembly assembly = Assembly.Load(assemblyData);
                                    DataStructure.LoadedAssemblyList.Add(assembly);
                                }
                                Type pluginType = typeof(IPlugin);
                                ICollection<Type> pluginTypes = new List<Type>();
                                foreach (Assembly assembly in DataStructure.LoadedAssemblyList)
                                {
                                    Type[] types = assembly.GetTypes();
                                    foreach (Type type in types)
                                    {
                                        if (!type.IsInterface || !type.IsAbstract)
                                        {
                                            if ((type.GetInterface(pluginType.FullName) != null) && (!type.IsInterface || !type.IsAbstract))
                                            {
                                                pluginTypes.Add(type);
                                            }
                                        }
                                    }
                                }
                                foreach (Type type in pluginTypes)
                                {
                                    IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                                    var pluginMetadata = (Metadata[])type.GetCustomAttributes(typeof(Metadata), true);
                                    DataStructure.PluginDictionary.Add(pluginMetadata[0], plugin);
                                    plugin.InstantiateClient();
                                }
                                break;
                            case PackageType.PluginCommand:
                                break;
                            case PackageType.Status:
                                break;
                            case PackageType.NoResponse:
                                break;
                            case PackageType.ClientResponse:
                                break;
                            case PackageType.ServerResponse:
                                break;
                            default:
                                returnPackage = inPackage;
                                break;
                        }
                        return returnPackage;
                    }
                    public static Package HandleMasterPackages(Package inPackage)
                    {
                        Package returnPackage = inPackage;
                        switch (inPackage.PackageType)
                        {
                            case PackageType.Base:
                                break;
                            case PackageType.Plugin:
                                DataStructure.AssemblyList = (List<byte[]>)inPackage.Content;
                                Console.WriteLine("LOADED ASSEMBLIES");
                                foreach (var assemblyData in DataStructure.AssemblyList)
                                {
                                    Assembly assembly = Assembly.Load(assemblyData);
                                    DataStructure.LoadedAssemblyList.Add(assembly);
                                }
                                Type pluginType = typeof(IPlugin);
                                ICollection<Type> pluginTypes = new List<Type>();
                                foreach (Assembly assembly in DataStructure.LoadedAssemblyList)
                                {
                                    Type[] types = assembly.GetTypes();
                                    foreach (Type type in types)
                                    {
                                        if (!type.IsInterface || !type.IsAbstract)
                                        {
                                            if ((type.GetInterface(pluginType.FullName) != null) && (!type.IsInterface || !type.IsAbstract))
                                            {
                                                pluginTypes.Add(type);
                                            }
                                        }
                                    }
                                }
                                foreach (Type type in pluginTypes)
                                {
                                    IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                                    var pluginMetadata = (Metadata[])type.GetCustomAttributes(typeof(Metadata), true);
                                    DataStructure.PluginDictionary.Add(pluginMetadata[0], plugin);
                                    plugin.InstantiateMaster();
                                }
                                break;
                            case PackageType.PluginCommand:
                                break;
                            case PackageType.Status:
                                break;
                            case PackageType.NoResponse:
                                break;
                            case PackageType.ClientResponse:
                                break;
                            case PackageType.ServerResponse:
                                break;
                            default:
                                break;
                        }
                        return inPackage;
                    }
                }
                [Serializable]
                public class Package
                {
                    public BaseInfo To { get; private set; }
                    public BaseInfo From { get; private set; }
                    public object Content { get; private set; }
                    public PackageType PackageType { get; private set; }
                    public Package(object obj, PackageType packagetype, BaseInfo to, BaseInfo from)
                    {
                        To = to;
                        PackageType = packagetype;
                        From = from;
                        Content = obj;
                    }
                    public Package(object obj, PackageType packagetype, BaseInfo from)
                    {
                        From = from;
                        Content = obj;
                        PackageType = packagetype;
                    }
                }
                [Serializable]
                public class TransportPackage
                {
                    public byte[] Data = new byte[1048576];
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
                sealed class AllowAllAssemblyVersionsDeserializationBinder : SerializationBinder
                {
                    public override Type BindToType(string assemblyName, string typeName)
                    {
                        Type typeToDeserialize = null;

                        String currentAssembly = Assembly.GetExecutingAssembly().FullName;

                        // In this case we are always using the current assembly
                        assemblyName = currentAssembly;

                        // Get the type using the typeName and assemblyName
                        typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
                            typeName, assemblyName));

                        return typeToDeserialize;
                    }
                }
                public class Serializer
                {
                    public static TransportPackage Serialize(object objToSerialize)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            bf.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
                            (bf).Serialize(memoryStream, objToSerialize);
                            return new TransportPackage(memoryStream.ToArray());
                        }
                    }
                    public static object Deserialize(TransportPackage package)
                    {
                        using (var memoryStream = new MemoryStream(package.Data))
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            bf.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            return (bf).Deserialize(memoryStream);
                        }
                    }
                    public static byte[] SerializeToByte(object objToSerialize)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            (new BinaryFormatter()).Serialize(memoryStream, objToSerialize);
                            return memoryStream.ToArray();
                        }
                    }
                    public static object DeserializeFromByte(byte[] data)
                    {
                        using (var memoryStream = new MemoryStream(data))
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            bf.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
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
                    public static Dictionary<Metadata, IPlugin> PluginDictionary = new Dictionary<Metadata, IPlugin>();
                    public static List<byte[]> AssemblyList = new List<byte[]>();
                    public static List<Assembly> LoadedAssemblyList = new List<Assembly>();
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
                public BaseInfo(string uid, BaseInfoType type)
                {
                    UID = uid;
                    Type = type;
                }
                public BaseInfo(string uid)
                {
                    UID = uid;
                    Type = BaseInfoType.Client;
                }
                public BaseInfo(BaseInfoType type)
                {
                    Type = type;
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
