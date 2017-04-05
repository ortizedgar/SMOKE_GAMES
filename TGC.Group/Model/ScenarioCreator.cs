using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.DirectX;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Group.Interfaces;

namespace TGC.Group.Model
{
    public class ScenarioCreator : IScenarioCreator
    {
        /// <summary>
        /// Representa la direccion Este
        /// </summary>
        private string Este { get; } = "E";

        /// <summary>
        /// Representa la orientacion horizontal
        /// </summary>
        private string Horizontal { get; } = "H";

        /// <summary>
        /// Directorio de texturas, etc.
        /// </summary>
        private string MediaDir { get; set; }

        /// <summary>
        /// Representa la direccion Norte
        /// </summary>
        private string Norte { get; } = "N";

        /// <summary>
        /// Lista con todos los objetos del escenario
        /// </summary>
        private List<IRenderObject> Objects { get; set; }

        /// <summary>
        /// Representa la direccion Oeste
        /// </summary>
        private string Oeste { get; } = "O";

        /// <summary>
        /// Tamano de los planos
        /// </summary>
        private float PlaneSize { get; set; }

        /// <summary>
        /// Largo total del escenario (eje Z)
        /// </summary>
        private int ScenarioDepth { get; } = 22;

        /// <summary>
        /// Ancho total del escenario (eje X)
        /// </summary>
        private int ScenarioWide { get; } = 21;

        /// <summary>
        /// Representa la direccion sur
        /// </summary>
        private string Sur { get; } = "S";

        /// <summary>
        /// Fabrica de <see cref="TgcPlane"/>
        /// </summary>
        private ITgcPlaneFactory TgcPlaneFactory { get; set; }

        /// <summary>
        /// Fabrica de <see cref="Vector3"/>
        /// </summary>
        private IVector3Factory Vector3Factory { get; set; }

        /// <summary>
        /// Representa la orientacion vertical
        /// </summary>
        private string Vertical { get; } = "V";

        /// <summary>
        /// Lista de objetos que representan las paredes
        /// </summary>
        private List<IRenderObject> Walls { get; set; }

        /// <summary>
        /// Textura de las paredes
        /// </summary>
        private TgcTexture WallTexture { get; set; }

        /// <summary>
        /// Crea la lista con todos los objetos que componen el escenario
        /// </summary>
        /// <param name="mediaDir">Directorio de medios</param>
        /// <param name="planeSize">Tamaño de las paredes</param>
        /// <param name="container">Container IOC</param>
        /// <returns></returns>
        public List<Tuple<string, List<IRenderObject>>> CreateScenario(string mediaDir, IContainer container, float planeSize)
        {
            MediaDir = mediaDir;
            PlaneSize = planeSize;
            TgcPlaneFactory = container.Resolve<TgcPlaneFactory>();
            Vector3Factory = container.Resolve<Vector3Factory>();

            return new List<Tuple<string, List<IRenderObject>>> {
                Tuple.Create("Floor", CreateFloor()),
                Tuple.Create("Roof", CreateRoof()),
                Tuple.Create(nameof(Walls), CreateWalls()),
                Tuple.Create(nameof(Objects), CreateObjects())
            };
        }

        /// <summary>
        /// Calcula la rotacion del objeto sobre el eje Y en base a la orientacion indicada
        /// </summary>
        /// <param name="orientation">Orientacion deseada</param>
        /// <returns><see cref="Vector3"/> indicando la rotacion del objeto</returns>
        private Vector3 CalculateRotation(string orientation)
        {
            if (orientation == Sur)
            {
                return Vector3Factory.CreateVector3(0, 600, 0);
            }
            if (orientation == Este)
            {
                return Vector3Factory.CreateVector3(0, 300, 0);
            }
            if (orientation == Oeste)
            {
                return Vector3Factory.CreateVector3(0, 900, 0);
            }

            return Vector3Factory.CreateVector3(0, 0, 0);
        }

        /// <summary>
        /// Calcula la transalacion del objeto, diferenciando las puertas
        /// </summary>
        /// <param name="mesh">Mesh a transladar</param>
        /// <param name="orientation">Orientacion del mesh</param>
        /// <param name="xCoordinate">Cordenada X</param>
        /// <param name="yCoordinate">Cordenada y</param>
        /// <param name="zCoordinate">Cordenada Z</param>
        /// <returns></returns>
        private Vector3 CalculateTranslation(TgcMesh mesh, string orientation, float xCoordinate, float yCoordinate, float zCoordinate)
        {
            if (mesh.Name.Equals("Puerta", StringComparison.OrdinalIgnoreCase))
            {
                return orientation == Este || orientation == Oeste ? Vector3Factory.CreateVector3(xCoordinate, yCoordinate, zCoordinate + 5) : Vector3Factory.CreateVector3(xCoordinate + 5, yCoordinate, zCoordinate);
            }

            return Vector3Factory.CreateVector3(xCoordinate, yCoordinate, zCoordinate);
        }

        /// <summary>
        /// Crea el piso
        /// </summary>
        /// <returns>La lista con todos los objetos que componen el piso</returns>
        private List<IRenderObject> CreateFloor()
        {
            return CreateHorizontalLayer(0, TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"\floor.bmp"));
        }

        /// <summary>
        /// Crea una capa horizontal de planos que ocupan todo el escenario
        /// </summary>
        /// <param name="yCoordinate">Indica la altura sobre la cual debe crearse la capa</param>
        /// <param name="texture">Indica la textura con la cual debe crearse cada elemento de la capa</param>
        /// <returns></returns>
        private List<IRenderObject> CreateHorizontalLayer(float yCoordinate, TgcTexture texture)
        {
            var layer = new List<IRenderObject>();
            TgcPlane layerElement;

            for (var i = 0; i < ScenarioWide; i++)
            {
                for (int j = 0; j < ScenarioDepth; j++)
                {
                    layerElement = TgcPlaneFactory.CreateTgcPlane();
                    layerElement.setTexture(texture);
                    layerElement.Origin = Vector3Factory.CreateVector3(PlaneSize * i, yCoordinate, PlaneSize * j);
                    layerElement.Size = Vector3Factory.CreateVector3(PlaneSize, PlaneSize, PlaneSize);
                    layerElement.Orientation = TgcPlane.Orientations.XZplane;
                    layerElement.AutoAdjustUv = false;
                    layerElement.UTile = 1;
                    layerElement.VTile = 1;
                    layer.Add(layerElement);
                }
            }

            return layer;
        }

        /// <summary>
        /// Crea una lista de objetos que componen el escenario
        /// </summary>
        /// <returns></returns>
        private List<IRenderObject> CreateObjects()
        {
            Objects = new List<IRenderObject>();
            var meshMesa = new TgcSceneLoader().loadSceneFromFile(MediaDir + @"Mesa\Mesa-TgcScene.xml").Meshes[0];
            var meshLamparaTecho = new TgcSceneLoader().loadSceneFromFile(MediaDir + @"LamparaTecho\LamparaTecho-TgcScene.xml").Meshes[0];
            var meshSillon = new TgcSceneLoader().loadSceneFromFile(MediaDir + @"Sillon\Sillon-TgcScene.xml").Meshes[0];
            var meshLockerMetal = new TgcSceneLoader().loadSceneFromFile(MediaDir + @"LockerMetal\LockerMetal-TgcScene.xml").Meshes[0];
            var meshPuerta = new TgcSceneLoader().loadSceneFromFile(MediaDir + @"Puerta\Puerta-TgcScene.xml").Meshes[0];

            // Puertas horizontales
            CreateObjectsLine(meshPuerta, Norte, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 5, 0, new float[] { 40, 200 });
            CreateObjectsLine(meshPuerta, Norte, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 25, 0, new float[] { 100, 200 });
            CreateObjectsLine(meshPuerta, Norte, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 45, 0, new float[] { 200 });
            CreateObjectsLine(meshPuerta, Norte, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 60, 0, new float[] { 200 });
            CreateObjectsLine(meshPuerta, Norte, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 95, 0, new float[] { 0, 60, 160, 220 });
            CreateObjectsLine(meshPuerta, Norte, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 105, 0, new float[] { 0, 60, 160, 220 });
            CreateObjectsLine(meshPuerta, Norte, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 120, 0, new float[] { 40 });
            CreateObjectsLine(meshPuerta, Norte, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 130, 0, new float[] { 200 });
            CreateObjectsLine(meshPuerta, Norte, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 150, 0, new float[] { 40, 200 });
            CreateObjectsLine(meshPuerta, Norte, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 170, 0, new float[] { 30, 40 });
            CreateObjectsLine(meshPuerta, Norte, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 175, 0, new float[] { 70 });
            CreateObjectsLine(meshPuerta, Norte, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 180, 0, new float[] { 200 });
            CreateObjectsLine(meshPuerta, Norte, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 190, 0, new float[] { 20 });
            CreateObjectsLine(meshPuerta, Norte, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 200, 0, new float[] { 190 });

            // Puertas verticales
            CreateObjectsLine(meshPuerta, Este, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 20, 0, new float[] { 80, 100, 140 });
            CreateObjectsLine(meshPuerta, Este, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 30, 0, new float[] { 10, 30 });
            CreateObjectsLine(meshPuerta, Este, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 50, 0, new float[] { 160 });
            CreateObjectsLine(meshPuerta, Este, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 60, 0, new float[] { 0, 20, 40, 80, 140, 205 });
            CreateObjectsLine(meshPuerta, Este, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 120, 0, new float[] { 170 });
            CreateObjectsLine(meshPuerta, Este, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 140, 0, new float[] { 50, 160 });
            CreateObjectsLine(meshPuerta, Este, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 150, 0, new float[] { 0, 20, 70, 120, 200 });
            CreateObjectsLine(meshPuerta, Este, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 170, 0, new float[] { 0, 40, 130 });
            CreateObjectsLine(meshPuerta, Este, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 190, 0, new float[] { 40, 80, 200 });
            CreateObjectsLine(meshPuerta, Este, Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 200, 0, new float[] { 90, 140, 160 });

            // Lockers
            CreateObjectsLine(meshLockerMetal, Norte, Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 5, 0, new float[] { 5 });

            // Lamparas
            CreateObjectsLine(meshLamparaTecho, Sur, Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 5, PlaneSize, new float[] { 50 });

            // Sillones
            CreateObjectsLine(meshSillon, Este, Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 5, 0, new float[] { 100 });

            // Mesas
            CreateObjectsLine(meshMesa, Oeste, Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 5, 0, new float[] { 150 });

            return Objects;
        }

        /// <summary>
        /// Crea una linea de objetos que componen el escenario
        /// </summary>
        /// <param name="mesh">Objeto con el cual crear la linea</param>
        /// <param name="orientation">Orientacion del objeto</param>
        /// <param name="xCoordinate">Indica la ubicacion en la coordenada X de la linea de objetos</param>
        /// <param name="yCoordinate">Indica la ubicacion en la coordenada Y de la linea de objetos</param>
        /// <param name="zCoordinates">Indica la ubicacion en la coordenada Z donde debe colocarse cada objeto</param>
        /// <param name="scale">Tamaño del objeto</param>
        private void CreateObjectsLine(TgcMesh mesh, string orientation, Vector3 scale, float xCoordinate, float yCoordinate, float[] zCoordinates)
        {
            TgcMesh meshInstance;
            foreach (var zCoordinate in zCoordinates)
            {
                meshInstance = mesh.createMeshInstance(
                    Objects.Count + "_" + mesh.Name,
                    CalculateTranslation(mesh, orientation, xCoordinate, yCoordinate, zCoordinate),
                    CalculateRotation(orientation),
                    scale);
                meshInstance.AutoTransformEnable = true;
                Objects.Add(meshInstance);
            }
        }

        /// <summary>
        /// Crea el techo del escenario
        /// </summary>
        /// <returns>Lista con los objetos que componen el techo</returns>
        private List<IRenderObject> CreateRoof()
        {
            return CreateHorizontalLayer(PlaneSize, TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"\roof.bmp"));
        }

        /// <summary>
        /// Crea las paredes del escenario
        /// </summary>
        /// <returns>Lista con los objetos que componen la pared del escenario</returns>
        private List<IRenderObject> CreateWalls()
        {
            Walls = new List<IRenderObject>();
            WallTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"\wall.bmp");

            // Paredes verticales
            CreateWallsLine(Vertical, 0, new float[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 });
            CreateWallsLine(Vertical, 2, new float[] { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 20, 21 });
            CreateWallsLine(Vertical, 3, new float[] { 0, 1, 2, 3 });
            CreateWallsLine(Vertical, 4, new float[] { 2, 3, 4, 5, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 20, 21 });
            CreateWallsLine(Vertical, 5, new float[] { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 });
            CreateWallsLine(Vertical, 6, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 20, 21 });
            CreateWallsLine(Vertical, 8, new float[] { 1, 2 });
            CreateWallsLine(Vertical, 9, new float[] { 0, 1, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            CreateWallsLine(Vertical, 12, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            CreateWallsLine(Vertical, 14, new float[] { 4, 5, 16 });
            CreateWallsLine(Vertical, 15, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            CreateWallsLine(Vertical, 16, new float[] { 0, 1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            CreateWallsLine(Vertical, 17, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 10, 11, 12, 13, 15, 16, 17, 18, 21 });
            CreateWallsLine(Vertical, 19, new float[] { 0, 1, 2, 4, 5, 7, 8, 13, 20, 21 });
            CreateWallsLine(Vertical, 20, new float[] { 9, 10, 14, 15, 16, 19 });
            CreateWallsLine(Vertical, 21, new float[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 });

            // Paredes horizontales
            CreateWallsLine(Horizontal, 0, new float[] { 0, 2, 4, 20, 22 });
            CreateWallsLine(Horizontal, 1, new float[] { 0, 2, 4, 20, 22 });
            CreateWallsLine(Horizontal, 2, new float[] { 0, 2, 4, 8, 10, 12, 18, 20, 22 });
            CreateWallsLine(Horizontal, 3, new float[] { 0, 2, 4, 8, 10, 12, 18, 20, 22 });
            CreateWallsLine(Horizontal, 4, new float[] { 0, 2, 6, 8, 20, 22 });
            CreateWallsLine(Horizontal, 5, new float[] { 0, 2, 6, 16, 18, 20, 22 });
            CreateWallsLine(Horizontal, 6, new float[] { 0, 1, 3, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            CreateWallsLine(Horizontal, 7, new float[] { 0, 1, 3, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            CreateWallsLine(Horizontal, 8, new float[] { 0, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            CreateWallsLine(Horizontal, 9, new float[] { 0, 6, 16, 22 });
            CreateWallsLine(Horizontal, 10, new float[] { 0, 6, 16, 22 });
            CreateWallsLine(Horizontal, 11, new float[] { 0, 6, 16, 22 });
            CreateWallsLine(Horizontal, 12, new float[] { 0, 1.5f, 3, 4, 6, 7, 10, 11, 15, 16, 17, 18, 20, 22 });
            CreateWallsLine(Horizontal, 13, new float[] { 0, 1.5f, 3, 4, 6, 7, 10, 11, 15, 16, 17, 18, 20, 22 });
            CreateWallsLine(Horizontal, 14, new float[] { 0, 1.5f, 3, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            CreateWallsLine(Horizontal, 15, new float[] { 0, 4, 6, 16, 20, 22 });
            CreateWallsLine(Horizontal, 16, new float[] { 0, 3, 4, 6, 20, 22 });
            CreateWallsLine(Horizontal, 17, new float[] { 0, 3, 4, 6, 7, 13, 14, 15, 19, 20, 22 });
            CreateWallsLine(Horizontal, 18, new float[] { 0, 3, 4, 6, 7, 13, 14, 15, 19, 20, 22 });
            CreateWallsLine(Horizontal, 19, new float[] { 0, 2, 4, 6, 7, 9, 15, 20, 22 });
            CreateWallsLine(Horizontal, 20, new float[] { 0, 2, 4, 6, 7, 9, 11, 14, 17, 19, 22 });

            return Walls;
        }

        /// <summary>
        /// Crea una linea de paredes
        /// </summary>
        /// <param name="orientation">Orientacion de la pared</param>
        /// <param name="xCoordinate">Indica la ubicacion en la coordenada X de la linea de objetos</param>
        /// <param name="zCoordinates">Indica la ubicacion en la coordenada Z donde debe colocarse cada objeto</param>
        private void CreateWallsLine(string orientation, float xCoordinate, float[] zCoordinates)
        {
            foreach (var zCoordinate in zCoordinates)
            {
                var wallElement = TgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(WallTexture);
                wallElement.Origin = Vector3Factory.CreateVector3(PlaneSize * xCoordinate, 0, PlaneSize * zCoordinate);
                wallElement.Size = Vector3Factory.CreateVector3(PlaneSize, PlaneSize, PlaneSize);
                wallElement.Orientation = orientation == Horizontal ? TgcPlane.Orientations.XYplane : TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                Walls.Add(wallElement);
            }
        }
    }
}
