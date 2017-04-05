using System;
using System.Collections.Generic;
using TGC.Core.SceneLoader;

namespace TGC.Group.Interfaces
{
    public interface IScenarioCreator
    {
        List<Tuple<string, List<IRenderObject>>> CreateScenario(string mediaDir, IVector3Factory vector3Factory, ITgcPlaneFactory tgcPlaneFactory, float planeSize);
    }
}