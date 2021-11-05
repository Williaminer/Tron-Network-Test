using System;

namespace ClientExec
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new GameClient())
                game.Run();
        }
    }
}
