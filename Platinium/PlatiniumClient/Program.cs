using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlatiniumClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(5000);
            ClientController client = new ClientController();
            Console.ReadLine();
        }
    }
}
