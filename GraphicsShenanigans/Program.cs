using System.Drawing;
using GraphicsShenanigans.Abstractions;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Shader = GraphicsShenanigans.Abstractions.Shader;
using Texture = GraphicsShenanigans.Abstractions.Texture;

namespace GraphicsShenanigans
{
    public class Program
    {
        private static IWindow _window;
        private static GL Gl;

        //Our new abstracted objects, here we specify what the types are.
        private static BufferObject<float> Vbo;
        private static BufferObject<uint> Ebo;
        private static VertexArrayObject<float, uint> Vao;
        private static Shader Shader;
        private static Texture Texture;

        
        private static bool _isWireframe;
        // OpenGL has image origin in the bottom-left corner.
        private static readonly float[] Vertices =
        {
            //X    Y      Z     U   V
            0.5f,  0.5f, 0.0f, 1f, 0f,
            0.5f, -0.5f, 0.0f, 1f, 1f,
            -0.5f, -0.5f, 0.0f, 0f, 1f,
            -0.5f,  0.5f, 0.5f, 0f, 0f
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
            _window.Closing += OnClose;
            _window.Run();
            _window.Dispose();
        }

        private static void WindowResize(Vector2D<int> size)
        {
            Console.WriteLine("Resized to : " + size);
        }
        private static void OnLoad()
        {
            Console.WriteLine("Monitor aspect ratio: " + _window.Monitor.VideoMode.AspectRatioEstimate.Value);
            Console.WriteLine("Window size: " + _window.Size);
            Console.WriteLine("VSYNC: " + _window.VSync);
            Console.WriteLine("Windowing API: " + _window.API);
            Console.WriteLine("Load!");
            Gl = _window.CreateOpenGL();
            IInputContext input = _window.CreateInput();
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += KeyDown;
            }
            //Instantiating our new abstractions
            Ebo = new BufferObject<uint>(Gl, Indices, BufferTargetARB.ElementArrayBuffer);
            Vbo = new BufferObject<float>(Gl, Vertices, BufferTargetARB.ArrayBuffer);
            Vao = new VertexArrayObject<float, uint>(Gl, Vbo, Ebo);

            //Telling the Vao object how to lay out the attribute pointers
            Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
            Vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);

            Shader = new Shader(Gl, "Shaders/shader.vert", "Shaders/shader.frag");
            Texture = new Texture(Gl, "Textures/minecraftIcon.png");
            
        }
        

        // These two methods are unused for this tutorial, aside from the logging we added earlier.
        private static void OnUpdate(double dt)
        {
            //Console.WriteLine("Update!");
        }

        private static unsafe void OnRender(double dt)
        {
            Gl.Clear((uint) ClearBufferMask.ColorBufferBit);

            //Binding and using our VAO and shader.
            Vao.Bind();
            Shader.Use();
            //Bind a texture and and set the uTexture0 to use texture0.
            Texture.Bind(TextureUnit.Texture0);
            Shader.SetUniform("uTexture0", 0);
            //Setting a uniform.
            //Shader.SetUniform("uBlue", (float) Math.Sin(DateTime.Now.Millisecond / 1000f * Math.PI));
            
            //Console.WriteLine("Render! FPS: "+ (1 / dt).ToString("0"));
            //DRAWIN' DAS SHIT 
            Gl.DrawElements(PrimitiveType.Triangles, (uint) Indices.Length, DrawElementsType.UnsignedInt, null);
            //_window.Position = new Vector2D<int>(Random.Shared.Next(0,1000), Random.Shared.Next(0,1000)); //HEHE FUNNY, WINDOW GOES BRRRRRRRRRRR
        }//NICE
        private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            if (key == Key.Escape)
                _window.Close();
            Console.WriteLine(key);
            if (key == Key.W)
            {
                if(!_isWireframe)
                {
                    Gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);
                    _isWireframe = true;
                    return;
                }
                Gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
                _isWireframe = false;
            }
        }
        private static void OnClose()
        {
            //Remember to dispose all the instances.
            Console.WriteLine("EXITING");
            Vbo.Dispose();
            Ebo.Dispose();
            Vao.Dispose();
            Shader.Dispose();
            
        }
    }
}