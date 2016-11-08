using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlatiniumMaster
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(5000);
            MasterController client = new MasterController();
            Console.ReadLine();
        }
    }
}
