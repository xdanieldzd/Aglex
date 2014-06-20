using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Aglex
{
    public class GLControl : OpenTK.GLControl
    {
        [Description("Rendering event; occures when idle")]
        public event EventHandler<EventArgs> Render;

        [Description("Type of projection to apply automatically")]
        public ProjectionTypes ProjectionType { get; set; }

        [Description("Enable or disable integrated first-person camera")]
        public bool Camera { get; set; }

        [Description("Current frames-per-second"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public float FPS { get { return (fpsMonitor != null ? fpsMonitor.Value : 0.0f); } }

        [Description("Enable or disable semi-automatic lighting")]
        public bool Lighting { get; set; }

        bool isRuntime;
        FirstPersonCamera fpsCamera;
        FPSMonitor fpsMonitor;
        float[] lightPos;

        bool isCamActive { get { return (Camera && fpsCamera != null); } }

        public GLControl()
            : base(new OpenTK.Graphics.GraphicsMode(GraphicsMode.Default.ColorFormat, GraphicsMode.Default.Depth, GraphicsMode.Default.Stencil, Helpers.GetMaxAASamples()))
        {
            isRuntime = (LicenseManager.UsageMode != LicenseUsageMode.Designtime);

            /* Return if not at runtime */
            if (!isRuntime) return;

            /* Idle function */
            Application.Idle += ((s, ev) =>
            {
                if (Toolkit.IsReady && IsIdle) this.Invalidate();
            });
        }

        public void ResetCamera()
        {
            fpsCamera.Reset();
        }

        public void SetLightPosition(float[] position)
        {
            lightPos = position;
        }

        protected virtual void OnRender(EventArgs e)
        {
            EventHandler<EventArgs> handler = Render;
            if (handler != null) handler(this, e);
        }

        protected override void OnLoad(EventArgs e)
        {
            /* Do not execute if not at runtime */
            if (!isRuntime) return;

            /* Initialize camera */
            if (Camera) fpsCamera = new FirstPersonCamera();

            /* Initialize FPS monitor */
            fpsMonitor = new FPSMonitor();

            /* Set a few defaults, then mark as ready */
            GL.ClearColor(this.BackColor);
            lightPos = new float[] { 0.0f, 1.0f, 1.5f, 0.0f };

            /* Call base */
            base.OnLoad(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            /* Draw red X if not at runtime */
            if (!isRuntime)
            {
                e.Graphics.Clear(this.BackColor);
                using (Pen pen = new Pen(Color.Red, 3.0f))
                {
                    /* Make it a gorgeous X! :D */
                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                    e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    e.Graphics.DrawLine(pen, Point.Empty, new Point(this.ClientRectangle.Right, this.ClientRectangle.Bottom));
                    e.Graphics.DrawLine(pen, new Point(0, this.ClientRectangle.Bottom), new Point(this.ClientRectangle.Right, 0));
                }
                return;
            }

            /* Update camera */
            if (isCamActive) fpsCamera.Update();

            Matrix4d cameraMatrix = Matrix4d.Identity;

            /* Set viewport/projection if requested */
            if (ProjectionType != ProjectionTypes.None)
            {
                if (isCamActive) cameraMatrix = fpsCamera.GetViewMatrix();
                Helpers.CreateViewportAndProjection(ClientRectangle, ProjectionType, cameraMatrix);
            }

            /* Perform lighting if requested */
            if (Lighting)
            {
                Matrix4d tempMatrix = Matrix4d.Invert(cameraMatrix);

                GL.PushMatrix();
                GL.MultMatrix(ref tempMatrix);
                GL.Light(LightName.Light0, LightParameter.Position, lightPos);
                GL.PopMatrix();
            }

            /* Call render function */
            this.OnRender(EventArgs.Empty);

            /* Update FPS monitor */
            fpsMonitor.Update();

            this.SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            /* Do not execute if not at runtime */
            if (!isRuntime) return;

            base.OnResize(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (isCamActive) fpsCamera.KeysHeld.Add(e.KeyCode);

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (isCamActive) fpsCamera.KeysHeld.Remove(e.KeyCode);

            base.OnKeyUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (isCamActive)
            {
                fpsCamera.MouseButtonsHeld |= e.Button;
                if (Convert.ToBoolean(fpsCamera.MouseButtonsHeld & MouseButtons.Left)) fpsCamera.MousePosition = fpsCamera.MouseCenter = e.Location;
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (isCamActive) fpsCamera.MouseButtonsHeld &= ~e.Button;

            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isCamActive)
            {
                if (Convert.ToBoolean(fpsCamera.MouseButtonsHeld & MouseButtons.Left)) fpsCamera.MousePosition = e.Location;
            }

            base.OnMouseMove(e);
        }
    }
}
