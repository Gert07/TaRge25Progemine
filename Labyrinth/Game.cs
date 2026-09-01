using System;
using System.Collections.Generic;
using System.Text;
using static System.Console;


namespace Labyrinth
{
    class Game
    {
        public void Start()
        {
            WriteLine("Game is starting");

            SetCursorPosition(4, 2);
            Write("X");

            string[,] grid = {
                { "1", "2", "3" },
                { "4", "5", "6" },
                { "7", "8", "9" }
            };
            World myWorld = new World(grid);
            myWorld.Draw();

            WriteLine("\n\nPress any key to exit...");
            ReadKey(true);
        }
    }
}