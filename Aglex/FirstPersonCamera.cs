using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Aglex
{
    /* Modified/extended from http://neokabuto.blogspot.de/2014/01/opentk-tutorial-5-basic-camera.html */
    class FirstPersonCamera
    {
        public const double MoveSpeed = 0.2;
        public const double MouseSensitivity = 0.005;

        public Vector3d Position { get { return position; } }
        public Vector3d Orientation { get { return orientation; } }

        Vector3d position, orientation;

        KeyConfigStruct keyConfig;
        public KeyConfigStruct KeyConfiguration
        {
            get { return keyConfig; }
            set { keyConfig = value; }
        }

        public HashSet<Keys> KeysHeld { get; set; }
        public MouseButtons MouseButtonsHeld { get; set; }
        public Point MouseCenter { get; set; }
        public Point MousePosition { get; set; }

        public FirstPersonCamera()
        {
            Reset();

            keyConfig = new KeyConfigStruct();
            keyConfig.MoveForward = Keys.W;
            keyConfig.MoveBackward = Keys.S;
            keyConfig.StrafeLeft = Keys.A;
            keyConfig.StrafeRight = Keys.D;

            KeysHeld = new HashSet<Keys>();
        }

        public void Reset()
        {
            position = Vector3d.Zero;
            orientation = new Vector3d(Math.PI, 0.0, 0.0);
        }

        public Matrix4d GetViewMatrix()
        {
            Vector3d lookat = new Vector3d();

            lookat.X = (Math.Sin(orientation.X) * Math.Cos(orientation.Y));
            lookat.Y = Math.Sin(orientation.Y);
            lookat.Z = (Math.Cos(orientation.X) * Math.Cos(orientation.Y));

            return Matrix4d.LookAt(position, position + lookat, Vector3d.UnitY);
        }

        public void Update()
        {
            if (KeysHeld.Contains(keyConfig.MoveForward)) this.Move(0.0, 0.1, 0.0);
            if (KeysHeld.Contains(keyConfig.MoveBackward)) this.Move(0.0, -0.1, 0.0);
            if (KeysHeld.Contains(keyConfig.StrafeLeft)) this.Move(-0.1, 0.0, 0.0);
            if (KeysHeld.Contains(keyConfig.StrafeRight)) this.Move(0.1, 0.0, 0.0);

            Point delta = new Point(MouseCenter.X - MousePosition.X, MouseCenter.Y - MousePosition.Y);
            this.AddRotation(delta.X, delta.Y);

            MouseCenter = MousePosition;
        }

        private void Move(double x, double y, double z)
        {
            Vector3d offset = new Vector3d();

            Vector3d forward = new Vector3d(Math.Sin(orientation.X), Math.Sin(orientation.Y), Math.Cos(orientation.X));
            Vector3d right = new Vector3d(-forward.Z, 0.0, forward.X);

            offset += x * right;
            offset += y * forward;
            offset.Y += z;

            offset.NormalizeFast();
            offset = Vector3d.Multiply(offset, MoveSpeed);

            position += offset;
        }

        private void AddRotation(double x, double y)
        {
            x = x * MouseSensitivity;
            y = y * MouseSensitivity;

            orientation.X = (orientation.X + x) % (Math.PI * 2.0f);
            orientation.Y = Math.Max(Math.Min(orientation.Y + y, Math.PI / 2.0f - 0.1f), -Math.PI / 2.0f + 0.1f);
        }

        public struct KeyConfigStruct
        {
            public Keys MoveForward { get; set; }
            public Keys MoveBackward { get; set; }
            public Keys StrafeLeft { get; set; }
            public Keys StrafeRight { get; set; }
        }
    }
}
