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

        // TODO: Move this into world to be handled
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
            this.camera = new Camera(new Vector3(0, -70, -50), new Vector3(0, 0, 0));
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
            Shape shape = new BoxShape(1.0f, 2.0f, 3.0f);
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Content.RootDirectory = "Content";
            Model characterModel = Content.Load<Model>("Models/dude"); //Loads your new model, point it towards your model
            GameObject randomPlayer = new GameObject(
                characterModel,
                shape,
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
            );
            randomPlayer.PlayAnimation("Take 001");
            Model cubeModel = Content.Load<Model>("Models/cube"); //Loads your new model, point it towards your model
            // Create Character
            Character character = new Character(
                characterModel,
                shape,
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
            );
            character.PlayAnimation("Take 001");
            // Create Our Block Templates
            GameObject nonPathBlock = new GameObject(
                cubeModel,
                shape,
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 0),
                new Vector3(10, 10, 10)
            );
            Model pathModel = Content.Load<Model>("Models/path"); //Loads your new model, point it towards your model
            GameObject pathBlock = new GameObject(
                pathModel,
                shape,
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 0),
                new Vector3(10, 10, 10)
            );
            // Load Our SpriteFont
            SpriteFont spriteFont = Content.Load<SpriteFont>("Arial");
            // Create Our World
            this.world = new WorldHandler(new CollisionSystemSAP(), nonPathBlock, pathBlock, spriteFont);
            world.addGameObject(randomPlayer);
            world.addGameObject(character);
            this.world.advanceTurn();
            this.world.advanceTurn();
            this.world.advanceTurn();
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
            if (this.inputHandler.isExitDown)
            {
                this.Exit();
            }
            // TODO: Move this into world to be handled
            this.camera.Update(gameTime, inputHandler);
            //this.character.Update(gameTime);
            // Update World
            this.world.Update(gameTime, inputHandler);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            this.world.Draw(camera, renderer, spriteBatch);

            // TODO: Add your drawing code here
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
