namespace TGC.Group.Model
{
    using Microsoft.DirectX;
    using TGC.Group.Interfaces;

    public class Vector3Factory : IVector3Factory
    {
        public Vector3 CreateVector3(float x, float y, float z) => new Vector3(x, y, z);
    }
}