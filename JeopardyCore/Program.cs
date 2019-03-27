using System;

namespace JeopardyCore
{
    class Program
    {
        static void Main(string[] args)
        {
            bool exit = false;

            while (!exit)
            {
                JeoController game = new JeoController();
                exit = game.StartGame();
            }
        }
    }
}
