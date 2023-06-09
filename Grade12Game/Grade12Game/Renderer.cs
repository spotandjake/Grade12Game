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
    class Renderer : Microsoft.Xna.Framework.Game
    {
        // Internals
        private int width;
        private int height;
        private float aspectRatio;
        private readonly float farPlaneDistance;
        private Matrix projection;

        // Constructor
        public Renderer(int width, int height, float farPlaneDistance)
        {
            // Set Internals
            this.farPlaneDistance = farPlaneDistance;
            // Handle Screen Sizing
            this.setScreenSize(width, height);
        }
        // Methods
        public void setScreenSize(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.aspectRatio = (float)width / (float)height;
            // Crate The 3d Proj Matrix
            this.projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                aspectRatio,
                1.0f,
                farPlaneDistance
            );
        }

        public int getWidth() {
            return this.width;
        }
        public int getHeight()
        {
            return this.height;
        }
        // Draw Method
        public void DrawGameObject(Camera camera, GameObject gameObject)
        {
            // Get Cam Internals
            Vector3 camPosition = camera.getPosition();
            Vector3 camRotation = camera.getRotation();
            // Get Our Object Resources
            Model model = gameObject.getModel();
            Vector3 position = gameObject.getPosition();
            Vector3 scale = gameObject.getScale();
            AnimationPlayer animationPlayer = gameObject.getAnimationPlayer();
            bool hasBones = gameObject.getHasBones();
            // Apply Model Animation
            Matrix[] bones = null;
            Matrix world = Matrix.CreateScale(0);
            if (hasBones)
            {
                bones = animationPlayer.GetSkinTransforms();
                // Apply trnaslation to each bone in mesh
                for (int i = 0; i < bones.Length; i++)
                {
                    // Apply Model Transforms, y using matrix math, its not that complex at heart ut hard to explain to someone new.
                    bones[i] *=
                        // Create a matrix from our JVector rotation
                        new Matrix(
                            gameObject.Orientation.M11, gameObject.Orientation.M12, gameObject.Orientation.M13, 0.0f,
                            gameObject.Orientation.M21, gameObject.Orientation.M22, gameObject.Orientation.M23, 0.0f,
                            gameObject.Orientation.M31, gameObject.Orientation.M32, gameObject.Orientation.M33, 0.0f,
                            0.0f, 0.0f, 0.0f, 1.0f
                        )
                        *
                        Matrix.CreateScale(scale / 2) //Applys the scale
                        *
                        Matrix.CreateWorld(position + scale / 2, Vector3.Forward, Vector3.Up); //Move the models position
                   // Was a bug initally, now i use it for rendering the funky looking enemies :)
                   if (gameObject.getMonsterMash()) bones[i].Translation = position;
                }
            } else
            {
                // This is a bugged version I decidedd to use as another enemy type
                // Create a matrix from our JVector rotation
                world = new Matrix(
                    gameObject.Orientation.M11, gameObject.Orientation.M12, gameObject.Orientation.M13, 0.0f,
                    gameObject.Orientation.M21, gameObject.Orientation.M22, gameObject.Orientation.M23, 0.0f,
                    gameObject.Orientation.M31, gameObject.Orientation.M32, gameObject.Orientation.M33, 0.0f,
                    0.0f, 0.0f, 0.0f, 1.0f
                );
                world *= Matrix.CreateScale(scale / 2);
                world.Translation = position + scale / 2;
            }
            // Create Camera View Matrix
            Matrix view = Matrix.CreateTranslation(camPosition*-1) *
                Matrix.CreateRotationY(camRotation.Y) *
                Matrix.CreateRotationX(-camRotation.X);
            // Draw Model
            foreach (ModelMesh mesh in model.Meshes)
            {
                // Handle setting the transforms
                if (hasBones)
                {
                    // ANimation player
                    foreach (SkinnedEffect effect in mesh.Effects)
                    {
                        effect.SetBoneTransforms(bones);
                        effect.View = view;
                        effect.Projection = this.projection;
                        effect.EnableDefaultLighting();
                        // Look into this
                        // TODO: let model take control of this
                        effect.SpecularColor = new Vector3(0.25f);
                        effect.SpecularPower = 16;
                    }
                } else
                {
                    // Normal Static Model
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.World = world;
                        effect.View = view;
                        effect.Projection = this.projection;
                        effect.EnableDefaultLighting();
                        // Look into this
                        // TODO: let model take control of this
                        effect.SpecularColor = new Vector3(0.25f);
                        effect.SpecularPower = 16;
                    }
                }
                // Draw The Mesh
                mesh.Draw();
            }
        }
    }
}