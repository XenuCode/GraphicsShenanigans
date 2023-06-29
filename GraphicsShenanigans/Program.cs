using System.Collections;
using System.Drawing;
using System.Numerics;
using GraphicsShenanigans.Abstractions;
using GraphicsShenanigans.Data;
using GraphicsShenanigans.Helpers;
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
        private static GL _gl;

        //Our new abstracted objects, here we specify what the types are.
        private static BufferObject<float>? _vbo;
        private static BufferObject<uint>? _ebo;
        private static VertexArrayObject<float, uint>? _vao;
        private static Shader? _shader;
        private static Texture? _texture;
        private static Dictionary<Key, bool> _pressedIn = new();

        //Setup the camera's location, and relative up and right directions
        private static Vector3 _cameraPosition = new Vector3(0.0f, 0.0f, 3.0f);
        private static Vector3 _cameraTarget = Vector3.Zero;
        private static Vector3 _cameraDirection = Vector3.Normalize(_cameraPosition - _cameraTarget);
        private static Vector3 _cameraRight = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, _cameraDirection));
        private static Vector3 _cameraUp = Vector3.Cross(_cameraDirection, _cameraRight);
        
        private static bool _isWireframe;
        // OpenGL has image origin in the bottom-left corner.
        private static readonly float[] Vertices =
        {
            //X    Y      Z     U   V
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
            0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
        };


        private static readonly uint[] Indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        public static void Main(string[] args)
        {
            WindowOptions options = WindowOptions.Default;
            options.PreferredDepthBufferBits = 32;
            options.API = new GraphicsAPI(ContextAPI.OpenGL,new APIVersion(Version.Parse("4.6")));
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
            foreach (var keyVar in Key.GetValuesAsUnderlyingType<Key>())
            {
                _pressedIn.TryAdd((Key)keyVar,false);
            }
            Console.WriteLine("Monitor aspect ratio: " + _window.Monitor.VideoMode.AspectRatioEstimate.Value);
            Console.WriteLine("Window size: " + _window.Size);
            Console.WriteLine("VSYNC: " + _window.VSync);
            Console.WriteLine("Windowing API: " + _window.API);
            Console.WriteLine("Load!");
            _gl = _window.CreateOpenGL();
            IInputContext input = _window.CreateInput();
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += OnKeyDown;
                input.Keyboards[i].KeyUp += OnKeyUp;
            }
            //Instantiating our new abstractions
            _ebo = new BufferObject<uint>(_gl, Indices, BufferTargetARB.ElementArrayBuffer);
            _vbo = new BufferObject<float>(_gl, Vertices, BufferTargetARB.ArrayBuffer);
            _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);

            //Telling the Vao object how to lay out the attribute pointers
            _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
            _vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);

            _shader = new Shader(_gl, "Shaders/shader.vert", "Shaders/shader.frag");
            _texture = new Texture(_gl, "Textures/dirt.png");
        }
        
        // These two methods are unused for this tutorial, aside from the logging we added earlier.

        private static void OnUpdate(double dt)
        {
            //Console.WriteLine("Update!");
            /*foreach (var keyVal in _pressedIn)
            {
                Console.WriteLine(keyVal.Key + " is pressed: " + keyVal.Value);
            }*/
        }

        private static void OnRender(double dt)
        {
            _gl.CullFace(TriangleFace.Front);
            _gl.Enable(EnableCap.DepthTest);
            _gl.DepthFunc(DepthFunction.Lequal);
            _gl.Clear(ClearBufferMask.DepthBufferBit);
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            
            //ITS FINE
            
            _vao.Bind();
            _texture.Bind();
            _shader.Use();
            _shader.SetUniform("uTexture0", 0);

            //Use elapsed time to convert to radians to allow our cube to rotate over time
            var difference = (float) (_window.Time * 10);

            var model = Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(difference)) * Matrix4x4.CreateRotationX(MathHelper.DegreesToRadians(difference));
            var view = Matrix4x4.CreateLookAt(_cameraPosition, _cameraTarget, _cameraUp);
            var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), _window.Size.X / _window.Size.Y, 0.001f, 1000.0f);

            _shader.SetUniform("uModel", model);
            _shader.SetUniform("uView", view);
            _shader.SetUniform("uProjection", projection);

            //We're drawing with just vertices and no indicies, and it takes 36 verticies to have a six-sided textured cube
            _gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
            //_window.Position = new Vector2D<int>(Random.Shared.Next(0,1000), Random.Shared.Next(0,1000)); //HEHE FUNNY, WINDOW GOES BRRRRRRRRRRR
        }

        //NICE
        private static void OnKeyUp(IKeyboard keyboard, Key key, int keyCode)
        {
            _pressedIn.Remove(key);
            bool added = _pressedIn.TryAdd(key,false);
            if (!added)
            {
                Console.Error.WriteLine("Ohno, Key didn't work");
            }        
        }
        private static void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            _pressedIn.Remove(key);
            bool added = _pressedIn.TryAdd(key,true);
            if (!added)
            {
                Console.Error.WriteLine("Ohno, Key didn't work");
            }
            if (key == Key.Escape)
                _window.Close();
            Console.WriteLine(key);
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
            Console.WriteLine("EXITING");
            _vbo.Dispose();
            _ebo.Dispose();
            _vao.Dispose();
            _shader.Dispose();
            
        }
    }
}