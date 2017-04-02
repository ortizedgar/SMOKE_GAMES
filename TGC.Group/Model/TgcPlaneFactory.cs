using TGC.Core.Geometry;
using TGC.Group.Interfaces;

namespace TGC.Group.Model
{
    public class TgcPlaneFactory : ITgcPlaneFactory
    {
        public TgcPlane CreateTgcPlane()
        {
            return new TgcPlane();
        }
    }
}