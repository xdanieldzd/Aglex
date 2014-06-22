using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Aglex
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector2 TexCoord;
        public Color4 Color;
        public Vector3 Normal;

        public static readonly int Stride = Marshal.SizeOf(default(Vertex));

        public static readonly int PositionOffset = Marshal.OffsetOf(typeof(Vertex), "Position").ToInt32();
        public static readonly int TexCoordOffset = Marshal.OffsetOf(typeof(Vertex), "TexCoord").ToInt32();
        public static readonly int ColorOffset = Marshal.OffsetOf(typeof(Vertex), "Color").ToInt32();
        public static readonly int NormalOffset = Marshal.OffsetOf(typeof(Vertex), "Normal").ToInt32();

        public Vertex(Vector3 position, Vector2 texCoord, Color4 color, Vector3 normal)
        {
            this.Position = position;
            this.TexCoord = texCoord;
            this.Color = color;
            this.Normal = normal;
        }
    }

    public class VertexBufferException : Exception
    {
        public VertexBufferException() : base() { }
        public VertexBufferException(string message) : base(message) { }
        public VertexBufferException(string message, Exception innerException) : base(message, innerException) { }
        public VertexBufferException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class VertexBuffer : IDisposable
    {
        public const int PositionAttributeIndex = 0;
        public const int TexCoordAttributeIndex = 1;
        public const int ColorAttributeIndex = 2;
        public const int NormalAttributeIndex = 3;

        int vertexBufferId, indexBufferId, indexCount;
        PrimitiveType primitiveType;
        Aglex.RenderPassManager renderPassManager;

        public VertexBuffer() : this(16) { }

        public VertexBuffer(int maxRenderPasses)
        {
            vertexBufferId = GL.GenBuffer();
            indexBufferId = GL.GenBuffer();

            primitiveType = PrimitiveType.Triangles;

            renderPassManager = new Aglex.RenderPassManager(maxRenderPasses);
            renderPassManager.SetRenderPass(0, Aglex.RenderPass.Default);
        }

        public void Dispose()
        {
            if (GL.IsBuffer(vertexBufferId)) GL.DeleteBuffer(vertexBufferId);
            if (GL.IsBuffer(indexBufferId)) GL.DeleteBuffer(indexBufferId);
        }

        private void BindBuffers()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);
        }

        private void ReleaseBuffers()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void SetPrimitiveType(PrimitiveType type)
        {
            primitiveType = type;
        }

        public void SetRenderPass(int number, Aglex.RenderPass info)
        {
            if (renderPassManager == null) throw new VertexBufferException("RenderPassManager is null");

            renderPassManager.SetRenderPass(number, info);
        }

        public void SetVertexData(Vertex[] data)
        {
            if (data == null) throw new VertexBufferException("Vertex data is null");

            int dataSize = (data.Length * Vertex.Stride);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)dataSize, data, BufferUsageHint.StaticDraw);

            int dataStoreSize;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out dataStoreSize);

            if (dataStoreSize != dataSize) throw new VertexBufferException(string.Format("Data upload failed; vertex data is {0} bytes, VBO data store is {1} bytes", dataSize, dataStoreSize));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void SetIndexData(uint[] data)
        {
            if (data == null) throw new VertexBufferException("Index data is null");

            indexCount = data.Length;

            int dataSize = (data.Length * sizeof(uint));
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)dataSize, data, BufferUsageHint.StaticDraw);

            int dataStoreSize;
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out dataStoreSize);

            if (dataStoreSize != dataSize) throw new VertexBufferException(string.Format("Data upload failed; index data is {0} bytes, IBO data store is {1} bytes", dataSize, dataStoreSize));

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Render(int shaderProgram)
        {
            BindBuffers();

            GL.EnableVertexAttribArray(PositionAttributeIndex);
            GL.EnableVertexAttribArray(TexCoordAttributeIndex);
            GL.EnableVertexAttribArray(ColorAttributeIndex);
            GL.EnableVertexAttribArray(NormalAttributeIndex);

            GL.VertexAttribPointer(PositionAttributeIndex, 3, VertexAttribPointerType.Float, false, Vertex.Stride, Vertex.PositionOffset);
            GL.VertexAttribPointer(TexCoordAttributeIndex, 2, VertexAttribPointerType.Float, false, Vertex.Stride, Vertex.TexCoordOffset);
            GL.VertexAttribPointer(ColorAttributeIndex, 4, VertexAttribPointerType.Float, false, Vertex.Stride, Vertex.ColorOffset);
            GL.VertexAttribPointer(NormalAttributeIndex, 3, VertexAttribPointerType.Float, false, Vertex.Stride, Vertex.NormalOffset);

            renderPassManager.Perform(new Action(() =>
            {
                GL.Uniform1(GL.GetUniformLocation(shaderProgram, "passNumber"), renderPassManager.CurrentPass);
                GL.DrawElements(primitiveType, indexCount, DrawElementsType.UnsignedInt, 0);
            }));

            GL.DisableVertexAttribArray(PositionAttributeIndex);
            GL.DisableVertexAttribArray(TexCoordAttributeIndex);
            GL.DisableVertexAttribArray(ColorAttributeIndex);
            GL.DisableVertexAttribArray(NormalAttributeIndex);

            ReleaseBuffers();
        }

        public static void CalculateNormals(ref Vertex[] vertices, uint[] indices, int numFaces)
        {
            /* Surface normals - http://www.opengl.org/wiki/Calculating_a_Surface_Normal#Newell.27s_Method */
            OpenTK.Vector3[] surfaceNormals = new OpenTK.Vector3[numFaces];

            for (int i = 0; i < indices.Length; i += 3)
            {
                OpenTK.Vector3 surfaceNormal = OpenTK.Vector3.Zero;

                for (int v = 0; v < 3; v++)
                {
                    OpenTK.Vector3 currentVertex = vertices[indices[i + v]].Position;
                    OpenTK.Vector3 nextVertex = vertices[indices[i + ((v + 1) % 3)]].Position;

                    surfaceNormal.X += (currentVertex.Y - nextVertex.Y) * (currentVertex.Z + nextVertex.Z);
                    surfaceNormal.Y += (currentVertex.Z - nextVertex.Z) * (currentVertex.X + nextVertex.X);
                    surfaceNormal.Z += (currentVertex.X - nextVertex.X) * (currentVertex.Y + nextVertex.Y);
                }

                surfaceNormal.Normalize();

                surfaceNormals[i / 3] = surfaceNormal;
            }

            /* Vertex normals - https://www.opengl.org/discussion_boards/showthread.php/128451-How-to-calculate-vertex-normals?p=966239&viewfull=1#post966239 */
            int shared = 0;
            OpenTK.Vector3 sum = OpenTK.Vector3.Zero;

            for (int v = 0; v < vertices.Length; v++)
            {
                for (int f = 0; f < indices.Length; f += 3)
                {
                    if (vertices[indices[f]].Position == vertices[v].Position || vertices[indices[f + 1]].Position == vertices[v].Position || vertices[indices[f + 2]].Position == vertices[v].Position)
                    {
                        sum += surfaceNormals[indices[f / 3]];
                        shared++;
                    }
                }

                sum /= (float)shared;
                sum.Normalize();

                vertices[v].Normal = sum;

                sum = OpenTK.Vector3.Zero;
                shared = 0;
            }
        }
    }
}
