using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace OpenGL
{
    internal class VertexArray
    {
        private int id;

        public VertexArray()
        {
            id = GL.GenVertexArray();
        }

        public void AddBuffer(VertexBuffer vb, VertexBufferLayout layout)
        {
            Bind();
            vb.Bind();

            int offset = 0;

            for (int i = 0; i < layout.Elements.Count; i++)
            {
                GL.VertexAttribPointer(i, layout.Elements[i].count, VertexAttribPointerType.Float, layout.Elements[i].normalized, layout.Stride, offset);
                GL.EnableVertexAttribArray(i);
                offset += layout.Elements[i].count * VertexBufferLayout.GetSize(layout.Elements[i].type);
            }
        }

        public void Bind()
        {
            GL.BindVertexArray(id);
        }

        public void UnBind()
        {
            GL.BindVertexArray(0);
        }
    }
}
