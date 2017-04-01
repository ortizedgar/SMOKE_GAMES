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
                //floorElement.setTexture(texture);
                //floorElement.Origin = vector3Factory.CreateVector3(0, 0, planeSize * i);
                //floorElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                //floorElement.Orientation = TgcPlane.Orientations.XZplane;
                //floorElement.AutoAdjustUv = false;
                //floorElement.UTile = 1;
                //floorElement.VTile = 1;
                //floor.Add(floorElement);
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
                //roofElement.setTexture(texture);
                //roofElement.Origin = vector3Factory.CreateVector3(0, planeSize, planeSize * i);
                //roofElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                //roofElement.Orientation = TgcPlane.Orientations.XZplane;
                //roofElement.AutoAdjustUv = false;
                //roofElement.UTile = 1;
                //roofElement.VTile = 1;
                //roof.Add(roofElement);
            }

            return roof;
        }

        private List<TgcPlane> CreateWalls()
        {
            var walls = new List<TgcPlane>();
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, mediaDir + @"\wall.bmp");

            //for (var i = 0; i < 10; i++)
            //{
            //    if (random.Next(1, 10) < 8)
            //    {
            //        var wallElement = tgcPlaneFactory.CreateTgcPlane();
            //        wallElement.setTexture(texture);
            //        wallElement.Origin = vector3Factory.CreateVector3(0, 0, planeSize * i);
            //        wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
            //        wallElement.Orientation = TgcPlane.Orientations.YZplane;
            //        wallElement.AutoAdjustUv = false;
            //        wallElement.UTile = 1;
            //        wallElement.VTile = 1;
            //        walls.Add(wallElement);
            //    }
            //}

            //for (var i = 0; i < 10; i++)
            //{
            //    if (random.Next(1, 10) < 8)
            //    {
            //        var wallElement = tgcPlaneFactory.CreateTgcPlane();
            //        wallElement.setTexture(texture);
            //        wallElement.Origin = vector3Factory.CreateVector3(planeSize * 3, 0, planeSize * i);
            //        wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
            //        wallElement.Orientation = TgcPlane.Orientations.YZplane;
            //        wallElement.AutoAdjustUv = false;
            //        wallElement.UTile = 1;
            //        wallElement.VTile = 1;
            //        walls.Add(wallElement);
            //    }
            //}

            // PAREDES VERTICALES
            var posiciones = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (-1 - 2 - 1 - 1 - 1 - 1 - 2), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 20, 21 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (-1 - 2 - 1 - 1 - 1 - 1), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }
            
            posiciones = new List<int> { 0, 1, 2, 3 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (-1 - 2 - 1 - 1 - 1), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 2, 3, 4, 5, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 20, 21 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (-1 - 2 - 1 - 1), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (-1 - 2 - 1), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 20, 21 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (-1 - 2), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 1, 2 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (-1), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 0, 1, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(0, 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * 3, 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 4, 5, 16 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (3 + 2), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (3 + 2 + 1), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 0, 1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (3 + 2 + 1 + 1), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 0, 1, 2, 4, 5, 7, 8, 9, 10, 11, 12, 13, 15, 16, 17, 18, 21 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (3 + 2 + 1 + 1 + 1), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 0, 1, 2, 4, 5, 7, 8, 13, 20, 21 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (3 + 2 + 1 + 1 + 1 + 2), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 9, 10, 14, 15, 16, 19 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (3 + 2 + 1 + 1 + 1 + 2 + 1), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (3 + 2 + 1 + 1 + 1 + 2 + 1 + 1), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            // PAREDES HORIZONTALES
            posiciones = new List<int> { 0, 6, 16, 22 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(0, 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.XYplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 0, 6, 16, 22 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * 1, 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.XYplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 0, 6, 16, 22 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (1 + 1), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.XYplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 0, 3, 4, 6, 7, 10, 11, 15, 16, 17, 18 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (1 + 1 + 1), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.XYplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 0, 3, 4, 6, 7, 10, 11, 15, 16, 17, 18 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (1 + 1 + 1 + 1), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.XYplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            posiciones = new List<int> { 0, 3, 6, 7, 10, 11, 15, 16, 18, 20 };
            foreach (var posicion in posiciones)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(texture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * (1 + 1 + 1 + 1 + 1), 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = TgcPlane.Orientations.XYplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }

            //for (int i = 0; i < 23; i++)
            //{
            //    if (i == 6 || i == 16 || i == 22)
            //    {
            //        var wallElement = tgcPlaneFactory.CreateTgcPlane();
            //        wallElement.setTexture(texture);
            //        wallElement.Origin = vector3Factory.CreateVector3(0, 0, planeSize * i);
            //        wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
            //        wallElement.Orientation = TgcPlane.Orientations.XYplane;
            //        wallElement.AutoAdjustUv = false;
            //        wallElement.UTile = 1;
            //        wallElement.VTile = 1;
            //        walls.Add(wallElement);
            //    }
            //}

            return walls;
        }
    }
}
