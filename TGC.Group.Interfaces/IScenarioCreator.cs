using System;
using System.Collections.Generic;
using Autofac;
using TGC.Core.SceneLoader;

namespace TGC.Group.Interfaces
{
    public interface IScenarioCreator
    {
        List<Tuple<string, List<IRenderObject>>> CreateScenario(string mediaDir, IContainer container, float planeSize);
    }
}