using System;
using System.Net.Sockets;

namespace GameServer
{
    class Client
    {
        public static int dataBufferSize = 4096;

        // Client unique id
        public int id;
        public TCP tcp;

        public Client(int _clientId)
        {
            id = _clientId;
            tcp = new TCP(id);
        }

        public class TCP 
        {
            // Will store instance we get in the server's connect callback
            public TcpClient socket;

            private readonly int id;

            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public TCP(int _id)
            {
                id = _id;
            }

            public void Connect(TcpClient _socket)
            {
                // Prepare socket for how large data received and sent should be
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                // Grab our stream of data so we can begin reading
                stream = socket.GetStream();

                receivedData = new Packet();
                // How large the data is that we can receive
                receiveBuffer = new byte[dataBufferSize];

                // Begin reading from our stream of data
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome to the server!");
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception _ex)
                {
                    Console.WriteLine($"Error sending data to player {id} via TCP: {_ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    // The size of the data represented as an int
                    int _byteLength = stream.EndRead(_result);

                    // If there is no data we would disconnect
                    if (_byteLength <= 0)
                    {
                        // TODO: disconnect
                        return;
                    }

                    // A new array for storing data; Size is based on what we received
                    byte[] _data = new byte[_byteLength];
                    // Copy the data we received to the _data byte array
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receivedData.Reset(HandleData(_data));

                    // Continue reading data from the stream
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception _ex)
                {
                    Console.WriteLine($"Error receiving TCP data: {_ex}");
                    // TODO: disconnect
                }
            }

            // Determine whether or not we have handled all data
            private bool HandleData(byte[] _data)
            {
                int _packetLength = 0;

                receivedData.SetBytes(_data);

                // Check if receivedData has 4 or more unread bytes, which indicates we have the start of one of our packets
                // An int consists of 4 bytes, and the first data of any packet is an int which represents its length
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    // If there is no more data then reset the received data
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }

                // If we still have data to read, and we have enough room to read that data
                //  then continue receiving the next complete packet
                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
                {
                    // Store the bytes of the packet into a new byte array
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                    // Since our code won't be run on the same thread, execute it on main thread
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt();
                            // Pass our handler a packet
                            Server.packetHandlers[_packetId](id, _packet);
                        }
                    });

                    _packetLength = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        _packetLength = receivedData.ReadInt();
                        // If there is no more data then reset the received data
                        if (_packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (_packetLength <= 1)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
