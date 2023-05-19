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
  public class Renderer : Microsoft.Xna.Framework.Game
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
        // Draw Method
        // TODO: Take A camera Object somewhere
        public void DrawGameObject(Camera camera, GameObject gameObject)
        {
              // Get Cam Internals
              Vector3 camPosition = camera.getPosition();
              Vector3 camRotation = camera.getRotation();
              // Get Our Object Resources
              Model model = gameObject.getModel();
              Vector3 position = gameObject.getPosition();
              Vector3 rotation = gameObject.getRotation();
              Vector3 scale = gameObject.getScale();
              AnimationPlayer animationPlayer = gameObject.getAnimationPlayer();
              // Apply Model Animation
              Matrix[] bones = animationPlayer.GetSkinTransforms();
              for (int i = 0; i < bones.Length; i++)
              {
                // Apply Model Transforms
                bones[i] *=
                      // TODO: I think we can do the rotation in one line
                      Matrix.CreateRotationX(rotation.X) //Computes the rotation
                    * Matrix.CreateRotationY(rotation.Y)
                    * Matrix.CreateRotationZ(rotation.Z)
                    // TODO: Our scale is no longer a float but a vector figure that out
                    * Matrix.CreateScale(scale) //Applys the scale
                    * Matrix.CreateWorld(position, Vector3.Forward, Vector3.Up); //Move the models position
              }
              // TODO: Create Camera View Matrix
            Matrix view = Matrix.CreateTranslation(camPosition.X, camPosition.Y, camPosition.Z) *
                Matrix.CreateRotationY((camRotation.Y)) *
                Matrix.CreateRotationX((camRotation.X)) *
                Matrix.CreateLookAt(new Vector3(0, 0, 300),
                                    new Vector3(0, 0, 0), Vector3.Up);
            // Draw Model
            foreach (ModelMesh mesh in model.Meshes)
              {
                // Handle setting the transforms
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                  effect.SetBoneTransforms(bones);
                  effect.View = view;
                  effect.Projection = this.projection;
                  effect.EnableDefaultLighting();
                  // Look into this
                  effect.SpecularColor = new Vector3(0.25f);
                  effect.SpecularPower = 16;
                }
                // Draw The Mesh
                mesh.Draw();
              }
        }
  }
}
