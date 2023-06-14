using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Jitter;
using Jitter.Collision;
using Jitter.Collision.Shapes;
using Jitter.DataStructures;
using Jitter.Dynamics;
using Jitter.LinearMath;

namespace Grade12Game
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Renderer renderer;
        float farPlaneDistance = 800f; //Defines your plane distance, the higher, the less 3D 'fog' and the more is displayed

        InputHandler inputHandler;

        Camera camera;
        WorldHandler world;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Initialization Logic
            this.renderer = new Renderer(
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height,
                farPlaneDistance
            );
            this.camera = new Camera(new Vector3(0, 70, -50), new Vector3(0));
            this.inputHandler = new InputHandler(PlayerIndex.One);
            // Physics Testing
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Content.RootDirectory = "Content";
            Shape characterShape = new BoxShape(1.0f, 20.0f, 3.0f);
            Model characterModel = Content.Load<Model>("Models/dude"); //Loads your new model, point it towards your model
            GameObject randomPlayer = new GameObject(
                characterModel,
                characterShape,
                new Vector3(0, 90, 0),
                new Vector3(0),
                new Vector3(1)
            );
            randomPlayer.PlayAnimation("Take 001");
            // Load Our Tower Bases
            Shape smallTowerShape = new BoxShape(15.0f, 15.0f, 15.0f);
            Model smallTowerModel = Content.Load<Model>("Models/Turrets/smallTurret");
            Shape mediumTowerShape = new BoxShape(15.0f, 15.0f, 15.0f);
            Model mediumTowerModel = Content.Load<Model>("Models/Turrets/mediumTurret");
            Shape largeTowerShape = new BoxShape(15.0f, 15.0f, 15.0f);
            Model largeTowerModel = Content.Load<Model>("Models/Turrets/largeTurret");
            Vector3 rot = new Vector3(0, -MathHelper.PiOver2, 0);
            IGameObject[] turretTypes = new IGameObject[]
            {
                 new SmallTurret(
                    smallTowerModel,
                    smallTowerShape,
                    new Vector3(0),
                    rot,
                    new Vector3(5)
                ),
                new MediumTurret(
                    mediumTowerModel,
                    mediumTowerShape,
                    new Vector3(0),
                    rot,
                    new Vector3(5)
                ),
                new LargeTurret(
                    largeTowerModel,
                    largeTowerShape,
                    new Vector3(0),
                    rot,
                    new Vector3(5)
                )
            };
            // Load Our Projectile
            Shape projectileShape = new BoxShape(2.0f, 2.0f, 2.0f);
            Model projectileModel = Content.Load<Model>("Models/Projectile");
            Projectile projectile = new Projectile(
                projectileModel,
                projectileShape,
                new Vector3(0),
                new Vector3(0),
                new Vector3(2),
                100,
                0,
                10000000
            );
            projectile.AffectedByGravity = false;
            // Create Our Block Templates
            Model nonPathModel = Content.Load<Model>("Models/cube"); //Loads your new model, point it towards your model
            Shape pathShape = new BoxShape(20.0f, 20.0f, 20.0f);
            GameObject nonPathBlock = new GameObject(
                nonPathModel,
                pathShape,
                new Vector3(0),
                new Vector3(0),
                new Vector3(20)
            );
            Model pathModel = Content.Load<Model>("Models/path"); //Loads your new model, point it towards your model
            GameObject pathBlock = new GameObject(
                pathModel,
                pathShape,
                new Vector3(0),
                new Vector3(0),
                new Vector3(20)
            );
            // Create Enemies
            Shape smallEnemyShape = new BoxShape(1.0f, 20.0f, 3.0f);
            Model smallEnemyModel = Content.Load<Model>("Models/dude");
            EnemyType[] enemyTypes = new EnemyType[] {
                new EnemyType(smallEnemyModel, smallEnemyShape, new Vector3(0), new Vector3(1), 10, 100, 1)
            };
            // Load Our SpriteFont
            SpriteFont spriteFont = Content.Load<SpriteFont>("Arial");
            // Create Our World
            this.world = new WorldHandler(
                camera,
                new CollisionSystemSAP(),
                // Templates
                nonPathBlock,
                pathBlock,
                spriteFont,
                turretTypes,
                projectile,
                enemyTypes
            );
            //world.addGameObject(randomPlayer);
            this.world.advanceTurn();
            this.world.advanceTurn();
            this.world.advanceTurn();
            //this.world.advanceTurn();
            world.startWave();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Update Inputs
            this.inputHandler.Update(gameTime);
            // Handle Exit
            if (this.inputHandler.isExitDown) this.Exit();
            // Update World
            this.world.Update(gameTime, inputHandler);
            // Write Old State
            this.inputHandler.writeOldState();
            // Xna Update
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Clear Screen
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            // When drawing 3d, if we want to draw textt we need to use spriteBatch.Begin
            // spriteBatch.Begin messes witth our drawing states, we need to sett these to make sure things render properly
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // Draw World
            this.world.Draw(renderer, spriteBatch);
            spriteBatch.End();
            // XNA Draw
            base.Draw(gameTime);
        }
    }
}
