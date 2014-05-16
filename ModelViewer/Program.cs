using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

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
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _model = Content.Load<Model>(_fileName);
            _bones = new Matrix[_model.Bones.Count];

            var bestFit = new BoundingSphere();
            foreach (var mesh in _model.Meshes)
            {
                if (bestFit.Contains(mesh.BoundingSphere) != ContainmentType.Contains)
                    bestFit = BoundingSphere.CreateMerged(bestFit, mesh.BoundingSphere);
            }

            _worldMatrix = Matrix.Identity;
            _viewMatrix = Matrix.CreateTranslation(bestFit.Center) * Matrix.CreateTranslation(0, 0, bestFit.Radius * 1.5f);
            _projMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, bestFit.Radius * 2.0f);
        }

        protected override void Update(GameTime gameTime)
        {
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _model.CopyAbsoluteBoneTransformsTo(_bones);
            foreach (var mesh in _model.Meshes)
            {
                foreach (IEffectMatrices em in mesh.Effects)
                {
                    em.World = _bones[mesh.ParentBone.Index];
                    em.View = _viewMatrix;
                    em.Projection = _projMatrix;
                }

                mesh.Draw();
            }
        }
    }
}
