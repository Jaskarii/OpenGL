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

        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        Cube cube;
        Road road;


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

            //cube = new Cube();

            road = new Road();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        }

        private void Game_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Game_MouseMove(object sender, MouseMoveEventArgs e)
        {
            //mousePosition.X = e.X;
            //mousePosition.Y = this.Height- e.Y;
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);




            road.Render(this.Width, this.Height);


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
        }
    }
}
