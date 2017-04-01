using Microsoft.DirectX;
using TGC.Group.Interfaces;

namespace TGC.Group
{
    public class Vector3Factory : IVector3Factory
    {
        public Vector3 CreateVector3(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }
    }
}