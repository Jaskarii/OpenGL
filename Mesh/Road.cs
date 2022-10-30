using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL
{
    internal class Road
    {
        private float[] vertices = new float[]
        {
            //Position   Texture coordinates
        0.0f,  0.0f, 0.0f,
        0.0f,  0.5f, 0.0f,
        0.8f,  0.0f, 0.0f,// top right

        };

        private int[] indices = new int[]
            {0,1,2};


        private VertexBuffer vb;
        private VertexBufferLayout layout;
        private VertexArray va;
        private Shader shader;
        private IndexBuffer ib;

        public Road()
        {
            vb = new VertexBuffer(vertices, vertices.Length * sizeof(float));
            layout = new VertexBufferLayout();
            layout.AddToBuffer(VertexAttribPointerType.Float, 3);
            va = new VertexArray();
            shader = new Shader(@"F:\Koodit\OpenGL\Resources\Road.shader");
            shader.Use();

            va.AddBuffer(vb, layout);
            va.Bind();
            ib = new IndexBuffer(indices, indices.Length * sizeof(int));
            ib.Bind();
        }

        public void Render(float Width, float Height)
        {
            GL.DrawElements(BeginMode.Triangles, indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);

        }
    }
}