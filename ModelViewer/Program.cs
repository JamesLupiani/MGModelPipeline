using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using SkinnedModel;

namespace ModelViewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new ModelViewer(args))
                game.Run();
        }
    }

    public class ModelViewer : Game
    {
        private readonly string _fileName;
        private readonly GraphicsDeviceManager _graphics;

        private Model _model;
        private Matrix[] _bones;
        private AnimationPlayer _animationPlayer;

        private Matrix _worldMatrix;
        private Matrix _viewMatrix;
        private Matrix _projMatrix;

        public ModelViewer(string[] args)
        {
            this.Window.Title = "Model Viewer";

            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";

            if (args.Length > 0)
            {
                Window.Title += ": " + args[0];

                _fileName = Path.Combine(
                    Path.GetDirectoryName(args[0]),
                    Path.GetFileNameWithoutExtension(args[0]));
            }
            else
            {
                _fileName = @"Dude\dude";
                //_fileName = @"Ship\ship";
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _model = Content.Load<Model>(_fileName);
            var skinningData = _model.Tag as SkinningData;
            if (skinningData != null)
            {
                _animationPlayer = new AnimationPlayer(skinningData);
                var clip = skinningData.AnimationClips["Take 001"];
                _animationPlayer.StartClip(clip);

                _bones = _animationPlayer.GetSkinTransforms();
            }
            else
            {
                _bones = new Matrix[_model.Bones.Count];
                _model.CopyAbsoluteBoneTransformsTo(_bones);
            }

            var bestFit = new BoundingSphere();
            foreach (var mesh in _model.Meshes)
            {
                if (bestFit.Contains(mesh.BoundingSphere) != ContainmentType.Contains)
                    bestFit = BoundingSphere.CreateMerged(bestFit, mesh.BoundingSphere);
            }

            _worldMatrix = Matrix.Identity;
            _viewMatrix = Matrix.CreateTranslation(bestFit.Center) * Matrix.CreateTranslation(0, 0, -bestFit.Radius * 4);
            _projMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, bestFit.Radius * 5.0f);

            foreach (var mesh in _model.Meshes)
            {
                foreach (var effect in mesh.Effects)
                {
                    var em = effect as IEffectMatrices;
                    em.World = _worldMatrix;
                    em.View = _viewMatrix;
                    em.Projection = _projMatrix;

                    var basic = effect as BasicEffect;
                    if (basic != null)
                        basic.EnableDefaultLighting();

                    var skinned = effect as SkinnedEffect;
                    if (skinned != null)
                        skinned.EnableDefaultLighting();
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            _worldMatrix = Matrix.CreateRotationY(MathHelper.WrapAngle(MathHelper.TwoPi * (float)gameTime.TotalGameTime.TotalSeconds * 0.25f));

            if (_animationPlayer != null)
                _animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            foreach (var mesh in _model.Meshes)
            {
                foreach (var effect in mesh.Effects)
                {
                    var em = effect as IEffectMatrices;
                    em.World = _worldMatrix *_bones[mesh.ParentBone.Index];
                    em.View = _viewMatrix;
                    em.Projection = _projMatrix;

                    var skinned = effect as SkinnedEffect;
                    if (skinned != null)
                        skinned.SetBoneTransforms(_bones);
                }

                mesh.Draw();
            }
        }
    }
}
