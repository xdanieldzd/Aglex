using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Aglex
{
    [Description("Used in ProjectionType")]
    public enum ProjectionTypes { None, Perspective, Orthographic };

    internal class Helpers
    {
        public static int GetMaxAASamples()
        {
            List<int> maxSamples = new List<int>();
            int retVal = 0;
            try
            {
                int samples = 0;
                do
                {
                    GraphicsMode mode = new GraphicsMode(GraphicsMode.Default.ColorFormat, GraphicsMode.Default.Depth, GraphicsMode.Default.Stencil, samples);
                    if (!maxSamples.Contains(mode.Samples)) maxSamples.Add(samples);
                    samples += 2;
                }
                while (samples <= 32);
            }
            finally
            {
                retVal = maxSamples.Last();
            }

            return retVal;
        }

        public static void CreateViewportAndProjection(Rectangle clientRectangle, ProjectionTypes projectionType, Matrix4d cameraMatrix)
        {
            /* Error checking, do not execute if... */
            if (!Toolkit.IsReady || clientRectangle.Width <= 0 || clientRectangle.Height <= 0) return;

            /* Set viewport */
            GL.Viewport(0, 0, clientRectangle.Width, clientRectangle.Height);

            /* Determine and create matrices needed */
            Matrix4d projectionMatrix = new Matrix4d();

            switch (projectionType)
            {
                case ProjectionTypes.Perspective:
                    double aspect = clientRectangle.Width / (double)clientRectangle.Height;
                    projectionMatrix = cameraMatrix * Matrix4d.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)aspect, 0.1f, 15000.0f);
                    break;

                case ProjectionTypes.Orthographic:
                    projectionMatrix = cameraMatrix * Matrix4d.CreateOrthographicOffCenter(clientRectangle.Left, clientRectangle.Right, clientRectangle.Bottom, clientRectangle.Top, -0.1f, 15000.0f);
                    break;

                default:
                    throw new AglexException("Unhandled AutoProjectionType specified");
            }

            /* Load and apply matrices */
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.MultMatrix(ref projectionMatrix);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }
    }
}
