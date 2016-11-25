using Platinium.Contracts.Connection;
using Platinium.Contracts.Content;
using Platinium.Contracts.Core;
using Platinium.Contracts.Info;
using Platinium.Contracts.Packages;
using Platinium.Contracts.Plugin;
using Platinium.Contracts.Serialization;
using Platinium.Contracts.Structures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Platinium.Contracts
{
    namespace Core
    {
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
    namespace Plugin
    {
        public partial class PluginFactory
        {
            public static Package HandlePluginMethods(Package inPackage, BaseInfoType baseInfoType)
            {
                Package returnPackage = inPackage;
                string[] commands = inPackage.Command.Split('|');
                string plugin_name = commands[0];
                string plugin_command = commands[1];
                object[] command_parameters = { inPackage };
                Type type = null;
                if (baseInfoType == BaseInfoType.Client)
                {
                    type = DataStructure.PluginDictionary.Where(l => l.Key.Name.Equals(plugin_name)).Select(l => l.Value).FirstOrDefault().ClientController.GetType();
                }
                else if (baseInfoType == BaseInfoType.Master)
                {
                    type = DataStructure.PluginDictionary.Where(l => l.Key.Name.Equals(plugin_name)).Select(l => l.Value).FirstOrDefault().MasterController.GetType();
                }

                if (type != null)
                {
                    MethodInfo methodInfo = type.GetMethod(plugin_command);
                    if (methodInfo != null)
                    {
                        ParameterInfo[] parameters = methodInfo.GetParameters();
                        IPlugin pluginInstance = DataStructure.PluginDictionary.Where(l => l.Key.Name.Equals(plugin_name)).Select(l => l.Value).FirstOrDefault();

                        if (parameters.Length == 0)
                        {
                            if (baseInfoType == BaseInfoType.Client)
                            {
                                returnPackage = (Package)methodInfo.Invoke(pluginInstance.ClientController, null);
                            }
                            else if (baseInfoType == BaseInfoType.Master)
                            {
                                returnPackage = (Package)methodInfo.Invoke(pluginInstance.MasterController, null);
                            }
                        }
                        else
                        {
                            if (baseInfoType == BaseInfoType.Client)
                            {
                                returnPackage = (Package)methodInfo.Invoke(pluginInstance.ClientController, command_parameters.ToArray());
                            }
                            else if (baseInfoType == BaseInfoType.Master)
                            {
                                returnPackage = (Package)methodInfo.Invoke(pluginInstance.MasterController, command_parameters.ToArray());
                            }
                        }
                    }
                }
                return returnPackage;
            }
        }
        /// <summary>
        /// The IPlugin Interface.
        /// </summary>
        public interface IPlugin
        {
            void Action();
            void InstantiateClient();
            void InstantiateMaster();
            IPluginClientController ClientController { get; set; }
            IPluginMasterController MasterController { get; set; }
            UserControlModule PluginInterfaceControl { get; set; }
        }
        public interface IPluginClientController
        {
            Package Action(Package inPackage);
        }
        public interface IPluginMasterController
        {
            Package Action(Package inPackage);
        }
        public class Metadata : Attribute
        {
            public string Name { get; set; }
            public string Version { get; set; }
            public string Description { get; set; }
        }
    }
    namespace Content
    {
        public enum PackageType
        {
            Base,
            Data,
            PluginCommand,
            Status,
            NoResponse,
            Response
        }
    }
    namespace Packages
    {
        public class PackageFactory
        {
            public static Package HandleServerPackages(Package inPackage)
            {
                Package returnPackage = new Package(null, null, PackageType.Response, inPackage.To, inPackage.From);
                switch (inPackage.PackageType)
                {
                    case PackageType.Base:
                        returnPackage = inPackage;
                        break;
                    case PackageType.Data:
                        switch (inPackage.Command)
                        {
                            case "LOAD_PLUGINS":
                                returnPackage = new Package("LOAD_PLUGINS", DataStructure.AssemblyRaw, PackageType.Data, inPackage.To, inPackage.From);
                                break;
                            case "CLIENT_LIST":
                                returnPackage = new Package("CLIENT_LIST", DataStructure.ClientList, PackageType.Data, inPackage.To, inPackage.From);
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
                Package returnPackage = new Package(null, null, PackageType.Response, inPackage.To, new ClientInfo(BaseInfoType.Server));
                switch (inPackage.PackageType)
                {
                    case PackageType.Base:
                        returnPackage = inPackage;
                        break;
                    case PackageType.Data:
                        switch (inPackage.Command)
                        {
                            case "LOAD_PLUGINS":
                                DataStructure.AssemblyRaw = (List<byte[]>)inPackage.Content;
                                Console.WriteLine("LOADED ASSEMBLIES");
                                foreach (var assemblyData in DataStructure.AssemblyRaw)
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
                                    DataStructure.PluginDictionary.Clear();
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
                Package returnPackage = inPackage;
                switch (inPackage.PackageType)
                {
                    case PackageType.Base:
                        returnPackage = inPackage;
                        break;
                    case PackageType.Data:
                        switch (inPackage.Command)
                        {
                            case "LOAD_PLUGINS":
                                DataStructure.AssemblyRaw = (List<byte[]>)inPackage.Content;
                                Console.WriteLine("LOADED ASSEMBLIES");
                                foreach (var assemblyData in DataStructure.AssemblyRaw)
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
                                    DataStructure.PluginDictionary.Clear();
                                    DataStructure.PluginMethodDictionary.Clear();
                                    DataStructure.PluginDictionary.Add(pluginMetadata[0], plugin);
                                    DataStructure.PluginMethodDictionary.Add(pluginMetadata[0], methodInfo);
                                    plugin.InstantiateMaster();
                                }
                                break;
                            case "CLIENT_LIST":
                                DataStructure.ClientList = (List<ClientInfo>)inPackage.Content;
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
            public Package(string command, object obj, PackageType packagetype, ClientInfo from, ClientInfo to)
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
    namespace Info
    {
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
            public string ComputerName { get; set; }
            public string UID { get; set; }
            public string Language { get; set; }
            public bool IsAdministrator { get; set; }
            public string OSName { get; set; }
            public double AppVersion { get; private set; }
            public string AppNetVersion { get; set; }
            public bool IsConnected { get; set; } = false;

            public BaseInfoType Type { get; set; }
            [NonSerialized]
            private Connector _connector;
            public Connector Connector { get { return _connector; } set { _connector = value; } }
            public ClientInfo()
            {
                AppVersion = 1.0;
            }
            public ClientInfo(string uid, BaseInfoType type)
            {
                AppVersion = 1.0;
                UID = uid;
                Type = type;
            }
            public ClientInfo(string uid)
            {
                AppVersion = 1.0;
                UID = uid;
                Type = BaseInfoType.Client;
            }
            public ClientInfo(BaseInfoType type)
            {
                AppVersion = 1.0;
                Type = type;
            }
            private IEnumerable<object> Info()
            {
                yield return IP;
                yield return MACAddress;
                yield return UserName;
                yield return ComputerName;
                yield return OSName;
                yield return AppVersion;
                yield return AppNetVersion;
                yield return IsAdministrator;
                yield return UID;
                yield return Language;
                yield return Type;
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
    namespace Connection
    {
        [Serializable]
        public class Connector
        {
            public TcpClient ClientSocket { get; private set; }
            private static NetworkStream networkStream = default(NetworkStream);
            public void StartConnection(TcpClient inClientSocket)
            {
                ClientSocket = inClientSocket;
                Thread handleThread = new Thread(handleConnection);
                handleThread.Name = "HANDLETHREAD";
                handleThread.Start();
            }
            private void handleConnection()
            {
                while (true)
                {
                    try
                    {
                        networkStream = null;
                        networkStream = ClientSocket.GetStream();
                        TransportPackage TransportPackage = new TransportPackage();
                        networkStream.Read(TransportPackage.Data, 0, TransportPackage.Data.Length);
                        networkStream.Flush();
                        Package package = (Package)Serializer.Deserialize(TransportPackage);
                        if (package.From != null)
                        {
                            Console.WriteLine("*************** GET PACKAGE ***************\n* Type - {0}\n* Value - {1}\n* From Type - {2}\n* From - {3}\n* To Type - {4}\n* To - {5}\n*************** END GET ***************", package.PackageType.ToString().EmptyIfNull(), package.Content.EmptyIfNull(), package.From.Type.EmptyIfNull(), package.From.UID.EmptyIfNull(), package.To.Type.EmptyIfNull(), package.To.UID.EmptyIfNull());
                        }
                        else
                        {
                            Console.WriteLine("*************** GET PACKAGE ***************\n* Type - {0}", package.PackageType);
                        }
                        Communicate(package);
                    }
                    catch (Exception) { break; }
                }
            }
            public static void Communicate(Package package)
            {
                if (package.To.Type == BaseInfoType.Client)
                {
                    foreach (var item in DataStructure.ClientList)
                    {
                        if (package.To.UID == item.UID)
                        {
                            TcpClient communicationSocket = item.Connector.ClientSocket;
                            NetworkStream communicationStream = communicationSocket.GetStream();
                            Package outPackage = PackageFactory.HandleServerPackages(package);
                            if (package.PackageType != PackageType.Response)
                            {
                                TransportPackage TransportPackage = Serializer.Serialize(outPackage);
                                communicationStream.Write(TransportPackage.Data, 0, TransportPackage.Data.Length);
                                communicationStream.Flush();
                            }
                        }
                    }
                }
                else if (package.To.Type == BaseInfoType.Master)
                {
                    foreach (var item in DataStructure.MasterList)
                    {
                        if (package.To.UID == item.UID)
                        {
                            TcpClient communicationSocket = item.Connector.ClientSocket;
                            NetworkStream communicationStream = communicationSocket.GetStream();
                            Package outPackage = PackageFactory.HandleServerPackages(package);
                            if (package.PackageType != PackageType.Response)
                            {
                                TransportPackage TransportPackage = Serializer.Serialize(outPackage);
                                communicationStream.Write(TransportPackage.Data, 0, TransportPackage.Data.Length);
                                communicationStream.Flush();
                            }
                        }
                    }
                }
                else if (package.To.Type == BaseInfoType.Server)
                {
                    if (package.From != null)
                    {
                        if (package.From.Type == BaseInfoType.Master)
                        {
                            foreach (var item in DataStructure.MasterList)
                            {
                                if (package.From.UID == item.UID)
                                {
                                    TcpClient communicationSocket = item.Connector.ClientSocket;
                                    NetworkStream communicationStream = communicationSocket.GetStream();
                                    Package outPackage = PackageFactory.HandleServerPackages(package);
                                    if (package.PackageType != PackageType.Response)
                                    {
                                        TransportPackage TransportPackage = Serializer.Serialize(outPackage);
                                        communicationStream.Write(TransportPackage.Data, 0, TransportPackage.Data.Length);
                                        communicationStream.Flush();
                                    }
                                }
                            }
                        }
                        else if (package.From.Type == BaseInfoType.Client)
                        {
                            foreach (var item in DataStructure.ClientList)
                            {
                                if (package.From.UID == item.UID)
                                {
                                    TcpClient communicationSocket = item.Connector.ClientSocket;
                                    NetworkStream communicationStream = communicationSocket.GetStream();
                                    Package outPackage = PackageFactory.HandleServerPackages(package);
                                    if (package.PackageType != PackageType.Response)
                                    {
                                        TransportPackage TransportPackage = Serializer.Serialize(outPackage);
                                        communicationStream.Write(TransportPackage.Data, 0, TransportPackage.Data.Length);
                                        communicationStream.Flush();
                                    }
                                }
                            }
                        }
                    }
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
            public static List<byte[]> AssemblyRaw = new List<byte[]>();
            public static List<Assembly> LoadedAssemblyList = new List<Assembly>();
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
}