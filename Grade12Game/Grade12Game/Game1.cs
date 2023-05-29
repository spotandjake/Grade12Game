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

        GameObject character;
        GameObject cube;
        WorldHandler world;
        CollisionSystem collision;
        World collisionWorld;
        RigidBody body;

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
            // TODO: Add your initialization logic here
            collision = new CollisionSystemSAP();
            collisionWorld = new World(collision);
            this.renderer = new Renderer(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, farPlaneDistance);
            this.camera = new Camera(new Vector3(0, -70, -50), new Vector3(0, 0, 0));
            this.inputHandler = new InputHandler(PlayerIndex.One);
            // Physics Testing
            Shape shape = new BoxShape(1.0f, 2.0f, 3.0f);
            body = new RigidBody(shape);
            collisionWorld.AddBody(body);
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
            Model characterModel = Content.Load<Model>("Models/dude"); //Loads your new model, point it towards your model
            this.character = new GameObject(characterModel, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
            this.character.PlayAnimation("Take 001");
            Model cubeModel = Content.Load<Model>("Models/cube"); //Loads your new model, point it towards your model
            this.cube = new GameObject(cubeModel, new Vector3(-10, 0, 0), new Vector3(0, 0, 0), new Vector3(10, 10, 10));
            // Create our World
            GameObject nonPathBlock = new GameObject(cubeModel, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(10, 10, 10));
            Model pathModel = Content.Load<Model>("Models/path"); //Loads your new model, point it towards your model
            GameObject pathBlock = new GameObject(pathModel, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(10, 10, 10));
            this.world = new WorldHandler(nonPathBlock, pathBlock);
            this.world.advanceTurn();
            this.world.advanceTurn();
            this.world.advanceTurn();
            this.world.advanceTurn();
            this.world.advanceTurn();

            // TODO: use this.Content to load your game content here
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
            float step = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (step > 1.0f / 100.0f) step = 1.0f / 100.0f;
            collisionWorld.Step(step, true);
            //this.character.setPosition(new Vector3(body.Position.X, body.Position.Y, body.Position.Z));
            Console.WriteLine(body.Position);
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            this.inputHandler.Update(gameTime);
            this.camera.Update(gameTime, inputHandler);
            this.character.Update(gameTime);
            this.cube.Update(gameTime);

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
            this.character.Draw(camera, renderer);
            //this.cube.Draw(camera, renderer);
            this.world.Draw(camera, renderer);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}