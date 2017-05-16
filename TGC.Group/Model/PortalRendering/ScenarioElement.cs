namespace TGC.Group.Interfaces
{
    using System.Collections.Generic;
    using BulletSharp;
    using TGC.Core.SceneLoader;

    public class ScenarioElement : IScenarioElement
    {
        public IRenderObject RenderObject { get; set; }
        public List<int> RoomsId { get; set; }
        public RigidBody Body { get; set; }
    }
}