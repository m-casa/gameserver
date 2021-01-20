using System;

namespace GameServer
{
    class ServerHandle
    {
        // Read the packet letting us know the welcome was received
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            // Read in the same order as what is being sent
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();

            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
            if (_fromClient != _clientIdCheck)
            {
                Console.WriteLine($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }
            Server.clients[_fromClient].SendIntoGame(_username);
        }
    }
}
