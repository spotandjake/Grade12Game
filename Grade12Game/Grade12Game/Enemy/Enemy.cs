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
    // EnemyTypes
    struct EnemyType
    {
        // This stores the enemy info for enemy Types
        // Props
        public readonly Model model;
        public readonly Shape shape;
        public readonly Vector3 rotation;
        public readonly Vector3 scale;
        // Spawn Traits
        public readonly float speed;
        public readonly int health;
        public readonly float difficulty;
        // Effects
        // Constructor
        public EnemyType(
            // Props
            Model model,
            Shape shape,
            Vector3 rotation,
            Vector3 scale,
            // Spawn Traits
            float speed,
            int health,
            float difficulty
            // Effects
        )
        {
            // Set Props
            this.model = model;
            this.shape = shape;
            this.rotation = rotation;
            this.scale = scale;
            // Set Spawn Traits
            this.speed = speed;
            this.health = health;
            this.difficulty = difficulty;
        }
    }
    // Enemy Class
    class Enemy : GameObject
    {
        // Props
        private Stack<Vector3> path;
        private readonly Stack<Vector3> immutablePath;
        private float speed;
        private int health;
        private int stepsUntilSpawn;

        private Vector3 currentTarget;

        private float y = 0;

        // Wont let me use const here
        private readonly Vector3 unset = new Vector3(float.MaxValue);
        // Constructor
        public Enemy(
            // Props
            Model model,
            Shape shape,
            // Data
            Vector3 position,
            Vector3 rotation,
            Vector3 scale,
            int stepsUntilSpawn,
            Stack<Vector3> path,
            Stack<Vector3> immutablePath,
            // Traits
            float speed,
            int health
        ) : base(model, shape, position, rotation, scale)
        {
            y = position.Y;
            this.path = path;
            this.immutablePath = immutablePath;
            this.stepsUntilSpawn = stepsUntilSpawn;
            this.speed = speed;
            this.health = health;
            currentTarget = unset;
        }
        // Custom Update Behaviour
        public override void Update(GameTime gameTime, WorldHandler world, InputHandler input)
        {
            this.setPosition(new Vector3(this.getPosition().X, y, this.getPosition().Z));
            // TODO: Figure out why we initially go the wrong way
            if (stepsUntilSpawn > 0)
            {
                this.setIsActive(false);
                if (path.Count > 0)
                    this.setPosition(new Vector3(path.Peek().X, this.position.Y, path.Peek().Z));
            }
            // Damage
            if (this.health <= 0)
            {
                world.removeGameObject(this);
            }
            // Path Finding
            if (currentTarget != unset)
            {
                // Face Target
                Vector3 targetRot = currentTarget - this.getPosition();
                Vector3 currentRot = this.getRotation();
                Vector3 rot = new Vector3((float)Math.Atan2(targetRot.X, targetRot.Z) + MathHelper.Pi, currentRot.Y, currentRot.Z);
                this.setRotation(rot);
                // Handle Activity
                if (stepsUntilSpawn < 0) this.setIsActive(true);
                // Apply Movement
                if (this.getIsActive())
                {
                    // Apply Movement
                    this.LinearVelocity = new JVector((float)Math.Sin(this.rotation.X) * -speed, 0, (float)Math.Cos(this.rotation.X) * -speed);
                    // If Close Then Set New Target
                    if (targetRot.LengthSquared() < 1)
                    {
                        path.Pop();
                        currentTarget = unset;
                    }
                }
            }
            {
                // Choose Target
                if (path.Count <= 1)
                {
                    // Remove Myself
                    world.removeGameObject(this);
                    // TODO: Make 1 not a magic number
                    world.doBaseDamage(1);
                } else
                {
                    currentTarget = path.Peek();
                    currentTarget = new Vector3(currentTarget.X, this.position.Y, currentTarget.Z);
                }
            }
            stepsUntilSpawn--;
            // Update Basse
            base.Update(gameTime, world, input);
        }

        public int getHealth()
        {
            return this.health;
        }
        // doDamge
        public void DoDamage(int damage)
        {
            this.health -= damage;
            if (this.health < 0) this.health = 0; 
        }
    }
}
