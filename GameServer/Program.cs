using System;
using System.Threading;

namespace GameServer
{
    class Program
    {
        private static bool isRunning = false;
        static void Main(string[] args)
        {
            Console.Title = "Game Server";
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(10, 26950);
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
            DateTime _nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (_nextLoop < DateTime.Now)
                {
                    // Update the server
                    GameLogic.Update();

                    // Set when the next update is; EX. Update every 33.33 ms, which is about 30 ticks per second
                    _nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    // Puts the thread to sleep so our CPU usage is not so high while the thread does nothing
                    if (_nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(_nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}
