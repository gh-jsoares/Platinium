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
        public class PlatiniumMaster
        {
            private BaseInfo MasterInfo = BuildMasterInfo();
            private TcpClient masterSocket = new TcpClient();
            private NetworkStream serverStream = default(NetworkStream);
            public PlatiniumMaster()
            {
                masterSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55555));
                serverStream = masterSocket.GetStream();
                Package package = new Package(MasterInfo, new BaseInfo(BaseInfoType.Server), MasterInfo);
                TransportPackage TransportPackage = Serializer.Serialize(package);
                serverStream.Write(TransportPackage.Data, 0, TransportPackage.Data.Length);
                serverStream.Flush();
                Thread GetThread = new Thread(Get);
                GetThread.Start();
                Thread WriteThread = new Thread(Write);
                WriteThread.Start();
            }
            private void Write()
            {
                while (true)
                {
                    Thread.Sleep(10000);
                    Command command = new Command("OK", "OK".GetType());
                    BaseInfo to = new BaseInfo("2");
                    Package package = new Package(new Command(command, command.GetType()), to, MasterInfo);
                    TransportPackage TransportPackage = Serializer.Serialize(package);
                    serverStream.Write(TransportPackage.Data, 0, TransportPackage.Data.Length);
                    serverStream.Flush();
                    Console.WriteLine("WRITE");
                }
            }
            private void Get()
            {
                while (true)
                {
                    serverStream = masterSocket.GetStream();
                    TransportPackage TransportPackage = new TransportPackage();
                    serverStream.Read(TransportPackage.Data, 0, masterSocket.ReceiveBufferSize);
                    Package package = (Package)Serializer.Deserialize(TransportPackage);
                    Console.WriteLine(package.Content.ToString());
                    Console.WriteLine("GET");
                }
            }
            private static BaseInfo BuildMasterInfo()
            {
                BaseInfo ci = new BaseInfo
                {
                    IP = "127.0.0.1",
                    MACAddress = "MAChhh",
                    Name = "TESTMaster",
                    UID = "1",
                    Type = BaseInfoType.Master
                };
                return ci;
            }
        }
    }

}