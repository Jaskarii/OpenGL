using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL
{
    internal class IndexBuffer
    {
        public int Id { get; private set; }

        public int Count { get; private set; }

        public IndexBuffer(int[] data, int size)
        {
            Count = size;

            Id = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Id);

            GL.BufferData(BufferTarget.ElementArrayBuffer, size, data, BufferUsageHint.StaticDraw);
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Id);
        }

        public void UnBind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

    }
}
