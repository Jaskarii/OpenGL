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

        Vector2 mousePosition;
        VertexBufferLayout layout;

        List<float> Positions = new List<float>();

        List<int> PositionIndices = new List<int>();

        
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

            this.MouseMove += Game_MouseMove;

            this.MouseDown += Game_MouseDown;

            Matrix4 rotation = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(75.0f));

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            layout = new VertexBufferLayout();
            layout.AddToBuffer(VertexAttribPointerType.Float, 3);

            shader = new Shader(@"F:\Koodit\OpenGL\Resources\Basic.shader");

            int location = GL.GetUniformLocation(shader.Handle, "transform");

            GL.UniformMatrix4(location, true, ref rotation);

            //ib = new IndexBuffer(indices, indices.Length * sizeof(int));

            va = new VertexArray();
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
            Positions.Add(x/ this.Width - 0.5f);
            Positions.Add(y/ this.Height - 0.5f);
            Positions.Add(0.0f);
            PositionIndices.Add(PositionIndices.Count -1);
            ib = new IndexBuffer(PositionIndices.ToArray(), PositionIndices.Count);
            va.AddBuffer(new VertexBuffer(Positions.ToArray(), Positions.Count * 3 * sizeof(float)), layout);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            int location = GL.GetUniformLocation(shader.Handle, "transform");
            int location1 = GL.GetUniformLocation(shader.Handle, "mouseposition");

            GL.Uniform2(location1, new Vector2( mousePosition.X, mousePosition.Y));

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
