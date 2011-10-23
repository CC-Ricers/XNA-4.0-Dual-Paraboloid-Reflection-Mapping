using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DPReflections
{
	public partial class Quad : BaseMesh
	{
		#region Private Members

		VertexDeclaration vertexDecl = null;
		VertexPositionTexture[] verts = null;
		short[] ib = null;

		GraphicsDevice device;

		#endregion

		public Quad(Game game)
			: base(game)
		{
			device = game.GraphicsDevice;
			// TODO: Construct any child components here
		}

		#region LoadGraphicsContent

		public override void LoadContent()
		{
			vertexDecl = VertexPositionTexture.VertexDeclaration;

			verts = new VertexPositionTexture[]
            {
                new VertexPositionTexture(
                    new Vector3(0,0,1),
                    new Vector2(1,1)),
                new VertexPositionTexture(
                    new Vector3(0,0,1),
                    new Vector2(0,1)),
                new VertexPositionTexture(
                    new Vector3(0,0,1),
                    new Vector2(0,0)),
                new VertexPositionTexture(
                    new Vector3(0,0,1),
                    new Vector2(1,0))
            };

			ib = new short[] { 0, 1, 2, 2, 3, 0 }; // 0 -- 1
												   // |    |
												   // 2 -- 3

			base.LoadContent();
		}
		#endregion

		#region void Render(Vector2 v1, Vector2 v2)

		public override void Draw(Camera camera)
		{
			Vector2 v1 = new Vector2(Scale.X, Scale.Y);
			Vector2 v2 = new Vector2(Scale.X, Scale.Y);
			v2 = -v2;

			verts[0].Position.X = v2.X;
			verts[0].Position.Y = v1.Y;

			verts[1].Position.X = v1.X;
			verts[1].Position.Y = v1.Y;

			verts[2].Position.X = v1.X;
			verts[2].Position.Y = v2.Y;

			verts[3].Position.X = v2.X;
			verts[3].Position.Y = v2.Y;
			/*
            foreach (ModelMesh mesh in mMesh.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = mWorld;//boneTransforms[mesh.ParentBone.Index] * mWorld;
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
			}
			*/

			Matrix world = mWorld * Matrix.CreateTranslation(camera.Position);

			mEffect.CurrentTechnique = mEffect.Techniques["Texture"];
			mEffect.Parameters["WorldViewProj"].SetValue(world * camera.ViewProj);

			foreach (EffectPass pass in mEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				device.DrawUserIndexedPrimitives<VertexPositionTexture>
					(PrimitiveType.TriangleList, verts, 0, 4, ib, 0, 2);
			}

			mEffect.CurrentTechnique = mEffect.Techniques["BuildDP"];
		}
		#endregion
	}
}

