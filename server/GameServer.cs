using System;
using System.Threading;
using System.Collections.Generic;
using Lidgren.Network;

namespace ServerExec
{
    class GameServer
    {
        private static Server server;
        private static Thread serverThread;
        private static Thread startingThread;
        private static bool Started { get; set; }

        private static bool quit;

        static void Main(string[] args)
        {
            quit = false;

            serverThread = new Thread(ServerThread);
            startingThread = new Thread(StartingThread);
            server = new Server();

            server.StartServer(7777);
            if(server.Status == NetPeerStatus.Running)
            {
                DrawScreen();
                serverThread.Start();
                startingThread.Start();

                while (!quit)
                {
                    Thread.Sleep(1000);
                    DrawScreen();

                    if (server.Restarting)
                    {
                        Started = false;
                        server.Restarting = false;
                    }
                }
            }
            else
            {
                Console.WriteLine("Server failed to start");
                Console.WriteLine("Press any key to quit");
                Console.ReadKey();
            }
        }

        public static void DrawScreen()
        {
            Console.Clear();
            Console.WriteLine("Server is running on port " + server.Port);
            string runningMessage = (Started) ? "Game is running" : "Game is not running";
            Console.WriteLine(runningMessage);

            Console.WriteLine("\nInformation\n" +
                server.Clients.Count + " Client(s) are stored\n" +
                server.Connections.Count + " Connection(s) are active\n" +
                server.Players.Count + " Player(s) are spawned in");

            Console.WriteLine("\nStatistics\n" +
                server.SpawnPacketsReceived + " SpawnPacket(s) Received\n" +
                server.SpawnPacketsSent + " SpawnPacket(s) Sent\n" +
                server.RejectionPacketsSent + " RejectionPacket(s) Sent\n" +
                server.AlivePlayers.Count + "/" + server.Players.Count + " Players Alive");
        }

        public static void ServerThread()
        {
            while(!quit)
            {
                Thread.Sleep(1);
                server.ReadMessages();
            }
        }

        public static void StartingThread()
        {
            while(true)
            {
                while (Console.ReadKey().Key != ConsoleKey.S)
                {
                    Thread.Sleep(1);
                }
                if(!Started)
                {
                    Started = true;
                    server.SendStart();
                }
            }
        }
    }
}
