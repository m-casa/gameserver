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
        private static UdpClient udpListener;

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

            udpListener = new UdpClient(port);
            udpListener.BeginReceive(UDPReceiveCallback, null);


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

        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                // This method will not only return any bytes received, 
                //  but will also set our IPEndPoint to the endpoint where the data came from
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                // Might not have to disconnect, as it could be a common occurence that data is less than 4 bytes
                if (_data.Length < 4)
                {
                    // TODO: disconnect
                    return;
                }

                using (Packet _packet = new Packet(_data))
                {
                    int _clientId = _packet.ReadInt();

                    // Make sure client id is not 0, as this id does not exist and can cause server crash
                    if (_clientId == 0)
                    {
                        return;
                    }

                    // Check if the udp end point is null, which means this is a new connection
                    //  and the packet received is the empty one that opens up the client's port
                    if (clients[_clientId].udp.endPoint == null)
                    {
                        clients[_clientId].udp.Connect(_clientEndPoint);
                        return;
                    }

                    // Check to make sure the endpoint we have stored for the client matches the client id of where the packet came from
                    // Stops hackers from trying to impersonate another client
                    if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    {
                        clients[_clientId].udp.HandleData(_packet);
                    }
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error receiving UDP data: {_ex}");
            }
        }

        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                if (_clientEndPoint != null)
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
            }
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
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
            };
            Console.WriteLine("Initialized packets.");
        }
    }
}
