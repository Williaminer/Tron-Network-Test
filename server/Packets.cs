using Lidgren.Network;

namespace ServerExec
{
    public enum PacketTypes
    {
        SpawnPacket,
        PositionPacket,
        RejectionPacket,
        StartPacket,
        LinePacket,
        DeadPacket,
        ResetPacket
    }

    public interface IPacket
    {
        void OutgoingPacket(NetOutgoingMessage message);
        void IncomingPacket(NetIncomingMessage message);
    }

    public abstract class Packet : IPacket
    {
        public abstract void OutgoingPacket(NetOutgoingMessage message);
        public abstract void IncomingPacket(NetIncomingMessage message);
    }

    //Sent from the client to server requesting to spawn at XY with name Player.
    public class SpawnPacket : Packet
    {
        public string Player { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public override void OutgoingPacket(NetOutgoingMessage message)
        {
            message.Write((byte)PacketTypes.SpawnPacket);
            message.Write(Player);
            message.Write(X);
            message.Write(Y);
        }
        public override void IncomingPacket(NetIncomingMessage message)
        {
            Player = message.ReadString();
            X = message.ReadInt32();
            Y = message.ReadInt32();
        }
    }

    //Sent from the client to the server to update the client's position, and sent
    //from the server to the client to update the positions of other players.
    public class PositionPacket : Packet
    {
        public string Player { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public override void OutgoingPacket(NetOutgoingMessage message)
        {
            message.Write((byte)PacketTypes.PositionPacket);
            message.Write(Player);
            message.Write(X);
            message.Write(Y);
        }
        public override void IncomingPacket(NetIncomingMessage message)
        {
            Player = message.ReadString();
            X = message.ReadInt32();
            Y = message.ReadInt32();
        }
    }

    //A rejection packet, used by the server to decline requests, such as a spawn
    //request from a client. Can contain a reason as to why.
    public class RejectionPacket : Packet
    {
        public string Reason { get; set; }
        public override void OutgoingPacket(NetOutgoingMessage message)
        {
            message.Write((byte)PacketTypes.RejectionPacket);
            message.Write(Reason);
        }
        public override void IncomingPacket(NetIncomingMessage message)
        {
            Reason = message.ReadString();
        }
    }

    //A line packet, sent by clients when they add another tile to their line.
    public class LinePacket : Packet
    {
        public string Player { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public override void OutgoingPacket(NetOutgoingMessage message)
        {
            message.Write((byte)PacketTypes.LinePacket);
            message.Write(Player);
            message.Write(X);
            message.Write(Y);
        }
        public override void IncomingPacket(NetIncomingMessage message)
        {
            Player = message.ReadString();
            X = message.ReadInt32();
            Y = message.ReadInt32();
        }
    }

    //A start packet, sent by the server to connected clients when the game begins.
    public class StartPacket : Packet
    {
        public override void OutgoingPacket(NetOutgoingMessage message)
        {
            message.Write((byte)PacketTypes.StartPacket);
        }
        public override void IncomingPacket(NetIncomingMessage message)
        {

        }
    }

    //Sent by a client when they die, to allow the server to track and identify the
    //winner of each round.
    public class DeadPacket : Packet
    {
        public string Player { get; set; }
        public override void OutgoingPacket(NetOutgoingMessage message)
        {
            message.Write((byte)PacketTypes.DeadPacket);
            message.Write(Player);
        }
        public override void IncomingPacket(NetIncomingMessage message)
        {
            Player = message.ReadString();
        }
    }

    //Sent by the server to get clients to reset after each round, includes the winner
    //of the previous round.
    public class ResetPacket : Packet
    {
        public string Winner { get; set; }
        public override void OutgoingPacket(NetOutgoingMessage message)
        {
            message.Write((byte)PacketTypes.ResetPacket);
            message.Write(Winner);
        }
        public override void IncomingPacket(NetIncomingMessage message)
        {
            Winner = message.ReadString();
        }
    }
}
