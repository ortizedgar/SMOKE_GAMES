namespace TGC.Group.Interfaces
{
    using System.Collections.Generic;
    using TGC.Core.SceneLoader;

    public interface IPortal
    {
        TgcMesh Door { get; set; }
        int RoomA { get; set; }
        int RoomB { get; set; }
        List<IScenarioElement> DoorWalls { get; set; }
    }
}