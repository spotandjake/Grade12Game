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
        // Axis
        public float ForwardAxis { get; private set; }
        public float SideAxis { get; private set; }
        // Constructor
        public InputHandler(PlayerIndex playerIndex)
        {
            this.setPlayerIndex(playerIndex);
        }
        // Public Methods
        public void Update(GameTime gameTime)
        {
            // get Input State
            GamePadState padState = GamePad.GetState(this.playerIndex);
            KeyboardState keyState = Keyboard.GetState();
            // Handle Forward Axis
            this.ForwardAxis = padState.ThumbSticks.Left.X;
            this.ForwardAxis += keyState.IsKeyDown(Keys.W) ? 1 : 0;
            this.ForwardAxis -= keyState.IsKeyDown(Keys.S) ? 1 : 0;
            if (this.ForwardAxis < -1) this.ForwardAxis = -1;
            if (this.ForwardAxis > 1) this.ForwardAxis = 1;
        }
        public void setPlayerIndex(PlayerIndex playerIndex)
        {
            this.playerIndex = playerIndex;
        }
  }
}
