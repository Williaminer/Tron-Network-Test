using Microsoft.Xna.Framework.Graphics;

namespace ClientExec
{
    class NearbyPlayer
    {
        public string Username { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int LastReceived { get; set; }

        public NearbyPlayer(string username, int x, int y)
        {
            LastReceived = 10;
            Username = username;
            X = x;
            Y = y;
        }
    }
}
