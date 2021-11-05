using Lidgren.Network;
using System.Collections.Generic;

namespace ServerExec
{
    class Server
    {
        private NetServer server;
        public List<NetPeer> Clients { get; set; }
        public List<Player> Players { get; set; }
        public List<NetConnection> Connections { get => server.Connections; }
        public NetPeerStatus Status { get => server.Status; }
        public int Port { get => server.Port; }
        public bool Restarting { get; set; }

        //Statistics
        public int SpawnPacketsReceived { get; set; }
        public int SpawnPacketsSent { get; set; }
        public int RejectionPacketsSent { get; set; }
        public List<string> AlivePlayers { get; set; }

        public void StartServer(int port)
        {
            var config = new NetPeerConfiguration("Multiplayer Test");
            config.Port = port;
            config.ConnectionTimeout = 1000;
            config.MaximumConnections = 20;
            config.AutoFlushSendQueue = false;
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

            server = new NetServer(config);
            server.Start();

            Clients = new List<NetPeer>(0);
            Players = new List<Player>(0);
            AlivePlayers = new List<string>(0);
        }

        public void ReadMessages()
        {
            NetIncomingMessage message;

            while((message = server.ReadMessage()) != null)
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
                                        SpawnPacketsReceived++;
                                        SpawnPacket packet = new SpawnPacket();
                                        NetConnection client = message.SenderConnection;
                                        packet.IncomingPacket(message);

                                        if(Players.Exists(s => packet.Player.Equals(s.Username)))
                                        {
                                            SendRejection("already exists", client);
                                        }
                                        else
                                        {
                                            Players.Add(new Player(client, packet.Player, 100,
                                                packet.X, packet.Y));
                                            SendBackSpawn(packet, client);
                                        }
                                        break;
                                    }
                                case (byte)PacketTypes.PositionPacket:
                                    {
                                        PositionPacket packet = new PositionPacket();
                                        packet.IncomingPacket(message);

                                        if(Players.Count > 0)
                                        {
                                            Player player = Players.Find(
                                            player => player.Username == packet.Player);
                                            player.X = packet.X;
                                            player.Y = packet.Y;

                                            SendNearbyPosition(packet);
                                        }
                                        break;
                                    }
                                case (byte)PacketTypes.RejectionPacket:
                                    {
                                        break;
                                    }
                                case (byte)PacketTypes.LinePacket:
                                    {
                                        LinePacket packet = new LinePacket();
                                        packet.IncomingPacket(message);

                                        SendLinePacket(packet);
                                        break;
                                    }
                                case (byte)PacketTypes.DeadPacket:
                                    {
                                        DeadPacket packet = new DeadPacket();
                                        packet.IncomingPacket(message);

                                        AlivePlayers.Remove(packet.Player);

                                        if(AlivePlayers.Count <= 1)
                                            SendReset();
                                        break;
                                    }
                            }
                            break;
                        }
                    case NetIncomingMessageType.StatusChanged:
                        {
                            if (message.SenderConnection.Status == NetConnectionStatus.Connected)
                            {
                                Clients.Add(message.SenderConnection.Peer);
                            }
                            if (message.SenderConnection.Status == NetConnectionStatus.Disconnected)
                            {
                                Clients.Remove(message.SenderConnection.Peer);
                            }
                            if (message.SenderConnection.Status == NetConnectionStatus.RespondedAwaitingApproval)
                            {
                                message.SenderConnection.Approve();
                            }
                            break;
                        }
                    default:
                        break;
                }
                server.Recycle(message);
            }
        }

        public void SendRejection(string reason, NetConnection client)
        {
            RejectionPacketsSent++;

            NetOutgoingMessage message = server.CreateMessage();
            RejectionPacket rejectionPacket = new RejectionPacket();

            rejectionPacket.Reason = reason;
            rejectionPacket.OutgoingPacket(message);
            server.SendMessage(message, client, NetDeliveryMethod.ReliableOrdered);
            server.FlushSendQueue();
        }

        public void SendBackSpawn(SpawnPacket packet, NetConnection client)
        {
            SpawnPacketsSent++;

            NetOutgoingMessage message = server.CreateMessage();
            
            packet.OutgoingPacket(message);
            server.SendMessage(message, client, NetDeliveryMethod.ReliableOrdered);
            server.FlushSendQueue();
        }

        public void SendNearbyPosition(PositionPacket positionPacket)
        {
            int X = positionPacket.X;
            int Y = positionPacket.Y;
            
            foreach(NetConnection connection in server.Connections)
            {
                NetOutgoingMessage message = server.CreateMessage();
                positionPacket.OutgoingPacket(message);

                server.SendMessage(message, connection, NetDeliveryMethod.ReliableOrdered);
                server.FlushSendQueue();
            }
        }

        public void SendLinePacket(LinePacket linePacket)
        {
            foreach(Player player in Players)
            {
                NetOutgoingMessage message = server.CreateMessage();
                linePacket.OutgoingPacket(message);

                server.SendMessage(message, player.Client, NetDeliveryMethod.ReliableOrdered);
                server.FlushSendQueue();
            }
        }

        public void SendStart()
        {
            StartPacket packet = new StartPacket();

            foreach (Player player in Players)
            {
                AlivePlayers.Add(player.Username);

                NetOutgoingMessage message = server.CreateMessage();
                packet.OutgoingPacket(message);

                server.SendMessage(message, player.Client, NetDeliveryMethod.ReliableOrdered);
                server.FlushSendQueue();
            }
        }

        public void SendReset()
        {
            ResetPacket packet = new ResetPacket();
            if (AlivePlayers.Count > 0)
                packet.Winner = AlivePlayers[0];
            else
                packet.Winner = "n/a";

            AlivePlayers = new List<string>(0);
            Restarting = true;

            foreach (Player player in Players)
            {
                NetOutgoingMessage message = server.CreateMessage();
                packet.OutgoingPacket(message);

                server.SendMessage(message, player.Client, NetDeliveryMethod.ReliableOrdered);
                server.FlushSendQueue();
            }

            Players = new List<Player>(0);
        }
    }
}
