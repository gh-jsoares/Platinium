﻿using Platinium.Connection;
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
                    networkStream = null;
                    networkStream = ClientSocket.GetStream();
                    TransportPackage TransportPackage = new TransportPackage();
                    networkStream.Read(TransportPackage.Data, 0, TransportPackage.Data.Length);
                    networkStream.Flush();
                    Package package = (Package)Serializer.Deserialize(TransportPackage);
                    Console.WriteLine("GET PACKAGE\nType - {0}\nValue - {1}\nFrom - {2}\nTo - {3}", package.PackageType.ToString(), package.Content.ToString(), package.From.UID, package.To.UID);
                    Communicate(package);
                }
            }
            public static void Communicate(Package package)
            {
                Console.WriteLine("COMMUNICATE");
                Console.WriteLine("TO {0}", package.To.Type);
                if (package.To.Type == BaseInfoType.Client)
                {
                    foreach (var item in DataStructure.ClientList)
                    {
                        if (package.To.UID == item.UID)
                        {
                            TcpClient communicationSocket = item.Connector.ClientSocket;
                            NetworkStream communicationStream = communicationSocket.GetStream();
                            TransportPackage TransportPackage = Serializer.Serialize(package);
                            communicationStream.Write(TransportPackage.Data, 0, TransportPackage.Data.Length);
                            communicationStream.Flush();
                            Console.WriteLine("SENT");
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
                            TransportPackage TransportPackage = Serializer.Serialize(package);
                            communicationStream.Write(TransportPackage.Data, 0, TransportPackage.Data.Length);
                            communicationStream.Flush();
                            Console.WriteLine("SENT");
                        }
                    }
                }
                else if (package.To.Type == BaseInfoType.Server)
                {
                    if (package.From.Type == BaseInfoType.Master)
                    {

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
                                TransportPackage TransportPackage = Serializer.Serialize(outPackage);
                                communicationStream.Write(TransportPackage.Data, 0, TransportPackage.Data.Length);
                                Console.WriteLine("SENT");
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
                LoadPlugins();
                IPEndPoint = new IPEndPoint(IPAddress.Any, 55555);
                HearthBeatIPEndPoint = new IPEndPoint(IPAddress.Any, 55554);
                Thread hearthBeatTask = new Thread(HearthBeat);
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
                    BaseInfo Info = (BaseInfo)package.Content;
                    Info.Connector = new Connector();
                    if (Info.Type == BaseInfoType.Client)
                    {
                        if (!DataStructure.ClientList.Contains(Info))
                        {
                            DataStructure.ClientList.Add(Info);
                        }
                    }
                    else if (Info.Type == BaseInfoType.Master)
                    {
                        if (!DataStructure.MasterList.Contains(Info))
                        {
                            DataStructure.MasterList.Add(Info);
                        }
                    }
                    if (!DataStructure.ClientList.Contains(Info) || !DataStructure.MasterList.Contains(Info))
                    {
                        Dictionary<string, object> ddata = Converter.ClassToDictionary(Info);
                        foreach (var item in ddata)
                        {
                            Console.WriteLine("{0} - {1}", item.Key, item.Value);
                        }
                        Info.Connector.StartConnection(Socket);
                    }
                }
            }
            private void LoadPlugins()
            {
                string[] dllFiles = Directory.GetFiles("Plugins", "*.dll", SearchOption.AllDirectories);
                foreach (string file in dllFiles)
                {
                    byte[] buffer = null;
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, (int)fs.Length);
                    }
                    DataStructure.AssemblyList.Add(buffer);
                }
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
        }
    }
}
