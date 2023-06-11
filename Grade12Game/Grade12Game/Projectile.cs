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
    class Projectile : GameObject
    {
        private long startTick;
        private long lifeTime;
        public Projectile(
            Model model,
            Shape shape,
            Vector3 position,
            Vector3 rotation,
            Vector3 scale,
            long startTick,
            long lifeTime
        ) : base(model, shape, position, rotation, scale) {
            this.startTick = startTick;
            this.lifeTime = lifeTime;
        }
        // Methods
        public void setStartTick(long startTick)
        {
            this.startTick = startTick;
        }
        // Custom Update
        public override void Update(GameTime gameTime, WorldHandler world, InputHandler inputHandler)
        {
           // Remove after tick Count
           if (gameTime.TotalGameTime.Ticks-startTick >= this.lifeTime)
            {
                world.removeGameObject(this);
            }
            // Call Base Update
            base.Update(gameTime, world, inputHandler);
        }

        public override IGameObject Clone()
        {
            // TODO: Look into cloning the model and shape
            Projectile obj = new Projectile(this.model, this.Shape, this.getPosition(), this.rotation, this.scale, this.startTick, this.lifeTime);
            obj.Force = this.Force;
            obj.Orientation = this.Orientation;
            obj.IsStatic = this.IsStatic;
            obj.AffectedByGravity = this.AffectedByGravity;
            return obj;
        }
    }
}
