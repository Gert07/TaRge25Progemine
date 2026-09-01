using System;
using System.Collections.Generic;
using System.Text;
using static System.Console;

namespace Labyrinth2
{
    class World
    {
        private string[,] Grid;
        private int Rows;
        private int Cols;

        public World(string[,] grid)
        {
            Rows = grid.GetLength(0);
            Cols = grid.GetLength(1);
            Grid = grid;
        }

        public void Draw()
        {
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Cols; x++)
                {
                    string element = Grid[y, x];
                    SetCursorPosition(x, y);
                    Write(element);
                }
            }
        }
        public string GetElementAt(int x, int y)
        {
            return Grid[y, x];
        }   

        public bool IsWalkable(int x, int y)
        {
            //check bounds first.
            if (x < 0 || x >= Cols || y < 0 || y >= Rows)
            {  
                return false; 
            }

            // Check if the position is walkable (not a wall)
            return Grid[y, x] == " " || Grid[y, x] == "X";
        }


    }
}
