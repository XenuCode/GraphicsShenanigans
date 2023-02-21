using System.Drawing;
using GraphicsShenanigans.Abstractions;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Shader = GraphicsShenanigans.Abstractions.Shader;

namespace GraphicsShenanigans
{
    public class Program
    {
        private static IWindow _window;
        private static GL _gl;

        //Our new abstracted objects, here we specify what the types are.
        private static BufferObject<float> _vbo;
        private static BufferObject<uint> _ebo;
        private static VertexArrayObject<float, uint> _vao;
        private static Shader _shader;
        private static bool _isWireframe;
        private static readonly float[] Vertices =
        {
            //X    Y      Z     R  G  B  A
            0.5f,  0.5f, 0.0f, 1, 0, 0, 1,
            0.5f, -0.5f, 0.0f, 0, 0, 0, 1,
            -0.5f, -0.5f, 0.0f, 0, 0, 1, 1,
            -0.5f,  0.5f, 0.5f, 0, 0, 0, 1
        };

        private static readonly uint[] Indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        public static void Main(string[] args)
        {
            WindowOptions options = WindowOptions.Default;
            options.API = GraphicsAPI.Default;
            options.Size = new Vector2D<int>(800, 800);
            options.Title = "My first Silk.NET program!";
            
            _window = Window.Create(options);

            _window.Load += OnLoad;
            _window.Update += OnUpdate;
            _window.Render += OnRender;
            _window.Resize += WindowResize;
            _window.Run();
        }

        private static void WindowResize(Vector2D<int> size)
        {
            Console.WriteLine("Resized to : " + size);
        }
        private static unsafe void OnLoad()
        {
            Console.WriteLine("Monitor aspect ratio: " + _window.Monitor.VideoMode.AspectRatioEstimate.Value);
            Console.WriteLine("Window size: " + _window.Size);
            Console.WriteLine("VSYNC: " + _window.VSync);
            Console.WriteLine("Windowing API: " + _window.API);
            Console.WriteLine("Load!");
            _gl = _window.CreateOpenGL();
            IInputContext input = _window.CreateInput();
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += KeyDown;
            }
            //Instantiating our new abstractions
            _ebo = new BufferObject<uint>(_gl, Indices, BufferTargetARB.ElementArrayBuffer);
            _vbo = new BufferObject<float>(_gl, Vertices, BufferTargetARB.ArrayBuffer);
            _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);

            //Telling the _vao object how to lay out the attribute pointers
            _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 7, 0);
            _vao.VertexAttributePointer(1, 4, VertexAttribPointerType.Float, 7, 3);

            _shader = new Shader(_gl, "Shaders/shader.vert", "Shaders/shader.frag");
        }

        // These two methods are unused for this tutorial, aside from the logging we added earlier.
        private static void OnUpdate(double dt)
        {
            //Console.WriteLine("Update!");
        }

        private static unsafe void OnRender(double dt)
        {
            _gl.Clear((uint) ClearBufferMask.ColorBufferBit);

            //Binding and using our VAO and shader.
            _vao.Bind();
            _shader.Use();
            //Setting a uniform.
            _shader.SetUniform("uBlue", (float) Math.Sin(DateTime.Now.Millisecond / 1000f * Math.PI));
            
            //Console.WriteLine("Render! FPS: "+ (1 / dt).ToString("0"));
            //DRAWIN' DAS SHIT 
            _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*) 0);
            //_window.Position = new Vector2D<int>(Random.Shared.Next(0,1000), Random.Shared.Next(0,1000)); //HEHE FUNNY, WINDOW GOES BRRRRRRRRRRR
        }//NICE
        private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            if (key == Key.Escape)
                _window.Close();
            if (key == Key.W)
            {
                if(!_isWireframe)
                {
                    _gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);
                    _isWireframe = true;
                    return;
                }
                _gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
                _isWireframe = false;
            }
        }
        private static void OnClose()
        {
            //Remember to dispose all the instances.
            _vbo.Dispose();
            _ebo.Dispose();
            _vao.Dispose();
            _shader.Dispose();
        }
    }
}