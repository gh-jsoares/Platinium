using Platinium.Shared.Connection;
using Platinium.Shared.Core;
using Platinium.Shared.Data.Packages;
using Platinium.Shared.Data.Serialization;
using Platinium.Shared.Data.Structures;
using Platinium.Shared.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Platinium
{
    namespace Entities
    {
        public class PlatiniumServer
        {
            public IPEndPoint ClientIPEndPoint { get; private set; }
            public IPEndPoint MasterIPEndPoint { get; private set; }
            public IPEndPoint HearthBeatIPEndPoint { get; private set; }
            public PlatiniumServer()
            {
                ClientIPEndPoint = new IPEndPoint(IPAddress.Any, 55556);
                MasterIPEndPoint = new IPEndPoint(IPAddress.Any, 55555);
                HearthBeatIPEndPoint = new IPEndPoint(IPAddress.Any, 55554);
                Thread hearthBeatTask = new Thread(HearthBeat);
                Thread masterListenTask = new Thread(MasterListen);
                Thread clientListenTask = new Thread(ClientListen);
                hearthBeatTask.Start();
                masterListenTask.Start();
                clientListenTask.Start();
            }
            private void MasterListen()
            {
                TcpListener MasterServerSocket = new TcpListener(MasterIPEndPoint);
                MasterServerSocket.Start();
                while (true)
                {
                    TcpClient masterSocket = MasterServerSocket.AcceptTcpClient();
                    TransportPackage TransportPackage = new TransportPackage();
                    NetworkStream networkStream = masterSocket.GetStream();
                    networkStream.Read(TransportPackage.Data, 0, masterSocket.ReceiveBufferSize);
                    Package package = (Package)Serializer.Deserialize(TransportPackage);
                    BaseInfo Info = (BaseInfo)package.Content;
                    Info.Connector = new Connector(masterSocket);
                    DataStructure.MasterList.Add(Info);
                    Dictionary<string, object> ddata = Converter.ClassToDictionary(Info);
                    Console.WriteLine("Master connected");
                    foreach (var item in ddata)
                    {
                        Console.WriteLine("{0} - {1}", item.Key, item.Value);
                    }
                    Info.Connector.StartConnection();
                }
            }
            private void ClientListen()
            {
                TcpListener ClientServerSocket = new TcpListener(ClientIPEndPoint);
                ClientServerSocket.Start();
                while (true)
                {
                    TcpClient clientSocket = ClientServerSocket.AcceptTcpClient();
                    TransportPackage TransportPackage = new TransportPackage();
                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(TransportPackage.Data, 0, clientSocket.ReceiveBufferSize);
                    Package package = (Package)Serializer.Deserialize(TransportPackage);
                    BaseInfo Info = (BaseInfo)package.Content;
                    Info.Connector = new Connector(clientSocket);
                    DataStructure.ClientList.Add(Info);
                    Dictionary<string, object> ddata = Converter.ClassToDictionary(Info);
                    Console.WriteLine("Client connected");
                    foreach (var item in ddata)
                    {
                        Console.WriteLine("{0} - {1}", item.Key, item.Value);
                    }
                    Info.Connector.StartConnection();
                    Package wcPackage = new Package("Client " + Info.UID + " connected", new BaseInfo(BaseInfoType.Master), new BaseInfo(BaseInfoType.Server));
                    foreach (BaseInfo master in DataStructure.MasterList)
                    {
                        TcpClient communicationSocket = master.Connector.ClientSocket;
                        NetworkStream communicationStream = communicationSocket.GetStream();
                        TransportPackage TransportPackageC = Serializer.Serialize(wcPackage);
                        communicationStream.Write(TransportPackageC.Data, 0, TransportPackageC.Data.Length);
                        communicationStream.Flush();
                    }
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
