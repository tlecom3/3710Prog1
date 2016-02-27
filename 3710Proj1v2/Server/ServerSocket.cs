using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class ServerSocket
    {
        public static Dictionary<string, TcpClient> Clients = new Dictionary<string, TcpClient>(); 

        public void Start()
        {
            var server = TcpListener.Create(3306);

            server.Start();
            Console.WriteLine("Server started..."+server.LocalEndpoint);
            while (true)
            {
                var client = server.AcceptTcpClient();
                var stream = client.GetStream();
                var bytesFrom = new byte[client.ReceiveBufferSize];
                stream.Read(bytesFrom, 0, client.ReceiveBufferSize);
                var name = Encoding.ASCII.GetString(bytesFrom);
                name = name.Substring(0, name.IndexOf("<EOF>", StringComparison.Ordinal));
                if (string.IsNullOrEmpty(name))
                    Broadcast("anonymous joined");
                else if (Clients.ContainsKey(name))
                    name = name + Clients.Keys.Where(x => x.StartsWith(name)).Count()+1;
                Broadcast(name + " joined");
                Clients.Add(name, client);
                Console.WriteLine(client.Client.RemoteEndPoint+" joined");
                var handler = new HandleClient();
                handler.Start(name, client);
            }
        }

        public static void Broadcast(string msg, string name = null)
        {
            List<string> toRemove = new List<string>();
            foreach (var socket in Clients)
            {
                var stream = socket.Value.GetStream();
                var data = name != null ? Encoding.ASCII.GetBytes(name + ": " + msg.Trim()) : Encoding.ASCII.GetBytes(msg);
                if (socket.Value.Connected)
                    stream.Write(data, 0, data.Length);
                else toRemove.Add(socket.Key);
                stream.FlushAsync();
            }
            foreach(var socket in toRemove)
            {
                RemoveClient(socket, Clients[socket]);
            }
        }

        public static void RemoveClient(string name, TcpClient client)
        {
            client.Close();
            Clients.Remove(name);
        }
    }

    public class HandleClient
    {
        private TcpClient _client;
        private Thread thread;
        private string _name;

        public void Start(string name, TcpClient clientSocket)
        {
            _client = clientSocket;
            _name = name;
            thread = new Thread(DoChat);
            thread.Start();
        }

        private void DoChat()
        {
            var bytesFrom = new byte[_client.ReceiveBufferSize];

            while (true)
            {
                try
                {
                    var networkStream = _client.GetStream();
                    if(networkStream.DataAvailable)
                        networkStream.Read(bytesFrom, 0, _client.ReceiveBufferSize);
                    else continue;
                    var dataFromClient = Encoding.ASCII.GetString(bytesFrom).Trim();
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("<EOF>", StringComparison.Ordinal));
                    if (dataFromClient.Equals("!c"))
                    {
                        ServerSocket.RemoveClient(_name, _client);
                        break;
                    }
                    ServerSocket.Broadcast(dataFromClient, _name);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    break;
                }
            }
            ServerSocket.RemoveClient(_name, _client);
            this.thread.Abort();
        }

    }
}
