namespace TGC.Group.Interfaces
{
    using System.Collections.Generic;
    using BulletSharp;
    using TGC.Core.SceneLoader;

    public interface IScenarioElement
    {
        IRenderObject RenderObject { get; set; }
        List<int> RoomsId { get; set; }
        RigidBody Body { get; set; }
    }
}