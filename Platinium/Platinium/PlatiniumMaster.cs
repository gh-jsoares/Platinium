using Platinium.Shared.Content;
using Platinium.Shared.Core;
using Platinium.Shared.Data.Network;
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
using System.Windows.Controls;

namespace Platinium
{
    namespace Entities
    {
        public class PlatiniumMaster
        {
            private ClientInfo MasterInfo = BuildMasterInfo();
            private TcpClient masterSocket = new TcpClient();
            public string FILE_LOG_PATH;
            private bool isConnected = false;
            private static Logger logger = new Logger();
            public PlatiniumMaster()
            {
                //Console.Title = "Platinium Beta Master";
            }
            public void Initialize()
            {
                try
                {
                    logger.Path = FILE_LOG_PATH;
                    masterSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55555));
                    Write(new Package(null, MasterInfo, PackageType.Base, MasterInfo, new ClientInfo(BaseInfoType.Server)));
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
                    NetworkManagement.WriteData(masterSocket, package);
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
                        package = NetworkManagement.ReadData(masterSocket, logger);
                    }
                    catch (Exception) { isConnected = false; MessageBox.Show("Unable to read data from the server"); break; }
                    package = PackageFactory.HandleMasterPackages(package);
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