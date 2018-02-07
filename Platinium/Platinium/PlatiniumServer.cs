using Platinium.Connection;
using Platinium.Shared.Content;
using Platinium.Shared.Core;
using Platinium.Shared.Data.Network;
using Platinium.Shared.Data.Packages;
using Platinium.Shared.Data.Serialization;
using Platinium.Shared.Data.Structures;
using Platinium.Shared.Info;
using Platinium.Shared.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Platinium
{
    namespace Connection
    {
        [Serializable]
        public class Connector
        {
            private static  Logger _logger = new Logger();
            public NetworkStream ClientStream { get; private set; }
            public void StartConnection(NetworkStream inNetworkStream, Logger logger)
            {
                ClientStream = inNetworkStream;
                
                _logger = logger;
                Thread handleThread = new Thread(handleConnection);
                handleThread.Start();
            }
            private void handleConnection()
            {
                while (true)
                {
                    try
                    {
                        Package package = NetworkManagement.ReadData(ClientStream, _logger);
                        if (package.From != null)
                        {
                            Console.WriteLine(_logger.LogMessageToFile($"*************** GET PACKAGE ***************\n* Type - {package.PackageType.ToString().NULLIfNull()}\n* Value - {package.Content.NULLIfNull()}\n* From Type - {package.From.Type.NULLIfNull()}\n* From - {package.From.UID.NULLIfNull()}\n* To Type - {package.To.Type.NULLIfNull()}\n* To - {package.To.UID.NULLIfNull()}\n*************** END GET ***************"));
                        }
                        else
                        {
                            Console.WriteLine(_logger.LogMessageToFile($"*************** GET PACKAGE ***************\n* Type - {package.PackageType}"));
                        }

                        Communicate(package);
                    }
                    catch (Exception) { break; }
                }
            }
            public static void Communicate(Package package)
            {
                switch (package.To.Type)
                {
                    case BaseInfoType.Client:
                        foreach (var item in DataStructure.ClientList)
                        {
                            if (package.To.UID == item.UID)
                            {
                                if (package.PackageType != PackageType.Response)
                                {
                                    NetworkManagement.WriteData(item.Connector.ClientStream, PackageFactory.HandleServerPackages(package), _logger);
                                }
                            }
                        }
                        break;
                    case BaseInfoType.Master:
                        foreach (var item in DataStructure.MasterList)
                        {
                            if (package.To.UID == item.UID)
                            {
                                if (package.PackageType != PackageType.Response)
                                {
                                    NetworkManagement.WriteData(item.Connector.ClientStream, PackageFactory.HandleServerPackages(package), _logger);
                                }
                            }
                        }
                        break;
                    case BaseInfoType.Server:
                        if (package.From != null)
                        {
                            if (package.From.Type == BaseInfoType.Master)
                            {
                                foreach (var item in DataStructure.MasterList)
                                {
                                    if (package.From.UID == item.UID)
                                    {
                                        if (package.PackageType != PackageType.Response)
                                        {
                                            NetworkManagement.WriteData(item.Connector.ClientStream, PackageFactory.HandleServerPackages(package), _logger);
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
                                        if (package.PackageType != PackageType.Response)
                                        {
                                            NetworkManagement.WriteData(item.Connector.ClientStream, PackageFactory.HandleServerPackages(package), _logger);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
    namespace Entities
    {
        public class PlatiniumServer
        {
            public string FILE_LOG_PATH;
            public bool Received = false;
            private static Logger logger = new Logger();
            public IPEndPoint IPEndPoint { get; private set; }
            public IPEndPoint HearthBeatIPEndPoint { get; private set; }
            private ClientInfo ServerInfo = BuildServerInfo();
            public PlatiniumServer()
            {
                Console.Title = "Platinium Beta Server";
                
            }
            public void Initialize()
            {
                try
                {
                    logger.Path = FILE_LOG_PATH;
                    LoadPlugins();
                    DataStructure.Info = ServerInfo;
                    IPEndPoint = new IPEndPoint(IPAddress.Any, 55555);
                    HearthBeatIPEndPoint = new IPEndPoint(IPAddress.Any, 55554);
                    Thread hearthBeatTask = new Thread(HearthBeat);
                    Thread clientConnection = new Thread(CheckClientConnection);
                    Thread masterConnection = new Thread(CheckMasterConnection);
                    clientConnection.Start();
                    masterConnection.Start();
                    Listen();
                }
                catch (Exception ex) { logger.LogMessageToFile($"FATAL EXCEPTION: {ex.Message}"); }
            }
            private void Listen()
            {
                TcpListener ServerSocket = new TcpListener(IPEndPoint);
                ServerSocket.Start();
                while (true)
                {
                    TcpClient Socket = ServerSocket.AcceptTcpClient();
                    Package package = NetworkManagement.ReadData(Socket.GetStream());
                    ClientInfo Info = (ClientInfo)package.Content;
                    Info.Connector = new Connector();
                    if (Info.Type == BaseInfoType.Client)
                    {
                        Console.WriteLine(logger.LogMessageToFile($"*************** CLIENT CONNECTED ***************"));
                        if (!DataStructure.ClientList.Any(x => x.UID == Info.UID))
                        {
                            Console.WriteLine(logger.LogMessageToFile($"*************** AUTHENTICATED ***************"));
                            DataStructure.ClientList.Add(Info);
                        }
                        else
                        {
                            Console.WriteLine(logger.LogMessageToFile($"************ BAD AUTHENTICATION ************"));
                        }
                    }
                    else if (Info.Type == BaseInfoType.Master)
                    {
                        Console.WriteLine(logger.LogMessageToFile($"*************** MASTER CONNECTED ***************"));
                        if (!DataStructure.MasterList.Any(x => x.UID == Info.UID))
                        {
                            Console.WriteLine(logger.LogMessageToFile($"*************** AUTHENTICATED ***************"));
                            DataStructure.MasterList.Add(Info);
                        }
                        else
                        {
                            Console.WriteLine(logger.LogMessageToFile($"************ BAD AUTHENTICATION ************"));
                        }
                    }
                    if (DataStructure.ClientList.Any(x => x.UID == Info.UID && x.IsConnected == false) || DataStructure.MasterList.Any(x => x.UID == Info.UID && x.IsConnected == false))
                    {
                        Dictionary<string, object> ddata = Converter.ObjectToDictionary(Info);
                        foreach (var item in ddata)
                        {
                            Console.WriteLine(logger.LogMessageToFile($"* {item.Key} - {item.Value}"));
                        }
                        Info.Connector.StartConnection(Socket.GetStream(), logger);
                    }
                    Console.WriteLine(logger.LogMessageToFile($"*******************************************"));
                }
            }
            private void LoadPlugins()
            {
                Console.WriteLine(logger.LogMessageToFile($"*************** LOADING PLUGINS ***************"));
                string[] dllFiles = Directory.GetFiles("Plugins", "*.dll", SearchOption.AllDirectories);
                foreach (string file in dllFiles)
                {
                    byte[] buffer = null;
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, (int)fs.Length);
                    }
                    DataStructure.AssemblyRaw.Add(buffer);
                }
                Console.WriteLine(logger.LogMessageToFile($"* PLUGINS FOUND: {dllFiles.Length}"));
                foreach (var assemblyData in DataStructure.AssemblyRaw)
                {
                    Assembly assembly = Assembly.Load(assemblyData);
                    DataStructure.LoadedAssemblyList.Add(assembly);
                }
                Type pluginType = typeof(IPluginImplementation);
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
                    IPluginImplementation plugin = (IPluginImplementation)Activator.CreateInstance(type, this);
                    var pluginMetadata = (Metadata[])type.GetCustomAttributes(typeof(Metadata), true);
                    DataStructure.PluginDictionary.Add(pluginMetadata[0], plugin);
                    Console.WriteLine(logger.LogMessageToFile($"* PLUGIN {pluginMetadata[0].Name} LOADED"));
                }
                Console.WriteLine(logger.LogMessageToFile($"*************** FINNISHED LOADING PLUGINS ***************"));
            }
            private void HearthBeat()
            {
                TcpListener HearthBeatServerSocket = new TcpListener(HearthBeatIPEndPoint);
                HearthBeatServerSocket.Start();
                logger.LogMessageToFile($"Started HearthBeat Server.");
                while (true)
                {
                    TcpClient hearthBeatClient = HearthBeatServerSocket.AcceptTcpClient();
                    hearthBeatClient.Close();
                    logger.LogMessageToFile($"Accepted TcpClient ({((IPEndPoint)hearthBeatClient.Client.RemoteEndPoint).Address.ToString()}) at HearthBeat Server.");
                }
            }
            private void CheckClientConnection()
            {
                while (true)
                {
                    Thread.Sleep(20);
                    try
                    {
                        foreach (var client in DataStructure.ClientList)
                        {
                            NetworkStream testTcp = client.Connector.ClientStream;
                            if (!testTcp.CanWrite)
                            {
                                if (client.IsConnected)
                                {
                                    client.IsConnected = false;
                                    logger.LogMessageToFile($"Changed Client {client.UserName} status to disconnected.");
                                }
                            }
                            else
                            {
                                if (!client.IsConnected)
                                {
                                    client.IsConnected = true;
                                    logger.LogMessageToFile($"Changed Master {client.UserName} status to connected.");
                                }
                            }
                        }
                    }
                    catch (Exception ex) { logger.LogMessageToFile($"EXCEPTION: {ex.Message}"); }
                }
            }
            private void CheckMasterConnection()
            {
                while (true)
                {
                    Thread.Sleep(20);
                    try
                    {
                        foreach (var master in DataStructure.MasterList)
                        {
                            NetworkStream testTcp = master.Connector.ClientStream;
                            if (!testTcp.CanWrite)
                            {
                                if (master.IsConnected)
                                {
                                    master.IsConnected = false;
                                    logger.LogMessageToFile($"Changed Master {master.UserName} status to disconnected.");
                                }
                            }
                            else
                            {
                                if (!master.IsConnected)
                                {
                                    master.IsConnected = true;
                                    logger.LogMessageToFile($"Changed Master {master.UserName} status to connected.");
                                }
                            }
                        }
                    }
                    catch (Exception ex) { logger.LogMessageToFile($"EXCEPTION: {ex.Message}"); }
                }
            }
            private static ClientInfo BuildServerInfo()
            {
                ClientInfo ci = new ClientInfo
                {
                    UserName = "SERVER",
                    Type = BaseInfoType.Server
                };
                return ci;
            }
        }
    }
}
