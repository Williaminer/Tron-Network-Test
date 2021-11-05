using Lidgren.Network;

namespace ServerExec
{
    class Player
    {
        public NetConnection Client { get; set; }
        public string Username { get; set; }
        public int Health { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Player(NetConnection client, string username, int health, int x, int y)
        {
            Client = client;
            Username = username;
            Health = health;
            X = x;
            Y = y;
        }
    }
}
