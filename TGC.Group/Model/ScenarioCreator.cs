using System;
using System.Collections.Generic;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Group.Interfaces;

namespace TGC.Group.Model
{
    public class ScenarioCreator : IScenarioCreator
    {
        private const string horizontal = "H";
        private const int scenarioDepth = 22;
        private const int scenarioWide = 21;
        private const int cantidadObjetos = 500;
        private const string vertical = "V";

        /// <summary>
        /// Directorio de texturas, etc.
        /// </summary>
        private string mediaDir;

        /// <summary>
        /// Tamano de los planos
        /// </summary>
        private float planeSize;

        /// <summary>
        /// Fabrica de <see cref="TgcPlane"/>
        /// </summary>
        private ITgcPlaneFactory tgcPlaneFactory;

        /// <summary>
        /// Fabrica de <see cref="Vector3"/>
        /// </summary>
        private IVector3Factory vector3Factory;

        private List<IRenderObject> walls;
        private TgcTexture wallTexture;
        private Random random;


        public List<List<IRenderObject>> CreateScenario(string mediaDir, IVector3Factory vector3Factory, ITgcPlaneFactory tgcPlaneFactory, float planeSize)
        {
            this.mediaDir = mediaDir;
            this.vector3Factory = vector3Factory;
            this.tgcPlaneFactory = tgcPlaneFactory;
            this.planeSize = planeSize;
            random = new Random();
            return new List<List<IRenderObject>> {
                CreateFloor(),
                CreateRoof(),
                CreateWalls(),
                CreateObjects()
            };
        }

        private List<IRenderObject> CreateObjects()
        {
            var objects = new List<IRenderObject>();
            var meshMesa = new TgcSceneLoader().loadSceneFromFile(mediaDir + @"Mesa\Mesa-TgcScene.xml").Meshes[0];
            var meshLamparaTecho = new TgcSceneLoader().loadSceneFromFile(mediaDir + @"LamparaTecho\LamparaTecho-TgcScene.xml").Meshes[0];
            var meshSillon = new TgcSceneLoader().loadSceneFromFile(mediaDir + @"Sillon\Sillon-TgcScene.xml").Meshes[0];
            var meshLockerMetal = new TgcSceneLoader().loadSceneFromFile(mediaDir + @"LockerMetal\LockerMetal-TgcScene.xml").Meshes[0];
            var meshInstance = meshMesa.createMeshInstance(meshMesa.Name + "_default");

            while (objects.Count < cantidadObjetos)
            {
                var value = random.Next(8);
                if (0 <= value && value <= 2)
                {
                    meshInstance = meshMesa.createMeshInstance(objects.Count + "_" + meshMesa.Name);
                }

                if (2 < value && value <= 4)
                {
                    meshInstance = meshLamparaTecho.createMeshInstance(objects.Count + "_" + meshLamparaTecho.Name);
                }

                if (4 < value && value <= 6)
                {
                    meshInstance = meshSillon.createMeshInstance(objects.Count + "_" + meshSillon.Name);
                }

                if (6 < value && value <= 8)
                {
                    meshInstance = meshLockerMetal.createMeshInstance(objects.Count + "_" + meshLockerMetal.Name);
                }

                meshInstance.AutoTransformEnable = true;
                meshInstance.Scale = vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f);
                meshInstance.move(random.Next(0, scenarioWide) * planeSize, 0, random.Next(0, scenarioDepth) * planeSize);
                objects.Add(meshInstance);
            }

            return objects;
        }

        private List<IRenderObject> CreateFloor()
        {
            return CreateHorizontalLayer(0, TgcTexture.createTexture(D3DDevice.Instance.Device, mediaDir + @"\floor.bmp"));
        }

        private List<IRenderObject> CreateHorizontalLayer(float yCoordinate, TgcTexture wallTexture)
        {
            var layer = new List<IRenderObject>();

            for (var i = 0; i < scenarioWide; i++)
            {
                for (int j = 0; j < scenarioDepth; j++)
                {
                    var floorElement = tgcPlaneFactory.CreateTgcPlane();
                    floorElement.setTexture(wallTexture);
                    floorElement.Origin = vector3Factory.CreateVector3(planeSize * i, yCoordinate, planeSize * j);
                    floorElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                    floorElement.Orientation = TgcPlane.Orientations.XZplane;
                    floorElement.AutoAdjustUv = false;
                    floorElement.UTile = 1;
                    floorElement.VTile = 1;
                    layer.Add(floorElement);
                }
            }

            return layer;
        }

        private List<IRenderObject> CreateRoof()
        {
            return CreateHorizontalLayer(planeSize, TgcTexture.createTexture(D3DDevice.Instance.Device, mediaDir + @"\roof.bmp"));
        }

        private void CreateWallLine(string orientation, float xCoordinate, float[] zCoordinates)
        {
            foreach (var posicion in zCoordinates)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(wallTexture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * xCoordinate, 0, planeSize * posicion);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = orientation == horizontal ? TgcPlane.Orientations.XYplane : TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }
        }

        private List<IRenderObject> CreateWalls()
        {
            walls = new List<IRenderObject>();
            wallTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, mediaDir + @"\wall.bmp");

            // PAREDES VERTICALES
            CreateWallLine(vertical, 0, new float[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 });
            CreateWallLine(vertical, 2, new float[] { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 20, 21 });
            CreateWallLine(vertical, 3, new float[] { 0, 1, 2, 3 });
            CreateWallLine(vertical, 4, new float[] { 2, 3, 4, 5, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 20, 21 });
            CreateWallLine(vertical, 5, new float[] { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 });
            CreateWallLine(vertical, 6, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 20, 21 });
            CreateWallLine(vertical, 8, new float[] { 1, 2 });
            CreateWallLine(vertical, 9, new float[] { 0, 1, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            CreateWallLine(vertical, 12, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            CreateWallLine(vertical, 14, new float[] { 4, 5, 16 });
            CreateWallLine(vertical, 15, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            CreateWallLine(vertical, 16, new float[] { 0, 1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            CreateWallLine(vertical, 17, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 10, 11, 12, 13, 15, 16, 17, 18, 21 });
            CreateWallLine(vertical, 19, new float[] { 0, 1, 2, 4, 5, 7, 8, 13, 20, 21 });
            CreateWallLine(vertical, 20, new float[] { 9, 10, 14, 15, 16, 19 });
            CreateWallLine(vertical, 21, new float[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 });

            // PAREDES HORIZONTALES
            CreateWallLine(horizontal, 0, new float[] { 0, 2, 4, 20, 22 });
            CreateWallLine(horizontal, 1, new float[] { 0, 2, 4, 20, 22 });
            CreateWallLine(horizontal, 2, new float[] { 0, 2, 4, 8, 10, 12, 18, 20, 22 });
            CreateWallLine(horizontal, 3, new float[] { 0, 2, 4, 8, 10, 12, 18, 20, 22 });
            CreateWallLine(horizontal, 4, new float[] { 0, 2, 6, 8, 20, 22 });
            CreateWallLine(horizontal, 5, new float[] { 0, 2, 6, 16, 18, 20, 22 });
            CreateWallLine(horizontal, 6, new float[] { 0, 1, 3, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            CreateWallLine(horizontal, 7, new float[] { 0, 1, 3, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            CreateWallLine(horizontal, 8, new float[] { 0, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            CreateWallLine(horizontal, 9, new float[] { 0, 6, 16, 22 });
            CreateWallLine(horizontal, 10, new float[] { 0, 6, 16, 22 });
            CreateWallLine(horizontal, 11, new float[] { 0, 6, 16, 22 });

            // DEL 13 AL 14 FALTA UNA PARED Q NO SE COMO ESCRIBIR
            CreateWallLine(horizontal, 12, new float[] { 0, 3, 4, 6, 7, 10, 11, 15, 16, 17, 18, 20, 22 });
            CreateWallLine(horizontal, 13, new float[] { 0, 3, 4, 6, 7, 10, 11, 15, 16, 17, 18, 20, 22 });
            CreateWallLine(horizontal, 14, new float[] { 0, 3, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            CreateWallLine(horizontal, 15, new float[] { 0, 3, 4, 6, 16, 20, 22 });
            CreateWallLine(horizontal, 16, new float[] { 0, 3, 4, 6, 20, 22 });
            CreateWallLine(horizontal, 17, new float[] { 0, 3, 4, 6, 7, 13, 14, 15, 19, 20, 22 });
            CreateWallLine(horizontal, 18, new float[] { 0, 3, 4, 6, 7, 13, 14, 15, 19, 20, 22 });
            CreateWallLine(horizontal, 19, new float[] { 0, 2, 4, 6, 7, 9, 15, 20, 22 });
            CreateWallLine(horizontal, 20, new float[] { 0, 2, 4, 6, 7, 9, 11, 14, 17, 19, 22 });

            return walls;
        }
    }
}
