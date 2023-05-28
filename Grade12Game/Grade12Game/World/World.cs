using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using AStarSharp;

namespace Grade12Game.World
{
    // Cell Struct
    struct Cell
    {
        public Stack<Node> path;
        public int[,] cellGrid;
        public Cell(Stack<Node> path, int[,] cellGrid)
        {
            this.path = path;
            this.cellGrid = cellGrid;
        }
    }
    // World Class
    class World
    {
        // This Class Holds All Of The Data For Generating And Handling Our World
        // Our world is split up into square chunks and will generate in a swirl
        // #####
        // ....#
        // ###.#
        // #...#
        // #####
        // Generating A Cell
        // To Generate A Cell We have a set input tile and output side,
        // we choose a random position on the output side so we have a start and end position
        // Then we will generate random obstacles in our matrix and ind a path through them, if there is no path we will regenerate the obstacles and try again.
        // Contants
        private const int cellSize = 12;
        private const int obstacleCount = 10; // The higher this is the more likely we are to fail but the better our path should be.
        public enum CellSide
        {
            YMinus, // Up
            YPlus, // Down
            XMinus, // Left
            XPlus // Right
        };
        // Properties
        private Random rand;
        private List<Cell> world;
        private int turnsTillNextCell;
        // World Construtor
        public World()
        {
            // Set Our Props
            rand = new Random();
            world = new List<Cell>();
            turnsTillNextCell = 0;
        }
        // CallNextTurn
        public void startTurn()
        {
            if (turnsTillNextCell <= 0)
            {
                //Cell cell = genCell();
                //world.Add(cell);
            }
            // Decrement turn Count
            turnsTillNextCell--;
        }
        // Convert Cell Dir to int
        private Vector2 makeExitCord(CellSide side, int cellIndex)
        {
            switch (side)
            {
                case CellSide.YMinus:
                    return new Vector2(cellIndex, 0);
                case CellSide.YPlus:
                    return new Vector2(cellIndex, cellSize-1);
                case CellSide.XMinus:
                    return new Vector2(0, cellIndex);
                case CellSide.XPlus:
                    return new Vector2(cellSize-1, cellIndex);
                default:
                    // This code path is just here to satisfy type checker, will never be hit
                    return new Vector2(-1, -1);
            }
        }
        // Generate Cell
        private Cell genCell(Vector2 startPosition, CellSide exitSide)
        {
            int[,] cellGrid = new int[cellSize, cellSize];
            Stack<Node> path;
            while (true)
            {
                // Createte The Array We Are Storing This In
                List<List<Node>> cellList = new List<List<Node>>();

                for (int i = 0; i < cellSize; i++)
                {
                    cellList.Add(new List<Node>());
                    for (int j = 0; j < cellSize; j++)
                    {
                        cellList[i].Add(new Node(new Vector2(i, j), true));
                        cellGrid[i, j] = 1;
                    }
                }
                // Choose A Random Position (Do Not Allow Corner Cells)
                int endIndex = rand.Next(1, cellSize); // Should output a value of 1 to 11
                Vector2 endPosition = makeExitCord(exitSide, endIndex);
                // Generate Random Obstacles
                for (int i = 0; i < obstacleCount; i++)
                {
                    // Choose A Random Position (Do Not Allow Side Cells, This should prevent it from appearing on start or end pos)
                    int obstacleX = rand.Next(1, cellSize);
                    int obstacleY = rand.Next(1, cellSize);
                    cellList[obstacleX][obstacleY].Walkable = false;
                }
                // Solve Our PathFinding
                Astar finder = new Astar(cellList);
                path = finder.FindPath(startPosition, endPosition);
                if (path == null) break;
                for (int i = 0; i < path.Count; i++)
                {
                    Node node = path.Pop();
                    cellGrid[(int)node.Position.X, (int)node.Position.Y] = 0;
                }
            }
            // Return The cellGrid
            return new Cell(path, cellGrid);
        }
    }
}
