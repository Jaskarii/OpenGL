using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL
{
    struct ShaderProgramSource
    {
        public string VertexSource;
        public string GeometrySource;
        public string FragmentSource;
    };

    public class Shader
    {
        public int Handle;
        public Shader(string ShaderPath)
        {
            ShaderProgramSource source = ParseShader(ShaderPath);
            CreateShader(source);
        }

        private void CreateShader(ShaderProgramSource source)
        {
            Handle = GL.CreateProgram();
            List<int> shaders = new List<int>();
            shaders.Add(CompileShader(ShaderType.VertexShader, source.VertexSource));
            shaders.Add(CompileShader(ShaderType.GeometryShader, source.GeometrySource));
            shaders.Add(CompileShader(ShaderType.FragmentShader, source.FragmentSource));
            shaders.ForEach(s => AttachShader(s));
            GL.LinkProgram(Handle);
            shaders.ForEach(s => DeleteShader(s));
        }

        private void AttachShader(int shaderID)
        {
            if (shaderID != -1)
            {
                GL.AttachShader(Handle, shaderID);
                GL.DeleteShader(shaderID);
            }
        }

        private void DeleteShader(int shaderID)
        {
            if (shaderID != -1)
            {
                GL.DetachShader(Handle, shaderID);
            }
        }

        private int CompileShader(ShaderType type, string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return -1;
            }
            int id = GL.CreateShader(type);
            GL.ShaderSource(id, source);
            GL.CompileShader(id);

            GL.GetShader(id, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(id);
                Console.WriteLine(infoLog);
            }
            return id;
        }

        private ShaderProgramSource ParseShader(string shaderPath)
        {
            ShaderProgramSource source = new ShaderProgramSource();

            StringBuilder VertexBuilder = new StringBuilder();
            StringBuilder FragmentBuilder = new StringBuilder();
            StringBuilder GeometryBuilder = new StringBuilder();
            using (StreamReader reader = new StreamReader(shaderPath))
            {
                string line;
                ShaderType type = ShaderType.TessControlShader;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#shader"))
                    {
                        if (line.Contains("vertex"))
                        {
                            type = ShaderType.VertexShader;
                        }
                        else if (line.Contains("fragment"))
                        {
                            type = ShaderType.FragmentShader;
                        }
                        else if (line.Contains("geometry"))
                        {
                            type = ShaderType.GeometryShader;
                        }
                    }
                    else
                    {
                        switch (type)
                        {
                            case ShaderType.FragmentShader:
                                FragmentBuilder.AppendLine(line);
                                break;
                            case ShaderType.VertexShader:
                                VertexBuilder.AppendLine(line);
                                break;
                            case ShaderType.GeometryShader:
                                GeometryBuilder.AppendLine(line);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            source.FragmentSource = FragmentBuilder.ToString();
            source.VertexSource = VertexBuilder.ToString();
            source.GeometrySource = GeometryBuilder.ToString();

            return source;
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }
        ~Shader()
        {
            GL.DeleteProgram(Handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
