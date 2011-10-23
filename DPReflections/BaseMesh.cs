using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace DPReflections
{
    public struct Light
    {
        public Vector3 Direction;
        public Vector4 DiffuseColor;
        public Vector4 AmbientColor;
    }

    public class BaseMesh 
    {
        #region Fields
        private int mID;

        private  Game mGame;

        protected GraphicsDevice mGraphicsDevice;
        protected Vector3 mPosition;
        protected Vector3 mVelocity;
        protected Vector3 mScale;
        protected float mRotation;
        protected Vector3 mRotationAxis;
        protected float mOrientation;
        protected Vector3 mOrientAxis;
        protected Matrix mWorld;

        protected Effect mEffect;
        protected string mEffectAsset;

        protected Model mMesh;
        protected string mMeshAsset;
        protected BoundingBox mBoundBox;
        protected BoundingSphere mBoundSphere;

        protected Texture2D mTexture;
        protected string mTexAsset;
        protected float mTexScale;

        protected TextureCube mEnvTexture;
        protected string mEnvTexAsset;

        protected List<Light> mLights;

        protected bool mLightingEnabled;

        protected float mTotalRotation;
        protected float mTotalOrientation;
        #endregion

        #region Properties
        public int ID
        {
            get { return mID; }
            set { mID = value; }
        }

        protected Game Game
        {
            get { return mGame; }
        }

        public Model Model
        {
            get { return mMesh; }
        }

        public Vector3 Position
        {
            get { return mPosition; }
            set { mPosition = value; }
        }

        public Vector3 Velocity
        {
            get { return mVelocity; }
            set { mVelocity = value; }
        }

        protected Vector3 Center
        {
            get
            {
                Vector3 min = Vector3.Transform(mBoundBox.Min, mWorld);
                Vector3 max = Vector3.Transform(mBoundBox.Max, mWorld);

                return (min + max) * .5f;
            }
        }

        public Vector3 Scale
        {
            get { return mScale; }
            set { mScale = value; }
        }

        public float Orienation
        {
            get { return mOrientation; }
            set { mOrientation = value; }
        }

        public Vector3 OrientationAxis
        {
            get { return mOrientAxis; }
            set { mOrientAxis = value; }
        }

        public float Rotation
        {
            get { return mRotation; }
            set { mRotation = value; }
        }

        public Vector3 RotationAxis
        {
            get { return mRotationAxis; }
            set { mRotationAxis = value; }
        }

        public Matrix World
        {
            get { return mWorld; }
            set { mWorld = value; }
        }

        public Effect Effect
        {
            get { return mEffect; }
            set { mEffect = value; }
        }

        public string EffectAsset
        {
            get { return mEffectAsset; }
            set { mEffectAsset = value; }
        }

        public string MeshAsset
        {
            get { return mMeshAsset; }
            set { mMeshAsset = value; }
        }

        public string TextureAsset
        {
            get { return mTexAsset; }
            set { mTexAsset = value; }
        }

        public float TextureScale
        {
            get { return mTexScale; }
            set { mTexScale = value; }
        }

        public string EnvironmentTextureAsset
        {
            get { return mEnvTexAsset; }
            set { mEnvTexAsset = value; }
        }

        public List<Light> Lights
        {
            get { return mLights; }
            set { mLights = value; }
        }

        public bool EnableLighting
        {
            get { return mLightingEnabled; }
            set { mLightingEnabled = value; }
        }
        #endregion

        public BaseMesh(Game game)
        {
            mGame = game;
            mGraphicsDevice = game.GraphicsDevice;

            mVelocity = Vector3.Zero;
            mPosition = Vector3.Zero;
            mScale = Vector3.One;
            mOrientation = 0.0f;
            mTotalRotation = 0.0f;
            mOrientAxis = Vector3.Up;

            mTexScale = 1.0f;

            mLights = new List<Light>(3);
            mLightingEnabled = true;

        }

        public virtual void Initialize()
        {
            mWorld = Matrix.CreateScale(mScale) * 
                     Matrix.CreateFromAxisAngle(mOrientAxis, 0.0f) *
                     Matrix.CreateTranslation(mPosition);
        }

        public virtual void LoadContent()
        {
            //create and load the effect
            if (mEffectAsset != null)
            {
                mEffect = Game.Content.Load<Effect>(mEffectAsset);

                int i = 0;
                try
                {
                    foreach (Light light in mLights)
                    {
                        mEffect.Parameters["LightDir" + i].SetValue(light.Direction);
                        mEffect.Parameters["LightDiffuse" + i].SetValue(light.DiffuseColor);
                        mEffect.Parameters["LightAmbient" + i].SetValue(light.AmbientColor);
                        i++;
                    }
                }
                catch
                {
                    //no light parameters specified in shader
                }
            }

            //create and load the mesh 
            if (mMeshAsset != null)
            {
                mMesh = Game.Content.Load<Model>(mMeshAsset);
            }

            //create and load the texture
            if (mTexAsset != null)
            {
                mTexture = Game.Content.Load<Texture2D>(mTexAsset);
            }

            //create and load the environment map
            if (mEnvTexAsset != null)
            {
                mEnvTexture = Game.Content.Load<TextureCube>(mEnvTexAsset);
                mEffect.Parameters["EnvMap"].SetValue(mEnvTexture);
            }
        }

        public virtual void UnloadContent()
        {

        }

        public virtual void Update(GameTime gameTime)
        {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            mTotalRotation += timeDelta * mRotation;
            mTotalOrientation += timeDelta * mOrientation;

            if (mTotalRotation >= MathHelper.TwoPi)
                mTotalRotation = 0.0f;

            if (mTotalOrientation >= MathHelper.TwoPi)
                mTotalRotation = 0.0f;

            mPosition += mVelocity;

            mWorld = Matrix.CreateScale(mScale) *
                     Matrix.CreateFromAxisAngle(mOrientAxis, mTotalOrientation) *
                     Matrix.CreateTranslation(mPosition) *
                     Matrix.CreateFromAxisAngle(mRotationAxis, mTotalRotation);
        }

        public virtual void Draw(Camera camera)
        {
			mGraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            //draw with the basic effect
            if (mEffect == null)
            {
                DrawBasicEffect(camera);
            }
            else
            {
                DrawCustomEffect(camera);
            }
        }

        protected virtual void DrawBasicEffect(Camera camera)
        {
            Matrix[] boneTransforms = new Matrix[mMesh.Bones.Count];
            mMesh.CopyAbsoluteBoneTransformsTo(boneTransforms);
            
            foreach (ModelMesh mesh in mMesh.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * mWorld;
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;

                    if (mTexture != null)
                    {
                        effect.Texture = mTexture;
                        effect.TextureEnabled = true;
                    }

                    if (mLightingEnabled)
                    {
                        effect.EnableDefaultLighting();
                        effect.PreferPerPixelLighting = true;
                    }
                    else
                    {
                        effect.LightingEnabled = false;
                    }
                }
                
                mesh.Draw();
            }
        }

        protected virtual void DrawCustomEffect(Camera camera)
        {
            Matrix[] boneTransforms = new Matrix[mMesh.Bones.Count];
            mMesh.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (ModelMesh mesh in mMesh.Meshes)
            {
                Matrix world = boneTransforms[mesh.ParentBone.Index] * mWorld;

                Matrix worldInvTrans = Matrix.Invert(world);
                worldInvTrans = Matrix.Transpose(worldInvTrans);

                mEffect.Parameters["World"].SetValue(world);
                mEffect.Parameters["WorldInvTrans"].SetValue(worldInvTrans);
                mEffect.Parameters["WorldViewProj"].SetValue(world * camera.ViewProj);
                
                if (mTexture != null)
                    mEffect.Parameters["DiffuseTex"].SetValue(mTexture);

                mEffect.Parameters["TexScale"].SetValue(mTexScale);

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
					//set the vertex and index buffers
					mGraphicsDevice.Indices = meshPart.IndexBuffer;
					mGraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);

					foreach (EffectPass pass in mEffect.CurrentTechnique.Passes)
					{
						pass.Apply();
						mGraphicsDevice.DrawIndexedPrimitives(
							PrimitiveType.TriangleList, 0, 0,
							meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
					}
                    // End pass
                }
            }
			// End mesh render
        }               
    }
}
