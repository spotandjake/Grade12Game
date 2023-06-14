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
    abstract class Turret : GameObject
    {
        protected long lastShot;
        protected long currentShotRate = 0;
        protected readonly long shotRate = 10000000;
        protected readonly float projSpeed = 70;

        protected float range;

        protected Enemy target;

        public Turret(
           Model model,
           Shape shape,
           Vector3 position,
           Vector3 rotation,
           Vector3 scale,
           long shotRate,
           float projSpeed,
           float range
       ) : base(model, shape, position, rotation, scale) {
            this.shotRate = shotRate;
            this.projSpeed = projSpeed;
            this.range = range;
        }

        public void aimTurret()
        {
            if (target == null) return;
            Vector3 targetRot = target.getPosition() - this.getPosition();
            Vector3 currentRot = this.getRotation();
            Vector3 rot = new Vector3(currentRot.X, currentRot.Y, (float)Math.Atan2(targetRot.X, targetRot.Z));
            this.setRotation(rot);
        }

        public float getDist(IGameObject target)
        {
            // Check if we are touching
            Vector3 diff = target.getPosition() - position;
            // TODO: We would prefer to use LengthSquared
            return diff.Length();
        }

        public void getTarget(WorldHandler world)
        {
            if (target == null)
            {
                // Choose Target
                List<Enemy> enemies = new List<Enemy>();
                foreach (Enemy e in world.getEnemies())
                {
                    // Ensure that the enemy is still within our radius
                    if (this.getDist(e) > range) continue;
                    // Add Enemy To List
                    enemies.Add(e);
                }
                // Choose Target
                if (enemies.Count > 0)
                {
                    target = enemies[world.rand.Next(enemies.Count)];
                }
            } else
            {
                // Ensure that the enemy is still within our radius
                if (target != null && this.getDist(target) > range) target = null;
                // Ensure Target Still Exists, to help prevent mem leaks
                if (target != null && !world.hasGameObject(target)) target = null;
                // Ensure Enemy Is Active
                if (target != null && !target.getIsActive()) target = null;
            }
        }

        public bool canShoot(GameTime gameTime)
        {
            // Breaks
            if (target == null) return false;
            if (gameTime.TotalGameTime.Ticks - lastShot < currentShotRate) return false;
            // Good
            return true;
        }

        public void UpdateShotRate(GameTime gameTime, WorldHandler world)
        {
            // TODO: base this off gameTime
            if (this.currentShotRate == 0) this.currentShotRate = shotRate / 2 + world.rand.Next((int)shotRate / 2);
        }
    }

    class SmallTurret: Turret
    {
        public SmallTurret(
            Model model,
            Shape shape,
            Vector3 position,
            Vector3 rotation,
            Vector3 scale
        ): base(
            model,
            shape,
            position,
            rotation,
            scale,
            // Attributes
            10000000,
            50,
            100
        ) {}
        // Custom Update
        public override void Update(GameTime gameTime, WorldHandler world, InputHandler inputHandler)
        {
            this.UpdateShotRate(gameTime, world);
            // Get Target
            this.getTarget(world);
            // Calculate Angle
            this.aimTurret();
            if (this.canShoot(gameTime))
            {
                this.currentShotRate = world.rand.Next((int)shotRate);
                // Spawn Projectile
                Projectile projectile = world.spawnProjetile();
                // Set Position
                Vector3 pos = this.getPosition();
                pos.X += 20 * (float)Math.Sin(this.rotation.Z);
                pos.Y += 20;
                pos.Z += 20 * (float)Math.Cos(this.rotation.Z);
                projectile.setPosition(pos);
                // TODO: Switch to targetRot so we account for the height
                projectile.setStartTick(gameTime.TotalGameTime.Ticks);
                projectile.LinearVelocity = new JVector((float)Math.Sin(this.rotation.Z) * projSpeed, 0, (float)Math.Cos(this.rotation.Z) * projSpeed);
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
    class MediumTurret : Turret
    {
        public MediumTurret(
            Model model,
            Shape shape,
            Vector3 position,
            Vector3 rotation,
            Vector3 scale
        ) : base(
            model,
            shape,
            position,
            rotation,
            scale,
            // Attributes
            1000000,
            75,
            100
        )
        { }
        // Custom Update
        public override void Update(GameTime gameTime, WorldHandler world, InputHandler inputHandler)
        {
            this.UpdateShotRate(gameTime, world);
            // Get Target
            this.getTarget(world);
            // Calculate Angle
            this.aimTurret();
            // Handle Shooting
            if (this.canShoot(gameTime))
            {
                this.currentShotRate = world.rand.Next((int)shotRate);
                // Spawn Projectile
                Projectile projectile = world.spawnProjetile();
                // Set Position
                Vector3 pos = this.getPosition();
                pos.X += 20 * (float)Math.Sin(this.rotation.Z);
                pos.Y += 20;
                pos.Z += 20 * (float)Math.Cos(this.rotation.Z);
                projectile.setPosition(pos);
                // TODO: Switch to targetRot so we account for the height
                projectile.setStartTick(gameTime.TotalGameTime.Ticks);
                projectile.LinearVelocity = new JVector((float)Math.Sin(this.rotation.Z) * projSpeed, 0, (float)Math.Cos(this.rotation.Z) * projSpeed);
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
    class LargeTurret : Turret
    {
        public LargeTurret(
            Model model,
            Shape shape,
            Vector3 position,
            Vector3 rotation,
            Vector3 scale
        ) : base(
            model,
            shape,
            position,
            rotation,
            scale,
            // Attributes
            1000000,
            100,
            100
        )
        { }
        // Custom Update
        public override void Update(GameTime gameTime, WorldHandler world, InputHandler inputHandler)
        {
            this.UpdateShotRate(gameTime, world);
            // Get Target
            this.getTarget(world);
            // Handle Shooting
            this.aimTurret();
            if (this.canShoot(gameTime))
            {
                this.currentShotRate = (long)((double)shotRate * world.rand.NextDouble());
                // Spawn First
                // Spawn Projectile
                Projectile projectile = world.spawnProjetile();
                // Set Position
                Vector3 pos = this.getPosition();
                pos.X += 20 * (float)Math.Sin(this.rotation.Z);
                pos.Y += 20;
                pos.Z += 20 * (float)Math.Cos(this.rotation.Z);

                pos.X += 2 * (float)Math.Sin(this.rotation.Z + MathHelper.PiOver2);
                pos.Z += 2 * (float)Math.Cos(this.rotation.Z + MathHelper.PiOver2);
                projectile.setPosition(pos);
                // TODO: Switch to targetRot so we account for the height
                projectile.setStartTick(gameTime.TotalGameTime.Ticks);
                projectile.LinearVelocity = new JVector((float)Math.Sin(this.rotation.Z) * projSpeed, 0, (float)Math.Cos(this.rotation.Z) * projSpeed);
                // Spawn Second
                // Spawn Projectile
                Projectile projectile2 = world.spawnProjetile();
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
