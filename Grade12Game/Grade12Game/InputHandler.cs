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
using SkinnedModel;

namespace Grade12Game
{
    public class InputHandler
    {
        // Internals
        private PlayerIndex playerIndex;
        private GamePadState oldPadState;
        private KeyboardState oldKeyState;
        private GamePadState padState;
        private KeyboardState keyState;

        // Axis
        public float ForwardAxis { get; private set; }
        public float SideAxis { get; private set; }
        public float VerticalAxis { get; private set; }
        public float PitchAxis { get; private set; }
        public float YawAxis { get; private set; }

        // States
        public bool isExitDown { get; private set; }
        public bool isSpawnTowerKeyPressed { get; private set; }

        // Constructor
        public InputHandler(PlayerIndex playerIndex)
        {
            this.setPlayerIndex(playerIndex);
        }

        // Public Methods
        public void Update(GameTime gameTime)
        {
            // get Input State
            padState = GamePad.GetState(this.playerIndex);
            keyState = Keyboard.GetState();
            // Handle Forward Axis
            this.ForwardAxis = padState.ThumbSticks.Left.Y;
            this.ForwardAxis += keyState.IsKeyDown(Keys.W) ? 1 : 0;
            this.ForwardAxis -= keyState.IsKeyDown(Keys.S) ? 1 : 0;
            if (this.ForwardAxis < -1)
                this.ForwardAxis = -1;
            if (this.ForwardAxis > 1)
                this.ForwardAxis = 1;
            // Handle Side Axis
            this.SideAxis = padState.ThumbSticks.Left.X;
            this.SideAxis += keyState.IsKeyDown(Keys.D) ? 1 : 0;
            this.SideAxis -= keyState.IsKeyDown(Keys.A) ? 1 : 0;
            if (this.SideAxis < -1)
                this.SideAxis = -1;
            if (this.SideAxis > 1)
                this.SideAxis = 1;
            // Handle Vertial Axis
            this.VerticalAxis = 0;
            this.VerticalAxis += keyState.IsKeyDown(Keys.Q) ? 1 : 0;
            this.VerticalAxis -= keyState.IsKeyDown(Keys.E) ? 1 : 0;
            if (this.VerticalAxis < -1)
                this.VerticalAxis = -1;
            if (this.VerticalAxis > 1)
                this.VerticalAxis = 1;
            // Handle Pitch
            this.PitchAxis = padState.ThumbSticks.Right.X;
            this.PitchAxis += keyState.IsKeyDown(Keys.Right) ? 1 : 0;
            this.PitchAxis -= keyState.IsKeyDown(Keys.Left) ? 1 : 0;
            if (this.PitchAxis < -1)
                this.PitchAxis = -1;
            if (this.PitchAxis > 1)
                this.PitchAxis = 1;
            // Handle Yaw
            this.YawAxis = padState.ThumbSticks.Right.Y;
            this.YawAxis += keyState.IsKeyDown(Keys.Up) ? 1 : 0;
            this.YawAxis -= keyState.IsKeyDown(Keys.Down) ? 1 : 0;
            if (this.YawAxis < -1)
                this.YawAxis = -1;
            if (this.YawAxis > 1)
                this.YawAxis = 1;
            // Set States
            // TODO: Map this onto controller
            this.isSpawnTowerKeyPressed = isKeyPressed(Keys.F);
            // Allows the game to exit
            this.isExitDown =
                padState.Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.Escape);
        }
        // Write Old States
        public void writeOldState()
        {
            this.oldPadState = this.padState;
            this.oldKeyState = this.keyState;
        }
        // Mehods
        public bool isKeyPressed(Keys key)
        {
            return this.keyState.IsKeyDown(key) && !this.oldKeyState.IsKeyDown(key);
        }

        public void setPlayerIndex(PlayerIndex playerIndex)
        {
            this.playerIndex = playerIndex;
        }
    }
}
