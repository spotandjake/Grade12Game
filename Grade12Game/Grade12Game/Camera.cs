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
    public class Camera
    {
        // Internals
        private Vector3 position;
        private Vector3 rotation;
        // Constructor
        public Camera(Vector3 position, Vector3 rotation)
        {
            // Set Internals
            this.setPosition(position);
            this.setRotation(rotation);
        }
        // Public Methods
        public void Update(GameTime gameTime, InputHandler inputHandler)
        {
            // Normalize for gameTime
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            float speed = time / 10;
            // Set Rotation
            this.rotation.Y += MathHelper.ToRadians(inputHandler.PitchAxis * speed);
            this.rotation.X += MathHelper.ToRadians(inputHandler.YawAxis * speed);
            // Lock Rotation
            if (this.rotation.X < -MathHelper.PiOver2) this.rotation.X = -MathHelper.PiOver2;
            if (this.rotation.X > MathHelper.PiOver2) this.rotation.X = MathHelper.PiOver2;
            // Calc Velocity
            Vector3 velocity = new Vector3(0, 0, 0);
            velocity.X += (inputHandler.ForwardAxis * (float)Math.Sin(-this.rotation.Y) + inputHandler.SideAxis * -(float)Math.Cos(-this.rotation.Y));
            velocity.Z += (inputHandler.ForwardAxis * (float)Math.Cos(-this.rotation.Y) + inputHandler.SideAxis * (float)Math.Sin(-this.rotation.Y));

            velocity.Y += inputHandler.VerticalAxis;
            // Set Position
            this.position.X -= velocity.X;
            this.position.Y -= velocity.Y;
            this.position.Z -= velocity.Z;
        }
        public void setPosition(Vector3 position)
        {
            this.position = position;
        }
        public Vector3 getPosition()
        {
            return this.position;
        }
        public void setRotation(Vector3 rotation)
        {
            this.rotation = rotation;
        }
        public Vector3 getRotation()
        {
            return this.rotation;
        }
    }
}