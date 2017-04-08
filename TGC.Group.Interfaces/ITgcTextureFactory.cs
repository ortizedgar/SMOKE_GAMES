namespace TGC.Group.Interfaces
{
    using Microsoft.DirectX.Direct3D;
    using TGC.Core.Textures;

    /// <summary>
    /// Creador del escenario
    /// </summary>
    public interface ITgcTextureFactory
    {
        TgcTexture CreateTexture(Device device, string filePath);
    }
}