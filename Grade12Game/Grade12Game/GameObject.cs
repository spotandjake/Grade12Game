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
  public abstract class GameObject
  {
    // Internals
    private Vector3 position;
    private Vector3 rotation;
    private Vector3 scale;
    private Model model;
    private SkinningData skinningData;
    private AnimationPlayer animationPlayer;
    public AnimationClip animationClip;
    // Constructor
    public GameObject(Model model, Vector3 position, Vector3 rotation, Vector3 scale)
    {
      // Set Internals
      this.setModel(model);
      this.setPosition(position);
      this.setRotation(rotation);
      this.setScale(scale);
    }
    // Public Methods
    public void Update(GameTime gameTime)
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
      if (this.skinningData == null)
        throw new InvalidOperationException
            ("This model does not contain a SkinningData tag.");
      // Set Animation Player
      this.animationPlayer = new AnimationPlayer(this.skinningData);
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
    public void PlayAnimation(String Animation)
    {
      this.animationClip = this.skinningData.AnimationClips[Animation];
      if (this.animationClip != this.animationPlayer.CurrentClip)
      {
        this.animationPlayer.StartClip(this.animationClip);
      }
    }
  }
}
