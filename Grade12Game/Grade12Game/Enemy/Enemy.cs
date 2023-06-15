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
        // Usedd for translating the enemy
        public readonly Vector3 spawnPosition;
        // Spawn Traits
        public readonly float speed;
        public readonly int health;
        public readonly float difficulty;
        public readonly int moneyDrop;
        public readonly int damage;
        // Effects
        public readonly bool isMonsterMatrix;
        // Constructor
        public EnemyType(
            // Props
            Model model,
            Shape shape,
            Vector3 rotation,
            Vector3 scale,
            Vector3 spawnPosition,
            // Spawn Traits
            float speed,
            int health,
            float difficulty,
            int moneyDrop,
            int damage,
            // Effects
            bool isMonsterMatrix
        )
        {
            // Set Props
            this.model = model;
            this.shape = shape;
            this.rotation = rotation;
            this.scale = scale;
            this.spawnPosition = spawnPosition;
            // Set Spawn Traits
            this.speed = speed;
            this.health = health;
            this.difficulty = difficulty;
            this.moneyDrop = moneyDrop;
            this.damage = damage;
            // Sett Spawn Effects
            this.isMonsterMatrix = isMonsterMatrix;
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
        private readonly int moneyDrop;

        private Vector3 currentTarget;

        private float y = 0;

        // The Damage Enemy's do to the base
        private int damage;

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
            int health,
            int moneyDrop,
            int damage,
            // Effects
            bool monsterMash
        ) : base(model, shape, position, rotation, scale)
        {
            y = position.Y;
            this.path = path;
            this.immutablePath = immutablePath;
            this.stepsUntilSpawn = stepsUntilSpawn;
            this.speed = speed;
            this.health = health;
            this.moneyDrop = moneyDrop;
            this.damage = damage;
            currentTarget = unset;
            this.setMonsterMash(monsterMash);
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
                world.addMoney(this.moneyDrop);
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
                    // If Close Then Set New Target, this is the squared distance, we need it to be a bit higher so we can go at faster speeds
                    if (targetRot.LengthSquared() < this.speed*this.speed)
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
                    world.doBaseDamage(this.damage);
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
