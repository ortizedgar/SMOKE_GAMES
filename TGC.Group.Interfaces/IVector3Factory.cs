using Microsoft.DirectX;

namespace TGC.Group.Interfaces
{
    public interface IVector3Factory
    {
        Vector3 CreateVector3(float x, float y, float z);
    }
}