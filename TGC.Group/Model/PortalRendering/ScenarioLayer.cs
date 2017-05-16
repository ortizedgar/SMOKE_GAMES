namespace TGC.Group.Interfaces
{
    using System.Collections.Generic;

    public class ScenarioLayer : IScenarioLayer
    {
        public string LayerName { get; set; }
        public List<IScenarioElement> ScenarioElements { get; set; }
    }
}