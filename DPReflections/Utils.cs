using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace DPReflections
{
    public static class Utils
    {
        public static void GenTriGrid(int numVertRows, int numVertCols, float dx, float dz,
                                Vector3 center, out Vector3[] verts, out int[] indices)
        {
            int numVertices = numVertRows * numVertCols;
            int numCellRows = numVertRows - 1;
            int numCellCols = numVertCols - 1;

            int mNumTris = numCellRows * numCellCols * 2;

            float width = (float)numCellCols * dx;
            float depth = (float)numCellRows * dz;

            //===========================================
            // Build vertices.

            // We first build the grid geometry centered about the origin and on
            // the xz-plane, row-by-row and in a top-down fashion.  We then translate
            // the grid vertices so that they are centered about the specified 
            // parameter 'center'.

            //verts.resize(numVertices);
            verts = new Vector3[numVertices];

            // Offsets to translate grid from quadrant 4 to center of 
            // coordinate system.
            float xOffset = -width * 0.5f;
            float zOffset = depth * 0.5f;

            int k = 0;
            for (float i = 0; i < numVertRows; ++i)
            {
                for (float j = 0; j < numVertCols; ++j)
                {
                    // Negate the depth coordinate to put in quadrant four.  
                    // Then offset to center about coordinate system.
                    verts[k] = new Vector3(0, 0, 0);
                    verts[k].X = j * dx + xOffset;
                    verts[k].Z = -i * dz + zOffset;
                    verts[k].Y = 0.0f;

                    Matrix translation = Matrix.CreateTranslation(center);
                    verts[k] = Vector3.Transform(verts[k], translation);

                    ++k; // Next vertex
                }
            }

            //===========================================
            // Build indices.

            //indices.resize(mNumTris * 3);
            indices = new int[mNumTris * 3];

            // Generate indices for each quad.
            k = 0;
            for (int i = 0; i < numCellRows; ++i)
            {
                for (int j = 0; j < numCellCols; ++j)
                {
                    indices[k + 2] = i * numVertCols + j;
                    indices[k + 1] = i * numVertCols + j + 1;
                    indices[k + 0] = (i + 1) * numVertCols + j;

                    indices[k + 5] = (i + 1) * numVertCols + j;
                    indices[k + 4] = i * numVertCols + j + 1;
                    indices[k + 3] = (i + 1) * numVertCols + j + 1;

                    // next quad
                    k += 6;
                }
            }
        }

        public static Rectangle LerpRect(Rectangle x, Rectangle y, float s)
        {
            if (s == 0.0f)
                return x;

            if (s == 1.0f)
                return y;

            Rectangle ret = new Rectangle();

            ret.X = (int)MathHelper.Lerp(x.X, y.X, s);
            ret.Y = (int)MathHelper.Lerp(x.Y, y.Y, s);
            ret.Width = (int)MathHelper.Lerp(x.Width, y.Width, s);
            ret.Height = (int)MathHelper.Lerp(x.Height, y.Height, s);

            return ret;
        }

        #region Random generator functions
        private static Random rand = new Random();
        public static float RandomFloat(float min, float max)
        {
            return (float)(min + (float)rand.NextDouble() * (max - min));

        }

        public static int RandomInt(int min, int max)
        {
            return rand.Next(min, max);
        }

        /// <summary>
        /// Returns either 1 or -1 with a 50% chance of either
        /// </summary>
        /// <returns></returns>
        public static float RandomSign()
        {
            return RandomFloat(0.0f, 1.0f) >= .5f ? 1.0f : -1.0f;
        }

        /// <summary>
        /// Returns a random 2d unit vector
        /// </summary>
        /// <returns></returns>
        public static Vector2 RandomVector2()
        {
            float azimuth = (float)rand.NextDouble() * 2.0f * (float)Math.PI;
            return new Vector2((float)Math.Cos(azimuth), (float)Math.Sin(azimuth));
        }

        /// <summary>
        /// Returns a random 2d unit vector
        /// </summary>
        /// <returns></returns>
        public static void RandomVector2(ref Vector2 vector)
        {
            float azimuth = (float)rand.NextDouble() * 2.0f * (float)Math.PI;
            vector.X = (float)Math.Cos(azimuth);
            vector.Y = (float)Math.Sin(azimuth);
        }

        /// <summary>
        /// Returns a random 3d unit vector
        /// </summary>
        /// <returns></returns>
        public static Vector3 RandomVector3()
        {
            float z = (2 * (float)rand.NextDouble()) - 1; // z is in the range [-1,1]
            Vector2 planar = RandomVector2() * (float)Math.Sqrt(1 - z * z);
            return new Vector3(planar, z);
        }

        /// <summary>
        /// Returns a random 3d unit vector
        /// </summary>
        /// <returns></returns>
        public static void RandomVector3(ref Vector3 vector)
        {
            float z = (2 * (float)rand.NextDouble()) - 1; // z is in the range [-1,1]
            Vector2 planar = RandomVector2() * (float)Math.Sqrt(1 - z * z);
            vector.X = planar.X;
            vector.Y = planar.Y;
            vector.Z = z;
        } 
        #endregion

        #region Vector functions
        public static Vector2 GetVector2FromPolarCoord(float p)
        {
            float azimuth = p * 2.0f * (float)Math.PI;
            return new Vector2((float)Math.Cos(azimuth), (float)Math.Sin(azimuth));
        }
        #endregion

        #region Matrix functions
        public static Matrix MultiplyMatrix(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrix;
            matrix.M11 = (((matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21)) + (matrix1.M13 * matrix2.M31)) + (matrix1.M14 * matrix2.M41);
            matrix.M12 = (((matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22)) + (matrix1.M13 * matrix2.M32)) + (matrix1.M14 * matrix2.M42);
            matrix.M13 = (((matrix1.M11 * matrix2.M13) + (matrix1.M12 * matrix2.M23)) + (matrix1.M13 * matrix2.M33)) + (matrix1.M14 * matrix2.M43);
            matrix.M14 = (((matrix1.M11 * matrix2.M14) + (matrix1.M12 * matrix2.M24)) + (matrix1.M13 * matrix2.M34)) + (matrix1.M14 * matrix2.M44);
            
			matrix.M21 = (((matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21)) + (matrix1.M23 * matrix2.M31)) + (matrix1.M24 * matrix2.M41);
            matrix.M22 = (((matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22)) + (matrix1.M23 * matrix2.M32)) + (matrix1.M24 * matrix2.M42);
            matrix.M23 = (((matrix1.M21 * matrix2.M13) + (matrix1.M22 * matrix2.M23)) + (matrix1.M23 * matrix2.M33)) + (matrix1.M24 * matrix2.M43);
            matrix.M24 = (((matrix1.M21 * matrix2.M14) + (matrix1.M22 * matrix2.M24)) + (matrix1.M23 * matrix2.M34)) + (matrix1.M24 * matrix2.M44);
            
			matrix.M31 = (((matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21)) + (matrix1.M33 * matrix2.M31)) + (matrix1.M34 * matrix2.M41);
            matrix.M32 = (((matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22)) + (matrix1.M33 * matrix2.M32)) + (matrix1.M34 * matrix2.M42);
            matrix.M33 = (((matrix1.M31 * matrix2.M13) + (matrix1.M32 * matrix2.M23)) + (matrix1.M33 * matrix2.M33)) + (matrix1.M34 * matrix2.M43);
            matrix.M34 = (((matrix1.M31 * matrix2.M14) + (matrix1.M32 * matrix2.M24)) + (matrix1.M33 * matrix2.M34)) + (matrix1.M34 * matrix2.M44);
            
			matrix.M41 = (((matrix1.M41 * matrix2.M11) + (matrix1.M42 * matrix2.M21)) + (matrix1.M43 * matrix2.M31)) + (matrix1.M44 * matrix2.M41);
            matrix.M42 = (((matrix1.M41 * matrix2.M12) + (matrix1.M42 * matrix2.M22)) + (matrix1.M43 * matrix2.M32)) + (matrix1.M44 * matrix2.M42);
            matrix.M43 = (((matrix1.M41 * matrix2.M13) + (matrix1.M42 * matrix2.M23)) + (matrix1.M43 * matrix2.M33)) + (matrix1.M44 * matrix2.M43);
            matrix.M44 = (((matrix1.M41 * matrix2.M14) + (matrix1.M42 * matrix2.M24)) + (matrix1.M43 * matrix2.M34)) + (matrix1.M44 * matrix2.M44);
            return matrix;
        }


        #endregion
    }
}
