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
        public readonly List<Vector3> path;
        public readonly int[,] cellGrid;
        public readonly IGameObject[,] cellObjects;
        public readonly int cellX;
        public readonly int cellY;

        public Cell(
            List<Vector3> path,
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
    class WorldHandler : World
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
        public const int cellNodeSize = 20;
        private const int obstacleCount = 30; // The higher this is the more likely we are to fail but the better our path should be.
        private const int textYSize = 16;

        //  I think the method we are using for sides has issues and makes things overcomplicated

        public enum CellSide
        {
            YMinus, // Up
            XPlus, // Right
            YPlus, // Down
            XMinus, // Left
        };

        // Properties
        public readonly Random rand;
        private float difficulty;
        private int level;
        // TODO: set this to private
        public Camera player;
        private CollisionSystem collision;
        private List<IGameObject> gameObjects;
        private List<Projectile> freeProjectiles;
        private List<Cell> world;
        private int turnsTillNextCell;
        private CellSide lastSide;
        private CellSide nextSide;
        private int cellEndIndex;
        private int nextCellX;
        private int nextCellY;
        private int currentStep = 0;
        private SpriteFont spriteFont;
        private IGameObject[] towerTemplates;
        private EnemyType[] enemyTemplates;

        public readonly Projectile projectile;

        // Money and stats
        private int money = 195;
        private int baseHealth = 100;
        private bool canStartWave = true;
        // Settings
        private bool debug;

        // TODO: Refine how we store this
        private GameObject nonPathCell;
        private GameObject pathCell;

        // World Constructor
        public WorldHandler(
            Camera player,
            CollisionSystem collision,
            GameObject nonPathCell,
            GameObject pathCell,
            SpriteFont spriteFont,
            // TODO: Remap This To Towers from GameObject
            IGameObject[] towerTemplates,
            Projectile projectile,
            EnemyType[] enemyTemplates
        )
            : base(collision)
        {
            // Set Our Props
            this.player = player;
            this.difficulty = 0;
            this.level = 0;
            this.collision = collision;
            gameObjects = new List<IGameObject>();
            freeProjectiles = new List<Projectile>();
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
            this.towerTemplates = towerTemplates;
            this.projectile = projectile;
            this.enemyTemplates = enemyTemplates;
            // Make Main platform
            this.advanceTurn();
        }
        // TODO: Rewrite the entire world gen bassically, relating to what side to spawn on

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
            List<Vector3> cellPath = new List<Vector3>();
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
                Stack<Node> path = finder.FindPath(startPosition, endPosition);
                // TODO: Ensure We have reached the end
                if (path == null)
                    continue;
                // Write to cell
                int pathCount = path.Count;
                for (int i = 0; i < pathCount; i++)
                {
                    Node node = path.Pop();
                    // Calculate Node Positon In WOrld
                    float nodeWorldX = cellX * cellSize * cellNodeSize + node.Position.X * cellNodeSize;
                    float nodeWorldY = cellY * cellSize * cellNodeSize + node.Position.Y * cellNodeSize;
                    Vector3 pos = new Vector3(
                        nodeWorldX + cellNodeSize / 2,
                        0,
                        nodeWorldY+cellNodeSize/2
                    );
                    // Write Position
                    cellPath.Add(pos);
                    cellGrid[(int)node.Position.X, (int)node.Position.Y] = 0;
                }
                // Clear start Pos
                cellGrid[(int)startPosition.X, (int)startPosition.Y] = 3;
                cellGrid[(int)endPosition.X, (int)endPosition.Y] = 4;
            } while (cellPath.Count == 0);
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
                    if (cellGrid[x, y] == 3)
                    {
                        Vector3 pos = gameObject.getPosition();
                        pos.Y = 0;
                        gameObject.setPosition(pos);
                    }
                    // Add The GameObject
                    cellGameObjects[x, y] = gameObject;
                }
            }
            // Return The cellGrid
            return new Cell(cellPath, cellGrid, cellGameObjects, cellX, cellY);
        }

        // getGameObjects
        public List<IGameObject> getGameObjects() {
            return this.gameObjects;
        }

        public bool hasGameObject(IGameObject gameObject)
        {
            return this.gameObjects.Contains(gameObject);
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

        public List<Enemy> getEnemies()
        {
            List<Enemy> e = new List<Enemy>();
            foreach (IGameObject enemy in this.gameObjects)
            {
                if (enemy is Enemy)
                {
                    e.Add((Enemy)enemy);
                }
            }
            return e;
        }
        // Handle Projectile
        public Projectile spawnProjetile()
        {
            // Check for free Projectiles
            if (freeProjectiles.Count > 0)
            {
                // Dequeue
                Projectile proj = freeProjectiles[0];
                freeProjectiles.RemoveAt(0);
                // Add To Scene
                gameObjects.Add(proj);
                // Return
                return proj;
            }
            else
            {
                Projectile proj = (Projectile)projectile.Clone();
                // Add To World
                this.addGameObject(proj);
                // Return
                return proj;
            }
        }
        public void removeProjectile(Projectile gameObject)
        {
            // Remove From gameObjects, Note: We do not remove from physics scene
            gameObjects.Remove(gameObject);
            // Add To freeProjectiles
            this.freeProjectiles.Add(gameObject);
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
        public Stack<Vector3> getWorldPath()
        {
            Stack<Vector3> Path = new Stack<Vector3>();
            // Loop Through Cells
            //for (int i = this.world.Count-1; i >= 0; i--)
            //{
            //    foreach (Vector3 n in this.world[i].path)
            //    {
            //        Path.Push(n);
            //    }
            //}
            foreach (Cell cell in this.world)
            {
                foreach (Vector3 n in cell.path)
                {
                    Path.Push(n);
                }
            }
            // Return The Path
            return Path;
        }

        public Stack<T> cloneStack<T>(Stack<T> original)
        {
            T[] arr = new T[original.Count];
            original.CopyTo(arr, 0);
            Array.Reverse(arr);
            return new Stack<T>(arr);
        }

        // Start Wave
        public void startWave()
        {
            this.advanceTurn();
            // Update Level and difficulty
            this.level++;
            // TODO: Change how we calculate difficulty
            this.difficulty++;
            if (this.difficulty % 5 == 0) this.difficulty += 5;
            // Spawn New World Item
            Stack<Vector3> currentWorldPath = getWorldPath();

            // TODO: Create Enemys
            float waveDifficulty = this.difficulty;
            int stepsUntilSpawn = 16;
            while (waveDifficulty > 0) {
                // Filter Spawning
                EnemyType[] validEnemyTypes = enemyTemplates.Where(enemyType => enemyType.difficulty <= waveDifficulty).ToArray();
                // Do Not Get Stuck In Infinite Loop If Somehow There Are No Valid Enemies
                if (validEnemyTypes.Length == 0) break;
                // Choose To Spawn A Random Enemy
                EnemyType choosenEnemy = validEnemyTypes[this.rand.Next(validEnemyTypes.Length)];
                // Spawn Enemy
                Enemy enemy = new Enemy(
                    choosenEnemy.model,
                    choosenEnemy.shape,
                    choosenEnemy.spawnPosition,
                    choosenEnemy.rotation,
                    choosenEnemy.scale,
                    stepsUntilSpawn,
                    cloneStack(currentWorldPath),
                    cloneStack(currentWorldPath),
                    choosenEnemy.speed,
                    choosenEnemy.health * (int)this.difficulty / 2,
                    choosenEnemy.moneyDrop,
                    choosenEnemy.isMonsterMatrix
                );
                enemy.AffectedByGravity = false;
                enemy.setIsActive(false);
                // Add Enemy To World
                this.addGameObject(enemy);
                // Remove Difficulty
                waveDifficulty -= choosenEnemy.difficulty;
                // Consider More Steps Until Spawn
                stepsUntilSpawn += 30;
            }
        }

        public void doBaseDamage(int damage)
        {
            this.baseHealth -= damage;
            if (this.baseHealth < 0) this.baseHealth = 0;
        }

        public int getBaseHealth()
        {
            return this.baseHealth;
        }

        public void takeMoney(int money)
        {
            this.money -= money;
            if (this.money < 0) this.money = 0;
        }
        public int getMoney()
        {
            return this.money;
        }
        public void addMoney(int money)
        {
            this.money += money;
        }

        // Update World
        public void Update(GameTime gameTime, InputHandler input)
        {
            // Update Player
            player.Update(gameTime, input);
            // Spawn Turret
            if (input.isNumber1KeyPressed)
            {
                if (this.getMoney() >= 100)
                {
                    this.takeMoney(100);
                    IGameObject tower = towerTemplates[0].Clone();
                    tower.setPosition(player.getPosition());
                    this.addGameObject(tower);
                }
            }
            if (input.isNumber2KeyPressed)
            {
                if (this.getMoney() >= 200)
                {
                    this.takeMoney(200);
                    IGameObject tower = towerTemplates[1].Clone();
                    tower.setPosition(player.getPosition());
                    this.addGameObject(tower);
                }
            }
            if (input.isNumber3KeyPressed)
            {
                if (this.getMoney() >= 300)
                {
                    this.takeMoney(300);
                    IGameObject tower = towerTemplates[2].Clone();
                    tower.setPosition(player.getPosition());
                    this.addGameObject(tower);
                }
            }
            // Call GameObject Updates
            List<IGameObject> tempGameOjects = new List<IGameObject>(gameObjects);
            foreach (IGameObject obj in tempGameOjects)
            {
                obj.Update(gameTime, this, input);
            }
            // Map To KeyBind
            canStartWave = getEnemies().Count == 0;
            if (canStartWave && input.startWave) this.startWave();
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
        public void Draw(Renderer renderer, SpriteBatch spriteBatch)
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
                spriteBatch.DrawString(
                    spriteFont,
                    "Cam Info:",
                    new Vector2(0, currentY += textYSize),
                    Color.White
                );
                spriteBatch.DrawString(
                    spriteFont,
                    "Position: " + player.getPosition().ToString(),
                    new Vector2(0, currentY += textYSize),
                    Color.White
                );
                spriteBatch.DrawString(
                    spriteFont,
                    "Rotation: " + player.getRotation().ToString(),
                    new Vector2(0, currentY += textYSize),
                    Color.White
                );
                spriteBatch.DrawString(
                    spriteFont,
                    "World Stats: ",
                    new Vector2(0, currentY += textYSize),
                    Color.White
                );
                spriteBatch.DrawString(
                    spriteFont,
                    "Money: " + this.getMoney(),
                    new Vector2(0, currentY += textYSize),
                    Color.White
                );
                spriteBatch.DrawString(
                    spriteFont,
                    "baseHealth: " + this.getBaseHealth(),
                    new Vector2(0, currentY += textYSize),
                    Color.White
                );
                int avgEnemyHealth = 0;
                int eCount = 0;
                foreach (Enemy e in this.getEnemies())
                {
                    avgEnemyHealth += e.getHealth();
                    eCount++;
                }
                if (eCount != 0) avgEnemyHealth /= eCount;
                spriteBatch.DrawString(
                    spriteFont,
                    "Avg Enemy Health: " + avgEnemyHealth,
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
                        cellGameObjects[x, y].Draw(player, renderer);
                    }
                }
            }
            // Render Wave Objects
            foreach (IGameObject obj in gameObjects)
            {
                if (obj.getIsActive())
                    obj.Draw(player, renderer);
            }
        }
    }
}
