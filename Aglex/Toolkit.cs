using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

namespace Aglex
{
    public class Toolkit
    {
        [DllImport("opengl32.dll", EntryPoint = "wglGetCurrentContext")]
        extern static IntPtr wglGetCurrentContext();

        internal static bool IsReady
        {
            get { return (wglGetCurrentContext() != IntPtr.Zero); }
        }

        public static Version OpenTKVersion
        {
            get
            {
                string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "OpenTK.dll");
                return new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo(path).ProductVersion);
            }
        }

        static DateTime RetrieveLinkerTimestamp()
        {
            string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            System.IO.Stream s = null;

            try
            {
                s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            int i = System.BitConverter.ToInt32(b, c_PeHeaderOffset);
            int secondsSince1970 = System.BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }

        public static string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            DateTime linkerTimestamp = RetrieveLinkerTimestamp();
            string buildString = string.Format("Build {0}", linkerTimestamp.ToString("MM/dd/yyyy HH:mm:ss UTCzzz", System.Globalization.CultureInfo.InvariantCulture));

            return string.Format("{0} {1} {2}", fileVersionInfo.ProductName, fileVersionInfo.ProductVersion, buildString);
        }

        public static string RendererString
        {
            get { return GL.GetString(StringName.Renderer) ?? "[null]"; }
        }

        public static string VendorString
        {
            get { return GL.GetString(StringName.Vendor) ?? "[null]"; }
        }

        public static string VersionString
        {
            get { return GL.GetString(StringName.Version) ?? "[null]"; }
        }

        public static string ShadingLanguageVersionString
        {
            get
            {
                string str = GL.GetString(StringName.ShadingLanguageVersion);
                if (str == null || str == string.Empty) return "[unsupported]";
                else return str;
            }
        }

        public static string[] SupportedExtensions
        {
            get { return GL.GetString(StringName.Extensions).Split(new char[] { ' ' }) ?? new string[] { "[null]" }; }
        }

        public static int GetInteger(GetPName name)
        {
            int outval;
            GL.GetInteger(name, out outval);
            return outval;
        }

        public static int MaxTextureUnits { get { int outval; GL.GetInteger(GetPName.MaxTextureUnits, out outval); return outval; } }
    }
}
