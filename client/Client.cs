using Lidgren.Network;
using System.Collections.Generic;
using ServerExec;

namespace ClientExec
{
    class Client
    {
        public List<NearbyPlayer> NearbyPlayers { get; set; }
        public List<LineTile> LineTiles { get; set; }
        private NetClient client;
        public string PacketMessage { get; set; }
        public string Message { get; set; }
        public string LastWinner { get; set; }
        public bool Started { get; set; }
        public bool Restarting { get; set; }
        public bool AttemptedRestart { get; set; }

        public void StartClient(string ip, int port)
        {
            NearbyPlayers = new List<NearbyPlayer>(0);
            LineTiles = new List<LineTile>(0);
            Started = false;
            Restarting = false;
            AttemptedRestart = false;
            PacketMessage = "";
            LastWinner = "";
            Message = "";

            var config = new NetPeerConfiguration("Multiplayer Test");
            config.AutoFlushSendQueue = false;
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

            client = new NetClient(config);
            client.Start();

            client.Connect(ip, port);
        }

        public void Disconnect()
        {
            client.Disconnect("bye");
        }

        public void SendSpawnPacket(string username, int x, int y)
        {
            NetOutgoingMessage message = client.CreateMessage();
            SpawnPacket packet = new SpawnPacket();
            packet.Player = username;
            packet.X = x;
            packet.Y = y;
            
            packet.OutgoingPacket(message);
            client.SendMessage(message, NetDeliveryMethod.ReliableOrdered);
            client.FlushSendQueue();
        }

        public void SendPositionPacket(string username, int x, int y)
        {
            NetOutgoingMessage message = client.CreateMessage();
            PositionPacket packet = new PositionPacket();
            packet.Player = username;
            packet.X = x;
            packet.Y = y;

            packet.OutgoingPacket(message);
            client.SendMessage(message, NetDeliveryMethod.ReliableOrdered);
            client.FlushSendQueue();
        }

        public void SendLinePacket(string username, int x, int y)
        {
            NetOutgoingMessage message = client.CreateMessage();
            LinePacket packet = new LinePacket();
            packet.Player = username;
            packet.X = x;
            packet.Y = y;

            packet.OutgoingPacket(message);
            client.SendMessage(message, NetDeliveryMethod.ReliableOrdered);
            client.FlushSendQueue();
        }

        public void SendDeadPacket(string username)
        {
            NetOutgoingMessage message = client.CreateMessage();
            DeadPacket packet = new DeadPacket();
            packet.Player = username;

            packet.OutgoingPacket(message);
            client.SendMessage(message, NetDeliveryMethod.ReliableOrdered);
            client.FlushSendQueue();
        }

        public string ReadMessages()
        {
            NetIncomingMessage message;

            while((message = client.ReadMessage()) != null)
            {
                switch(message.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        {
                            byte type = message.ReadByte();

                            switch(type)
                            {
                                case (byte)PacketTypes.SpawnPacket:
                                    {
                                        SpawnPacket packet = new SpawnPacket();
                                        packet.IncomingPacket(message);
                                        PacketMessage = "spawn";
                                        break;
                                    }
                                case (byte)PacketTypes.RejectionPacket:
                                    {
                                        RejectionPacket packet = new RejectionPacket();
                                        packet.IncomingPacket(message);
                                        PacketMessage = "reject";
                                        break;
                                    }
                                case (byte)PacketTypes.PositionPacket:
                                    {
                                        Message = "position";
                                        PositionPacket packet = new PositionPacket();
                                        packet.IncomingPacket(message);

                                        NearbyPlayer player;
                                        if((player = NearbyPlayers.Find(player =>
                                        player.Username.Equals(packet.Player))) != null)
                                        {
                                            Message = "already";
                                            player.X = packet.X;
                                            player.Y = packet.Y;
                                            player.LastReceived = 10;
                                        }
                                        else
                                        {
                                            Message = "add";
                                            NearbyPlayers.Add(new NearbyPlayer(
                                            packet.Player, packet.X, packet.Y));
                                        }

                                        break;
                                    }
                                case (byte)PacketTypes.StartPacket:
                                    {
                                        Message = "start";
                                        Started = true;
                                        break;
                                    }
                                case (byte)PacketTypes.LinePacket:
                                    {
                                        Message = "line";
                                        LinePacket packet = new LinePacket();
                                        packet.IncomingPacket(message);

                                        if(!LineTiles.Exists(line => line.X == packet.X && line.Y == packet.Y))
                                        {
                                            Message = "add line";
                                            LineTiles.Add(new LineTile(packet.Player,
                                                packet.X, packet.Y));
                                        }

                                        break;
                                    }
                                case (byte)PacketTypes.ResetPacket:
                                    {
                                        AttemptedRestart = true;
                                        Message = "reset";
                                        ResetPacket packet = new ResetPacket();
                                        packet.IncomingPacket(message);
                                        LastWinner = packet.Winner;

                                        LineTiles = new List<LineTile>(0);
                                        Restarting = true;
                                        break;
                                    }
                            }
                            break;
                        }
                    default:
                        break;
                }
                client.Recycle(message);
            }
            return PacketMessage;
        }
    }
}
