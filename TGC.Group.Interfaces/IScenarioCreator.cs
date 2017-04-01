using System.Collections.Generic;
using TGC.Core.Geometry;

namespace TGC.Group.Interfaces
{
    public interface IScenarioCreator
    {
        List<List<TgcPlane>> CreateScenario(string mediaDir, IVector3Factory vector3Factory, ITgcPlaneFactory tgcPlaneFactory, int planeSize);
    }
}