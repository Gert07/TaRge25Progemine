using System;
using System.Collections.Generic;
using System.Text;
using static System.Console;

namespace Labyrinth2
{
    class Game
    {
        private World MyWorld;
        private Player CurrentPlayer;
        public void Start()
        {
            // Game logic goes here

            string[,] grid = {
                { " ", "#", "#", "#", "#", "#", "#", "#", "#", " " },
                { " ", "#", " ", " ", " ", " ", " ", " ", "#", " " },
                { " ", "#", " ", "#", "#", "#", "#", " ", "#", " " },
                { " ", "#", " ", "#", "X", "#", "#", " ", "#", " " },
                { " ", "#", " ", "#", " ", "#", "#", " ", "#", " " },
                { " ", " ", " ", " ", " ", " ", " ", " ", "#", " " },
                { " ", "#", "#", "#", "#", "#", "#", "#", "#", "#" },
                { "#", " ", " ", " ", " ", " ", " ", " ", " ", "#" },
            };
            MyWorld = new World(grid);

            CurrentPlayer = new Player(1, 1);

            RunGameLoop();

            //WriteLine("\n\nPress any key to exit...");
            ReadKey(true);
        }

        private void Intro()
        {
            WriteLine("Welcome to the Labyrinth Game!");
            WriteLine("Use the arrow keys to navigate through the labyrinth.");
            WriteLine("Your goal is to reach the exit marked with 'X'.");
            WriteLine("Press any key to start...");
            ReadKey(true);
        }

        private void Outro()
        {
            Clear();
            WriteLine("Thank you for playing the Labyrinth Game!");
            WriteLine("Press any key to exit...");
            ReadKey(true);
        }   
        private void DrawFrames()
        {
            Clear();
            MyWorld.Draw();
            CurrentPlayer.Draw();
        }

        private void HandlePlayerInput()
        {
            ConsoleKeyInfo keyInfo = ReadKey(true);
            ConsoleKey key = keyInfo.Key;
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    if (MyWorld.IsWalkable(CurrentPlayer.X, CurrentPlayer.Y - 1))
                    { 
                        CurrentPlayer.Y -= 1; 
                    }
                    break;
                case ConsoleKey.DownArrow:
                    if (MyWorld.IsWalkable(CurrentPlayer.X, CurrentPlayer.Y + 1))
                    {
                        CurrentPlayer.Y += 1;
                    }
                    break;
                case ConsoleKey.LeftArrow:
                    if (MyWorld.IsWalkable(CurrentPlayer.X - 1, CurrentPlayer.Y))
                    {
                        CurrentPlayer.X -= 1;
                    }
                    break;
                case ConsoleKey.RightArrow:
                    if (MyWorld.IsWalkable(CurrentPlayer.X + 1, CurrentPlayer.Y))
                    {
                        CurrentPlayer.X += 1;
                    }
                    break;
            }
        }
        private void RunGameLoop()
        {
            Intro();
            // Implement the game loop logic here
            while (true) 
            {
                //Draw Everything
                DrawFrames();
                //Check for player input from keyboard
                HandlePlayerInput();
                //Check if the player has reached exit
                string elementAtPlayerPosition = MyWorld.GetElementAt(CurrentPlayer.X, CurrentPlayer.Y);
                if (elementAtPlayerPosition == "X")
                {
                    break;
                }
                System.Threading.Thread.Sleep(50);

            }
            Outro();
        }   
    }
}
