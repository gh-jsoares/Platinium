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
            Thread.Sleep(10000);
            Client client = new Client();
            Console.ReadLine();
        }
    }
}
