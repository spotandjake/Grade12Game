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
  class ModelRenderer : Microsoft.Xna.Framework.Game
  {
    public AnimationPlayer animationPlayer; //Controls the animation, references a method in the pre-loaded project
    public AnimationClip clip; //Contains the animation clip currently playing
    public Model currentModel; //Model reference
    public SkinningData skinningData; //Used by the AnimationPlayer method
    public Vector3 Translation = new Vector3(0, 0, 0); //Fishes current position on the screen
    public Vector3 Rotation = new Vector3(0, 5 * MathHelper.Pi / 6, -MathHelper.Pi / 2); //Current rotation
    public float Scale = .2f; //Current scale
    public ModelRenderer(Model currentModelInput)
    {
      currentModel = currentModelInput;
      // Look up our custom skinning information.
      skinningData = currentModel.Tag as SkinningData;
      if (skinningData == null)
        throw new InvalidOperationException
            ("This model does not contain a SkinningData tag.");
      // Create an animation player, and start decoding an animation clip.
      animationPlayer = new AnimationPlayer(skinningData);
    }
    public new void Update(GameTime gameTime)
    {
      if ((clip != null))
        animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
    }
    public void ModelDraw(GraphicsDevice device, Vector3 cameraPosition, Vector3 cameraRotation, float farPlaneDistance)
    {
      Matrix[] bones = animationPlayer.GetSkinTransforms();
      for (int i = 0; i < bones.Length; i++)
      {
        bones[i] *=
              Matrix.CreateRotationX(Rotation.X) //Computes the rotation
            * Matrix.CreateRotationY(Rotation.Y)
            * Matrix.CreateRotationZ(Rotation.Z)
            * Matrix.CreateScale(Scale) //Applys the scale
            * Matrix.CreateWorld(Translation, Vector3.Forward, Vector3.Up); //Move the models position
      }

      float aspectRatio = (float)device.Viewport.Width /
                          (float)device.Viewport.Height;

      // Compute camera matrices
      Matrix view = Matrix.CreateLookAt(cameraPosition, Translation, Vector3.Right);

      //Create the 3D projection for this model
      Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio,
          1.0f, farPlaneDistance);

      // Render the skinned mesh.
      foreach (ModelMesh mesh in currentModel.Meshes)
      {
        foreach (SkinnedEffect effect in mesh.Effects)
        {
          effect.SetBoneTransforms(bones);

          effect.View = view;
          effect.Projection = projection;

          effect.EnableDefaultLighting();

          effect.SpecularColor = new Vector3(0.25f);
          effect.SpecularPower = 16;
        }
        mesh.Draw();
      }
    }
    public void PlayAnimation(String Animation)
    {
      clip = skinningData.AnimationClips[Animation];
      if (clip != animationPlayer.CurrentClip)
        animationPlayer.StartClip(clip);
    }
  }
}
