using Platinium.Shared.Content;
using Platinium.Shared.Core;
using Platinium.Shared.Data.Network;
using Platinium.Shared.Data.Packages;
using Platinium.Shared.Data.Serialization;
using Platinium.Shared.Data.Structures;
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
            public NetworkStream networkStream;
            public TcpClient masterSocket = new TcpClient();
            private static PackageFactory PFactory;
            public string FILE_LOG_PATH;
            private bool isConnected = false;
            public bool Received = false;
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
                    networkStream = masterSocket.GetStream();
                    NetworkManagement.SetNetworkStream(networkStream);
                    PFactory = new PackageFactory(this);
                    DataStructure.Info = MasterInfo;
                    Write(new Package(null, MasterInfo, PackageType.Base, MasterInfo, new ClientInfo(BaseInfoType.Server), null));
                    Thread GetThread = new Thread(Get);
                    GetThread.Start();
                    isConnected = true;
                }
                catch (Exception) { }
            }
            public string GetPlugins()
            {
                return Write(new Package(new Command(null, null, "load_plugins", null, null), null, PackageType.Data, MasterInfo, new ClientInfo(BaseInfoType.Server), null));
            }
            public string GetClients()
            {
                return Write(new Package(new Command(null, null, "client_list", null, null), null, PackageType.Data, MasterInfo, new ClientInfo(BaseInfoType.Server), null));
            }
            public string SendCommand(string command)
            {
                Command parsedCommand = Command.ParseCommand(command, logger);
                PackageType type;
                if (parsedCommand.Type == "plugin")
                {
                    type = PackageType.PluginCommand;
                }
                else
                {
                    type = PackageType.Base;
                }
                return Write(new Package(parsedCommand, null, type, MasterInfo, new ClientInfo(parsedCommand.UID)));
            }
            private string Write(Package package)
            {
                try
                {
                    NetworkManagement.WriteData(networkStream, package, logger);
                    if (!DataStructure.PackageStatus.ContainsKey(package.ID))
                    {
                        DataStructure.PackageStatus.Add(package.ID, PackageStatus.NotProcessed);
                    }
                    return package.ID;
                }
                catch (Exception ex) { isConnected = false; logger.LogMessageToFile($"FATAL EXCEPTION: {ex.Message}"); }
                return null;
            }
            private void Get()
            {
                while (true)
                {
                    Package package;
                    try
                    {
                        package = NetworkManagement.ReadData(networkStream, logger);
                    }
                    catch (Exception ex) { isConnected = false; logger.LogMessageToFile($"FATAL EXCEPTION: {ex.Message}"); break; }
                    package = PFactory.HandleMasterPackages(package);
                }
            }
            private static ClientInfo BuildMasterInfo()
            {
                ClientInfo ci = new ClientInfo
                {
                    IP = DeviceInfo.GetPublicIP(),
                    MACAddress = DeviceInfo.GetMacAddress(),
                    UserName = "MASTER",
                    ComputerName = DeviceInfo.GetComputerName(),
                    UID = FingerPrint.Value(),
                    Type = BaseInfoType.Master,
                    IsAdministrator = DeviceInfo.IsAdministrator(),
                    Language = DeviceInfo.GetCurrentCulture(),
                    AppNetVersion = DeviceInfo.GetAppNetVersion(),
                    OSName = DeviceInfo.GetOSName(),
                };
                return ci;
            }
        }
    }

}