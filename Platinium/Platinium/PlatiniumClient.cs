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
                clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55555));
                serverStream = clientSocket.GetStream();
                Write(new Package(ClientInfo, PackageType.Base, new BaseInfo(BaseInfoType.Server), ClientInfo));
                Thread GetThread = new Thread(Get);
                GetThread.Start();
                LoadPlugins();
            }
            private void LoadPlugins()
            {
                Console.WriteLine("DOWNLOADING PLUGINS");
                Write(new Package(new BaseCommand("LOAD_PLUGINS", "LOAD_PLUGINS".GetType()), PackageType.Plugin, new BaseInfo(BaseInfoType.Server), ClientInfo));
            }
            private void Write(Package package)
            {
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
                    serverStream.Read(TransportPackage.Data, 0, TransportPackage.Data.Length);
                    serverStream.Flush();
                    Package package = (Package)Serializer.Deserialize(TransportPackage);
                    package = PackageFactory.HandleClientPackages(package);
                    BaseCommand bC = (BaseCommand)package.Content;
                    Console.WriteLine(bC.Data.ToString());
                    Console.WriteLine(bC.DataType.ToString());
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
