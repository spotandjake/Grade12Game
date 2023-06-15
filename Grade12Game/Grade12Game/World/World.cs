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
    // Scene
    enum Scene
    {
        MainMenu,
        GamePlay,
        Dead,
    }
    // Cell Struct
    struct Cell
    {
        public readonly List<Vector3> path;
        public readonly List<IGameObject> cellObjects;
        public readonly int cellX;
        public readonly int cellY;

        public Cell(
            List<Vector3> path,
            List<IGameObject> cellObjects,
            int cellX,
            int cellY
        )
        {
            this.path = path;
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
        private const int mapXLength = 3;

        // Properties
        private Scene scene;
        public readonly Random rand;
        public readonly MidiPlayer soundManager;
        private float difficulty;
        private int level;
        // TODO: set this to private
        public Camera player;
        private CollisionSystem collision;
        private GameObject baseObject;
        private List<IGameObject> gameObjects;
        private List<Projectile> freeProjectiles;
        private List<Cell> world;
        private int cellEndIndex;
        private SpriteFont spriteFont;
        private IGameObject[] towerTemplates;
        private EnemyType[] enemyTemplates;
        private int cellX;
        private int cellY;
        private bool xPlus = true;
        private bool turnLastGen = false;

        public readonly Projectile projectile;

        // Money and stats
        private int money = 195;
        private int baseHealth = 100;
        private bool canStartWave = true;
        private bool autoPlay = false;
        private int turnsUntilNextCell;
        private int currentWave;
        // Settings
        private bool debug;

        // TODO: Refine how we store this
        private GameObject nonPathCell;
        private GameObject pathCell;

        // World Constructor
        public WorldHandler(
            Camera player,
            CollisionSystem collision,
            MidiPlayer soundManager,
            GameObject baseObject,
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
            turnsUntilNextCell = 0;
            this.player = player;
            this.difficulty = 0;
            this.level = 0;
            this.collision = collision;
            this.soundManager = soundManager;
            this.baseObject = baseObject;
            gameObjects = new List<IGameObject>();
            freeProjectiles = new List<Projectile>();
            rand = new Random();
            world = new List<Cell>();
            cellEndIndex = cellSize / 2;
            debug = false;
            autoPlay = false;
            this.spriteFont = spriteFont;
            this.nonPathCell = nonPathCell;
            this.pathCell = pathCell;
            this.towerTemplates = towerTemplates;
            this.projectile = projectile;
            this.enemyTemplates = enemyTemplates;
            // Make Main platform
            this.cellX = 0;
            this.cellY = 0;
            this.xPlus = true;
            turnLastGen = true;
            this.generateCell();

            this.setScene(Scene.MainMenu);
        }
        public void generateCell()
        {
            // We Generate Towards xPlus
            List<IGameObject> cellGameObjects = new List<IGameObject>();
            int cellWorldX = cellX * cellSize * cellNodeSize;
            int cellWorldY = cellY * cellSize * cellNodeSize;
            IGameObject cellBlock = nonPathCell.Clone();
            cellBlock.setPosition(new Vector3(cellWorldX, -5, cellWorldY));
            cellGameObjects.Add(cellBlock);
            // Get Start Coords
            Vector2 startPosition;
            if (cellX == 0 && cellY == 0)
            {
                startPosition = new Vector2(cellEndIndex / 2, cellEndIndex / 2);
                baseObject.setPosition(new Vector3(startPosition.X*cellNodeSize, 40, startPosition.Y * cellNodeSize+5));
                this.addGameObject(baseObject);
            }
            else if (turnLastGen) {
                // Turn towards Yplus
                startPosition = new Vector2(cellEndIndex, 0);
            }
            else if (turnLastGen)
            {
                startPosition = new Vector2(cellEndIndex, cellSize-1);
            }
            else if (xPlus)
            {
                startPosition = new Vector2(0, cellEndIndex);
            }
            else
            {
                startPosition = new Vector2(cellSize-1, cellEndIndex);
            }
            // Generate The Cell
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
                    }
                }
                // Choose A Random Position (Do Not Allow Corner Cells)
                cellEndIndex = rand.Next(1, cellSize);
                Vector2 endPosition;
                if (cellX == mapXLength - 1 && !turnLastGen)
                {
                    endPosition = new Vector2(cellEndIndex, cellSize - 1); ;
                }
                else if (cellX == 0 && !turnLastGen)
                {
                    endPosition = new Vector2(cellEndIndex, cellSize - 1);
                }
                else if (xPlus)
                {
                    endPosition = new Vector2(cellSize - 1, cellEndIndex);
                }
                else
                {
                    endPosition = new Vector2(0, cellEndIndex);
                }
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
                        nodeWorldY + cellNodeSize / 2
                    );
                    // Write Position
                    cellPath.Add(pos);
                    // Writte to gameObjects
                    IGameObject gameObject = pathCell.Clone();
                    // Set Position
                    gameObject.setPosition(new Vector3(nodeWorldX, -5, nodeWorldY));
                    // Add To Cell
                    cellGameObjects.Add(gameObject);
                }
                // Add Our End Start Object
                IGameObject startGameObject = pathCell.Clone();
                // Set Position
                startGameObject.setPosition(new Vector3(cellX * cellSize * cellNodeSize + startPosition.X * cellNodeSize, -5, cellY * cellSize * cellNodeSize + startPosition.Y * cellNodeSize));
                // Add To Cell
                cellGameObjects.Add(startGameObject);
                // Add Our End Game Object
                IGameObject endGameObject = pathCell.Clone();
                // Set Position
                endGameObject.setPosition(new Vector3(cellX * cellSize * cellNodeSize + endPosition.X * cellNodeSize, -5, cellY * cellSize * cellNodeSize + endPosition.Y * cellNodeSize));
                // Add To Cell
                cellGameObjects.Add(endGameObject);
            } while (cellPath.Count == 0);
            // Create Cell
            Cell cell = new Cell(cellPath, cellGameObjects, cellX, cellY);
            // Add To Cell List
            this.addWorldCell(cell);
            // Incr cellX and y
            turnLastGen = false;
            if (xPlus) cellX++;
            else cellX--;
            if (cellX >= mapXLength)
            {
                cellY++;
                cellX--;
                xPlus = false;
                turnLastGen = true;
            }
            if (cellX < 0)
            {
                cellY++;
                cellX++;
                xPlus = true;
                turnLastGen = true;
            }
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
            foreach (IGameObject gameObject in cell.cellObjects)
            {
                // Add The GameObject
                RigidBody r = (RigidBody)gameObject;
                r.AffectedByGravity = false;
                r.IsStatic = true;
                this.AddBody(r);
            }
            // Add Physics Objects for every cell
            for (int i = 0; i < cellSize; i++)
            {
                for (int j = 0; j < cellSize; j++)
                {
                    // We need to add a physics object for each cell the math takes the local coordinates and converts them to world coordinates
                    float nodeWorldX = cell.cellX * cellSize * cellNodeSize + j * cellNodeSize;
                    float nodeWorldY = cell.cellY * cellSize * cellNodeSize + i * cellNodeSize;
                    // Clone a nonPathCell from template
                    IGameObject tmpNonPathCell = nonPathCell.Clone();
                    // set it to its world position
                    tmpNonPathCell.setPosition(new Vector3(nodeWorldX, -5, nodeWorldY));
                    // We need this to be a rigid body which we know it is.
                    RigidBody r = (RigidBody)tmpNonPathCell;
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
            currentWave++;
            turnsUntilNextCell++;
            if (turnsUntilNextCell >= 5)
            {
                this.generateCell();
                turnsUntilNextCell = 0;
            }
            // Update Level and difficulty
            this.level++;
            // TODO: Change how we calculate difficulty
            this.difficulty++;
            if (this.difficulty % 5 == 0) this.difficulty += 5;
            // Spawn New World Item
            Stack<Vector3> currentWorldPath = getWorldPath();

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
                    choosenEnemy.damage,
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

        public void setScene(Scene scene)
        {
            this.scene = scene;
        }
        public Scene getScene()
        {
            return this.scene;
        }

        // Update World
        public void Update(GameTime gameTime, InputHandler input)
        {
            // Update behaviour based on scene
            switch (this.getScene())
            {
                case Scene.MainMenu:
                    if (input.startGame)
                    {
                        // On space start game
                        this.setScene(Scene.GamePlay);
                    }
                    break;
                case Scene.GamePlay:
                    // debug Menu
                    if (input.clearWave)
                    {
                        // KIlls everything part of the developer tools
                        foreach (Enemy e in this.getEnemies())
                        {
                            e.DoDamage(e.getHealth());
                        }
                    }
                    if (input.toggleAutoPlay)
                    {
                        // AUto play mode where you dont need to start ROunds
                        autoPlay = !autoPlay;
                    }
                    if (input.debugMenu)
                        debug = !debug;
                    if (input.debugAdddMoney) this.addMoney(1000);
                    // Update Player
                    player.Update(gameTime, input);
                    if (input.tpBase)
                    {
                        // Tp To THe Base
                        player.setPosition(new Vector3(this.getWorldPath().Last().X, player.getPosition().Y, this.getWorldPath().Last().Z));
                    }
                    if (input.tpStart)
                    {
                        // Tp to the enemy start
                        player.setPosition(new Vector3(this.getWorldPath().First().X, player.getPosition().Y, this.getWorldPath().First().Z));
                    }
                    // Spawn Turret, / Buy Turret
                    if (input.isNumber1KeyPressed)
                    {
                        // If we have enough money
                        if (this.getMoney() >= 100)
                        {
                            this.takeMoney(100); // Take Money
                            // Clone the turret from template
                            IGameObject tower = towerTemplates[0].Clone();
                            tower.setPosition(player.getPosition());
                            // Spawn
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
                    if ((canStartWave && input.startWave) || (canStartWave && autoPlay)) this.startWave();
                    // Update Physics
                    float step = (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (step > 1.0f / 100.0f)
                        step = 1.0f / 100.0f;
                    this.Step(step * 10, true);

                    // Handle Death
                    if (this.baseHealth < 0)
                    {
                        this.setScene(Scene.Dead);
                    }
                    break;
                case Scene.Dead:
                    // Do Nothing
                    break;
            }
        }

        // Toggle Debug
        public void toggleDebug()
        {
            this.debug = !debug;
        }

        // Draw World
        public void Draw(Renderer renderer, SpriteBatch spriteBatch)
        {
            // Draw Based On Scene
            int currentY = 0;
            switch (this.getScene()) {
                case Scene.MainMenu:
                    string[] lines = new string[]
                    {
                        "Press Space To Start Game",
                        "Controls:",
                        "WASD To Move Or GamePad Left Stick",
                        "1,2,3 Or GamePad To Buy Towers",
                        "   Each Tower Costs its number MUltiplied by 100",
                        "E and Q to go up and down, No GamePad Equivlant",
                        "F on keyboard or A on GamePad to start round",
                        "Tab on keyboard to auto play",
                        "Arrow keys or right stick to look",
                    };
                    currentY = 0;
                    foreach (string line in lines)
                    {
                        Vector2 lineSize = spriteFont.MeasureString(line);
                        spriteBatch.DrawString(
                            spriteFont,
                            line,
                            new Vector2(renderer.getWidth() / 2 - (int)lineSize.X / 2, currentY),
                            Color.White
                        );
                        // Padding of 15 and the line height
                        currentY += (int)lineSize.Y + 16;
                    }
                    break;
                case Scene.GamePlay:
                    // Render Debug Info
                    if (this.debug)
                    {
                        currentY = 0;
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
                    else
                    {
                        currentY = 0;
                        spriteBatch.DrawString(
                            spriteFont,
                            "World Stats: ",
                            new Vector2(0, currentY),
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
                        spriteBatch.DrawString(
                            spriteFont,
                            "Enemy Count: " + this.getEnemies().Count,
                            new Vector2(0, currentY += textYSize),
                            Color.White
                        );
                        spriteBatch.DrawString(
                            spriteFont,
                            "Wave Active: " + (this.getEnemies().Count > 0),
                            new Vector2(0, currentY += textYSize),
                            Color.White
                        );
                        spriteBatch.DrawString(
                            spriteFont,
                            "Current Wave: " + (this.currentWave),
                            new Vector2(0, currentY += textYSize),
                            Color.White
                        );
                    }
                    // Render The Cells In The World
                    foreach (Cell cell in this.world)
                    {
                        // Render Each Cell Node
                        foreach (IGameObject gameObject in cell.cellObjects)
                        {
                            gameObject.Draw(player, renderer);
                        }
                    }
                    // Render Wave Objects
                    foreach (IGameObject obj in gameObjects)
                    {
                        if (obj.getIsActive())
                            obj.Draw(player, renderer);
                    }
                    break;
                case Scene.Dead:
                    string[] lines2 = new string[]
                    {
                        "Dead Please Restart Game To Try Again",
                        "You Made It To Wave" + this.currentWave,
                    };
                    currentY = 0;
                    foreach (string line in lines2)
                    {
                        Vector2 lineSize = spriteFont.MeasureString(line);
                        spriteBatch.DrawString(
                            spriteFont,
                            line,
                            new Vector2(renderer.getWidth() / 2 - (int)lineSize.X / 2, currentY),
                            Color.White
                        );
                        // Padding of 15 and the line height
                        currentY += (int)lineSize.Y + 16;
                    }
                    break;
            }
        }
    }
}
