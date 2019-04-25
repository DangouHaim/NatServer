using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NATServer
{
    internal class NatUdpClient
    {
        public NatUdpClient()
        {
            StartClient();
        }

        private void StartClient()
        {
            Console.Write("Enter server ip: ");
            string ip = Console.ReadLine();
            Console.Write("Enter server port: ");
            string port = Console.ReadLine();

            if (string.IsNullOrEmpty(ip))
            {
                ip = "92.38.69.185";
            }

            var c = new UdpClient(566);
            c.AllowNatTraversal(true);
            c.Connect(new IPEndPoint(IPAddress.Parse(ip), int.Parse(port)));

            HoldConnection(c);

            Receive(c);

            while (true)
            {
                Console.Write(">");
                var buffer = ASCIIEncoding.UTF8.GetBytes(Console.ReadLine());
                c.Send(buffer, buffer.Length);
            }
        }

        private async void HoldConnection(UdpClient c)
        {
            await c.SendAsync(new byte[0], 0);
            await Task.Delay(3000);
        }

        private async void Receive(UdpClient c)
        {
            while(true)
            {
                var res = await c.ReceiveAsync();
                if(res.Buffer.Length > 0)
                {
                    Console.WriteLine(ASCIIEncoding.UTF8.GetString(res.Buffer));
                }
            }
        }
    }
}
