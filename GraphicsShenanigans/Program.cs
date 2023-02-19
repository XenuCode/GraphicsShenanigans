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
        private static uint _ebo;
        private static uint _program;

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
            _vao = _gl.GenVertexArray();
            _gl.BindVertexArray(_vao);
            uint[] indices =
            {
                0u, 1u, 3u,
                1u, 2u, 3u
            };
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
            _ebo = _gl.GenBuffer();
            
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
            fixed (uint* buf = indices)
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);
            // SHADER BULLSHIT
            const string vertexCode = @"
#version 330 core

layout (location = 0) in vec3 aPosition;

void main()
{
    gl_Position = vec4(aPosition, 1.0);
}";

            const string fragmentCode = @"
#version 330 core

out vec4 out_color;

void main()
{
    out_color = vec4(1.0, 0.5, 0.2, 1.0);
}";
            //SHADER COMPILATION
            uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
            _gl.ShaderSource(vertexShader, vertexCode);
            
            _gl.CompileShader(vertexShader);

            _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int) GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + _gl.GetShaderInfoLog(vertexShader));
            uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
            _gl.ShaderSource(fragmentShader, fragmentCode);

            _gl.CompileShader(fragmentShader);
            
            _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
            if (fStatus != (int) GLEnum.True)
                throw new Exception("Fragment shader failed to compile: " + _gl.GetShaderInfoLog(fragmentShader));
            //LOTSA COMPILIN' AND CREATING A PROGRAM (A PACK OF VERTEX AND FRAGMENT SHADERS [A PIPELINE ?] )
            
            _program = _gl.CreateProgram();
            _gl.AttachShader(_program, vertexShader);
            _gl.AttachShader(_program, fragmentShader);
            //LINKIN' SHADERS
            _gl.LinkProgram(_program);

            _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
            if (lStatus != (int) GLEnum.True)
                throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program));
            
            //DELETING SHADERS from memory leaving them only in pipeline ??? 
            _gl.DetachShader(_program, vertexShader);
            _gl.DetachShader(_program, fragmentShader);
            _gl.DeleteShader(vertexShader);
            _gl.DeleteShader(fragmentShader);
            
            //EM… memory MAGIC
            const uint positionLoc = 0;
            _gl.EnableVertexAttribArray(positionLoc);
            _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*) 0);
            
            _gl.BindVertexArray(0);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
            
            
            _gl.ClearColor(Color.Black);
            IInputContext input = _window.CreateInput();
            for (int i = 0; i < input.Keyboards.Count; i++)
                input.Keyboards[i].KeyDown += KeyDown;
        }

        // These two methods are unused for this tutorial, aside from the logging we added earlier.
        private static void OnUpdate(double dt)
        {
            //Console.WriteLine("Update!");
        }

        private static unsafe void OnRender(double dt)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            //Console.WriteLine("Render! FPS: "+ (1 / dt).ToString("0"));
            //DRAWIN' DAS SHIT 
            _gl.BindVertexArray(_vao);
            _gl.UseProgram(_program);
            _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*) 0);
            //_window.Position = new Vector2D<int>(Random.Shared.Next(0,1000), Random.Shared.Next(0,1000)); //HEHE FUNNY, WINDOW GOES BRRRRRRRRRRR
        }//NICE

        private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            if (key == Key.Escape)
                _window.Close();
        }
    }
}