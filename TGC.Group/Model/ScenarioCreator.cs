﻿namespace TGC.Group.Model
{
    using System;
    using System.Collections.Generic;
    using Autofac;
    using Microsoft.DirectX;
    using TGC.Core.Direct3D;
    using TGC.Core.Geometry;
    using TGC.Core.SceneLoader;
    using TGC.Core.Textures;
    using TGC.Group.Interfaces;

    public class ScenarioCreator : IScenarioCreator
    {
        /// <summary>
        /// Representa la direccion Este
        /// </summary>
        private string Este { get; } = "E";

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
        /// Tamaño del plano de las paredes, techo y piso
        /// </summary>
        private float PlaneSize { get; } = 10;

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
        /// Objeto <see cref="TgcSceneLoader"/>
        /// </summary>
        private TgcSceneLoader TgcSceneLoader { get; set; }

        /// <summary>
        /// Fabrica de <see cref="Vector3"/>
        /// </summary>
        private IVector3Factory Vector3Factory { get; set; }

        /// <summary>
        /// Fabrica de <see cref="TgcTexture"/>
        /// </summary>
        private ITgcTextureFactory TgcTextureFactory { get; set; }

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
        /// <param name="container">Container IOC</param>
        /// <returns></returns>
        public List<Tuple<string, List<IRenderObject>>> CreateScenario(string mediaDir, IContainer container)
        {
            this.MediaDir = mediaDir;
            this.TgcSceneLoader = container.Resolve<TgcSceneLoader>();
            this.TgcPlaneFactory = container.Resolve<ITgcPlaneFactory>();
            this.Vector3Factory = container.Resolve<IVector3Factory>();
            this.TgcTextureFactory = container.Resolve<ITgcTextureFactory>();

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
            if (orientation == this.Sur)
            {
                return this.Vector3Factory.CreateVector3(0, 600, 0);
            }
            if (orientation == this.Este)
            {
                return this.Vector3Factory.CreateVector3(0, 300, 0);
            }
            if (orientation == this.Oeste)
            {
                return this.Vector3Factory.CreateVector3(0, 900, 0);
            }

            return this.Vector3Factory.CreateVector3(0, 0, 0);
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
                var doorDisplacement = this.PlaneSize / 2;
                return orientation == this.Este || orientation == this.Oeste ?
                    this.Vector3Factory.CreateVector3(xCoordinate, yCoordinate, zCoordinate + doorDisplacement)
                    : this.Vector3Factory.CreateVector3(xCoordinate + doorDisplacement, yCoordinate, zCoordinate);
            }

            return this.Vector3Factory.CreateVector3(xCoordinate, yCoordinate, zCoordinate);
        }

        /// <summary>
        /// Crea el piso
        /// </summary>
        /// <returns>La lista con todos los objetos que componen el piso</returns>
        private List<IRenderObject> CreateFloor() => CreateHorizontalLayer(0, this.TgcTextureFactory.CreateTexture(D3DDevice.Instance.Device, this.MediaDir + @"\floor.bmp"));

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
            for (var i = 0; i < this.ScenarioWide; i++)
            {
                for (var j = 0; j < this.ScenarioDepth; j++)
                {
                    layerElement = this.TgcPlaneFactory.CreateTgcPlane();
                    layerElement.setTexture(texture);
                    layerElement.Origin = this.Vector3Factory.CreateVector3(this.PlaneSize * i, yCoordinate, this.PlaneSize * j);
                    layerElement.Size = this.Vector3Factory.CreateVector3(this.PlaneSize, this.PlaneSize, this.PlaneSize);
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
            this.Objects = new List<IRenderObject>();

            var meshMesa = this.TgcSceneLoader.loadSceneFromFile(this.MediaDir + @"Mesa\Mesa-TgcScene.xml").Meshes[0];
            var meshLamparaTecho = this.TgcSceneLoader.loadSceneFromFile(this.MediaDir + @"LamparaTecho\LamparaTecho-TgcScene.xml").Meshes[0];
            var meshSillon = this.TgcSceneLoader.loadSceneFromFile(this.MediaDir + @"Sillon\Sillon-TgcScene.xml").Meshes[0];
            var meshLockerMetal = this.TgcSceneLoader.loadSceneFromFile(this.MediaDir + @"LockerMetal\LockerMetal-TgcScene.xml").Meshes[0];
            var meshPuerta = this.TgcSceneLoader.loadSceneFromFile(this.MediaDir + @"Puerta\Puerta-TgcScene.xml").Meshes[0];
            var meshDispenserAgua = this.TgcSceneLoader.loadSceneFromFile(this.MediaDir + @"DispenserAgua\DispenserAgua-TgcScene.xml").Meshes[0];
            var meshExpendedor = this.TgcSceneLoader.loadSceneFromFile(this.MediaDir + @"ExpendedorDeBebidas\ExpendedorDeBebidas-TgcScene.xml").Meshes[0];
            var meshMesaRedonda = this.TgcSceneLoader.loadSceneFromFile(this.MediaDir + @"MesaRedonda\MesaRedonda-TgcScene.xml").Meshes[0];
            var meshCama = this.TgcSceneLoader.loadSceneFromFile(this.MediaDir + @"Cama\Cama-TgcScene.xml").Meshes[0];
            var meshMesaDeLuz = this.TgcSceneLoader.loadSceneFromFile(this.MediaDir + @"MesaDeLuz\MesaDeLuz-TgcScene.xml").Meshes[0];

            // Puertas horizontales
            CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 5, 0, new float[] { 40, 200 });
            CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 25, 0, new float[] { 100, 200 });
            CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 45, 0, 200);
            CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 60, 0, 200);
            CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 95, 0, new float[] { 0, 60, 160, 220 });
            CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 105, 0, new float[] { 0, 60, 160, 220 });
            CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 120, 0, 40);
            CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 130, 0, 200);
            CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 150, 0, new float[] { 40, 200 });
            CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 170, 0, new float[] { 30, 40 });
            CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 175, 0, 70);
            CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 180, 0, 200);
            CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 190, 0, 20);
            CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 200, 0, 190);

            // Puertas verticales
            CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 20, 0, new float[] { 80, 100, 140 });
            CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 30, 0, new float[] { 10, 30 });
            CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 50, 0, 160);
            CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 60, 0, new float[] { 0, 20, 40, 80, 140, 205 });
            CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 120, 0, 170);
            CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 140, 0, new float[] { 50, 160 });
            CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 150, 0, new float[] { 0, 20, 70, 120, 200 });
            CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 170, 0, new float[] { 0, 40, 130 });
            CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 190, 0, new float[] { 40, 80, 200 });
            CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 200, 0, new float[] { 90, 140, 160 });

            // Entarda
            // Lockers
            CreateObjectsLine(meshLockerMetal, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 91.5f, 0, new float[] { 49, 51, 53, 55 });
            CreateObjectsLine(meshLockerMetal, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 118.5f, 0, new float[] { 49, 51, 53, 55 });

            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 105, this.PlaneSize, new float[] { 10, 30, 50 });

            // Sillones
            CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 92, 0, new float[] { 5, 15, 44 });

            // Mesas
            CreateObjectsLine(meshMesa, this.Oeste, this.Vector3Factory.CreateVector3(0.080f, 0.090f, 0.050f), 118.5f, 0, 44);

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Oeste, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 118.5f, 0, 15);

            // Expendedor
            CreateObjectsLine(meshExpendedor, this.Oeste, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 118.5f, 0, 10);

            // 1
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 10, 20 }, this.PlaneSize, 10);

            // Sillones
            CreateObjectsLine(meshSillon, this.Norte, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 20, 0, new float[] { 2, 18 });

            // Mesas
            CreateObjectsLine(meshMesa, this.Oeste, this.Vector3Factory.CreateVector3(0.080f, 0.090f, 0.050f), 10, 0, 5);

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Norte, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 5, 0, 18);

            // 2
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 40, 50 }, this.PlaneSize, 10);

            // Sillones
            CreateObjectsLine(meshSillon, this.Norte, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), new float[] { 35, 55 }, 0, 18);
            CreateObjectsLine(meshSillon, this.Sur, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), new float[] { 35, 55 }, 0, 2);

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Norte, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 45, 0, 18);

            // Expendedor
            CreateObjectsLine(meshExpendedor, this.Sur, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 45, 0, 2);

            // 3
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 70, 85 }, this.PlaneSize, 5);
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 85, this.PlaneSize, 35);

            // 4
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 125, 145 }, this.PlaneSize, new float[] { 5, 25 });

            // 5
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 155, this.PlaneSize, new float[] { 5, 25 });

            // Sillones
            CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 158, 0, 15);

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Oeste, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 158.5f, 0, 25);

            // Expendedor
            CreateObjectsLine(meshExpendedor, this.Oeste, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 158.5f, 0, 5);

            // 6
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 165, this.PlaneSize, new float[] { 5, 25 });

            // 7
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 180, this.PlaneSize, new float[] { 5, 25 });

            // Mesa redonda
            CreateObjectsLine(meshMesaRedonda, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 180, 0, 15);

            // Sillones
            CreateObjectsLine(meshSillon, this.Sur, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), new float[] { 175, 185 }, 0, 2);

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Este, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 171.5f, 0, 27);
            CreateObjectsLine(meshDispenserAgua, this.Oeste, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 188.5f, 0, 27);

            // 8
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 200, this.PlaneSize, 10);

            // Sillones
            CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 192, 0, 15);
            CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 208, 0, 15);

            // Mesas
            CreateObjectsLine(meshMesa, this.Norte, this.Vector3Factory.CreateVector3(0.080f, 0.090f, 0.050f), 200, 0, 5);

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Norte, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 203, 0, 18.5f);

            // 9
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 10, 20 }, this.PlaneSize, 30);

            // Sillones
            CreateObjectsLine(meshSillon, this.Sur, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), new float[] { 5, 25 }, 0, 22);

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 15, 0, 22);

            // Mesa redonda
            CreateObjectsLine(meshMesaRedonda, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 15, 0, 30);

            // 10
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 35, this.PlaneSize, 30);

            // Locker
            CreateObjectsLine(meshLockerMetal, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 38.5f, 0, new float[] { 22, 24, 26, 28, 30 });

            // 11
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 50, this.PlaneSize, new float[] { 50, 30 });
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 65, this.PlaneSize, 35);

            // Sillones
            CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 42, 0, new float[] { 30, 50 });

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Norte, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 50, 0, 58);

            // Expendedor
            CreateObjectsLine(meshExpendedor, this.Sur, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 50, 0, 21);

            // 12
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 70, this.PlaneSize, 20);

            // Sillones
            CreateObjectsLine(meshSillon, this.Sur, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 65, 0, 12);

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Norte, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 72, 0, 28);

            // Mesas
            CreateObjectsLine(meshMesa, this.Este, this.Vector3Factory.CreateVector3(0.080f, 0.090f, 0.050f), 75, 0, 15);

            // 13
            // Sillones
            CreateObjectsLine(meshSillon, this.Norte, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), new float[] { 195, 205 }, 0, 38);

            // Expendedor
            CreateObjectsLine(meshExpendedor, this.Sur, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 205, 0, 22);

            // 14
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 10, this.PlaneSize, new float[] { 50, 70, 90, 110, 130, 150, 170, 190 });
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 30, this.PlaneSize, 60);

            // Sillones
            CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 2, 0, new float[] { 85, 105, 125, 145, 165, 185 });
            CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 25, 0, new float[] { 45, 55 });
            CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 35, 0, new float[] { 45, 55 });
            CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 17.5f, 0, new float[] { 160, 120 });

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Este, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 2, 0, new float[] { 95, 135, 175 });

            // Locker
            CreateObjectsLine(meshLockerMetal, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 48.5f, 0, new float[] { 61, 63, 65, 67, 69, 71, 73, 75, 77, 79 });

            // Expendedor
            CreateObjectsLine(meshExpendedor, this.Oeste, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 18.5f, 0, new float[] { 90, 130, 150 });

            // Mesa redonda
            CreateObjectsLine(meshMesaRedonda, this.Oeste, this.Vector3Factory.CreateVector3(0.2f, 0.1f, 0.1f), 30, 0, 50);

            // 15
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 70, 80 }, this.PlaneSize, 50);

            // Mesas
            CreateObjectsLine(meshMesa, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 83, 0, 50);

            // Sillones
            CreateObjectsLine(meshSillon, this.Norte, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 70, 0, 58);

            // Locker
            CreateObjectsLine(meshLockerMetal, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 75, 77, 79 }, 0, 58);

            // 16
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 130, this.PlaneSize, 50);

            // Locker
            CreateObjectsLine(meshLockerMetal, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 121.5f, 0, new float[] { 41, 43, 45, 47, 49, 51, 53, 55, 57, 59 });
            CreateObjectsLine(meshLockerMetal, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 130, 0, new float[] { 41, 43, 45, 47, 49, 51, 53, 55 });
            CreateObjectsLine(meshLockerMetal, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 138.5f, 0, new float[] { 41, 43, 45, 47, 49, });

            // 17
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 145, this.PlaneSize, 50);

            // 18
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 155, this.PlaneSize, 50);

            // Mesas
            CreateObjectsLine(meshMesa, this.Norte, this.Vector3Factory.CreateVector3(0.080f, 0.090f, 0.050f), 155, 0, 53);

            // Sillones
            CreateObjectsLine(meshSillon, this.Norte, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 155, 0, 56);

            // 19
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 165, this.PlaneSize, 50);

            // Locker
            CreateObjectsLine(meshLockerMetal, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 161.5f, 0, new float[] { 41, 43, 45, 47, 49, 51, 53, 55, 57, 59 });

            // 20
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 180, this.PlaneSize, 50);

            // Mesa redonda
            CreateObjectsLine(meshMesaRedonda, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 180, 0, 50);

            // Sillones
            CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 172, 0, 50);
            CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 187, 0, 50);
            CreateObjectsLine(meshSillon, this.Norte, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 180, 0, 57.5f);

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 185, 0, 41.5f);

            // 21
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 200, this.PlaneSize, 50);

            // Locker
            CreateObjectsLine(meshLockerMetal, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 208, 0, new float[] { 41, 43, 45, 47, 49, 51, 53, 55, 57, 59 });

            // Mesas
            CreateObjectsLine(meshMesa, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 200, 0, 50);

            // 22
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 30, this.PlaneSize, new float[] { 90, 110 });

            // Mesas
            CreateObjectsLine(meshMesa, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 30, 0, new float[] { 90, 110 });

            // Sillones
            CreateObjectsLine(meshLockerMetal, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 38, 0, 90);

            // 23
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 45, this.PlaneSize, new float[] { 90, 110, 130, 150, 170, 190 });

            // 24
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 75, this.PlaneSize, new float[] { 80, 90, 120, 140 });

            // Mesa redonda
            CreateObjectsLine(meshMesaRedonda, this.Oeste, this.Vector3Factory.CreateVector3(0.2f, 0.1f, 0.1f), 75, 0, 85);

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 65, 0, 71.5f);

            // Expendedor
            CreateObjectsLine(meshExpendedor, this.Sur, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 85, 0, 71.5f);

            // Sillones
            CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 80, 0, 85);
            CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 70, 0, 85);

            // Locker
            CreateObjectsLine(meshLockerMetal, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 70, 72, 74, 76, 78, 80, 82, 84 }, 0, 98.5f);

            // 25
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 135, this.PlaneSize, new float[] { 80, 90, 120, 140 });

            // Expendedor
            CreateObjectsLine(meshExpendedor, this.Este, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 121.5f, 0, new float[] { 75, 85, 95 });

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Oeste, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 148.5f, 0, new float[] { 90, 95 });

            // Mesas
            CreateObjectsLine(meshMesa, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 135, 0, new float[] { 76, 93 });

            // Sillones
            CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 130, 0, new float[] { 93, 76 });
            CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 140, 0, new float[] { 93, 76 });

            // 26
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 165, this.PlaneSize, new float[] { 70, 90, 110, 130, 150, 170, 190 });

            // Sillones
            CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 172, 0, new float[] { 95, 105, 115, 125 });

            // 27
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 180, this.PlaneSize, new float[] { 80, 110, 135, 145 });
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 195, this.PlaneSize, 115);
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 200, this.PlaneSize, 80);
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 205.5f, this.PlaneSize, 100);

            // Sillones
            CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 208, 0, 80);
            CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 195, 0, 80);

            // Mesas
            CreateObjectsLine(meshMesa, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 202, 0, 80);

            // 28
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 30, this.PlaneSize, new float[] { 135, 165 });

            // Mesa redonda
            CreateObjectsLine(meshMesaRedonda, this.Oeste, this.Vector3Factory.CreateVector3(0.2f, 0.1f, 0.1f), 30, 0, new float[] { 135, 165 });

            // Sillones
            CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 38, 0, 110);
            CreateObjectsLine(meshSillon, this.Norte, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 30, 0, new float[] { 145, 175 });
            CreateObjectsLine(meshSillon, this.Sur, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 30, 0, new float[] { 124, 155 });

            // 29
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 60, 80 }, this.PlaneSize, 170);

            // Camas
            CreateObjectsLine(meshCama, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 60, 70, 80 }, 0, 175.5f);

            // MesaDeLuz
            CreateObjectsLine(meshMesaDeLuz, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 55, 65, 75, 85 }, 0, 178.5f);

            // Locker
            CreateObjectsLine(meshLockerMetal, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 88.5f, 0, new float[] { 163, 165, 167, 169 });

            // 30
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 145, this.PlaneSize, 170);
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 130, this.PlaneSize, 165);

            // 31
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 185, this.PlaneSize, 170);
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 205, this.PlaneSize, 155);

            // 32
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 10, this.PlaneSize, 210);

            // Camas
            CreateObjectsLine(meshCama, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 10, 0, 215.5f);

            // MesaDeLuz
            CreateObjectsLine(meshMesaDeLuz, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 5, 15 }, 0, 218.5f);

            // Locker
            CreateObjectsLine(meshLockerMetal, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 19, 0, 201.5f);

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 16, 0, 201.5f);

            // 33
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 30, this.PlaneSize, 210);

            // Camas
            CreateObjectsLine(meshCama, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 30, 0, 215.5f);

            // MesaDeLuz
            CreateObjectsLine(meshMesaDeLuz, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 25, 35 }, 0, 218.5f);

            // Locker
            CreateObjectsLine(meshLockerMetal, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 39, 0, 201.5f);

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 36, 0, 201.5f);

            // 34
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 50, this.PlaneSize, 210);

            // Camas
            CreateObjectsLine(meshCama, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 50, 0, 215.5f);

            // MesaDeLuz
            CreateObjectsLine(meshMesaDeLuz, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 45, 55 }, 0, 218.5f);

            // Locker
            CreateObjectsLine(meshLockerMetal, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 59, 0, 201.5f);

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 56, 0, 201.5f);

            // 35
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 70, 80 }, this.PlaneSize, 210);

            // Camas
            CreateObjectsLine(meshCama, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 70, 80 }, 0, 215.5f);

            // MesaDeLuz
            CreateObjectsLine(meshMesaDeLuz, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 65, 75, 85 }, 0, 218.5f);

            // Locker
            CreateObjectsLine(meshLockerMetal, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 80, 89 }, 0, 201.5f);

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 87, 0, 201.5f);

            // 36
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 130, 140 }, this.PlaneSize, 210);

            // Camas
            CreateObjectsLine(meshCama, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 130, 140 }, 0, 215.5f);

            // MesaDeLuz
            CreateObjectsLine(meshMesaDeLuz, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 125, 135, 145 }, 0, 218.5f);

            // Locker
            CreateObjectsLine(meshLockerMetal, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 140, 149 }, 0, 201.5f);

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 145, 0, 201.5f);

            // 37
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 160, 180 }, this.PlaneSize, 210);

            // Camas
            CreateObjectsLine(meshCama, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 160, 180 }, 0, 215.5f);

            // MesaDeLuz
            CreateObjectsLine(meshMesaDeLuz, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 155, 165, 175, 185 }, 0, 218.5f);

            // Locker
            CreateObjectsLine(meshLockerMetal, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 170, 189 }, 0, 201.5f);

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 175, 0, 201.5f);

            // 38
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 200, this.PlaneSize, 210);

            return this.Objects;
        }

        /// <summary>
        /// Crea una linea de objetos que componen el escenario
        /// </summary>
        /// <param name="mesh">Objeto con el cual crear la linea</param>
        /// <param name="orientation">Orientacion del objeto</param>
        /// <param name="xCoordinate">Indica la ubicacion en la coordenada X de la linea de objetos</param>
        /// <param name="yCoordinate">Indica la ubicacion en la coordenada Y de la linea de objetos</param>
        /// <param name="zCoordinate">Indica la ubicacion en la coordenada Z de la linea de objetos</param>
        /// <param name="scale">Tamaño del objeto</param>
        private void CreateObjectsLine(TgcMesh mesh, string orientation, Vector3 scale, float xCoordinate, float yCoordinate, float zCoordinate) => CreateObjectsLine(mesh, orientation, scale, new float[] { xCoordinate }, yCoordinate, new float[] { zCoordinate });

        /// <summary>
        /// Crea una linea de objetos que componen el escenario
        /// </summary>
        /// <param name="mesh">Objeto con el cual crear la linea</param>
        /// <param name="orientation">Orientacion del objeto</param>
        /// <param name="xCoordinate">Indica la ubicacion en la coordenada X de la linea de objetos</param>
        /// <param name="yCoordinate">Indica la ubicacion en la coordenada Y de la linea de objetos</param>
        /// <param name="zCoordinates">Indica la ubicacion en la coordenada Z de la linea de objetos</param>
        /// <param name="scale">Tamaño del objeto</param>
        private void CreateObjectsLine(TgcMesh mesh, string orientation, Vector3 scale, float xCoordinate, float yCoordinate, float[] zCoordinates) => CreateObjectsLine(mesh, orientation, scale, new float[] { xCoordinate }, yCoordinate, zCoordinates);

        /// <summary>
        /// Crea una linea de objetos que componen el escenario
        /// </summary>
        /// <param name="mesh">Objeto con el cual crear la linea</param>
        /// <param name="orientation">Orientacion del objeto</param>
        /// <param name="xCoordinates">Indica la ubicacion en la coordenada X de la linea de objetos</param>
        /// <param name="yCoordinate">Indica la ubicacion en la coordenada Y de la linea de objetos</param>
        /// <param name="zCoordinate">Indica la ubicacion en la coordenada Z de la linea de objetos</param>
        /// <param name="scale">Tamaño del objeto</param>
        private void CreateObjectsLine(TgcMesh mesh, string orientation, Vector3 scale, float[] xCoordinates, float yCoordinate, float zCoordinate) => CreateObjectsLine(mesh, orientation, scale, xCoordinates, yCoordinate, new float[] { zCoordinate });

        /// <summary>
        /// Crea una linea de objetos que componen el escenario
        /// </summary>
        /// <param name="mesh">Objeto con el cual crear la linea</param>
        /// <param name="orientation">Orientacion del objeto</param>
        /// <param name="xCoordinates">Indica la ubicacion en la coordenada X de la linea de objetos</param>
        /// <param name="yCoordinate">Indica la ubicacion en la coordenada Y de la linea de objetos</param>
        /// <param name="zCoordinates">Indica la ubicacion en la coordenada Z de la linea de objetos</param>
        /// <param name="scale">Tamaño del objeto</param>
        private void CreateObjectsLine(TgcMesh mesh, string orientation, Vector3 scale, float[] xCoordinates, float yCoordinate, float[] zCoordinates)
        {
            TgcMesh meshInstance;
            foreach (var xCoordinate in xCoordinates)
            {
                foreach (var zCoordinate in zCoordinates)
                {
                    meshInstance = mesh.createMeshInstance(
                        this.Objects.Count + "_" + mesh.Name,
                        CalculateTranslation(mesh, orientation, xCoordinate, yCoordinate, zCoordinate),
                        CalculateRotation(orientation),
                        scale);
                    meshInstance.AutoTransformEnable = true;
                    this.Objects.Add(meshInstance);
                }
            }
        }

        /// <summary>
        /// Crea el techo del escenario
        /// </summary>
        /// <returns>Lista con los objetos que componen el techo</returns>
        private List<IRenderObject> CreateRoof() => CreateHorizontalLayer(this.PlaneSize, this.TgcTextureFactory.CreateTexture(D3DDevice.Instance.Device, this.MediaDir + @"\roof.bmp"));

        /// <summary>
        /// Crea las paredes del escenario
        /// </summary>
        /// <returns>Lista con los objetos que componen la pared del escenario</returns>
        private List<IRenderObject> CreateWalls()
        {
            this.Walls = new List<IRenderObject>();
            this.WallTexture = this.TgcTextureFactory.CreateTexture(D3DDevice.Instance.Device, this.MediaDir + @"\wall.bmp");

            // Paredes verticales
            CreateWallsLine(this.Este, 0, new float[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 });
            CreateWallsLine(this.Este, 2, new float[] { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 20, 21 });
            CreateWallsLine(this.Este, 3, new float[] { 0, 1, 2, 3 });
            CreateWallsLine(this.Este, 4, new float[] { 2, 3, 4, 5, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 20, 21 });
            CreateWallsLine(this.Este, 5, new float[] { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 });
            CreateWallsLine(this.Este, 6, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 20, 21 });
            CreateWallsLine(this.Este, 8, new float[] { 1, 2 });
            CreateWallsLine(this.Este, 9, new float[] { 0, 1, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            CreateWallsLine(this.Este, 12, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            CreateWallsLine(this.Este, 14, new float[] { 4, 5, 16 });
            CreateWallsLine(this.Este, 15, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            CreateWallsLine(this.Este, 16, new float[] { 0, 1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            CreateWallsLine(this.Este, 17, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 10, 11, 12, 13, 15, 16, 17, 18, 21 });
            CreateWallsLine(this.Este, 19, new float[] { 0, 1, 2, 4, 5, 7, 8, 13, 20, 21 });
            CreateWallsLine(this.Este, 20, new float[] { 9, 10, 14, 15, 16, 19 });
            CreateWallsLine(this.Este, 21, new float[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 });

            // Paredes horizontales
            CreateWallsLine(this.Norte, 0, new float[] { 0, 2, 4, 20, 22 });
            CreateWallsLine(this.Norte, 1, new float[] { 0, 2, 4, 20, 22 });
            CreateWallsLine(this.Norte, 2, new float[] { 0, 2, 4, 8, 10, 12, 18, 20, 22 });
            CreateWallsLine(this.Norte, 3, new float[] { 0, 2, 4, 8, 10, 12, 18, 20, 22 });
            CreateWallsLine(this.Norte, 4, new float[] { 0, 2, 6, 8, 20, 22 });
            CreateWallsLine(this.Norte, 5, new float[] { 0, 2, 6, 16, 18, 20, 22 });
            CreateWallsLine(this.Norte, 6, new float[] { 0, 1, 3, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            CreateWallsLine(this.Norte, 7, new float[] { 0, 1, 3, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            CreateWallsLine(this.Norte, 8, new float[] { 0, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            CreateWallsLine(this.Norte, 9, new float[] { 0, 6, 16, 22 });
            CreateWallsLine(this.Norte, 10, new float[] { 0, 6, 16, 22 });
            CreateWallsLine(this.Norte, 11, new float[] { 0, 6, 16, 22 });
            CreateWallsLine(this.Norte, 12, new float[] { 0, 1.5f, 3, 4, 6, 7, 10, 11, 15, 16, 17, 18, 20, 22 });
            CreateWallsLine(this.Norte, 13, new float[] { 0, 1.5f, 3, 4, 6, 7, 10, 11, 15, 16, 17, 18, 20, 22 });
            CreateWallsLine(this.Norte, 14, new float[] { 0, 1.5f, 3, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            CreateWallsLine(this.Norte, 15, new float[] { 0, 4, 6, 16, 20, 22 });
            CreateWallsLine(this.Norte, 16, new float[] { 0, 3, 4, 6, 20, 22 });
            CreateWallsLine(this.Norte, 17, new float[] { 0, 3, 4, 6, 7, 13, 14, 15, 19, 20, 22 });
            CreateWallsLine(this.Norte, 18, new float[] { 0, 3, 4, 6, 7, 13, 14, 15, 19, 20, 22 });
            CreateWallsLine(this.Norte, 19, new float[] { 0, 2, 4, 6, 7, 9, 15, 20, 22 });
            CreateWallsLine(this.Norte, 20, new float[] { 0, 2, 4, 6, 7, 9, 11, 14, 17, 19, 22 });

            return this.Walls;
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
                var wallElement = this.TgcPlaneFactory.CreateTgcPlane();
                wallElement.setTexture(this.WallTexture);
                wallElement.Origin = this.Vector3Factory.CreateVector3(this.PlaneSize * xCoordinate, 0, this.PlaneSize * zCoordinate);
                wallElement.Size = this.Vector3Factory.CreateVector3(this.PlaneSize, this.PlaneSize, this.PlaneSize);
                wallElement.Orientation = orientation == this.Norte ? TgcPlane.Orientations.XYplane : TgcPlane.Orientations.YZplane;
                wallElement.AutoAdjustUv = false;
                wallElement.UTile = 1;
                wallElement.VTile = 1;
                this.Walls.Add(wallElement);
            }
        }
    }
}