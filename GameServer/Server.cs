using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    class Server
    {
        public static int maxPlayers { get; private set; }
        public static int port { get; private set; }
        // A new dictionary to keep track of our clients and their ids
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        // A delegate basically says "feel free to assign any method to this delegate if the signature matches"
        // Since our HandleData method has a "using" that matches the "Packet _packet" signature,
        //  we know that is where the packet is being handled, hence this delegate's name
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;

        public static void Start(int _maxPlayers, int _port)
        {
            maxPlayers = _maxPlayers;
            port = _port;

            Console.WriteLine("Starting server...");

            InitializeServerData();

            // To listen in on the specified port for any IP Address trying to connect
            tcpListener = new TcpListener(IPAddress.Any, port);
            // Start listening
            tcpListener.Start();
            // Accept any client that attempts to connect
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Server started on {port}.");
        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            // Store our client connection attempt
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            // Continue listening for client connections
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint}...");

            // Need to assign our newly connected clients their id
            for (int i = 1; i <= maxPlayers; i++)
            {
                // If the socket our client is trying to use is not being used,
                //  store our client's information through that socket
                if (clients[i].tcp.socket == null)
                {
                    // _client and _socket are interchangeable names
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }

            Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        // Add clients to our dictionary
        private static void InitializeServerData()
        {
            for (int i = 1; i <= maxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler> 
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived }
            };
            Console.WriteLine("Initialized packets.");
        }
    }
}
