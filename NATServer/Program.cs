using LumiSoft.Net.STUN.Client;
using LumiSoft.Net.TCP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NATServer
{
    class Program
    {
        static void Main(string[] args)
        {
            while(true)
            {
                Console.Write("Enter peer type (client - c; server - s): ");
                var peerType = Console.ReadLine();
                
                try
                {
                    if (peerType.ToLower() == "s")
                    {
                        NatUdpListener l = new NatUdpListener();
                        Console.WriteLine();
                        Console.WriteLine("Net type: " + l.StunEndPoint.NetType.ToString());
                        Console.WriteLine("STUN server address: " + l.StunAddress);
                        Console.WriteLine("\r\n" + string.Format("Server address: {0}:{1}", l.StunEndPoint.PublicEndPoint.Address.ToString(), l.StunEndPoint.PublicEndPoint.Port));
                        Console.WriteLine();
                    }
                    else
                    {
                        NatUdpClient c = new NatUdpClient();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }

                Console.ReadLine();
            }
        }
    }
}
