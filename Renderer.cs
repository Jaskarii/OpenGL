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
                count = ib.Count;
            }




            GL.DrawElements(BeginMode.Points, count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            if (ib != null)
            {
                ib.UnBind();
            }
            va.UnBind();
        }
    }
}
