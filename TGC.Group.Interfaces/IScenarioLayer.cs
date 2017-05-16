namespace TGC.Group.Interfaces
{
    using System.Collections.Generic;

    public interface IScenarioLayer
    {
        string LayerName { get; set; }
        List<IScenarioElement> ScenarioElements { get; set; }
    }
}