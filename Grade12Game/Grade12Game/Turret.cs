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
    class SmallTurret: GameObject
    {
        private long lastShot;
        private long currentShotRate = 0;
        private const long shotRate = 10000000;
        private const float projSpeed = 50;
        public SmallTurret(
            Model model,
            Shape shape,
            Vector3 position,
            Vector3 rotation,
            Vector3 scale
        ):base(model, shape, position, rotation, scale) { }
        // Custom Update
        public override void Update(GameTime gameTime, WorldHandler world, InputHandler inputHandler)
        {
            if (this.currentShotRate == 0) this.currentShotRate = shotRate / 2 + world.rand.Next((int)shotRate / 2);
            // Calculate Angle
            Camera target = world.player;

            Vector3 targetRot = target.getPosition() - this.getPosition();
            Vector3 currentRot = this.getRotation();
            Vector3 rot = new Vector3(currentRot.X, currentRot.Y, (float)Math.Atan2(targetRot.X, targetRot.Z));
            this.setRotation(rot);
            if (gameTime.TotalGameTime.Ticks - lastShot >= currentShotRate)
            {
                this.currentShotRate = world.rand.Next((int)shotRate);
                // Spawn Projectile
                Projectile projectile = (Projectile)world.projectile.Clone();
                // Set Position
                Vector3 pos = this.getPosition();
                pos.X += 20 * (float)Math.Sin(this.rotation.Z);
                pos.Y += 20;
                pos.Z += 20 * (float)Math.Cos(this.rotation.Z);
                projectile.setPosition(pos);
                // TODO: Switch to targetRot so we account for the height
                projectile.setStartTick(gameTime.TotalGameTime.Ticks);
                projectile.LinearVelocity = new JVector((float)Math.Sin(this.rotation.Z) * projSpeed, 0, (float)Math.Cos(this.rotation.Z) * projSpeed);
                world.addGameObject(projectile);
                lastShot = gameTime.TotalGameTime.Ticks;
            }
            // Call Base Update
            base.Update(gameTime, world, inputHandler);
        }

        public override IGameObject Clone()
        {
            // TODO: Look into cloning the model and shape
            SmallTurret obj = new SmallTurret(this.model, this.Shape, this.getPosition(), this.rotation, this.scale);
            obj.Force = this.Force;
            obj.Orientation = this.Orientation;
            obj.IsStatic = this.IsStatic;
            obj.AffectedByGravity = this.AffectedByGravity;
            return obj;
        }
    }
    class MediumTurret : GameObject
    {
        private long lastShot;
        private long currentShotRate = 0;
        private const long shotRate = 1000000;
        private const float projSpeed = 75;
        public MediumTurret(
            Model model,
            Shape shape,
            Vector3 position,
            Vector3 rotation,
            Vector3 scale
        ) : base(model, shape, position, rotation, scale) { }
        // Custom Update
        public override void Update(GameTime gameTime, WorldHandler world, InputHandler inputHandler)
        {
            if (this.currentShotRate == 0) this.currentShotRate = shotRate / 2 + world.rand.Next((int)shotRate / 2);
            // Calculate Angle
            Camera target = world.player;

            Vector3 targetRot = target.getPosition() - this.getPosition();
            Vector3 currentRot = this.getRotation();
            Vector3 rot = new Vector3(currentRot.X, currentRot.Y, (float)Math.Atan2(targetRot.X, targetRot.Z));
            this.setRotation(rot);
            if (gameTime.TotalGameTime.Ticks - lastShot >= this.currentShotRate)
            {
                this.currentShotRate = world.rand.Next((int)shotRate);
                // Spawn Projectile
                Projectile projectile = (Projectile)world.projectile.Clone();
                // Set Position
                Vector3 pos = this.getPosition();
                pos.X += 20 * (float)Math.Sin(this.rotation.Z);
                pos.Y += 20;
                pos.Z += 20 * (float)Math.Cos(this.rotation.Z);
                projectile.setPosition(pos);
                // TODO: Switch to targetRot so we account for the height
                projectile.setStartTick(gameTime.TotalGameTime.Ticks);
                projectile.LinearVelocity = new JVector((float)Math.Sin(this.rotation.Z) * projSpeed, 0, (float)Math.Cos(this.rotation.Z) * projSpeed);
                world.addGameObject(projectile);
                lastShot = gameTime.TotalGameTime.Ticks;
            }
            // Call Base Update
            base.Update(gameTime, world, inputHandler);
        }

        public override IGameObject Clone()
        {
            // TODO: Look into cloning the model and shape
            MediumTurret obj = new MediumTurret(this.model, this.Shape, this.getPosition(), this.rotation, this.scale);
            obj.Force = this.Force;
            obj.Orientation = this.Orientation;
            obj.IsStatic = this.IsStatic;
            obj.AffectedByGravity = this.AffectedByGravity;
            return obj;
        }
    }
    class LargeTurret : GameObject
    {
        private long lastShot;
        private long currentShotRate = 0;
        private const long shotRate = 1000000;
        private const float projSpeed = 100;
        public LargeTurret(
            Model model,
            Shape shape,
            Vector3 position,
            Vector3 rotation,
            Vector3 scale
        ) : base(model, shape, position, rotation, scale) { }
        // Custom Update
        public override void Update(GameTime gameTime, WorldHandler world, InputHandler inputHandler)
        {
            if (this.currentShotRate == 0) this.currentShotRate = shotRate / 2 + world.rand.Next((int)shotRate / 2);
            // Calculate Angle
            Camera target = world.player;

            Vector3 targetRot = target.getPosition() - this.getPosition();
            Vector3 currentRot = this.getRotation();
            Vector3 rot = new Vector3(currentRot.X, currentRot.Y, (float)Math.Atan2(targetRot.X, targetRot.Z));
            this.setRotation(rot);
            if (gameTime.TotalGameTime.Ticks - lastShot >= currentShotRate)
            {
                this.currentShotRate = (long)((double)shotRate * world.rand.NextDouble());
                // Spawn First
                // Spawn Projectile
                Projectile projectile = (Projectile)world.projectile.Clone();
                // Set Position
                Vector3 pos = this.getPosition();
                pos.X += 20 * (float)Math.Sin(this.rotation.Z);
                pos.Y += 20;
                pos.Z += 20 * (float)Math.Cos(this.rotation.Z);

                pos.X += 2 * (float)Math.Sin(this.rotation.Z+MathHelper.PiOver2);
                pos.Z += 2 * (float)Math.Cos(this.rotation.Z + MathHelper.PiOver2);
                projectile.setPosition(pos);
                // TODO: Switch to targetRot so we account for the height
                projectile.setStartTick(gameTime.TotalGameTime.Ticks);
                projectile.LinearVelocity = new JVector((float)Math.Sin(this.rotation.Z) * projSpeed, 0, (float)Math.Cos(this.rotation.Z) * projSpeed);
                world.addGameObject(projectile);
                // Spawn Second
                // Spawn Projectile
                Projectile projectile2 = (Projectile)world.projectile.Clone();
                // Set Position
                Vector3 pos2 = this.getPosition();
                pos2.X += 20 * (float)Math.Sin(this.rotation.Z);
                pos2.Y += 20;
                pos2.Z += 20 * (float)Math.Cos(this.rotation.Z);

                pos2.X += 2 * (float)Math.Sin(this.rotation.Z - MathHelper.PiOver2);
                pos2.Z += 2 * (float)Math.Cos(this.rotation.Z - MathHelper.PiOver2);
                projectile2.setPosition(pos2);
                // TODO: Switch to targetRot so we account for the height
                projectile2.setStartTick(gameTime.TotalGameTime.Ticks);
                projectile2.LinearVelocity = new JVector((float)Math.Sin(this.rotation.Z) * projSpeed, 0, (float)Math.Cos(this.rotation.Z) * projSpeed);
                world.addGameObject(projectile2);
                // set lastShot
                lastShot = gameTime.TotalGameTime.Ticks;
            }
            // Call Base Update
            base.Update(gameTime, world, inputHandler);
        }

        public override IGameObject Clone()
        {
            // TODO: Look into cloning the model and shape
            LargeTurret obj = new LargeTurret(this.model, this.Shape, this.getPosition(), this.rotation, this.scale);
            obj.Force = this.Force;
            obj.Orientation = this.Orientation;
            obj.IsStatic = this.IsStatic;
            obj.AffectedByGravity = this.AffectedByGravity;
            return obj;
        }
    }
}
