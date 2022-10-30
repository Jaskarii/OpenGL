using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL
{
    internal static class Renderer
    {
        [Obsolete]
        public static void Draw(VertexArray va, IndexBuffer ib, Shader shader)
        {
            int count = 0;
            shader.Use();

            va.Bind();

            if (ib != null)
            {
                ib.Bind();
            }

            GL.DrawElements(BeginMode.Lines,6,DrawElementsType.UnsignedInt, IntPtr.Zero);

            va.UnBind();
        }
    }
}
