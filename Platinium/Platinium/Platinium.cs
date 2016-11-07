using Platinium.Shared.Data.Packages;
using Platinium.Shared.Data.Serialization;
using Platinium.Shared.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Net;
using System.Runtime.Serialization;

namespace Platinium
{
    namespace Shared
    {
        namespace Connection
        {
            public class DataTransfer
            {
                public static void Broadcast()
                {

                }
            }
        }
        namespace Plugin
        {
            public interface IPlugin
            {

            }
            public class PluginHelper : IPlugin
            {

            }
        }
        namespace Data
        {
            namespace Packages
            {
                public class PackageHelper
                {
                }
                [Serializable]
                public class WelcomePackage
                {
                    public IInfo Info { get; set; }
                    public WelcomePackage(IInfo info)
                    {
                        Info = info;
                    }
                }
                [Serializable]
                public class BroadcastPackage
                {
                    public string Message { get; set; }
                    public BroadcastPackage(string message)
                    {
                        Message = message;
                    }
                }
                [Serializable]
                public class TransportPackage
                {
                    public byte[] Data = new byte[65536];
                    public TransportPackage()
                    {

                    }
                    public TransportPackage(byte[] data)
                    {
                        Data = data;
                    }
                }
            }
            namespace Serialization
            {
                public class Binder : SerializationBinder
                {
                    public override Type BindToType(string i_AssemblyName, string i_TypeName)
                    {
                        Type typeToDeserialize = Type.GetType(i_TypeName);
                        return typeToDeserialize;
                    }
                }
                public class Serializer
                {
                    public static TransportPackage Serialize(object objToSerialize)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            (new BinaryFormatter()).Serialize(memoryStream, objToSerialize);
                            return new TransportPackage(memoryStream.ToArray());
                        }
                    }
                    public static object Deserialize(TransportPackage package)
                    {
                        using (var memoryStream = new MemoryStream(package.Data))
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            bf.Binder = new Binder();
                            bf.Binder.BindToType("Platinium, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Platinium.Shared.Data.Packages.TransportPackage");
                            return (bf).Deserialize(memoryStream);
                        }
                    }
                }
            }
        }
        namespace Entities
        {
            public class Client
            {
                #region test
                //private TcpClient ClientSocket { get; set; }
                //private ClientInfo ClientInfo { get; set; }
                //public Client(TcpClient clientSocket, ClientInfo clientInfo)
                //{
                //    ClientSocket = clientSocket;
                //    ClientInfo = clientInfo;
                //}
                //private void Handle()
                //{
                //    while (true)
                //    {
                //        BroadcastPackage data;
                //        TransportPackage package = new TransportPackage();
                //        NetworkStream networkStream = null;
                //        try
                //        {
                //            networkStream = ClientSocket.GetStream();
                //            networkStream.Read(package.Data, 0, ClientSocket.ReceiveBufferSize);
                //            data = (BroadcastPackage)Serializer.Deserialize(package);

                //        }
                //        catch (Exception)
                //        {

                //        }
                //    }
                //}
                #endregion
                private TcpClient clientSocket = new TcpClient();
                private NetworkStream serverStream = default(NetworkStream);
                public Client()
                {
                    clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55555));
                    serverStream = clientSocket.GetStream();
                    WelcomePackage welcomePackage = new WelcomePackage(new ClientInfo { IP = "127.0.0.1", MACAddress = "MAC", Name = "PCTESTE", UID = "123456a" });
                    TransportPackage package = Serializer.Serialize(welcomePackage);
                    serverStream.Write(package.Data, 0, package.Data.Length);
                    serverStream.Flush();
                }
            }
            public class Master
            {

            }
            public class Server
            {
                private TcpClient clientSocket = default(TcpClient);
                private TcpListener MainServerSocket;
                private TcpListener HearthBeatServerSocket;
                public IPEndPoint MainIPEndPoint { get; private set; }
                public IPEndPoint HearthBeatIPEndPoint { get; private set; }
                public Server()
                {
                    MainIPEndPoint = new IPEndPoint(IPAddress.Any, 55555);
                    HearthBeatIPEndPoint = new IPEndPoint(IPAddress.Any, 55554);
                    Task hearthBeatTask = new Task(HearthBeat);
                    hearthBeatTask.Start();
                    Listen();
                }
                private void Listen()
                {
                    MainServerSocket = new TcpListener(MainIPEndPoint);
                    MainServerSocket.Start();
                    while (true)
                    {
                        WelcomePackage data;
                        clientSocket = MainServerSocket.AcceptTcpClient();
                        TransportPackage package = new TransportPackage();
                        NetworkStream networkStream = clientSocket.GetStream();
                        networkStream.Read(package.Data, 0, clientSocket.ReceiveBufferSize);
                        data = (WelcomePackage)Serializer.Deserialize(package);
                        foreach (var item in data.Info)
                        {
                            Console.WriteLine(item);
                        }
                    }
                }
                private void HearthBeat()
                {
                    HearthBeatServerSocket = new TcpListener(HearthBeatIPEndPoint);
                    HearthBeatServerSocket.Start();
                    while (true)
                    {
                        TcpClient hearthBeatClient = HearthBeatServerSocket.AcceptTcpClient();
                        hearthBeatClient.Close();
                    }
                }
            }
        }
        namespace Info
        {
            public interface IInfo : IEnumerable<object>
            {
                string Name { get; set; }
                string IP { get; set; }
                string MACAddress { get; set; }
                string UID { get; set; }
            }
            [Serializable]
            public class ClientInfo : IInfo
            {
                public string IP { get; set; }
                public string MACAddress { get; set; }
                public string Name { get; set; }
                public string UID { get; set; }
                private IEnumerable<object> Info()
                {
                    yield return IP;
                    yield return MACAddress;
                    yield return Name;
                    yield return UID;
                }
                public IEnumerator<object> GetEnumerator()
                {
                    return Info().GetEnumerator();
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }
            [Serializable]
            public class MasterInfo : IInfo
            {
                public string IP { get; set; }
                public string MACAddress { get; set; }
                public string Name { get; set; }
                public string UID { get; set; }
                private IEnumerable<object> Info()
                {
                    yield return IP;
                    yield return MACAddress;
                    yield return Name;
                    yield return UID;
                }
                public IEnumerator<object> GetEnumerator()
                {
                    return Info().GetEnumerator();
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }
            [Serializable]
            public class ServerInfo : IInfo
            {
                public string IP { get; set; }
                public string MACAddress { get; set; }
                public string Name { get; set; }
                public string UID { get; set; }
                private IEnumerable<object> Info()
                {
                    yield return IP;
                    yield return MACAddress;
                    yield return Name;
                    yield return UID;
                }
                public IEnumerator<object> GetEnumerator()
                {
                    return Info().GetEnumerator();
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }
        }
    }
}
