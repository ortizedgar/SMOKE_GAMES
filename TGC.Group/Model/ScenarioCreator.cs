namespace TGC.Group.Model
{
    using System;
    using System.Collections.Generic;
    using Autofac;
    using BulletSharp;
    using Microsoft.DirectX;
    using Microsoft.DirectX.Direct3D;
    using TGC.Core.Direct3D;
    using TGC.Core.Geometry;
    using TGC.Core.SceneLoader;
    using TGC.Core.Textures;
    using TGC.Group.Interfaces;

    public class ScenarioCreator : IScenarioCreator
    {
        /// <summary>
        /// Mundo dinamico
        /// </summary>
        private DiscreteDynamicsWorld DynamicsWorld { get; set; }

        /// <summary>
        /// Representa la direccion Este
        /// </summary>
        private string Este { get; } = "E";

        /// <summary>
        /// Lista de objetos que representan el piso
        /// </summary>
        private List<IScenarioElement> Floor { get; set; }

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
        private List<IScenarioElement> Objects { get; set; }

        /// <summary>
        /// Representa la direccion Oeste
        /// </summary>
        private string Oeste { get; } = "O";

        /// <summary>
        /// Tamaño del plano de las paredes, techo y piso
        /// </summary>
        private float PlaneSize { get; } = 10;

        /// <summary>
        /// Lista de objetos que representan el techo
        /// </summary>
        private List<IScenarioElement> Roof { get; set; }

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
        /// Fabrica de <see cref="TgcTexture"/>
        /// </summary>
        private ITgcTextureFactory TgcTextureFactory { get; set; }

        /// <summary>
        /// Fabrica de <see cref="Vector3"/>
        /// </summary>
        private IVector3Factory Vector3Factory { get; set; }

        /// <summary>
        /// Tamaño del plano de las paredes, techo y piso
        /// </summary>
        private Vector3 WallSize { get; set; }

        /// <summary>
        /// Lista de objetos que representan las paredes
        /// </summary>
        private List<IScenarioElement> Walls { get; set; }

        /// <summary>
        /// Crea una caja de prueba
        /// </summary>
        /// <param name="x">Coordenada X inicial</param>
        /// <param name="y">Coordenada Y inicial</param>
        /// <param name="z">Coordenada Z inicial</param>
        /// <param name="mass">Masa de la caja</param>
        public void CreateDebugBox(float x, float y, float z, float mass)
        {
            var boxShape = new BoxShape(2, 2, 2);
            var boxTransform = BulletSharp.Math.Matrix.RotationYawPitchRoll(MathUtil.SIMD_HALF_PI, MathUtil.SIMD_QUARTER_PI, MathUtil.SIMD_2_PI);
            boxTransform.Origin = new BulletSharp.Math.Vector3(x, y, z);
            var boxBody = new RigidBody(new RigidBodyConstructionInfo(mass, new DefaultMotionState(boxTransform), boxShape, boxShape.CalculateLocalInertia(mass)));
            this.DynamicsWorld.AddRigidBody(boxBody);
            this.Objects.Add(
                new ScenarioElement
                {
                    RoomsId = new List<int> { 1 },
                    RenderObject = TgcBox.fromSize(new Vector3(4, 4, 4), TgcTexture.createTexture(D3DDevice.Instance.Device, this.MediaDir + "\\table.jpg")),
                    Body = boxBody
                });
        }

        /// <summary>
        /// Crea la lista con todos los objetos que componen el escenario
        /// </summary>
        /// <param name="container">Container IOC</param>
        /// <param name="mediaDir">Directorio de medios</param>
        /// <param name="dynamicsWorld">Mundo fisico</param>
        /// <returns>Una lista con todos los elementos del escenario</returns>
        public void CreateScenario(IContainer container, string mediaDir, DiscreteDynamicsWorld dynamicsWorld)
        {
            this.TgcSceneLoader = container.Resolve<TgcSceneLoader>();
            this.TgcPlaneFactory = container.Resolve<ITgcPlaneFactory>();
            this.Vector3Factory = container.Resolve<IVector3Factory>();
            this.TgcTextureFactory = container.Resolve<ITgcTextureFactory>();
            this.WallSize = this.Vector3Factory.CreateVector3(0.5f, this.PlaneSize, this.PlaneSize);
            this.MediaDir = mediaDir;
            this.DynamicsWorld = dynamicsWorld;
            this.PortalUnionList = new List<IPortal>();
            this.ScenarioLayers = new List<IScenarioLayer>();

            CreateFloor();
            this.ScenarioLayers.Add(
                new ScenarioLayer
                {
                    LayerName = nameof(this.Floor),
                    ScenarioElements = this.Floor
                });

            CreateRoof();
            this.ScenarioLayers.Add(
                new ScenarioLayer
                {
                    LayerName = nameof(this.Roof),
                    ScenarioElements = this.Roof
                });

            CreateWalls();
            this.ScenarioLayers.Add(
                new ScenarioLayer
                {
                    LayerName = nameof(this.Walls),
                    ScenarioElements = this.Walls
                });

            CreateObjects();
            this.ScenarioLayers.Add(
                new ScenarioLayer
                {
                    LayerName = nameof(this.Objects),
                    ScenarioElements = this.Objects
                });
        }

        /// <summary>
        /// Calcula la rotacion del objeto sobre el eje Y en base a la orientacion indicada
        /// </summary>
        /// <param name="orientation">Orientacion deseada</param>
        /// <returns><see cref="Vector3"/> indicando la rotacion del objeto</returns>
        private Vector3 CalculateRotation(string orientation)
        {
            if (orientation == this.Oeste)
            {
                return this.Vector3Factory.CreateVector3(0, Geometry.DegreeToRadian(90), 0);
            }
            if (orientation == this.Sur)
            {
                return this.Vector3Factory.CreateVector3(0, Geometry.DegreeToRadian(180), 0);
            }
            if (orientation == this.Este)
            {
                return this.Vector3Factory.CreateVector3(0, Geometry.DegreeToRadian(270), 0);
            }
            if (orientation == this.Horizontal)
            {
                return this.Vector3Factory.CreateVector3(0, 0, Geometry.DegreeToRadian(90));
            }

            return this.Vector3Factory.CreateVector3(0, Geometry.DegreeToRadian(0), 0);
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
                var doorDisplacement = (this.PlaneSize / 2) - 1;
                return orientation == this.Este || orientation == this.Oeste ?
                    this.Vector3Factory.CreateVector3(xCoordinate - 1, yCoordinate, zCoordinate + doorDisplacement + 1)
                    : this.Vector3Factory.CreateVector3(xCoordinate + doorDisplacement, yCoordinate, zCoordinate);
            }

            return this.Vector3Factory.CreateVector3(xCoordinate, yCoordinate, zCoordinate);
        }

        /// <summary>
        /// Crea el piso
        /// </summary>
        private void CreateFloor()
        {
            // El piso es un plano estatico, masa 0.
#pragma warning disable CC0022 // Should dispose object
            this.DynamicsWorld.AddRigidBody(new RigidBody(new RigidBodyConstructionInfo(0, new DefaultMotionState(), new StaticPlaneShape(new BulletSharp.Math.Vector3(0, 1, 0), 0f))));
#pragma warning restore CC0022 // Should dispose object

            this.Floor = new List<IScenarioElement>();
            CreateHorizontalLayer(this.Floor, 0, TgcTexture.createTexture(D3DDevice.Instance.Device, this.MediaDir + @"Piso\Textures\techua.jpg"), 0, 0, 4, 4, new List<int> { 1 });
        }

        /// <summary>
        /// Crea una capa horizontal de planos que ocupan todo el escenario
        /// </summary>
        /// <param name="layer">Capa que se quiere crear</param>
        /// <param name="yCoordinate">Indica la altura sobre la cual debe crearse la capa</param>
        /// <param name="texture">Textura que se utilizara en los elementos de la capa</param>
        private void CreateHorizontalLayer(List<IScenarioElement> layer, float yCoordinate, TgcTexture texture, int xOrigin, int yOrigin, int xEnd, int yEnd, List<int> roomId)
        {
            TgcBox tgcBox;
            Vector3 translation;
            var rotation = CalculateRotation(this.Horizontal);
            for (var i = xOrigin; i < xEnd; i++)
            {
                for (var j = yOrigin; j < yEnd; j++)
                {
                    tgcBox = TgcBox.fromSize(this.Vector3Factory.CreateVector3(0, this.WallSize.Y, this.WallSize.Z), texture);
                    tgcBox.Rotation = rotation;
                    translation = this.Vector3Factory.CreateVector3(this.PlaneSize * i + 5f, yCoordinate, this.PlaneSize * j + 5f);
                    tgcBox.move(translation.X, translation.Y, translation.Z);
                    tgcBox.AutoTransformEnable = true;
                    layer.Add(
                        new ScenarioElement
                        {
                            RoomsId = roomId,
                            RenderObject = tgcBox,
                            Body = null
                        });
                }
            }
        }

        public List<IPortal> PortalUnionList { get; set; }

        public List<IScenarioLayer> ScenarioLayers { get; set; }

        private void CreatePortal(TgcMesh doorMesh, TgcTexture doorWallsTexture, string orientation, Vector3 scale, float xCoordinate, float yCoordinate, float zCoordinate, int roomA, int roomB)
        {
            var rotation = CalculateRotation(orientation);
            var door = doorMesh.createMeshInstance(
                this.Objects.Count + "_" + doorMesh.Name,
                CalculateTranslation(doorMesh, orientation, xCoordinate, yCoordinate, zCoordinate),
                rotation,
                scale);

            var size = this.Vector3Factory.CreateVector3(
                this.WallSize.X,
                this.WallSize.Y + 1,
                this.WallSize.Z);
            var wallRotation = this.Vector3Factory.CreateVector3(
                rotation.X,
                rotation.Y + Geometry.DegreeToRadian(90),
                rotation.Z);
            var doorLenght = door.BoundingBox.calculateSize().X;
            var halfDoorLenght = doorLenght / 2;
            var doorWidth = door.BoundingBox.calculateSize().Z;
            var halfDoorWidth = doorWidth / 2;
            var sideWallLenght = (this.PlaneSize - doorLenght) / 2;
            var sideWallHalfLenght = sideWallLenght / 2;
            var sideWallEastDisplacementX = halfDoorLenght - this.WallSize.X / 2;

            var leftWall = CreateLeftWall(doorWallsTexture, orientation, door, size, wallRotation, doorLenght, halfDoorLenght, doorWidth, halfDoorWidth, sideWallLenght, sideWallHalfLenght, sideWallEastDisplacementX);
            var rightWall = CreateRightWall(doorWallsTexture, orientation, door, size, wallRotation, halfDoorLenght, doorWidth, halfDoorWidth, sideWallLenght, sideWallHalfLenght, sideWallEastDisplacementX);
            var upWall = CreateUpWall(doorWallsTexture, door, size, wallRotation, doorLenght, halfDoorLenght, halfDoorWidth);

            var doorRigidBody = this.CreateRigidBody(door, rotation, false);
            var leftWallRigidBody = this.CreateRigidBody(leftWall, wallRotation, true);
            var rightWallRigidBody = this.CreateRigidBody(rightWall, wallRotation, true);

            if (orientation == this.Este)
            {
                var constraint = new HingeConstraint(
                        doorRigidBody,
                        leftWallRigidBody,
                        new BulletSharp.Math.Vector3(door.BoundingBox.calculateAxisRadius().X, -door.BoundingBox.calculateAxisRadius().Y, door.BoundingBox.calculateAxisRadius().Z),
                        new BulletSharp.Math.Vector3(-leftWall.BoundingBox.calculateAxisRadius().X, -leftWall.BoundingBox.calculateAxisRadius().Y, -leftWall.BoundingBox.calculateAxisRadius().Z),
                        new BulletSharp.Math.Vector3(0, 1, 0),
                        new BulletSharp.Math.Vector3(0, 1, 0),
                        true);
                constraint.SetLimit(Geometry.DegreeToRadian(0), Geometry.DegreeToRadian(90), 0.1f, 0.5f, 1f);
                this.DynamicsWorld.AddConstraint(constraint);
            }

            if (orientation == this.Oeste)
            {
                var constraint = new HingeConstraint(
                        doorRigidBody,
                        rightWallRigidBody,
                        new BulletSharp.Math.Vector3(door.BoundingBox.calculateAxisRadius().X, -door.BoundingBox.calculateAxisRadius().Y, door.BoundingBox.calculateAxisRadius().Z),
                        new BulletSharp.Math.Vector3(-rightWall.BoundingBox.calculateAxisRadius().X, -rightWall.BoundingBox.calculateAxisRadius().Y, -rightWall.BoundingBox.calculateAxisRadius().Z),
                        new BulletSharp.Math.Vector3(0, 1, 0),
                        new BulletSharp.Math.Vector3(0, 1, 0),
                        true);
                constraint.SetLimit(Geometry.DegreeToRadian(0), Geometry.DegreeToRadian(90), 0.1f, 0.5f, 1f);
                this.DynamicsWorld.AddConstraint(constraint);
            }

            if (orientation == this.Norte)
            {
                var constraint = new HingeConstraint(
                        doorRigidBody,
                        rightWallRigidBody,
                        new BulletSharp.Math.Vector3(door.BoundingBox.calculateAxisRadius().X, -door.BoundingBox.calculateAxisRadius().Y, door.BoundingBox.calculateAxisRadius().Z),
                        new BulletSharp.Math.Vector3(-rightWall.BoundingBox.calculateAxisRadius().X, -rightWall.BoundingBox.calculateAxisRadius().Y, -rightWall.BoundingBox.calculateAxisRadius().Z),
                        new BulletSharp.Math.Vector3(0, 1, 0),
                        new BulletSharp.Math.Vector3(0, 1, 0),
                        true);
                constraint.SetLimit(Geometry.DegreeToRadian(0), Geometry.DegreeToRadian(90), 0.1f, 0.5f, 1f);
                this.DynamicsWorld.AddConstraint(constraint);
            }

            if (orientation == this.Sur)
            {
                var constraint = new HingeConstraint(
                        doorRigidBody,
                        leftWallRigidBody,
                        new BulletSharp.Math.Vector3(door.BoundingBox.calculateAxisRadius().X, -door.BoundingBox.calculateAxisRadius().Y, door.BoundingBox.calculateAxisRadius().Z),
                        new BulletSharp.Math.Vector3(-leftWall.BoundingBox.calculateAxisRadius().X, -leftWall.BoundingBox.calculateAxisRadius().Y, -leftWall.BoundingBox.calculateAxisRadius().Z),
                        new BulletSharp.Math.Vector3(0, 1, 0),
                        new BulletSharp.Math.Vector3(0, 1, 0),
                        true);
                constraint.SetLimit(Geometry.DegreeToRadian(0), Geometry.DegreeToRadian(90), 0.1f, 0.5f, 1f);
                this.DynamicsWorld.AddConstraint(constraint);
            }

            var doorWallsList = new List<IScenarioElement>
            {
                new ScenarioElement
                {
                    RoomsId = new List<int>(),
                    RenderObject = door,
                    Body = doorRigidBody
                },
                new ScenarioElement
                {
                    RoomsId = new List<int>(),
                    RenderObject = leftWall,
                    Body = leftWallRigidBody
                },
                new ScenarioElement
                {
                    RoomsId = new List<int>(),
                    RenderObject = rightWall,
                    Body = rightWallRigidBody
                },
                new ScenarioElement
                {
                    RoomsId = new List<int>(),
                    RenderObject = upWall,
                }
            };

            this.PortalUnionList.Add(new Portal(door, roomA, roomB, doorWallsList));
            this.Objects.AddRange(doorWallsList);
        }

        private TgcBox CreateRightWall(TgcTexture doorWallsTexture, string orientation, TgcMesh door, Vector3 size, Vector3 wallRotation, float halfDoorLenght, float doorWidth, float halfDoorWidth, float sideWallLenght, float sideWallHalfLenght, float sideWallEastDisplacementX)
        {
            var rightWall = TgcBox.fromSize(
                            this.Vector3Factory.CreateVector3(size.X, size.Y, sideWallLenght),
                            doorWallsTexture);
            rightWall.Rotation = wallRotation;
            var rightWallTranslate = orientation.Equals(this.Este, StringComparison.OrdinalIgnoreCase) || orientation.Equals(this.Oeste, StringComparison.OrdinalIgnoreCase) ? this.Vector3Factory.CreateVector3(
                door.Position.X + sideWallEastDisplacementX,
                door.Position.Y,
                door.Position.Z - halfDoorLenght + halfDoorWidth - rightWall.Size.Z)
                : this.Vector3Factory.CreateVector3(
                door.Position.X + sideWallLenght + doorWidth * 2.2f,
                door.Position.Y,
                door.Position.Z - sideWallHalfLenght + halfDoorWidth);
            rightWall.move(rightWallTranslate);
            return rightWall;
        }

        private TgcBox CreateUpWall(TgcTexture doorWallsTexture, TgcMesh door, Vector3 size, Vector3 wallRotation, float doorLenght, float halfDoorLenght, float halfDoorWidth)
        {
            var upWall = TgcBox.fromSize(
                            this.Vector3Factory.CreateVector3(size.X, size.Y - door.BoundingBox.calculateSize().Y, doorLenght),
                            doorWallsTexture);
            upWall.Rotation = wallRotation;
            upWall.move(
                door.Position.X + halfDoorLenght,
                size.Y - upWall.Size.Y / 2,
                door.Position.Z + halfDoorWidth);
            upWall.AutoTransformEnable = true;
            return upWall;
        }

        private TgcBox CreateLeftWall(TgcTexture doorWallsTexture, string orientation, TgcMesh door, Vector3 size, Vector3 wallRotation, float doorLenght, float halfDoorLenght, float doorWidth, float halfDoorWidth, float sideWallLenght, float sideWallHalfLenght, float sideWallEastDisplacementX)
        {
            var leftWall = TgcBox.fromSize(
                            this.Vector3Factory.CreateVector3(size.X, size.Y, sideWallLenght),
                            doorWallsTexture);
            leftWall.Rotation = wallRotation;
            var leftWallTranslate = orientation.Equals(this.Este, StringComparison.OrdinalIgnoreCase) || orientation.Equals(this.Oeste, StringComparison.OrdinalIgnoreCase) ? this.Vector3Factory.CreateVector3(
                door.Position.X + sideWallEastDisplacementX,
                door.Position.Y,
                door.Position.Z + halfDoorLenght + halfDoorWidth)
                : this.Vector3Factory.CreateVector3(
                door.Position.X - doorLenght * 2 + sideWallLenght + doorWidth * 0.8f,
                door.Position.Y,
                door.Position.Z - sideWallHalfLenght + halfDoorWidth);
            leftWall.move(leftWallTranslate);
            return leftWall;
        }

        /// <summary>
        /// Crea una lista de objetos que componen el escenario
        /// </summary>
        /// <returns></returns>
        private void CreateObjects()
        {
            this.Objects = new List<IScenarioElement>();

            var meshMesa = this.LoadMesh(@"Mesa\Mesa-TgcScene.xml");
            var meshLamparaTecho = this.LoadMesh(@"LamparaTecho\LamparaTecho-TgcScene.xml");
            var meshSillon = this.LoadMesh(@"Sillon\Sillon-TgcScene.xml");
            var meshLockerMetal = this.LoadMesh(@"LockerMetal\LockerMetal-TgcScene.xml");
            var meshPuerta = this.LoadMesh(@"Puerta\Puerta-TgcScene.xml");
            var meshDispenserAgua = this.LoadMesh(@"DispenserAgua\DispenserAgua-TgcScene.xml");
            var meshExpendedor = this.LoadMesh(@"ExpendedorDeBebidas\ExpendedorDeBebidas-TgcScene.xml");
            var meshMesaRedonda = this.LoadMesh(@"MesaRedonda\MesaRedonda-TgcScene.xml");
            var meshCama = this.LoadMesh(@"Cama\Cama-TgcScene.xml");
            var meshMesaDeLuz = this.LoadMesh(@"MesaDeLuz\MesaDeLuz-TgcScene.xml");
            var meshEsqueleto = this.LoadMesh(@"Esqueleto\Esqueleto-TgcScene.xml");
            var doorWallsTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, this.MediaDir + @"Pared\Textures\techua.jpg");

            this.CreateDebugBox(133, 100, 50, 10);

            // Puertas horizontales
            CreatePortal(meshPuerta, doorWallsTexture, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 20, 0, 20, 1, 2);
            CreatePortal(meshPuerta, doorWallsTexture, this.Sur, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 10, 0, 20, 1, 2);
            //CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 5, 0, new float[] { 40, 200 });
            //CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 25, 0, new float[] { 100, 200 });
            //CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 45, 0, 200);
            //CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 60, 0, 200);
            //CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 95, 0, new float[] { 0, 60, 160, 220 });
            //CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 105, 0, new float[] { 0, 60, 160, 220 });
            //CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 120, 0, 40);
            //CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 130, 0, 200);
            //CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 150, 0, new float[] { 40, 200 });
            //CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 170, 0, new float[] { 30, 40 });
            //CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 175, 0, 70);
            //CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 180, 0, 200);
            //CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 190, 0, 20);
            //CreateObjectsLine(meshPuerta, this.Norte, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 200, 0, 190);

            //// Puertas verticales
            CreatePortal(meshPuerta, doorWallsTexture, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 10, 0, 0, 1, 2);
            CreatePortal(meshPuerta, doorWallsTexture, this.Oeste, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 20, 0, 0, 1, 2);
            //CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 20, 0, new float[] { 80, 100, 140 });
            ////CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 30, 0, new float[] { 10, 30 });
            //CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 50, 0, 160);
            //CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 60, 0, new float[] { 0, 20, 40, 80, 140, 205 });
            //CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 120, 0, 170);
            //CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 140, 0, new float[] { 50, 160 });
            //CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 150, 0, new float[] { 0, 20, 70, 120, 200 });
            //CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 170, 0, new float[] { 0, 40, 130 });
            //CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 190, 0, new float[] { 40, 80, 200 });
            //CreateObjectsLine(meshPuerta, this.Este, this.Vector3Factory.CreateVector3(0.17f, 0.17f, 0.17f), 200, 0, new float[] { 90, 140, 160 });

            // Entarda
            // Lockers
            //CreateObjectsLine(meshLockerMetal, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 91.5f, 0, new float[] { 49, 51, 53, 55 });
            //CreateObjectsLine(meshLockerMetal, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 118.5f, 0, new float[] { 49, 51, 53, 55 });

            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 105, this.PlaneSize * 0.98f, new float[] { 10, 30, 50 });

            //// Sillones
            //CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 92, 0, new float[] { 5, 15, 44 });

            //// Mesas
            //CreateObjectsLine(meshMesa, this.Oeste, this.Vector3Factory.CreateVector3(0.080f, 0.090f, 0.050f), 118.5f, 0, 44);

            //// Dispenser
            //CreateObjectsLine(meshDispenserAgua, this.Oeste, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 118.5f, 0, 15);

            //// Expendedor
            //CreateObjectsLine(meshExpendedor, this.Oeste, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 118.5f, 0, 10);

            // 1
            //Esqueleto
            CreateObjectsLine(meshEsqueleto, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 2, 0, new float[] { 5 }, new List<int> { 1 });

            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 10, 20 }, this.PlaneSize * 0.98f, 10, new List<int> { 1 });

            // Sillones
            CreateObjectsLine(meshSillon, this.Norte, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 20, 0, 18, new List<int> { 1 });
            CreateObjectsLine(meshSillon, this.Sur, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 20, 0, 2, new List<int> { 1 });

            // Mesas
            CreateObjectsLine(meshMesa, this.Oeste, this.Vector3Factory.CreateVector3(0.080f, 0.090f, 0.050f), 10, 0, 5, new List<int> { 1 });

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Norte, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 5, 0, 18, new List<int> { 1 });

            // 2
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 40, 50 }, this.PlaneSize * 0.98f, 10, new List<int> { 2 });

            // Sillones
            CreateObjectsLine(meshSillon, this.Norte, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), new float[] { 35, 55 }, 0, 18, new List<int> { 2 });
            CreateObjectsLine(meshSillon, this.Sur, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), new float[] { 35, 55 }, 0, 2, new List<int> { 2 });

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Norte, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 45, 0, 18, new List<int> { 2 });

            // Expendedor
            CreateObjectsLine(meshExpendedor, this.Sur, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 45, 0, 2, new List<int> { 2 });

            // 3
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 70, 85 }, this.PlaneSize * 0.98f, 5, new List<int> { 3 });
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 85, this.PlaneSize * 0.98f, 35, new List<int> { 3 });

            // 4
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 125, 145 }, this.PlaneSize * 0.98f, new float[] { 5, 25 }, new List<int> { 4 });

            // 5
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 155, this.PlaneSize * 0.98f, new float[] { 5, 25 }, new List<int> { 5 });

            // Sillones
            CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 158, 0, 15, new List<int> { 5 });

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Oeste, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 158.5f, 0, 25, new List<int> { 5 });

            // Expendedor
            CreateObjectsLine(meshExpendedor, this.Oeste, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 158.5f, 0, 5, new List<int> { 5 });

            // 6
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 165, this.PlaneSize * 0.98f, new float[] { 5, 25 }, new List<int> { 6 });

            // 7
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 180, this.PlaneSize * 0.98f, new float[] { 5, 25 }, new List<int> { 7 });

            // Mesa redonda
            CreateObjectsLine(meshMesaRedonda, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 180, 0, 15, new List<int> { 7 });

            // Sillones
            CreateObjectsLine(meshSillon, this.Sur, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), new float[] { 175, 185 }, 0, 2, new List<int> { 7 });

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Este, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 171.5f, 0, 27, new List<int> { 7 });
            CreateObjectsLine(meshDispenserAgua, this.Oeste, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 188.5f, 0, 27, new List<int> { 7 });

            // 8
            // Lamparas
            CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 200, this.PlaneSize * 0.98f, 10, new List<int> { 4 });

            // Sillones
            CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 192, 0, 15, new List<int> { 4 });
            CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 208, 0, 15, new List<int> { 4 });

            // Mesas
            CreateObjectsLine(meshMesa, this.Norte, this.Vector3Factory.CreateVector3(0.080f, 0.090f, 0.050f), 200, 0, 5, new List<int> { 4 });

            // Dispenser
            CreateObjectsLine(meshDispenserAgua, this.Norte, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 203, 0, 18.5f, new List<int> { 4 });

            // 9
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 10, 20 }, this.PlaneSize * 0.98f, 30);

            //// Sillones
            //CreateObjectsLine(meshSillon, this.Sur, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), new float[] { 5, 25 }, 0, 22);

            //// Dispenser
            //CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 15, 0, 22);

            //// Mesa redonda
            //CreateObjectsLine(meshMesaRedonda, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 15, 0, 30);

            //// 10
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 35, this.PlaneSize * 0.98f, 30);

            //// Locker
            //CreateObjectsLine(meshLockerMetal, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 38.5f, 0, new float[] { 22, 24, 26, 28, 30 });

            //// 11
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 50, this.PlaneSize * 0.98f, new float[] { 50, 30 });
            //CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 65, this.PlaneSize * 0.98f, 35);

            //// Sillones
            //CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 42, 0, new float[] { 30, 50 });

            //// Dispenser
            //CreateObjectsLine(meshDispenserAgua, this.Norte, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 50, 0, 58);

            //// Expendedor
            //CreateObjectsLine(meshExpendedor, this.Sur, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 50, 0, 21);

            //// 12
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 70, this.PlaneSize * 0.98f, 20);

            //// Sillones
            //CreateObjectsLine(meshSillon, this.Sur, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 65, 0, 12);

            //// Dispenser
            //CreateObjectsLine(meshDispenserAgua, this.Norte, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 72, 0, 28);

            //// Mesas
            //CreateObjectsLine(meshMesa, this.Este, this.Vector3Factory.CreateVector3(0.080f, 0.090f, 0.050f), 75, 0, 15);

            //// 13
            //// Sillones
            //CreateObjectsLine(meshSillon, this.Norte, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), new float[] { 195, 205 }, 0, 38);

            //// Expendedor
            //CreateObjectsLine(meshExpendedor, this.Sur, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 205, 0, 22);

            //// 14
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 10, this.PlaneSize * 0.98f, new float[] { 50, 70, 90, 110, 130, 150, 170, 190 });
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 30, this.PlaneSize * 0.98f, 60);

            //// Sillones
            //CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 2, 0, new float[] { 85, 105, 125, 145, 165, 185 });
            //CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 25, 0, new float[] { 45, 55 });
            //CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 35, 0, new float[] { 45, 55 });
            //CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 17.5f, 0, new float[] { 160, 120 });

            //// Dispenser
            //CreateObjectsLine(meshDispenserAgua, this.Este, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 2, 0, new float[] { 95, 135, 175 });

            //// Locker
            //CreateObjectsLine(meshLockerMetal, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 48.5f, 0, new float[] { 61, 63, 65, 67, 69, 71, 73, 75, 77, 79 });

            //// Expendedor
            //CreateObjectsLine(meshExpendedor, this.Oeste, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 18.5f, 0, new float[] { 90, 130, 150 });

            //// Mesa redonda
            //CreateObjectsLine(meshMesaRedonda, this.Oeste, this.Vector3Factory.CreateVector3(0.2f, 0.1f, 0.1f), 30, 0, 50);

            //// 15
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 70, 80 }, this.PlaneSize * 0.98f, 50);

            //// Mesas
            //CreateObjectsLine(meshMesa, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 83, 0, 50);

            //// Sillones
            //CreateObjectsLine(meshSillon, this.Norte, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 70, 0, 58);

            //// Locker
            //CreateObjectsLine(meshLockerMetal, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 75, 77, 79 }, 0, 58);

            //// 16
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 130, this.PlaneSize * 0.98f, 50);

            //// Locker
            //CreateObjectsLine(meshLockerMetal, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 121.5f, 0, new float[] { 41, 43, 45, 47, 49, 51, 53, 55, 57, 59 });
            //CreateObjectsLine(meshLockerMetal, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 130, 0, new float[] { 41, 43, 45, 47, 49, 51, 53, 55 });
            //CreateObjectsLine(meshLockerMetal, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 138.5f, 0, new float[] { 41, 43, 45, 47, 49, });

            //// 17
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 145, this.PlaneSize * 0.98f, 50);

            //// 18
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 155, this.PlaneSize * 0.98f, 50);

            //// Mesas
            //CreateObjectsLine(meshMesa, this.Norte, this.Vector3Factory.CreateVector3(0.080f, 0.090f, 0.050f), 155, 0, 53);

            //// Sillones
            //CreateObjectsLine(meshSillon, this.Norte, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 155, 0, 56);

            //// 19
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 165, this.PlaneSize * 0.98f, 50);

            //// Locker
            //CreateObjectsLine(meshLockerMetal, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 161.5f, 0, new float[] { 41, 43, 45, 47, 49, 51, 53, 55, 57, 59 });

            //// 20
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 180, this.PlaneSize * 0.98f, 50);

            //// Mesa redonda
            //CreateObjectsLine(meshMesaRedonda, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 180, 0, 50);

            //// Sillones
            //CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 172, 0, 50);
            //CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 187, 0, 50);
            //CreateObjectsLine(meshSillon, this.Norte, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 180, 0, 57.5f);

            //// Dispenser
            //CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 185, 0, 41.5f);

            //// 21
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 200, this.PlaneSize * 0.98f, 50);

            //// Locker
            //CreateObjectsLine(meshLockerMetal, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 208, 0, new float[] { 41, 43, 45, 47, 49, 51, 53, 55, 57, 59 });

            //// Mesas
            //CreateObjectsLine(meshMesa, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 200, 0, 50);

            //// 22
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 30, this.PlaneSize * 0.98f, new float[] { 90, 110 });

            //// Mesas
            //CreateObjectsLine(meshMesa, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 30, 0, new float[] { 90, 110 });

            //// Sillones
            //CreateObjectsLine(meshLockerMetal, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 38, 0, 90);

            //// 23
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 45, this.PlaneSize * 0.98f, new float[] { 90, 110, 130, 150, 170, 190 });

            //// 24
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 75, this.PlaneSize * 0.98f, new float[] { 80, 90, 120, 140 });

            //// Mesa redonda
            //CreateObjectsLine(meshMesaRedonda, this.Oeste, this.Vector3Factory.CreateVector3(0.2f, 0.1f, 0.1f), 75, 0, 85);

            //// Dispenser
            //CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 65, 0, 71.5f);

            //// Expendedor
            //CreateObjectsLine(meshExpendedor, this.Sur, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 85, 0, 71.5f);

            //// Sillones
            //CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 80, 0, 85);
            //CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 70, 0, 85);

            //// Locker
            //CreateObjectsLine(meshLockerMetal, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 70, 72, 74, 76, 78, 80, 82, 84 }, 0, 98.5f);

            //// 25
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 135, this.PlaneSize * 0.98f, new float[] { 80, 90, 120, 140 });

            //// Expendedor
            //CreateObjectsLine(meshExpendedor, this.Este, this.Vector3Factory.CreateVector3(0.070f, 0.070f, 0.070f), 121.5f, 0, new float[] { 75, 85, 95 });

            //// Dispenser
            //CreateObjectsLine(meshDispenserAgua, this.Oeste, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 148.5f, 0, new float[] { 90, 95 });

            //// Mesas
            //CreateObjectsLine(meshMesa, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 135, 0, new float[] { 76, 93 });

            //// Sillones
            //CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 130, 0, new float[] { 93, 76 });
            //CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 140, 0, new float[] { 93, 76 });

            //// 26
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 165, this.PlaneSize * 0.98f, new float[] { 70, 90, 110, 130, 150, 170, 190 });

            //// Sillones
            //CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 172, 0, new float[] { 95, 105, 115, 125 });

            //// 27
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 180, this.PlaneSize * 0.98f, new float[] { 80, 110, 135, 145 });
            //CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 195, this.PlaneSize * 0.98f, 115);
            //CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 200, this.PlaneSize * 0.98f, 80);
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 205.5f, this.PlaneSize * 0.98f, 100);

            //// Sillones
            //CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 208, 0, 80);
            //CreateObjectsLine(meshSillon, this.Este, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 195, 0, 80);

            //// Mesas
            //CreateObjectsLine(meshMesa, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 202, 0, 80);

            //// 28
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 30, this.PlaneSize * 0.98f, new float[] { 135, 165 });

            //// Mesa redonda
            //CreateObjectsLine(meshMesaRedonda, this.Oeste, this.Vector3Factory.CreateVector3(0.2f, 0.1f, 0.1f), 30, 0, new float[] { 135, 165 });

            //// Sillones
            //CreateObjectsLine(meshSillon, this.Oeste, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 38, 0, 110);
            //CreateObjectsLine(meshSillon, this.Norte, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 30, 0, new float[] { 145, 175 });
            //CreateObjectsLine(meshSillon, this.Sur, this.Vector3Factory.CreateVector3(0.05f, 0.05f, 0.05f), 30, 0, new float[] { 124, 155 });

            //// 29
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 60, 80 }, this.PlaneSize * 0.98f, 170);

            //// Camas
            //CreateObjectsLine(meshCama, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 60, 70, 80 }, 0, 175.5f);

            //// MesaDeLuz
            //CreateObjectsLine(meshMesaDeLuz, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 55, 65, 75, 85 }, 0, 178.5f);

            //// Locker
            //CreateObjectsLine(meshLockerMetal, this.Oeste, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 88.5f, 0, new float[] { 163, 165, 167, 169 });

            //// 30
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 145, this.PlaneSize * 0.98f, 170);
            //CreateObjectsLine(meshLamparaTecho, this.Este, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 130, this.PlaneSize * 0.98f, 165);

            //// 31
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 185, this.PlaneSize * 0.98f, 170);
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 205, this.PlaneSize * 0.98f, 155);

            //// 32
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 10, this.PlaneSize * 0.98f, 210);

            //// Camas
            //CreateObjectsLine(meshCama, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 10, 0, 215.5f);

            //// MesaDeLuz
            //CreateObjectsLine(meshMesaDeLuz, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 5, 15 }, 0, 218.5f);

            //// Locker
            //CreateObjectsLine(meshLockerMetal, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 19, 0, 201.5f);

            //// Dispenser
            //CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 16, 0, 201.5f);

            //// 33
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 30, this.PlaneSize * 0.98f, 210);

            //// Camas
            //CreateObjectsLine(meshCama, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 30, 0, 215.5f);

            //// MesaDeLuz
            //CreateObjectsLine(meshMesaDeLuz, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 25, 35 }, 0, 218.5f);

            //// Locker
            //CreateObjectsLine(meshLockerMetal, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 39, 0, 201.5f);

            //// Dispenser
            //CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 36, 0, 201.5f);

            //// 34
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 50, this.PlaneSize * 0.98f, 210);

            //// Camas
            //CreateObjectsLine(meshCama, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 50, 0, 215.5f);

            //// MesaDeLuz
            //CreateObjectsLine(meshMesaDeLuz, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 45, 55 }, 0, 218.5f);

            //// Locker
            //CreateObjectsLine(meshLockerMetal, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 59, 0, 201.5f);

            //// Dispenser
            //CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 56, 0, 201.5f);

            //// 35
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 70, 80 }, this.PlaneSize * 0.98f, 210);

            //// Camas
            //CreateObjectsLine(meshCama, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 70, 80 }, 0, 215.5f);

            //// MesaDeLuz
            //CreateObjectsLine(meshMesaDeLuz, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 65, 75, 85 }, 0, 218.5f);

            //// Locker
            //CreateObjectsLine(meshLockerMetal, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 80, 89 }, 0, 201.5f);

            //// Dispenser
            //CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 87, 0, 201.5f);

            //// 36
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 130, 140 }, this.PlaneSize * 0.98f, 210);

            //// Camas
            //CreateObjectsLine(meshCama, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 130, 140 }, 0, 215.5f);

            //// MesaDeLuz
            //CreateObjectsLine(meshMesaDeLuz, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 125, 135, 145 }, 0, 218.5f);

            //// Locker
            //CreateObjectsLine(meshLockerMetal, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 140, 149 }, 0, 201.5f);

            //// Dispenser
            //CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 145, 0, 201.5f);

            //// 37
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 160, 180 }, this.PlaneSize * 0.98f, 210);

            //// Camas
            //CreateObjectsLine(meshCama, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 160, 180 }, 0, 215.5f);

            //// MesaDeLuz
            //CreateObjectsLine(meshMesaDeLuz, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 155, 165, 175, 185 }, 0, 218.5f);

            //// Locker
            //CreateObjectsLine(meshLockerMetal, this.Sur, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), new float[] { 170, 189 }, 0, 201.5f);

            //// Dispenser
            //CreateObjectsLine(meshDispenserAgua, this.Sur, this.Vector3Factory.CreateVector3(0.080f, 0.080f, 0.080f), 175, 0, 201.5f);

            //// 38
            //// Lamparas
            //CreateObjectsLine(meshLamparaTecho, this.Norte, this.Vector3Factory.CreateVector3(0.1f, 0.1f, 0.1f), 200, this.PlaneSize * 0.98f, 210);
        }

        /// <summary>
        /// Carga el mesh
        /// </summary>
        /// <param name="xmlPath">Path del XML del mesh</param>
        private TgcMesh LoadMesh(string xmlPath)
        {
            var mesh = this.TgcSceneLoader.loadSceneFromFile(this.MediaDir + xmlPath).Meshes[0];
            var adj = new int[mesh.D3dMesh.NumberFaces * 3];
            mesh.D3dMesh.GenerateAdjacency(0, adj);
            mesh.D3dMesh.ComputeNormals(adj);
            return mesh;
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
        private void CreateObjectsLine(TgcMesh mesh, string orientation, Vector3 scale, float xCoordinate, float yCoordinate, float zCoordinate, List<int> rooms) => CreateObjectsLine(mesh, orientation, scale, new float[] { xCoordinate }, yCoordinate, new float[] { zCoordinate }, rooms);

        /// <summary>
        /// Crea una linea de objetos que componen el escenario
        /// </summary>
        /// <param name="mesh">Objeto con el cual crear la linea</param>
        /// <param name="orientation">Orientacion del objeto</param>
        /// <param name="xCoordinate">Indica la ubicacion en la coordenada X de la linea de objetos</param>
        /// <param name="yCoordinate">Indica la ubicacion en la coordenada Y de la linea de objetos</param>
        /// <param name="zCoordinates">Indica la ubicacion en la coordenada Z de la linea de objetos</param>
        /// <param name="scale">Tamaño del objeto</param>
        private void CreateObjectsLine(TgcMesh mesh, string orientation, Vector3 scale, float xCoordinate, float yCoordinate, float[] zCoordinates, List<int> rooms) => CreateObjectsLine(mesh, orientation, scale, new float[] { xCoordinate }, yCoordinate, zCoordinates, rooms);

        /// <summary>
        /// Crea una linea de objetos que componen el escenario
        /// </summary>
        /// <param name="mesh">Objeto con el cual crear la linea</param>
        /// <param name="orientation">Orientacion del objeto</param>
        /// <param name="xCoordinates">Indica la ubicacion en la coordenada X de la linea de objetos</param>
        /// <param name="yCoordinate">Indica la ubicacion en la coordenada Y de la linea de objetos</param>
        /// <param name="zCoordinate">Indica la ubicacion en la coordenada Z de la linea de objetos</param>
        /// <param name="scale">Tamaño del objeto</param>
        private void CreateObjectsLine(TgcMesh mesh, string orientation, Vector3 scale, float[] xCoordinates, float yCoordinate, float zCoordinate, List<int> rooms) => CreateObjectsLine(mesh, orientation, scale, xCoordinates, yCoordinate, new float[] { zCoordinate }, rooms);

        /// <summary>
        /// Crea una linea de objetos que componen el escenario
        /// </summary>
        /// <param name="mesh">Objeto con el cual crear la linea</param>
        /// <param name="orientation">Orientacion del objeto</param>
        /// <param name="xCoordinates">Indica la ubicacion en la coordenada X de la linea de objetos</param>
        /// <param name="yCoordinate">Indica la ubicacion en la coordenada Y de la linea de objetos</param>
        /// <param name="zCoordinates">Indica la ubicacion en la coordenada Z de la linea de objetos</param>
        /// <param name="scale">Tamaño del objeto</param>
        private void CreateObjectsLine(TgcMesh mesh, string orientation, Vector3 scale, float[] xCoordinates, float yCoordinate, float[] zCoordinates, List<int> rooms)
        {
            TgcMesh meshInstance;
            var rotation = CalculateRotation(orientation);
            foreach (var xCoordinate in xCoordinates)
            {
                foreach (var zCoordinate in zCoordinates)
                {
                    meshInstance = mesh.createMeshInstance(
                        this.Objects.Count + "_" + mesh.Name,
                        CalculateTranslation(mesh, orientation, xCoordinate, yCoordinate, zCoordinate),
                        rotation,
                        scale);

                    this.Objects.Add(new ScenarioElement
                    {
                        RoomsId = rooms,
                        RenderObject = meshInstance,
                        Body = this.CreateRigidBody(meshInstance, rotation, false)
                    });
                }
            }
        }

        /// <summary>
        /// Crea el <see cref="RigidBody"/> para calcular la fisica del objeto />
        /// </summary>
        /// <param name="renderObject">Objeto renderizable</param>
        /// <param name="rotation">Rotacion del objeto</param>
        /// <param name="isTgcBox">Indica si el objeto es una pared para hacerlo estatico</param>
        /// <returns>El objeto que contiene la fisica del objeto renderizable</returns>
        private RigidBody CreateRigidBody(IRenderObject renderObject, Vector3 rotation, bool isTgcBox)
        {
            BoxShape boxshape;
            BulletSharp.Math.Matrix transform;
            float mass;
            Vector3 position;
            Vector3 boundingBoxAxisRadius;
            var isDoor = false;
            if (isTgcBox)
            {
                var box = renderObject as TgcBox;
                position = box.Position;
                boundingBoxAxisRadius = box.BoundingBox.calculateAxisRadius();
                boxshape = new BoxShape(
                        boundingBoxAxisRadius.X,
                        boundingBoxAxisRadius.Y + 0.5f,
                        boundingBoxAxisRadius.Z);
                transform = BulletSharp.Math.Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
                    * BulletSharp.Math.Matrix.Translation(position.X + boundingBoxAxisRadius.X, position.Y + boundingBoxAxisRadius.Y, position.Z + boundingBoxAxisRadius.Z);
                mass = 0f;
            }
            else
            {
                var mesh = renderObject as TgcMesh;
                isDoor = mesh.Name.Contains("Puerta");
                position = mesh.Position;
                boundingBoxAxisRadius = mesh.BoundingBox.calculateAxisRadius();
                boxshape = new BoxShape(
                         boundingBoxAxisRadius.X,
                         boundingBoxAxisRadius.Y,
                         boundingBoxAxisRadius.Z);
                transform = BulletSharp.Math.Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
                    * BulletSharp.Math.Matrix.Translation(position.X + boundingBoxAxisRadius.X, position.Y + boundingBoxAxisRadius.Y, position.Z + boundingBoxAxisRadius.Z);
                mass = mesh.Name.Contains("LamparaTecho") ? 0f : 1f;
            }

            //if (isDoor)
            //{
            //    mass = 2.5f;
            //}

            var rigidBody = new RigidBody(new RigidBodyConstructionInfo(mass, new DefaultMotionState(transform), boxshape, boxshape.CalculateLocalInertia(mass)));

            this.DynamicsWorld.AddRigidBody(rigidBody);
            return rigidBody;
        }

        /// <summary>
        /// Crea el techo del escenario
        /// </summary>
        private void CreateRoof()
        {
            this.Roof = new List<IScenarioElement>();
            this.CreateHorizontalLayer(this.Roof, this.PlaneSize + 0.5f, TgcTexture.createTexture(D3DDevice.Instance.Device, this.MediaDir + @"Techo\Textures\techua.jpg"), 0, 0, 4, 4, new List<int> { 1 });
        }

        /// <summary>
        /// Crea las paredes del escenario
        /// </summary>
        private void CreateWalls()
        {
            this.Walls = new List<IScenarioElement>();

            // Paredes verticales
            //CreateWallsLine(this.Norte, 0, new float[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 });
            CreateWallsLine(this.Norte, 0, new float[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 }, new List<int> { 1, 2 });

            //CreateWallsLine(this.Norte, 2, new float[] { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 20, 21 });
            //CreateWallsLine(this.Norte, 3, new float[] { 0, 1, 2, 3 });
            //CreateWallsLine(this.Norte, 4, new float[] { 2, 3, 4, 5, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 20, 21 });
            //CreateWallsLine(this.Norte, 5, new float[] { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 });
            //CreateWallsLine(this.Norte, 6, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 20, 21 });
            //CreateWallsLine(this.Norte, 8, new float[] { 1, 2 });
            //CreateWallsLine(this.Norte, 9, new float[] { 0, 1, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            //CreateWallsLine(this.Norte, 12, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            //CreateWallsLine(this.Norte, 14, new float[] { 4, 5, 16 });
            //CreateWallsLine(this.Norte, 15, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 11, 12, 13, 14, 16, 17, 20, 21 });
            //CreateWallsLine(this.Norte, 16, new float[] { 0, 1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            //CreateWallsLine(this.Norte, 17, new float[] { 0, 1, 2, 4, 5, 7, 8, 9, 10, 11, 12, 13, 15, 16, 17, 18, 21 });
            //CreateWallsLine(this.Norte, 19, new float[] { 0, 1, 2, 4, 5, 7, 8, 13, 20, 21 });
            //CreateWallsLine(this.Norte, 20, new float[] { 9, 10, 14, 15, 16, 19 });
            //CreateWallsLine(this.Norte, 21, new float[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 });

            ////Paredes horizontales
            //CreateWallsLine(this.Este, 0, new float[] { 0, 2, 4, 20, 22 });
            //CreateWallsLine(this.Este, 1, new float[] { 0, 2, 4, 20, 22 });
            //CreateWallsLine(this.Este, 2, new float[] { 0, 2, 4, 8, 10, 12, 18, 20, 22 });
            //CreateWallsLine(this.Este, 3, new float[] { 0, 2, 4, 8, 10, 12, 18, 20, 22 });
            //CreateWallsLine(this.Este, 4, new float[] { 0, 2, 6, 8, 20, 22 });
            //CreateWallsLine(this.Este, 5, new float[] { 0, 2, 6, 16, 18, 20, 22 });
            //CreateWallsLine(this.Este, 6, new float[] { 0, 1, 3, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            //CreateWallsLine(this.Este, 7, new float[] { 0, 1, 3, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            //CreateWallsLine(this.Este, 8, new float[] { 0, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            //CreateWallsLine(this.Este, 9, new float[] { 0, 6, 16, 22 });
            //CreateWallsLine(this.Este, 10, new float[] { 0, 6, 16, 22 });
            //CreateWallsLine(this.Este, 11, new float[] { 0, 6, 16, 22 });
            //CreateWallsLine(this.Este, 12, new float[] { 0, 1.5f, 3, 4, 6, 7, 10, 11, 15, 16, 17, 18, 20, 22 });
            //CreateWallsLine(this.Este, 13, new float[] { 0, 1.5f, 3, 4, 6, 7, 10, 11, 15, 16, 17, 18, 20, 22 });
            //CreateWallsLine(this.Este, 14, new float[] { 0, 1.5f, 3, 4, 6, 7, 10, 11, 15, 16, 18, 20, 22 });
            //CreateWallsLine(this.Este, 15, new float[] { 0, 4, 6, 16, 20, 22 });
            //CreateWallsLine(this.Este, 16, new float[] { 0, 3, 4, 6, 20, 22 });
            //CreateWallsLine(this.Este, 17, new float[] { 0, 3, 4, 6, 7, 13, 14, 15, 19, 20, 22 });
            //CreateWallsLine(this.Este, 18, new float[] { 0, 3, 4, 6, 7, 13, 14, 15, 19, 20, 22 });
            //CreateWallsLine(this.Este, 19, new float[] { 0, 2, 4, 6, 7, 9, 15, 20, 22 });
            //CreateWallsLine(this.Este, 20, new float[] { 0, 2, 4, 6, 7, 9, 11, 14, 17, 19, 22 });
        }

        /// <summary>
        /// Crea una linea de paredes
        /// </summary>
        /// <param name="orientation">Orientacion de la pared</param>
        /// <param name="xCoordinate">Indica la ubicacion en la coordenada X de la linea de objetos</param>
        /// <param name="zCoordinates">Indica la ubicacion en la coordenada Z donde debe colocarse cada objeto</param>
        private void CreateWallsLine(string orientation, float xCoordinate, float[] zCoordinates, List<int> roomId)
        {
            TgcBox tgcBox;
            Vector3 translation;
            Vector3 rotation;
            Vector3 size;
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, this.MediaDir + @"Pared\Textures\techua.jpg");
            foreach (var zCoordinate in zCoordinates)
            {
                translation = orientation.Equals(this.Norte, StringComparison.OrdinalIgnoreCase) || orientation.Equals(this.Sur, StringComparison.OrdinalIgnoreCase) ?
                    this.Vector3Factory.CreateVector3(xCoordinate * 10, 0, zCoordinate * 10)
                    : this.Vector3Factory.CreateVector3(xCoordinate * 10 + this.PlaneSize / 2, 0, zCoordinate * 10 - this.PlaneSize / 2);
                rotation = CalculateRotation(orientation);
                size = this.Vector3Factory.CreateVector3(this.WallSize.X, this.WallSize.Y + 1, this.WallSize.Z);
                tgcBox = TgcBox.fromSize(size, texture);
                tgcBox.Rotation = rotation;
                tgcBox.move(translation.X, translation.Y - 0.5f, translation.Z);
                this.Walls.Add(
                    new ScenarioElement
                    {
                        RoomsId = roomId,
                        RenderObject = tgcBox,
                        Body = this.CreateRigidBody(tgcBox, rotation, true)
                    });
            }
        }
    }
}