using System.Collections.Generic;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Group.Interfaces;

namespace TGC.Group.Model
{
    public class ScenarioCreator : IScenarioCreator
    {
        private const string horizontal = "H";
        private const string vertical = "V";

        /// <summary>
        /// Directorio de texturas, etc.
        /// </summary>
        private string mediaDir;

        /// <summary>
        /// Tamano de los planos
        /// </summary>
        private int planeSize;

        /// <summary>
        /// Fabrica de <see cref="TgcPlane"/>
        /// </summary>
        private ITgcPlaneFactory tgcPlaneFactory;

        /// <summary>
        /// Fabrica de <see cref="Vector3"/>
        /// </summary>
        private IVector3Factory vector3Factory;

        private List<TgcPlane> walls;
        private TgcTexture wallTexture;

        public List<List<TgcPlane>> CreateScenario(string mediaDir, IVector3Factory vector3Factory, ITgcPlaneFactory tgcPlaneFactory, int planeSize)
        {
            this.mediaDir = mediaDir;
            this.vector3Factory = vector3Factory;
            this.tgcPlaneFactory = tgcPlaneFactory;
            this.planeSize = planeSize;
            return new List<List<TgcPlane>> {
                //CreateFloor(),
                //CreateRoof(),
                CreateWalls()
            };
        }

        private List<TgcPlane> CreateFloor()
        {
            var floor = new List<TgcPlane>();
            var wallTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, mediaDir + @"\floor.bmp");

            for (var i = 0; i < 10; i++)
            {
                var floorElement = tgcPlaneFactory.CreateTgcPlane();
                floorElement.setTexture(wallTexture);
                floorElement.Origin = vector3Factory.CreateVector3(0, 0, planeSize * i);
                floorElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                floorElement.Orientation = TgcPlane.Orientations.XZplane;
                floorElement.AutoAdjustUv = false;
                floorElement.UTile = 1;
                floorElement.VTile = 1;
                floor.Add(floorElement);
            }

            return floor;
        }

        private List<TgcPlane> CreateRoof()
        {
            var roof = new List<TgcPlane>();
            var wallTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, mediaDir + @"\roof.bmp");

            for (var i = 0; i < 10; i++)
            {
                var roofElement = tgcPlaneFactory.CreateTgcPlane();
                roofElement.setTexture(wallTexture);
                roofElement.Origin = vector3Factory.CreateVector3(0, planeSize, planeSize * i);
                roofElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                roofElement.Orientation = TgcPlane.Orientations.XZplane;
                roofElement.AutoAdjustUv = false;
                roofElement.UTile = 1;
                roofElement.VTile = 1;
                roof.Add(roofElement);
            }

            return roof;
        }

        private void CreateWallLine(string orientation, short coordenadaX, int[] coordenadasZ)
        {
            foreach (var posicion in coordenadasZ)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(wallTexture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * coordenadaX, 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = orientation == horizontal ? TgcPlane.Orientations.XYplane : TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }
        }

        private List<TgcPlane> CreateWalls()
        {
            walls = new List<TgcPlane>();
            wallTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, mediaDir + @"\wall.bmp");

            // PAREDES VERTICALES
            CreateWallLine(vertical, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 });
            CreateWallLine(vertical, 2, new int[] { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 20, 21 });
            CreateWallLine(vertical, 3, new int[] { 0, 1, 2, 3 });
            CreateWallLine(vertical, 4, new int[] { 2, 3, 4, 5, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 20, 21 });
            CreateWallLine(vertical, 5, new int[] { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 });
            CreateWallLine(vertical, 6, new int[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 20, 21 });
            CreateWallLine(vertical, 8, new int[] { 1, 2 });
            CreateWallLine(vertical, 9, new int[] { 0, 1, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            CreateWallLine(vertical, 12, new int[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            CreateWallLine(vertical, 14, new int[] { 4, 5, 16 });
            CreateWallLine(vertical, 15, new int[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            CreateWallLine(vertical, 16, new int[] { 0, 1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            CreateWallLine(vertical, 17, new int[] { 0, 1, 2, 4, 5, 7, 8, 9, 10, 11, 12, 13, 15, 16, 17, 18, 21 });
            CreateWallLine(vertical, 19, new int[] { 0, 1, 2, 4, 5, 7, 8, 13, 20, 21 });
            CreateWallLine(vertical, 20, new int[] { 9, 10, 14, 15, 16, 19 });
            CreateWallLine(vertical, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 });

            // PAREDES HORIZONTALES
            CreateWallLine(horizontal, 9, new int[] { 0, 6, 16, 22 });
            CreateWallLine(horizontal, 10, new int[] { 0, 6, 16, 22 });
            CreateWallLine(horizontal, 11, new int[] { 0, 6, 16, 22 });
            CreateWallLine(horizontal, 12, new int[] { 0, 3, 4, 6, 7, 10, 11, 15, 16, 17, 18, 22 });
            CreateWallLine(horizontal, 13, new int[] { 0, 3, 4, 6, 7, 10, 11, 15, 16, 17, 18, 22 });
            CreateWallLine(horizontal, 14, new int[] { 0, 3, 6, 7, 10, 11, 15, 16, 18, 20, 22 });

            return walls;
        }
    }
}
