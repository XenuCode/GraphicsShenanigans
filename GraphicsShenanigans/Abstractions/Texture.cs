using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace GraphicsShenanigans.Abstractions;

public class Texture : IDisposable
{
    private uint _handle;
    private GL _gl;

    /// <summary>
    /// Loads texture from a specified path
    /// </summary>
    /// <param name="gl">GL instance</param>
    /// <param name="path">path to texture</param>
    public unsafe Texture(GL gl, string path)
    {
        _gl = gl;

        _handle = _gl.GenTexture();
        Bind();

        //Loading an image using imagesharp.
        using (var img = Image.Load<Rgba32>(path))
        {
            //Reserve enough memory from the gpu for the whole image
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint) img.Width, (uint) img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

            img.ProcessPixelRows(accessor =>
            {
                //ImageSharp 2 does not store images in contiguous memory by default, so we must send the image row by row
                for (int y = 0; y < accessor.Height; y++)
                {
                    fixed (void* data = accessor.GetRowSpan(y))
                    {
                        //Loading the actual image.
                        gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, y, (uint) accessor.Width, 1, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                    }
                }
            });
        }

        SetParameters();

    }

    /// <summary>
    /// Loads a Texture from byte-data  
    /// </summary>
    /// <param name="gl">GL instance</param>
    /// <param name="data">pixel data</param>
    /// <param name="width">width of texture</param>
    /// <param name="height">height of texture</param>
    public unsafe Texture(GL gl, Span<byte> data, uint width, uint height)
    {
        //Saving the gl instance.
        _gl = gl;

        //Generating the opengl handle;
        _handle = _gl.GenTexture();
        Bind();

        //We want the ability to create a texture using data generated from code as well.
        fixed (void* d = &data[0])
        {
            //Setting the data of a texture.
            _gl.TexImage2D(TextureTarget.Texture2D, 0, (int) InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, d);
            SetParameters();
        }
    }

    private void SetParameters()
    {
        //Setting some texture perameters so the texture behaves as expected.
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) GLEnum.LinearMipmapLinear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
        //Generating mipmaps.
        _gl.GenerateMipmap(TextureTarget.Texture2D);
    }

    /// <summary>
    /// When we bind a texture we can choose which texture slot we can bind it to.
    /// </summary>
    /// <param name="textureSlot">texture slot to which bind it to</param>
    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        //When we bind a texture we can choose which texture slot we can bind it to.
        _gl.ActiveTexture(textureSlot);
        
        _gl.BindTexture(TextureTarget.Texture2D, _handle);
    }
    /// <summary>
    /// Disposes of texture
    /// </summary>
    public void Dispose()
    {
        //In order to dispose we need to delete the opengl handle for the texure.
        _gl.DeleteTexture(_handle);
    }
}