using Platinium.Shared.Data.Packages;
using Platinium.Shared.Data.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlatiniumServer
{
    class Server
    {
        private TcpClient clientSocket = default(TcpClient);
        TcpListener MainServerSocket;
        TcpListener HearthBeatServerSocket;
        public IPEndPoint MainIPEndPoint { get; private set; }
        public IPEndPoint HearthBeatIPEndPoint { get; private set; }
        public Server()
        {
            MainIPEndPoint = new IPEndPoint(IPAddress.Any, 55555);
            HearthBeatIPEndPoint = new IPEndPoint(IPAddress.Any, 55554);
            Initialize();
        }
        private void Initialize()
        {
            //Thread hearthBeatThread = new Thread(HearthBeat);
            Task hearthBeatTask = new Task(HearthBeat);
            hearthBeatTask.Start();
            MainServerSocket = new TcpListener(MainIPEndPoint);
            MainServerSocket.Start();
            MainServer();
        }
        private void InitializeEnvironment()
        {

        }
        private void MainServer()
        {
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
