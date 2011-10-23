/*
 * Dual Paraboloid Reflection Maps
 * 
 * Original code by Kyle Hayward
 * graphicsrunner@gmail.com
 * http://graphicsrunner.blogspot.com
 *
 * XNA 4.0 port by Chris Cajas
 * ccricers@gmail.com
 * http://electronicmeteor.wordpress.com
 *
 */

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace DPReflections
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class DPReflections : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch mSpriteBatch;
        private SpriteFont mSpriteFont;

        /// The main viewing camera
        private Camera camera;

		/// The 'camera' of the paraboloids
        private Camera dualParaCamera;

        /// A list of models that we will render
        private List<BaseMesh> mModels;

        /// Keep a pointer to our reflective object
        private BaseMesh mReflector;

        /// The paraboloid maps that we will use for reflections
        private RenderTarget2D mDPMapFront;
        private RenderTarget2D mDPMapBack;

		/// Moving the mouse
        private float mVelocity;
        private float mMouseScale;

		/// Framerate tracking
        private int mFrameCount;
        private float mElapsedTime;
        private int mFrameRate; 

		/// <summary>
		/// Setup basic program settings
		/// </summary>

        public DPReflections()
        {
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = 1280;
			graphics.PreferredBackBufferHeight = 720;
			graphics.PreferMultiSampling = true;
			graphics.SynchronizeWithVerticalRetrace = false;

			IsFixedTimeStep = false;
            IsMouseVisible = false;

            Content.RootDirectory = "Content";

            mFrameCount = 0;
            mElapsedTime = 0.0f;
            mFrameRate = 0;

            mVelocity = 20.0f;
            mMouseScale = 0.5f;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>

        protected override void Initialize()
        {
            mModels = new List<BaseMesh>();

            // Create our camera

            camera = new Camera();
            camera.LookAt(new Vector3(0, 5, 20), new Vector3(0, 0, 0));
			float aspect = (float)graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight;
            camera.SetLens(MathHelper.ToRadians(45.0f), aspect, .1f, 1000.0f);
            camera.BuildView();

            // Setup our dual-paraboloid camera

            dualParaCamera = new Camera();
            dualParaCamera.LookAt(new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitZ * 6);
            dualParaCamera.SetLens(MathHelper.ToRadians(90.0f), aspect, .1f, 1000.0f);

            // Make sure to set the projection matrix to the identity matrix sence we won't be using it

            dualParaCamera.Projection = Matrix.Identity;
            dualParaCamera.BuildView(); 

            List<Light> lights = createLights();

			// Initialize the reflective model

            mReflector = new BaseMesh(this);
			mReflector.MeshAsset = "Models/Dragon";
            mReflector.EffectAsset = "Shaders/DPMReflect";
			mReflector.Position = dualParaCamera.View.Translation;
            mReflector.Scale = Vector3.One * 3.0f;

            mModels.Add(mReflector);
			
            // Initialize the Environment box

            BaseMesh mesh = new Environment(this);
			mesh.MeshAsset = "Models/SphereHighPoly";
            mesh.Scale = Vector3.One * 500.0f;
            mesh.EffectAsset = "Shaders/EnvironmentMap";
            mesh.TextureAsset = "Textures/whitetex";
            mesh.EnvironmentTextureAsset = "Textures/graceCUBE";

            mModels.Add(mesh);

            // Initialize the impostors

			BaseMesh quadMesh = new BaseMesh(this);
			quadMesh.Position = new Vector3(0, -5f, 0);
			quadMesh.Rotation = 0f;
			quadMesh.Scale = new Vector3(1.5f);
			quadMesh.MeshAsset = "Models/Ground";
			quadMesh.EffectAsset = "Shaders/Texture";
			quadMesh.TextureAsset = "Textures/wood";
			quadMesh.EnableLighting = false;

			mModels.Add(quadMesh);

            float p = 0.0f, y = 0.0f;

            for (int i = 0; i < 8; i++)
            {
                Vector3 pos = Vector3.Zero;
                pos.Y = y;

                p += 1.0f / 8.0f;
                Vector2 pos2 = Utils.GetVector2FromPolarCoord(p) * 8.0f;
                pos.X = pos2.X;
                pos.Z = pos2.Y;

                mesh = new BaseMesh(this);
                mesh.Position = pos;
                mesh.Scale = Vector3.One * 1.0f;
                mesh.RotationAxis = Vector3.UnitY;
                mesh.Rotation = 1.0f;
                mesh.EffectAsset = "Shaders/Phong";
				mesh.MeshAsset = "Models/SphereHighPoly";
                mesh.TextureAsset = "Textures/whitetex";
                mesh.Lights = lights;

                mModels.Add(mesh);
            }

			// Initialize the render targets for the paraboloid maps

			int width = graphics.PreferredBackBufferWidth;
			int height = graphics.PreferredBackBufferHeight;
            mDPMapFront = new RenderTarget2D(GraphicsDevice, width, height, true, 
				SurfaceFormat.Color, DepthFormat.Depth24);
            mDPMapBack = new RenderTarget2D(GraphicsDevice, width, height, true, 
				SurfaceFormat.Color, DepthFormat.Depth24);

            // Finally, initialize the models

            foreach (BaseMesh model in mModels)
			{
                model.Initialize();
			}

            base.Initialize();
        }

		Vector2 stringPos;

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            mSpriteBatch = new SpriteBatch(GraphicsDevice);

            //load the sprite font
            mSpriteFont = Content.Load<SpriteFont>("Font");

            //load the content for our models
            foreach (BaseMesh mesh in mModels)
                mesh.LoadContent();

			stringPos = new Vector2(30, 60);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>

        protected override void Update(GameTime gameTime)
        {
			
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            mElapsedTime += timeDelta;
			
            #region Update Input
            KeyboardState keyState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (keyState.IsKeyDown(Keys.Escape))
                this.Exit();

            if (keyState.IsKeyDown(Keys.W))
                camera.Walk(-mVelocity * timeDelta);
            if (keyState.IsKeyDown(Keys.S))
                camera.Walk(mVelocity * timeDelta);
            if (keyState.IsKeyDown(Keys.A))
                camera.Strafe(-mVelocity * timeDelta);
            if (keyState.IsKeyDown(Keys.D))
                camera.Strafe(mVelocity * timeDelta);

            camera.UpdateMouse(Mouse.GetState(), timeDelta * mMouseScale);
            camera.BuildView(); 
            #endregion

            //update the models
            foreach (BaseMesh mesh in mModels)
                mesh.Update(gameTime);
			
            //update the frame rate
            if (mElapsedTime >= 0.5f)
            {
                mFrameRate = mFrameCount * 2;
                mElapsedTime = 0.0f;
                mFrameCount = 0;
            }
			
			base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>

        protected override void Draw(GameTime gameTime)
        {		
            //first update the paraboloid maps
			updateDPMaps();

			//now apply the maps to the shader for the reflector
			mReflector.Effect.Parameters["Front"].SetValue(mDPMapFront);
			mReflector.Effect.Parameters["Back"].SetValue(mDPMapBack);
			mReflector.Effect.Parameters["ParaboloidBasis"].SetValue(dualParaCamera.View);

			GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // finally, draw all our objects
            foreach (BaseMesh mesh in mModels)
			{
                mesh.Draw(camera);
			}
			
			// Display the framerate
            string fps = String.Format("FPS: {0}", mFrameRate);

            mSpriteBatch.Begin();
            mSpriteBatch.DrawString(mSpriteFont, fps, new Vector2(30, 30), Color.Red);
            mSpriteBatch.End();
			base.Draw(gameTime);

			mFrameCount++;
        }

		/// <summary>
		/// Redraw the dual paraboloid reflection maps
		/// </summary>

        private void updateDPMaps()
        {
            //translate the paraboloid camera to the origin <0, 0, 0>
            Matrix translate = Matrix.CreateTranslation(-dualParaCamera.View.Translation);
            Matrix view = dualParaCamera.View;
            Matrix viewProj = dualParaCamera.ViewProj;

            dualParaCamera.View = translate * dualParaCamera.View;
            dualParaCamera.ViewProj = dualParaCamera.View * dualParaCamera.Projection;

            //render the objects into the dual-paraboloid map
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //render the front dp map
            GraphicsDevice.SetRenderTarget(mDPMapFront);
            GraphicsDevice.Clear(Color.Transparent);

            foreach (BaseMesh mesh in mModels)
            {
                //make sure we don't render the reflective object
                if (mesh.EffectAsset.Contains("DPMReflect"))
                    continue;

                string tech = mesh.Effect.CurrentTechnique.Name;
                mesh.Effect.CurrentTechnique = mesh.Effect.Techniques["BuildDP"];
                mesh.Effect.Parameters["Direction"].SetValue(1.0f);

                mesh.Draw(dualParaCamera);
                mesh.Effect.CurrentTechnique = mesh.Effect.Techniques[tech];
            }

            // Render the back dp map
            GraphicsDevice.SetRenderTarget(mDPMapBack);
            GraphicsDevice.Clear(Color.Transparent);

            foreach (BaseMesh mesh in mModels)
            {
                // Make sure we don't render the reflective object
                if (mesh.EffectAsset.Contains("DPMReflect"))
                    continue;

                string tech = mesh.Effect.CurrentTechnique.Name;
                mesh.Effect.CurrentTechnique = mesh.Effect.Techniques["BuildDP"];
                mesh.Effect.Parameters["Direction"].SetValue(-1.0f);

                mesh.Draw(dualParaCamera);
                mesh.Effect.CurrentTechnique = mesh.Effect.Techniques[tech];
            }

			GraphicsDevice.SetRenderTarget(null);
        }

		/// <summary>
		/// Set up some directional lights to light up the orbiting spheres
		/// </summary>

        private List<Light> createLights()
        {
            List<Light> lights = new List<Light>(3);

            Light light;
            light.AmbientColor = new Vector4(.15f, .15f, .15f, 1.0f);

            light.DiffuseColor = new Vector4(1.0f, 0.3f, 0.3f, 1.0f);
            light.Direction = new Vector3(1, -1, 1);
            light.Direction.Normalize();

            lights.Add(light);

            light.DiffuseColor = new Vector4(0.15f, 0.15f, 0.5f, 1.0f);
            light.Direction = new Vector3(0, 1, 1);
            light.Direction.Normalize();

            lights.Add(light);

            light.DiffuseColor = new Vector4(0.15f, 0.5f, 0.15f, 1.0f);
            light.Direction = new Vector3(-1, -1, -1);
            light.Direction.Normalize();

            lights.Add(light);

            return lights;
        }
    }
}
