using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//http://csharp.net-informations.com/communications/csharp-chat-server-programming.htm

namespace Client
{
    public class ClientSocket
    {
        public TcpClient Socket = new TcpClient(AddressFamily.InterNetworkV6);
        NetworkStream _stream = default(NetworkStream);

        public void Start(IPAddress host, int port)
        {
            Socket.Client.DualMode = true;
            Socket.Connect(host, port);
            //Console.WriteLine("Connection made to " +Socket.Client.RemoteEndPoint);
            //var ctThread = new Thread(GetMessage);
            //ctThread.Start();
            _stream = Socket.GetStream();

            //while ((true))
            //{
            //    var input = Console.ReadLine();
            //    SendMessage(input);
            //}
        }

        public void Start(string host, int port)
        {
            Socket.Client.DualMode = true;
            Socket.Connect(IPAddress.Parse(host), port);
            //Console.WriteLine("Connection made to " +Socket.Client.RemoteEndPoint);
            //var ctThread = new Thread(GetMessage);
            //ctThread.Start();
            _stream = Socket.GetStream();

            //while ((true))
            //{
            //    var input = Console.ReadLine();
            //    SendMessage(input);
            //}
        }

        private void GetMessage()
        {
            //while (true)
            //{
                var buffSize = Socket.ReceiveBufferSize;
                var inStream = new byte[buffSize];
                if (_stream.DataAvailable)
                    _stream.Read(inStream, 0, buffSize);
                //else continue;
                var returndata = Encoding.ASCII.GetString(inStream);
                Console.WriteLine(""+returndata.Trim('\0'));
            //}
        }

        public string CheckForMessage()
        {
            var buffSize = Socket.ReceiveBufferSize;
            var inStream = new byte[buffSize];
            if (_stream.DataAvailable)
                _stream.Read(inStream, 0, buffSize);
            return ""+Encoding.ASCII.GetString(inStream).Trim('\0');
        }

        public void SendMessage(string str)
        {
            var data = Encoding.ASCII.GetBytes(str + "<EOF>");
            _stream.Write(data, 0, data.Length);
            _stream.FlushAsync();
        }

    }
}
