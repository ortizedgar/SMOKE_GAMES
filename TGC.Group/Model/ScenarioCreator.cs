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
        private const string vertical = "V";
        private int cantidadObjetos = 500;
        private List<IRenderObject> doors;
        private int doorsCount;

        /// <summary>
        /// Directorio de texturas, etc.
        /// </summary>
        private string mediaDir;
        private TgcMesh meshPuerta;

        /// <summary>
        /// Tamano de los planos
        /// </summary>
        private float planeSize;
        private Random random;

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

        public List<List<IRenderObject>> CreateScenario(string mediaDir, IVector3Factory vector3Factory, ITgcPlaneFactory tgcPlaneFactory, float planeSize)
        {
            this.mediaDir = mediaDir;
            this.vector3Factory = vector3Factory;
            this.tgcPlaneFactory = tgcPlaneFactory;
            this.planeSize = planeSize;
            doorsCount = 0;
            random = new Random();
            return new List<List<IRenderObject>> {
                //CreateFloor(),
                //CreateRoof(),
                CreateWalls(),
                CreateDoors(),
                //createwindow
                //CreateObjects()
            };
        }


        private List<IRenderObject> CreateDoors()
        {
            doors = new List<IRenderObject>();
            meshPuerta = new TgcSceneLoader().loadSceneFromFile(mediaDir + @"Puerta\Puerta-TgcScene.xml").Meshes[0];


            CreateDoorsLine(horizontal, 5, new float[] { 40, 200 });
            CreateDoorsLine(horizontal, 25, new float[] {100, 200 });
            CreateDoorsLine(horizontal, 45, new float[] { 200 });
            CreateDoorsLine(horizontal, 60, new float[] { 200 });
            CreateDoorsLine(horizontal, 95, new float[] { 0, 60, 160, 220 });
            CreateDoorsLine(horizontal, 105, new float[] { 0, 60, 160, 220 });
            CreateDoorsLine(horizontal, 120, new float[] { 40 });
            CreateDoorsLine(horizontal, 130, new float[] { 200 });
            CreateDoorsLine(horizontal, 150, new float[] { 40, 200 });
            CreateDoorsLine(horizontal, 170, new float[] { 30, 40 });
            CreateDoorsLine(horizontal, 175, new float[] { 70 });
            CreateDoorsLine(horizontal, 180, new float[] { 200 });
            CreateDoorsLine(horizontal, 190, new float[] { 20 });
            CreateDoorsLine(horizontal, 200, new float[] { 190 });






            CreateDoorsLine(vertical, 20, new float[] { 80, 100, 140});
            CreateDoorsLine(vertical, 30, new float[] { 10, 30 });
            CreateDoorsLine(vertical, 50, new float[] { 160 });
            CreateDoorsLine(vertical, 60, new float[] { 0, 20, 40, 80, 140, 205 });
            CreateDoorsLine(vertical, 120, new float[] { 170 });
            CreateDoorsLine(vertical, 140, new float[] { 50, 160 });
            CreateDoorsLine(vertical, 150, new float[] { 0, 20, 70, 120, 200 });
            CreateDoorsLine(vertical, 170, new float[] { 0, 40, 130 });
            CreateDoorsLine(vertical, 190, new float[] { 40, 80, 200 });
            CreateDoorsLine(vertical, 200, new float[] { 90, 140, 160 });





            return doors;
        }

        private void CreateDoorsLine(string orientation, float xCoordinate, float[] zCoordinates)
        {
            foreach (var zCoordinate in zCoordinates)
            {
                var meshInstance = meshPuerta.createMeshInstance(
                    doors.Count + "_" + meshPuerta.Name,
                    orientation == horizontal ? vector3Factory.CreateVector3(xCoordinate + 5, 0, zCoordinate) : vector3Factory.CreateVector3(xCoordinate, 0, zCoordinate + 5),
                    orientation == horizontal ? vector3Factory.CreateVector3(0, 0, 0) : vector3Factory.CreateVector3(0, 300, 0),
                    vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f));
                meshInstance.AutoTransformEnable = true;
                doors.Add(meshInstance);
                doorsCount++;
            }
        }

        private List<IRenderObject> CreateFloor()
        {
            return CreateHorizontalLayer(0, TgcTexture.createTexture(D3DDevice.Instance.Device, mediaDir + @"\floor.bmp"));
        }

        private List<IRenderObject> CreateHorizontalLayer(float yCoordinate, TgcTexture texture)
        {
            var layer = new List<IRenderObject>();

            for (var i = 0; i < scenarioWide; i++)
            {
                for (int j = 0; j < scenarioDepth; j++)
                {
                    var floorElement = tgcPlaneFactory.CreateTgcPlane();
                    floorElement.setTexture(texture);
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

        private List<IRenderObject> CreateObjects()
        {
            var objects = new List<IRenderObject>();
            var meshMesa = new TgcSceneLoader().loadSceneFromFile(mediaDir + @"Mesa\Mesa-TgcScene.xml").Meshes[0];
            var meshLamparaTecho = new TgcSceneLoader().loadSceneFromFile(mediaDir + @"LamparaTecho\LamparaTecho-TgcScene.xml").Meshes[0];
            var meshSillon = new TgcSceneLoader().loadSceneFromFile(mediaDir + @"Sillon\Sillon-TgcScene.xml").Meshes[0];
            var meshLockerMetal = new TgcSceneLoader().loadSceneFromFile(mediaDir + @"LockerMetal\LockerMetal-TgcScene.xml").Meshes[0];
            var meshInstance = meshMesa.createMeshInstance(meshMesa.Name + "_default");
            cantidadObjetos -= doorsCount;

            while (objects.Count < cantidadObjetos)
            {
                var value = random.Next(8);
                var xCoordinate = (float)random.NextDouble() * scenarioWide * planeSize;
                var zCoordinate = (float)random.NextDouble() * scenarioDepth * planeSize;
                if (0 <= value && value <= 2)
                {
                    meshInstance = meshMesa.createMeshInstance(objects.Count + "_" + meshMesa.Name, vector3Factory.CreateVector3(xCoordinate, 0, zCoordinate), vector3Factory.CreateVector3(0, 0, 0), vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f));
                }

                if (2 < value && value <= 4)
                {
                    meshInstance = meshLamparaTecho.createMeshInstance(objects.Count + "_" + meshLamparaTecho.Name, vector3Factory.CreateVector3(xCoordinate, planeSize, zCoordinate), vector3Factory.CreateVector3(0, 0, 0), vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f));
                }

                if (4 < value && value <= 6)
                {
                    meshInstance = meshSillon.createMeshInstance(objects.Count + "_" + meshSillon.Name, vector3Factory.CreateVector3(xCoordinate, 0, zCoordinate), vector3Factory.CreateVector3(0, 0, 0), vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f));
                }

                if (6 < value && value <= 8)
                {
                    meshInstance = meshLockerMetal.createMeshInstance(objects.Count + "_" + meshLockerMetal.Name, vector3Factory.CreateVector3(xCoordinate, 0, zCoordinate), vector3Factory.CreateVector3(0, 0, 0), vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f));
                }

                meshInstance.AutoTransformEnable = true;
                objects.Add(meshInstance);
            }

            return objects;
        }

        private List<IRenderObject> CreateRoof()
        {
            return CreateHorizontalLayer(planeSize, TgcTexture.createTexture(D3DDevice.Instance.Device, mediaDir + @"\roof.bmp"));
        }

        private List<IRenderObject> CreateWalls()
        {
            walls = new List<IRenderObject>();
            wallTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, mediaDir + @"\wall.bmp");

            // PAREDES VERTICALES
            CreateWallsLine(vertical, 0, new float[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 });
            CreateWallsLine(vertical, 2, new float[] { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 20, 21 });
            CreateWallsLine(vertical, 3, new float[] { 0, 1, 2, 3 });
            CreateWallsLine(vertical, 4, new float[] { 2, 3, 4, 5, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 20, 21 });
            CreateWallsLine(vertical, 5, new float[] { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 });
            CreateWallsLine(vertical, 6, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 20, 21 });
            CreateWallsLine(vertical, 8, new float[] { 1, 2 });
            CreateWallsLine(vertical, 9, new float[] { 0, 1, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            CreateWallsLine(vertical, 12, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            CreateWallsLine(vertical, 14, new float[] { 4, 5, 16 });
            CreateWallsLine(vertical, 15, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            CreateWallsLine(vertical, 16, new float[] { 0, 1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            CreateWallsLine(vertical, 17, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 10, 11, 12, 13, 15, 16, 17, 18, 21 });
            CreateWallsLine(vertical, 19, new float[] { 0, 1, 2, 4, 5, 7, 8, 13, 20, 21 });
            CreateWallsLine(vertical, 20, new float[] { 9, 10, 14, 15, 16, 19 });
            CreateWallsLine(vertical, 21, new float[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 });

            // PAREDES HORIZONTALES
            CreateWallsLine(horizontal, 0, new float[] { 0, 2, 4, 20, 22 });
            CreateWallsLine(horizontal, 1, new float[] { 0, 2, 4, 20, 22 });
            CreateWallsLine(horizontal, 2, new float[] { 0, 2, 4, 8, 10, 12, 18, 20, 22 });
            CreateWallsLine(horizontal, 3, new float[] { 0, 2, 4, 8, 10, 12, 18, 20, 22 });
            CreateWallsLine(horizontal, 4, new float[] { 0, 2, 6, 8, 20, 22 });
            CreateWallsLine(horizontal, 5, new float[] { 0, 2, 6, 16, 18, 20, 22 });
            CreateWallsLine(horizontal, 6, new float[] { 0, 1, 3, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            CreateWallsLine(horizontal, 7, new float[] { 0, 1, 3, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            CreateWallsLine(horizontal, 8, new float[] { 0, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            CreateWallsLine(horizontal, 9, new float[] { 0, 6, 16, 22 });
            CreateWallsLine(horizontal, 10, new float[] { 0, 6, 16, 22 });
            CreateWallsLine(horizontal, 11, new float[] { 0, 6, 16, 22 });
            CreateWallsLine(horizontal, 12, new float[] { 0, 1.5f, 3, 4, 6, 7, 10, 11, 15, 16, 17, 18, 20, 22 });
            CreateWallsLine(horizontal, 13, new float[] { 0, 1.5f, 3, 4, 6, 7, 10, 11, 15, 16, 17, 18, 20, 22 });
            CreateWallsLine(horizontal, 14, new float[] { 0, 1.5f, 3, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            CreateWallsLine(horizontal, 15, new float[] { 0, 4, 6, 16, 20, 22 });
            CreateWallsLine(horizontal, 16, new float[] { 0, 3, 4, 6, 20, 22 });
            CreateWallsLine(horizontal, 17, new float[] { 0, 3, 4, 6, 7, 13, 14, 15, 19, 20, 22 });
            CreateWallsLine(horizontal, 18, new float[] { 0, 3, 4, 6, 7, 13, 14, 15, 19, 20, 22 });
            CreateWallsLine(horizontal, 19, new float[] { 0, 2, 4, 6, 7, 9, 15, 20, 22 });
            CreateWallsLine(horizontal, 20, new float[] { 0, 2, 4, 6, 7, 9, 11, 14, 17, 19, 22 });

            return walls;
        }

        private void CreateWallsLine(string orientation, float xCoordinate, float[] zCoordinates)
        {
            foreach (var zCoordinate in zCoordinates)
            {
                var wallElement = tgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(wallTexture);
                wallElement.Origin = vector3Factory.CreateVector3(planeSize * xCoordinate, 0, planeSize * zCoordinate);
                wallElement.Size = vector3Factory.CreateVector3(planeSize, planeSize, planeSize);
                wallElement.Orientation = orientation == horizontal ? TgcPlane.Orientations.XYplane : TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                walls.Add(wallElement);
            }
        }
    }
}
