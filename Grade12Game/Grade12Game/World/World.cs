using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using AStarSharp;
using Jitter;
using Jitter.Collision;
using Jitter.Collision.Shapes;
using Jitter.DataStructures;
using Jitter.Dynamics;
using Jitter.LinearMath;

namespace Grade12Game
{
    // Cell Struct
    struct Cell
    {
        public readonly Stack<Node> path;
        public readonly int[,] cellGrid;
        public readonly IGameObject[,] cellObjects;
        public readonly int cellX;
        public readonly int cellY;

        public Cell(
            Stack<Node> path,
            int[,] cellGrid,
            IGameObject[,] cellObjects,
            int cellX,
            int cellY
        )
        {
            this.path = path;
            this.cellGrid = cellGrid;
            this.cellObjects = cellObjects;
            this.cellX = cellX;
            this.cellY = cellY;
        }
    }

    // World Class
    public class WorldHandler : World
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
        // Constants
        private const int cellSize = 11;
        public const int cellNodeSize = 10;
        private const int obstacleCount = 30; // The higher this is the more likely we are to fail but the better our path should be.
        private const int textYSize = 16;

        // TODO: I think the method we are using for sides has issues and makes things overcomplicated

        public enum CellSide
        {
            YMinus, // Up
            XPlus, // Right
            YPlus, // Down
            XMinus, // Left
        };

        // Properties
        private CollisionSystem collision;
        private List<IGameObject> gameObjects;
        private Random rand;
        private List<Cell> world;
        private int turnsTillNextCell;
        private CellSide lastSide;
        private CellSide nextSide;
        private int cellEndIndex;
        private int nextCellX;
        private int nextCellY;
        private int currentStep = 0;
        private SpriteFont spriteFont;

        // Settings
        private bool debug;

        // TODO: Refine how we store this
        private GameObject nonPathCell;
        private GameObject pathCell;

        // World Constructor
        public WorldHandler(
            CollisionSystem collision,
            GameObject nonPathCell,
            GameObject pathCell,
            SpriteFont spriteFont
        )
            : base(collision)
        {
            // Set Our Props
            this.collision = collision;
            gameObjects = new List<IGameObject>();
            rand = new Random();
            world = new List<Cell>();
            turnsTillNextCell = 0;
            lastSide = CellSide.XMinus;
            nextSide = getNextCellSide(lastSide);
            cellEndIndex = cellSize / 2;
            debug = true;
            nextCellX = 0;
            nextCellY = 0;
            this.spriteFont = spriteFont;
            this.nonPathCell = nonPathCell;
            this.pathCell = pathCell;
        }

        // Get Next Cell Side
        private CellSide getNextCellSide(CellSide cellSide)
        {
            currentStep++;
            // TODO: Handle Turning
            // Corner Behavior
            switch (cellSide)
            {
                case CellSide.YMinus:
                    return CellSide.XPlus;
                case CellSide.XPlus:
                    return CellSide.YPlus;
                case CellSide.YPlus:
                    return CellSide.XMinus;
                case CellSide.XMinus:
                    return CellSide.YMinus;
            }
            // We should never hit here
            throw new Exception("Impossible Error");
        }

        // Start Turn
        public void advanceTurn()
        {
            if (turnsTillNextCell <= 0)
            {
                // Create Start Position
                Vector2 startPosition;
                switch (lastSide)
                {
                    case CellSide.YMinus:
                        startPosition = new Vector2(cellEndIndex, cellSize - 1);
                        break;
                    case CellSide.XPlus:
                        startPosition = new Vector2(0, cellEndIndex);
                        break;
                    case CellSide.YPlus:
                        startPosition = new Vector2(cellEndIndex, 0);
                        break;
                    case CellSide.XMinus:
                        startPosition = new Vector2(cellSize - 1, cellEndIndex);
                        break;
                    default:
                        throw new Exception("Impossible Error");
                }
                // Generate The Cell
                Cell cell = genCell(startPosition, nextSide, nextCellX, nextCellY);
                this.addWorldCell(cell);
                // Increment The Next Cell Position
                switch (nextSide)
                {
                    case CellSide.YMinus:
                        nextCellY--;
                        break;
                    case CellSide.XPlus:
                        nextCellX++;
                        break;
                    case CellSide.YPlus:
                        nextCellY++;
                        break;
                    case CellSide.XMinus:
                        nextCellX--;
                        break;
                    default:
                        throw new Exception("Impossible Error");
                }
                // Setup for next
                lastSide = nextSide;
                nextSide = getNextCellSide(nextSide);
                turnsTillNextCell = 0;
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
                case CellSide.XPlus:
                    return new Vector2(cellSize - 1, cellIndex);
                case CellSide.YPlus:
                    return new Vector2(cellIndex, cellSize - 1);
                case CellSide.XMinus:
                    return new Vector2(0, cellIndex);
                default:
                    // This code path is just here to satisfy type checker, will never be hit
                    return new Vector2(-1, -1);
            }
        }

        // Generate Cell
        private Cell genCell(Vector2 startPosition, CellSide exitSide, int cellX, int cellY)
        {
            int[,] cellGrid = new int[cellSize, cellSize];
            Stack<Node> path;
            do
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
                int endIndex = rand.Next(1, cellSize / 2);
                switch (lastSide)
                {
                    case CellSide.YMinus:
                    case CellSide.XMinus:
                        endIndex += cellSize / 2;
                        break;
                }
                endIndex = 1;
                cellEndIndex = endIndex;
                Vector2 endPosition = makeExitCord(exitSide, endIndex);
                // Add Obstacles in each corner
                cellList[0][0].Walkable = false;
                cellList[0][cellSize - 1].Walkable = false;
                cellList[cellSize - 1][0].Walkable = false;
                cellList[cellSize - 1][cellSize - 1].Walkable = false;
                // Generate Random Obstacles
                for (int i = 0; i < obstacleCount; i++)
                {
                    // Choose A Random Position
                    int obstacleX = rand.Next(0, cellSize);
                    int obstacleY = rand.Next(0, cellSize);
                    if (
                        (obstacleX == startPosition.X && obstacleY == startPosition.Y)
                        || (obstacleX == endPosition.X && obstacleY == endPosition.Y)
                    )
                    {
                        i--;
                        continue;
                    }
                    cellList[obstacleX][obstacleY].Walkable = false;
                    cellGrid[obstacleX, obstacleY] = 2;
                }
                // Solve Our PathFinding
                Astar finder = new Astar(cellList);
                path = finder.FindPath(startPosition, endPosition);
                // TODO: Ensure We have reached the end
                if (path == null)
                    continue;
                // Write to cell
                int pathCount = path.Count;
                for (int i = 0; i < pathCount; i++)
                {
                    Node node = path.Pop();
                    cellGrid[(int)node.Position.X, (int)node.Position.Y] = 0;
                }
                // Clear start Pos
                cellGrid[(int)startPosition.X, (int)startPosition.Y] = 3;
                cellGrid[(int)endPosition.X, (int)endPosition.Y] = 4;
            } while (path == null);
            // Build Our Cell GameObjects
            IGameObject[,] cellGameObjects = new IGameObject[cellSize, cellSize];
            int cellWorldX = cellX * cellSize * cellNodeSize;
            int cellWorldY = cellY * cellSize * cellNodeSize;
            for (int x = 0; x < cellSize; x++)
            {
                for (int y = 0; y < cellSize; y++)
                {
                    // Create The GameObject
                    IGameObject gameObject;
                    switch (cellGrid[x, y])
                    {
                        case 0:
                            gameObject = pathCell.Clone();
                            break;
                        case 1:
                            gameObject = nonPathCell.Clone();
                            break;
                        case 2:
                            gameObject = nonPathCell.Clone();
                            break;
                        case 3:
                            gameObject = pathCell.Clone();
                            break;
                        case 4:
                            gameObject = pathCell.Clone();
                            break;
                        default:
                            throw new Exception("Unknown Cell State");
                    }
                    // Set Position
                    int localX = cellWorldX + x * cellNodeSize;
                    int localY = cellWorldY + y * cellNodeSize;
                    gameObject.setPosition(new Vector3(localX, -5, localY));
                    // Set Props
                    // Add The GameObject
                    cellGameObjects[x, y] = gameObject;
                }
            }
            // Return The cellGrid
            return new Cell(path, cellGrid, cellGameObjects, cellX, cellY);
        }

        // Add GameObject
        public void addGameObject(IGameObject gameObject)
        {
            // Add GameObject to gameObject List
            gameObjects.Add(gameObject);
            // Add RigidBody To Collision World
            this.AddBody((RigidBody)gameObject);
        }

        // Remove GameObject
        public void removeGameObject(IGameObject gameObject)
        {
            // Remove GameObject from gameObject List
            gameObjects.Remove(gameObject);
            // Remove RigidBody from Collision World
            this.RemoveBody((RigidBody)gameObject);
        }

        // Add Cell
        private void addWorldCell(Cell cell)
        {
            // Add The Cell
            world.Add(cell);
            // Add The Cell To The Collision World
            for (int x = 0; x < cellSize; x++)
            {
                for (int y = 0; y < cellSize; y++)
                {
                    // Get The GameObject
                    IGameObject gameObject = cell.cellObjects[x, y];
                    // Add The GameObject
                    RigidBody r = (RigidBody)gameObject;
                    r.AffectedByGravity = false;
                    r.IsStatic = true;
                    this.AddBody(r);
                }
            }
        }

        // Get World Path
        public Stack<JVector> getWorldPath()
        {
            Stack<JVector> Path = new Stack<JVector>();
            // Loop Through Cells
            foreach (Cell cell in this.world)
            {
                // TODO: Loop Through Path
            }
            // Return The Path
        }

        // Update World
        public void Update(GameTime gameTime, InputHandler input)
        {
            // TODO: Perform Wave Actions
            foreach (IGameObject obj in gameObjects)
            {
                obj.Update(gameTime, this, input);
            }
            // Update Physics
            float step = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (step > 1.0f / 100.0f)
                step = 1.0f / 100.0f;
            this.Step(step * 10, true);
        }

        // Toggle Debug
        public void toggleDebug()
        {
            this.debug = !debug;
        }

        // Draw World
        public void Draw(Camera cam, Renderer renderer, SpriteBatch spriteBatch)
        {
            // Render Debug Info
            if (this.debug)
            {
                int currentY = 0;
                spriteBatch.DrawString(
                    spriteFont,
                    "Garbage Collection:",
                    new Vector2(0, currentY),
                    Color.White
                );
                spriteBatch.DrawString(
                    spriteFont,
                    "gen0: " + GC.CollectionCount(0).ToString(),
                    new Vector2(0, currentY += textYSize),
                    Color.White
                );
                spriteBatch.DrawString(
                    spriteFont,
                    "gen1: " + GC.CollectionCount(1).ToString(),
                    new Vector2(0, currentY += textYSize),
                    Color.White
                );
                spriteBatch.DrawString(
                    spriteFont,
                    "gen2: " + GC.CollectionCount(2).ToString(),
                    new Vector2(0, currentY += textYSize),
                    Color.White
                );
                spriteBatch.DrawString(
                    spriteFont,
                    "Physics:",
                    new Vector2(0, currentY += textYSize),
                    Color.White
                );
                double total = 0;

                for (int i = 0; i < (int)Jitter.World.DebugType.Num; i++)
                {
                    total += this.DebugTimes[i];
                }
                spriteBatch.DrawString(
                    spriteFont,
                    "RigidBodys: " + this.RigidBodies.Count.ToString(),
                    new Vector2(0, currentY += textYSize),
                    Color.White
                );
                spriteBatch.DrawString(
                    spriteFont,
                    "TotalTime: " + total.ToString("0.00"),
                    new Vector2(0, currentY += textYSize),
                    Color.White
                );
                spriteBatch.DrawString(
                    spriteFont,
                    "FrameRate: " + (1000.0d / total).ToString("0"),
                    new Vector2(0, currentY += textYSize),
                    Color.White
                );
            }
            // Render The Cells In The World
            foreach (Cell cell in this.world)
            {
                // Calculate The Cell World Position
                int cellWorldX = cell.cellX * cellSize * cellNodeSize;
                int cellWorldY = cell.cellY * cellSize * cellNodeSize;
                // Render Each Cell Node
                int[,] cellGrid = cell.cellGrid;
                IGameObject[,] cellGameObjects = cell.cellObjects;
                for (int x = 0; x < cellSize; x++)
                {
                    for (int y = 0; y < cellSize; y++)
                    {
                        cellGameObjects[x, y].Draw(cam, renderer);
                    }
                }
            }
            // Render Wave Objects
            foreach (IGameObject obj in gameObjects)
            {
                obj.Draw(cam, renderer);
            }
        }
    }
}
