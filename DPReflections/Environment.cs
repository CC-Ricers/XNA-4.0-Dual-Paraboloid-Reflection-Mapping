using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace DPReflections
{
    public class Environment : BaseMesh
    {
        public Environment(Game game)
            : base(game)
        {

        }

        public override void LoadContent()
        {
            base.LoadContent();
        }

        public override void Draw(Camera camera)
        {
			mGraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
			mGraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            Matrix world = mWorld * Matrix.CreateTranslation(camera.Position);
            mEffect.Parameters["WorldViewProj"].SetValue(world * camera.ViewProj);

			foreach (ModelMesh mesh in mMesh.Meshes)
			{
				foreach (ModelMeshPart meshPart in mesh.MeshParts)
				{
					//set the index buffer 
					mGraphicsDevice.Indices = meshPart.IndexBuffer;
					mGraphicsDevice.SetVertexBuffer(
						meshPart.VertexBuffer, meshPart.VertexOffset);

					foreach (EffectPass pass in mEffect.CurrentTechnique.Passes)
					{
						pass.Apply();
						mGraphicsDevice.DrawIndexedPrimitives(
							PrimitiveType.TriangleList, 0, 0,
							meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
					}
                }
            }

            mGraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }
    }
}
