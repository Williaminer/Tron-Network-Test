namespace ClientExec
{
    class LineTile
    {
        public string Player { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public LineTile(string colour, int x, int y)
        {
            Player = colour;
            X = x;
            Y = y;
        }
    }
}
