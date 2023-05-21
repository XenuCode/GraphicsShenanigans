using Silk.NET.OpenGL;

namespace GraphicsShenanigans.Abstractions;

public class BufferObject<TDataType> : IDisposable
    where TDataType : unmanaged
{
    //Our handle, buffertype and the GL instance this class will use, these are private because they have no reason to be public.
    //Most of the time you would want to abstract items to make things like this invisible.
    private uint _handle;
    private BufferTargetARB _bufferType;
    private GL GL;

    public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
    {
        //Setting the gl instance and storing our buffer type.
        GL = gl;
        _bufferType = bufferType;

        //Getting the handle, and then uploading the data to said handle.
        _handle = GL.GenBuffer();
        Bind();
        fixed (void* d = data)
        {
            GL.BufferData(bufferType, (nuint) (data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
        }
    }

    public void Bind()
    {
        //Binding the buffer object, with the correct buffer type.
        GL.BindBuffer(_bufferType, _handle);
    }

    public void Dispose()
    {
        //Remember to delete our buffer.
        GL.DeleteBuffer(_handle);
    }
}