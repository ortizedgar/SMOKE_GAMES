using System;
using System.Collections.Generic;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Group.Interfaces;

namespace TGC.Group.Model
{
    public class ScenarioCreator : IScenarioCreator
    {

        /// <summary>
        /// Directorio de texturas, etc.
        /// </summary>
        private string mediaDir;

        /// <summary>
        /// Tamano de los planos
        /// </summary>
        private int planeSize;

        /// <summary>
        /// Generador de numeros al azar
        /// </summary>
        private Random random;

        /// <summary>
        /// Fabrica de <see cref="TgcPlane"/>
        /// </summary>
        private ITgcPlaneFactory tgcPlaneFactory;

        /// <summary>
        /// Fabrica de <see cref="Vector3"/>
        /// </summary>
        private IVector3Factory vector3Factory;

        public List<List<TgcPlane>> CreateScenario(string mediaDir, IVector3Factory vector3Factory, ITgcPlaneFactory tgcPlaneFactory, int planeSize)
        {
            this.mediaDir = mediaDir;
            this.vector3Factory = vector3Factory;
            this.tgcPlaneFactory = tgcPlaneFactory;
            this.planeSize = planeSize;
            this.random = new Random();
            return new List<List<TgcPlane>> {
                CreateFloor(),
                CreateRoof(),
                CreateWalls()
            };
        }

        private List<TgcPlane> CreateFloor()
        {
            var floor = new List<TgcPlane>();
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, mediaDir + @"\floor.bmp");

            for (var i = 0; i < 10; i++)
            {
                var floorElement = tgcPlaneFactory.CreateTgcPlane();
                floorElement.setTexture(texture);
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
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, mediaDir + @"\roof.bmp");

            for (var i = 0; i < 10; i++)
            {
                var roofElement = tgcPlaneFactory.CreateTgcPlane();
                roofElement.setTexture(texture);
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

        private List<TgcPlane> CreateWalls()
        {
            var walls = new List<TgcPlane>();
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, mediaDir + @"\wall.bmp");

            for (var i = 0; i < 10; i++)
            {
                if (random.Next(1, 10) < 8)
                {
                    var wallElement = tgcPlaneFactory.CreateTgcPlane();
                    wallElement.setTexture(texture);
                    wallElement.Origin = vector3Factory.CreateVector3(0, 0, planeSize * i);
                    wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                    wallElement.Orientation = TgcPlane.Orientations.YZplane;
                    wallElement.AutoAdjustUv = false;
                    wallElement.UTile = 1;
                    wallElement.VTile = 1;
                    walls.Add(wallElement);
                }
            }

            for (var i = 0; i < 10; i++)
            {
                if (random.Next(1, 10) < 8)
                {
                    var wallElement = tgcPlaneFactory.CreateTgcPlane();
                    wallElement.setTexture(texture);
                    wallElement.Origin = vector3Factory.CreateVector3(planeSize, 0, planeSize * i);
                    wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                    wallElement.Orientation = TgcPlane.Orientations.YZplane;
                    wallElement.AutoAdjustUv = false;
                    wallElement.UTile = 1;
                    wallElement.VTile = 1;
                    walls.Add(wallElement);
                }
            }

            return walls;
        }
    }
}
