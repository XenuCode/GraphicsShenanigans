using Silk.NET.OpenGL;

namespace GraphicsShenanigans.Abstractions;

public class Shader : IDisposable
{
    //Our handle and the GL instance this class will use, these are private because they have no reason to be public.
    //Most of the time you would want to abstract items to make things like this invisible.
    private uint _handle;
    private GL GL;

    public Shader(GL gl, string vertexPath, string fragmentPath)
    {
        GL = gl;

        //Load the individual shaders.
        uint vertex = LoadShader(ShaderType.VertexShader, vertexPath);
        uint fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);
        //Create the shader program.
        _handle = GL.CreateProgram();
        //Attach the individual shaders.
        GL.AttachShader(_handle, vertex);
        GL.AttachShader(_handle, fragment);
        GL.LinkProgram(_handle);
        //Check for linking errors.
        GL.GetProgram(_handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            throw new Exception($"Program failed to link with error: {GL.GetProgramInfoLog(_handle)}");
        }
        //Detach and delete the shaders
        GL.DetachShader(_handle, vertex);
        GL.DetachShader(_handle, fragment);
        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);
    }

    public void Use()
    {
        //Using the program
        GL.UseProgram(_handle);
    }

    //Uniforms are properties that applies to the entire geometry
    public void SetUniform(string name, int value)
    {
        //Setting a uniform on a shader using a name.
        int location = GL.GetUniformLocation(_handle, name);
        if (location == -1) //If GetUniformLocation returns -1 the uniform is not found.
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
        GL.Uniform1(location, value);
    }

    public void SetUniform(string name, float value)
    {
        int location = GL.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
        GL.Uniform1(location, value);
    }

    public void Dispose()
    {
        //Remember to delete the program when we are done.
        GL.DeleteProgram(_handle);
    }

    private uint LoadShader(ShaderType type, string path)
    {
        //To load a single shader we need to:
        //1) Load the shader from a file.
        //2) Create the handle.
        //3) Upload the source to opengl.
        //4) Compile the shader.
        //5) Check for errors.
        string src = File.ReadAllText(path);
        uint handle = GL.CreateShader(type);
        GL.ShaderSource(handle, src);
        GL.CompileShader(handle);
        string infoLog = GL.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
        }

        return handle;
    }
}