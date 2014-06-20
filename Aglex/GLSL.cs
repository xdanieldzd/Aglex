using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Aglex
{
    public static class GLSL
    {
        /* Modified from OpenTK examples \Source\Examples\OpenGL\2.x\SimpleGLSL.cs */
        public static void CreateFragmentShader(ref int fragmentObject, Stream shaderFile)
        {
            StreamReader streamReader = new StreamReader(shaderFile);
            string shaderString = streamReader.ReadToEnd();
            CreateFragmentShader(ref fragmentObject, shaderString);
            streamReader.Close();
        }

        public static void CreateFragmentShader(ref int fragmentObject, string shaderString)
        {
            int statusCode;
            string info;

            fragmentObject = GL.CreateShader(ShaderType.FragmentShader);

            /* Compile fragment shader */
            GL.ShaderSource(fragmentObject, shaderString);
            GL.CompileShader(fragmentObject);
            GL.GetShaderInfoLog(fragmentObject, out info);
            GL.GetShader(fragmentObject, ShaderParameter.CompileStatus, out statusCode);

            if (statusCode != 1) throw new AglexException(info);
        }

        public static void CreateVertexShader(ref int vertexObject, Stream shaderFile)
        {
            StreamReader streamReader = new StreamReader(shaderFile);
            string shaderString = streamReader.ReadToEnd();
            CreateVertexShader(ref vertexObject, shaderString);
            streamReader.Close();
        }

        public static void CreateVertexShader(ref int vertexObject, string shaderString)
        {
            int statusCode;
            string info;

            vertexObject = GL.CreateShader(ShaderType.VertexShader);

            /* Compile vertex shader */
            GL.ShaderSource(vertexObject, shaderString);
            GL.CompileShader(vertexObject);
            GL.GetShaderInfoLog(vertexObject, out info);
            GL.GetShader(vertexObject, ShaderParameter.CompileStatus, out statusCode);

            if (statusCode != 1) throw new AglexException(info);
        }

        public static void CreateProgram(ref int program, int fragmentObject, int vertexObject)
        {
            program = GL.CreateProgram();
            GL.AttachShader(program, fragmentObject);
            GL.AttachShader(program, vertexObject);

            GL.LinkProgram(program);
            GL.UseProgram(program);
        }
    }
}
