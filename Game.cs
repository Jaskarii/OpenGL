using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace OpenGL
{
    internal class Game : GameWindow
    {
        Shader shader;
        Shader shader1;

        VertexArray va;
        IndexBuffer ib;

        VertexBuffer vb;
        int count = 0;

        Vector2 mousePosition;
        VertexBufferLayout layout;

        List<float> Positions = new List<float>();

        float[] indies = new float[] 
            { 0.0f, -0.2f, 0.0f, 
            0.0f, 0.2f, 0.0f};
        List<int> PositionIndices = new List<int>();

        List<VertexBuffer> Vertices = new List<VertexBuffer>();

        
        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            vb = new VertexBuffer(indies, indies.Length * sizeof(float));
            this.MouseMove += Game_MouseMove;

            this.MouseDown += Game_MouseDown;

            Matrix4 rotation = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(75.0f));

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            layout = new VertexBufferLayout();
            layout.AddToBuffer(VertexAttribPointerType.Float, 3);

            int[] index = new int[] { 0, 1 };
            ib = new IndexBuffer(index,6*sizeof(int));
            ib.Bind();
            va = new VertexArray();

            va.AddBuffer(vb, layout);

            shader = new Shader(@"C:\Users\anssikoi\OneDrive - Epec Oy\Desktop\MyOpenGL\Resources\Basic.shader");

            int location = GL.GetUniformLocation(shader.Handle, "transform");

            GL.UniformMatrix4(location, true, ref rotation);
        }

        private void Game_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AddVertexToBuffer(e.X, this.Height - e.Y);
        }

        private void Game_MouseMove(object sender, MouseMoveEventArgs e)
        {
            mousePosition.X = e.X;
            mousePosition.Y = this.Height- e.Y;
        }

        private void AddVertexToBuffer(float x, float y)
        {
            Positions.Add(2 * (x / this.Width - 0.5f));
            Positions.Add(2 * (y / this.Width - 0.5f));
            Positions.Add(0.0f);
            PositionIndices.Add(count);
            count++;

            if (Positions.Count == 3)
            {
                Vertices.Add(new VertexBuffer(Positions.ToArray(), 6 * sizeof(float)));
                Positions.Clear();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            int location = GL.GetUniformLocation(shader.Handle, "windowWidth");
            int location1 = GL.GetUniformLocation(shader.Handle, "windowHeight");

            GL.Uniform1(location, (float)this.Width);
            GL.Uniform1(location1, (float)this.Height);

            
            Renderer.Draw(va, ib, shader);


            Context.SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Width, Height);
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            shader.Dispose();
        }
    }
}
