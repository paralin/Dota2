using System;
using System.Net;
using System.Reflection;
using System.Text;
using Dota2.Datagram;
using log4net;
using log4net.Config;

namespace DatagramTest
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            DatagramClient client = new DatagramClient();
            client.Connect(new IPEndPoint(IPAddress.Parse("162.254.195.70"), 27022));
            client.SendPing();
            var resp = client.WaitOneResponse();
            Console.WriteLine(ASCIIEncoding.ASCII.GetString(resp));
            Console.ReadLine();
        }
    }
}
