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
    public interface IGameObject
    {
        void setModel(Model model);
        Model getModel();
        void setPosition(Vector3 vector);
        Vector3 getPosition();
        void setRotation(Vector3 rot);
        Vector3 getRotation();
        void setScale(Vector3 scale);
        Vector3 getScale();
        AnimationPlayer getAnimationPlayer();
        bool getHasBones();
        void PlayAnimation(String Animation);
        GameObject Clone(); // TODO Look into using a generic here
        void Update(GameTime gameTime, World world, InputHandler inputHandler);
        void Draw(Camera cam, Renderer renderer);
    }

    public class GameObject : RigidBody, IGameObject
    {
        // Internals
        protected Vector3 position
        {
            get { return new Vector3(this.Position.X, this.Position.Y, this.Position.Z); }
            set { this.Position = new JVector(value.X, value.Y, value.Z); }
        }
        protected Vector3 rotation;
        protected Vector3 scale;
        protected Model model;
        protected SkinningData skinningData;
        protected AnimationPlayer animationPlayer;
        protected AnimationClip animationClip;
        protected bool hasBones;

        // Constructor
        public GameObject(
            Model model,
            Shape shape,
            Vector3 position,
            Vector3 rotation,
            Vector3 scale
        )
            : base(shape)
        {
            // Set Internals
            this.setModel(model);
            this.setPosition(position);
            this.setRotation(rotation);
            this.setScale(scale);
        }

        // Public Methods
        public virtual void Update(GameTime gameTime, World world, InputHandler inputHandler)
        {
            // Update The AnimationPlayer
            if (this.animationClip != null)
            {
                this.animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
            }
        }

        public void setModel(Model model)
        {
            this.model = model;
            // Get Skinning Data
            this.skinningData = model.Tag as SkinningData;
            this.hasBones = true;
            this.animationPlayer = null;
            if (this.skinningData == null)
            {
                this.hasBones = false;
                //        throw new InvalidOperationException
                //          ("This model does not contain a SkinningData tag.");
            }
            else
            {
                // Set Animation Player
                this.animationPlayer = new AnimationPlayer(this.skinningData);
            }

            // Clear Animation Clip
            this.animationClip = null;
        }

        public Model getModel()
        {
            return this.model;
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

        public void setScale(Vector3 scale)
        {
            this.scale = scale;
        }

        public Vector3 getScale()
        {
            return this.scale;
        }

        public AnimationPlayer getAnimationPlayer()
        {
            return this.animationPlayer;
        }

        public bool getHasBones()
        {
            return this.hasBones;
        }

        public void PlayAnimation(String Animation)
        {
            this.animationClip = this.skinningData.AnimationClips[Animation];
            if (this.animationClip != this.animationPlayer.CurrentClip)
            {
                this.animationPlayer.StartClip(this.animationClip);
            }
        }

        public GameObject Clone()
        {
            // TODO: Look into cloning the model and shape
            GameObject obj = new GameObject(this.model, this.Shape, this.getPosition(), this.rotation, this.scale);
            return obj;
        }

        public void Draw(Camera cam, Renderer renderer)
        {
            renderer.DrawGameObject(cam, this);
        }
    }
}
