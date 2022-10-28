using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows;

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

        public IndexBuffer(int num)
        {
            List<int> temp = new List<int>();
            for (int i = 0; i < num; i++)
            {
                temp.Add(i);
            }
            Count = num;

            Id = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Id);
            GL.BufferData(BufferTarget.ElementArrayBuffer, num, temp.ToArray(), BufferUsageHint.StaticDraw);
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
