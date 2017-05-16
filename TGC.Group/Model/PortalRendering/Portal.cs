namespace TGC.Group.Model
{
    using System.Collections.Generic;
    using TGC.Core.SceneLoader;
    using TGC.Group.Interfaces;

    public class Portal : IPortal
    {
        public TgcMesh Door { get; set; }
        public int RoomA { get; set; }
        public int RoomB { get; set; }
        public List<IScenarioElement> DoorWalls { get; set; }

        public Portal(TgcMesh mesh, int RoomA, int RoomB, List<IScenarioElement> doorWalls)
        {
            this.Door = mesh;
            this.RoomA = RoomA;
            this.RoomB = RoomB;
            this.DoorWalls = doorWalls;
        }
    }
}