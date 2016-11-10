using Platinium.Shared.Content;
using Platinium.Shared.Data.Packages;
using Platinium.Shared.Data.Serialization;
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
        class PlatiniumClient
        {
            private BaseInfo ClientInfo = BuildClientInfo();
            private TcpClient clientSocket = new TcpClient();
            private NetworkStream serverStream = default(NetworkStream);
            public PlatiniumClient()
            {
                clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55556));
                serverStream = clientSocket.GetStream();
                Package package = new Package(ClientInfo, new BaseInfo(BaseInfoType.Server), ClientInfo);
                TransportPackage TransportPackage = Serializer.Serialize(package);
                serverStream.Write(TransportPackage.Data, 0, TransportPackage.Data.Length);
                serverStream.Flush();
                Thread GetThread = new Thread(Get);
                GetThread.Start();
            }
            private void Write(BaseInfo to, BaseInfo from)
            {
                Package package = new Package(new Command("RECEIVE", "RECEIVE".GetType()), to, from);
                TransportPackage TransportPackage = Serializer.Serialize(package);
                serverStream.Write(TransportPackage.Data, 0, TransportPackage.Data.Length);
                serverStream.Flush();
                Console.WriteLine("WRITE");
            }
            private void Get()
            {
                while (true)
                {
                    serverStream = clientSocket.GetStream();
                    TransportPackage TransportPackage = new TransportPackage();
                    serverStream.Read(TransportPackage.Data, 0, clientSocket.ReceiveBufferSize);
                    serverStream.Flush();
                    Package package = (Package)Serializer.Deserialize(TransportPackage);
                    Command command = (Command)package.Content;
                    Console.WriteLine(command.Data);
                    Write(package.From, ClientInfo);
                    Console.WriteLine("GET");
                }
            }
            private static BaseInfo BuildClientInfo()
            {
                BaseInfo ci = new BaseInfo
                {
                    IP = "127.0.0.1",
                    MACAddress = "MAC",
                    Name = "TESTClient",
                    UID = "2",
                    Type = BaseInfoType.Client
                };
                return ci;
            }
        }
    }
}
