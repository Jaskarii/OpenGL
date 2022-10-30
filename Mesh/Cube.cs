using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL
{
    internal class Cube
    {
        Texture text;

        private float[] vertices = new float[]
        {
            //Position          Texture coordinates
     0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
     0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
    -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
    -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
        };

        private int[] indices = new int[]
            {0, 1, 2,0,2,3 
            //0,1,5,0,5,4,
            //0,3,7,0,7,4,
            //1,2,5,2,5,6,
            //4,5,6,4,6,7
            };

        float[] texCoords = {
        0.0f, 0.0f,  // lower-left corner  
        1.0f, 0.0f,  // lower-right corner
        0.5f, 1.0f   // top-center corner
        };

        private VertexBuffer vb;
        private VertexBufferLayout layout;
        private VertexArray va;
        private Shader shader;
        private IndexBuffer ib;

        public Cube()
        {
            text = new Texture(@"F:\Koodit\OpenGL\Resources\wall.jpg");
            vb = new VertexBuffer(vertices, vertices.Length * sizeof(float));
            layout = new VertexBufferLayout();
            layout.AddToBuffer(VertexAttribPointerType.Float, 3);
            layout.AddToBuffer(VertexAttribPointerType.Float, 2);
            va = new VertexArray();
            shader = new Shader(@"F:\Koodit\OpenGL\Resources\Cube.shader");
            shader.Use();

            va.AddBuffer(vb, layout);
            va.Bind();
            ib = new IndexBuffer(indices, indices.Length * sizeof(int));
            ib.Bind();
        }

        float z = 0;
        float x = 0;
        float rateZ = 0.05f;
        float rateX = 0.01f;
        public void Render()
        {
            int location = GL.GetUniformLocation(shader.Handle, "rotate");
            Matrix4 rotation = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(z)) + Matrix4.CreateRotationY(MathHelper.DegreesToRadians(x));
            GL.UniformMatrix4(location,true,ref rotation);
            GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);

            z += rateZ;
            x += rateX;
        }
    }
}
