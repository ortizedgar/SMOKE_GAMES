namespace TGC.Group.Model
{
    using Microsoft.DirectX.Direct3D;
    using TGC.Core.Textures;
    using TGC.Group.Interfaces;

    public class TgcTextureFactory : ITgcTextureFactory
    {
        public TgcTexture CreateTexture(Device device, string filePath) => TgcTexture.createTexture(device, filePath);
    }
}