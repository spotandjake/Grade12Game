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
using Jitter;
using Jitter.Collision;
using Jitter.Collision.Shapes;
using Jitter.DataStructures;
using Jitter.Dynamics;
using Jitter.LinearMath;

namespace Grade12Game
{
    class Character : GameObject
    {
        // Constructor
        public Character(
            Model model,
            Shape shape,
            Vector3 position,
            Vector3 rotation,
            Vector3 scale
            ) : base(model, shape, position, rotation, scale)
        { }
        // My Character GameObjectt
        public override void Update(GameTime gameTime, World world, InputHandler inputHandler)
        {
            // Normalize for gameTime
            //float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            //float speed = time / 10;
            // Set Rotation
            //this.rotation.Y -= MathHelper.ToRadians(inputHandler.PitchAxis * speed);
            // Calc Velocity
            //Vector3 velocity = new Vector3(0, 0, 0);
            //velocity.X += (inputHandler.ForwardAxis * (float)Math.Sin(-this.rotation.Y) + inputHandler.SideAxis * -(float)Math.Cos(-this.rotation.Y));
            //velocity.Z += (inputHandler.ForwardAxis * (float)Math.Cos(-this.rotation.Y) + inputHandler.SideAxis * (float)Math.Sin(-this.rotation.Y));

            //velocity.Y += inputHandler.VerticalAxis;
            // Set Position
            //Vector3 pos = this.getPosition();
            //pos.X += velocity.X;
            //pos.Y += velocity.Y;
            //pos.Z += velocity.Z;
            //this.setPosition(pos);
            // Call basse update
            base.Update(gameTime, world, inputHandler);
        }
    }
}
