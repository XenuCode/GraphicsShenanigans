using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace GraphicsShenanigans
{
    public class Program
    {
        private static IWindow _window;
        private static GL _gl;
        private static uint _vao;
        private static uint _vbo;

        public static void Main(string[] args)
        {
            WindowOptions options = WindowOptions.Default;
            options.API = GraphicsAPI.Default;
            options.Size = new Vector2D<int>(800, 600);
            options.Title = "My first Silk.NET program!";
            
            _window = Window.Create(options);

            _window.Load += OnLoad;
            _window.Update += OnUpdate;
            _window.Render += OnRender;
            _window.Run();
        }

        private static unsafe void OnLoad()
        {
            Console.WriteLine("Load!");
            _gl = _window.CreateOpenGL();
            _vao = _gl.GenVertexArray();
            _gl.BindVertexArray(_vao);
            float[] vertices =
            {
                0.5f,  0.5f, 0.0f,
                0.5f, -0.5f, 0.0f,
                -0.5f, -0.5f, 0.0f,
                -0.5f,  0.5f, 0.0f
            };
            _vbo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            fixed (float* buf = vertices)
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
            _gl.ClearColor(Color.Black);
            IInputContext input = _window.CreateInput();
            for (int i = 0; i < input.Keyboards.Count; i++)
                input.Keyboards[i].KeyDown += KeyDown;
        }

        // These two methods are unused for this tutorial, aside from the logging we added earlier.
        private static void OnUpdate(double dt)
        {
            Console.WriteLine("Update!");
        }

        private static void OnRender(double dt)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            Console.WriteLine("Render! FPS: "+ (1 / dt).ToString("0"));
        }

        private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            if (key == Key.Escape)
                _window.Close();
        }
    }
}