using Platinium.Connection;
using Platinium.Shared.Content;
using Platinium.Shared.Core;
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
    namespace Entities
    {
        public class PlatiniumServer
        {
            public IPEndPoint IPEndPoint { get; private set; }
            public IPEndPoint HearthBeatIPEndPoint { get; private set; }
            public PlatiniumServer()
            {
                Console.Title = "Platinium Beta Server";
                LoadPlugins();
                IPEndPoint = new IPEndPoint(IPAddress.Any, 55555);
                HearthBeatIPEndPoint = new IPEndPoint(IPAddress.Any, 55554);
                Thread hearthBeatTask = new Thread(HearthBeat);
                Thread clientConnection = new Thread(CheckClientConnection);
                Thread masterConnection = new Thread(CheckMasterConnection);
                clientConnection.Start();
                masterConnection.Start();
                Listen();
            }
            private void Listen()
            {
                TcpListener ServerSocket = new TcpListener(IPEndPoint);
                ServerSocket.Start();
                while (true)
                {
                    TcpClient Socket = ServerSocket.AcceptTcpClient();
                    TransportPackage TransportPackage = new TransportPackage();
                    NetworkStream networkStream = Socket.GetStream();
                    networkStream.Read(TransportPackage.Data, 0, TransportPackage.Data.Length);
                    Package package = (Package)Serializer.Deserialize(TransportPackage);
                    ClientInfo Info = (ClientInfo)package.Content;
                    Info.Connector = new Connector();
                    if (Info.Type == BaseInfoType.Client)
                    {
                        Console.WriteLine("*************** CLIENT CONNECTED ***************");
                        if (!DataStructure.ClientList.Any(x => x.UID == Info.UID))
                        {
                            Console.WriteLine("*************** AUTHENTICATED ***************");
                            DataStructure.ClientList.Add(Info);
                        }
                    }
                    else if (Info.Type == BaseInfoType.Master)
                    {
                        Console.WriteLine("*************** MASTER CONNECTED ***************");
                        if (!DataStructure.MasterList.Any(x => x.UID == Info.UID))
                        {
                            Console.WriteLine("*************** AUTHENTICATED ***************");
                            DataStructure.MasterList.Add(Info);
                        }
                    }
                    if (DataStructure.ClientList.Any(x => x.UID == Info.UID && x.IsConnected == false) || DataStructure.MasterList.Any(x => x.UID == Info.UID && x.IsConnected == false))
                    {
                        Dictionary<string, object> ddata = Converter.ClassToDictionary(Info);
                        foreach (var item in ddata)
                        {
                            Console.WriteLine("* {0} - {1}", item.Key, item.Value);
                        }
                        Info.Connector.StartConnection(Socket);
                    }
                    Console.WriteLine("*******************************************");
                }
            }
            private void LoadPlugins()
            {
                Console.WriteLine("*************** LOADING PLUGINS ***************");
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
                Console.WriteLine("* PLUGINS FOUND: {0}", dllFiles.Length);
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
                                Console.WriteLine("* PLUGIN: {0}", type.ToString());
                            }
                        }
                    }
                }
                foreach (Type type in pluginTypes)
                {
                    IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                    var pluginMetadata = (Metadata[])type.GetCustomAttributes(typeof(Metadata), true);
                    DataStructure.PluginDictionary.Add(pluginMetadata[0], plugin);
                    Console.WriteLine("* {0} LOADED", type.ToString());
                }
                Console.WriteLine("*************** FINNISHED LOADING PLUGINS ***************");
            }
            private void HearthBeat()
            {
                TcpListener HearthBeatServerSocket = new TcpListener(HearthBeatIPEndPoint);
                HearthBeatServerSocket.Start();
                while (true)
                {
                    TcpClient hearthBeatClient = HearthBeatServerSocket.AcceptTcpClient();
                    hearthBeatClient.Close();
                }
            }
            private void CheckClientConnection()
            {
                while (true)
                {
                    try
                    {
                        foreach (var client in DataStructure.ClientList)
                        {
                            TcpClient testTcp = client.Connector.ClientSocket;
                            if (!testTcp.Connected)
                            {
                                client.IsConnected = false;
                            }
                            else
                            {
                                client.IsConnected = true;
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }
            private void CheckMasterConnection()
            {
                while (true)
                {
                    try
                    {
                        foreach (var master in DataStructure.MasterList)
                        {
                            TcpClient testTcp = master.Connector.ClientSocket;
                            if (!testTcp.Connected)
                            {
                                master.IsConnected = false;
                            }
                            else
                            {
                                master.IsConnected = true;
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }
        }
    }
}
