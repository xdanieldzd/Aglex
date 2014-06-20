using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

namespace Aglex
{
    public class Texture : IDisposable
    {
        bool disposed;
        int glID;

        public Texture(string fn) : this((Bitmap)Bitmap.FromFile(fn)) { }

        public Texture(Bitmap srcBitmap)
        {
            Bitmap bitmap = new Bitmap(srcBitmap.Width, srcBitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(srcBitmap, new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height));
            }

            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            int numBytes = (bmpData.Stride * bitmap.Height);
            byte[] byteData = new byte[numBytes];
            Marshal.Copy(bmpData.Scan0, byteData, 0, numBytes);

            bitmap.UnlockBits(bmpData);

            Load(bitmap.Width, bitmap.Height, byteData);
        }

        public Texture(int width, int height, byte[] data)
        {
            Load(width, height, data);
        }

        ~Texture()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (GL.IsTexture(glID)) GL.DeleteTexture(glID);
                }

                disposed = true;
            }
        }

        private void Load(int width, int height, byte[] data)
        {
            glID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, glID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        }

        public void Bind()
        {
            Bind(TextureUnit.Texture0);
        }

        public void Bind(TextureUnit textureUnit)
        {
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, glID);
        }

        public int GetTextureID()
        {
            return glID;
        }
    }
}
