namespace TGC.Group.Model
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Autofac;
    using BulletSharp;
    using BulletSharp.Math;
    using Microsoft.DirectX.Direct3D;
    using TGC.Core.Collision;
    using TGC.Core.Example;
    using TGC.Core.Geometry;
    using TGC.Core.SceneLoader;
    using TGC.Core.Shaders;
    using TGC.Core.Utils;
    using TGC.Group.Interfaces;

    /// <summary>
    ///     Survial horror game
    /// </summary>
    public class GameModel : TgcExample
    {
        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
        /// <param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
        public GameModel(string mediaDir, string shadersDir, IContainer container) : base(mediaDir, shadersDir)
        {
            this.Category = Game.Default.Category;
            this.Name = Game.Default.Name;
            this.Description = Game.Default.Description;
            this.Container = container;
            this.Vector3Factory = container.Resolve<IVector3Factory>();
            this.LayerRenderList = new List<IScenarioLayer>();

            // Se crea el mundo fisico por defecto
            this.CollisionConfiguration = new DefaultCollisionConfiguration();
            this.Dispatcher = new CollisionDispatcher(this.CollisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(this.Dispatcher);
            this.DynamicsWorld = new DiscreteDynamicsWorld(this.Dispatcher, new DbvtBroadphase(), new SequentialImpulseConstraintSolver(), this.CollisionConfiguration)
            {
                Gravity = new Vector3(0, -10f, 0)
            };
        }

        /// <summary>
        /// Configuracion de colisiones
        /// </summary>
        private DefaultCollisionConfiguration CollisionConfiguration { get; set; }

        /// <summary>
        /// Container IOC
        /// </summary>
        private IContainer Container { get; set; }

        /// <summary>
        /// Manejador de colisiones
        /// </summary>
        private CollisionDispatcher Dispatcher { get; set; }

        /// <summary>
        /// Mundo dinamico
        /// </summary>
        private DiscreteDynamicsWorld DynamicsWorld { get; set; }

        /// <summary>
        /// Intensidad de la luz
        /// </summary>
        private float LightIntensity { get; set; } = 1;

        /// <summary>
        /// Variacion de la intensidad de la luz
        /// </summary>
        private float LightIntensityVariation { get; set; } = 0.000001f;

        /// <summary>
        /// Mesh para la linterna
        /// </summary>
        private TgcBox LightMesh { get; set; }

        /// <summary>
        /// Indica si se debe dibujar el bounding box o no
        /// </summary>
        private bool RenderBoundingBox { get; set; } = false;

        /// <summary>
        /// Indica si el techo y el piso deben renderizarse
        /// </summary>
        private bool RenderFloorAndRoof { get; set; } = false;

        /// <summary>
        /// Fabrica de objetos <see cref="Vector3"/>
        /// </summary>
        private IVector3Factory Vector3Factory { get; set; }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            this.DynamicsWorld.Dispose();
            this.Dispatcher.Dispose();
            this.CollisionConfiguration.Dispose();
            this.DisposeScenario();
            this.LightMesh.dispose();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        /// </summary>
        public override void Init()
        {
            InitCamara();
            InitScenario();
            InitLights();
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        /// </summary>
        public override void Render()
        {
            // Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            RenderInstructions();
            RenderScenario();

            // Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        /// <summary>
        ///     Se llama en cada frame.
        /// </summary>
        public override void Update()
        {
            PreUpdate();

            this.DynamicsWorld.StepSimulation(1 / 60f, 10);

            if (this.ElapsedTime >= 1 / 30)
            {
                ActivateRoofAndFloor();
                ActivateBoundingBox();
                UpdateLights();
                UpdateScenario();
                UpdateCharacter();

                UpdateLayerRenderList();
            }
        }

        private string FrustumPortal { get; set; }

        private void UpdateLayerRenderList()
        {
            var portalFrustumList = this.ScenarioCreator.PortalUnionList.Where(element =>
            {
                //FrustumPortal = TgcCollisionUtils.classifyFrustumAABB(this.Frustum, element.Door.BoundingBox).Equals(TgcCollisionUtils.FrustumResult.INSIDE);

                return TgcCollisionUtils.classifyFrustumAABB(this.Frustum, element.Door.BoundingBox).Equals(TgcCollisionUtils.FrustumResult.INSIDE) || TgcCollisionUtils.classifyFrustumAABB(this.Frustum, element.Door.BoundingBox).Equals(TgcCollisionUtils.FrustumResult.INTERSECT);
            });

            var portal = getClosestPortal(this.Camara.Position, portalFrustumList.ToList());

            FrustumPortal = portal?.RoomA.ToString() + " - " + portal?.RoomB.ToString();

            if (portal == null)
            {
                portal = getClosestPortal(this.Camara.Position, this.ScenarioCreator.PortalUnionList);
            }

            this.LayerRenderList = this.ScenarioCreator.ScenarioLayers;
            //this.LayerRenderList.Clear();
            //this.ScenarioCreator.PortalUnionList.ForEach(element => this.LayerRenderList.Add(new ScenarioLayer { LayerName = "Portals", ScenarioElements = element.DoorWalls }));
            //this.ScenarioCreator.ScenarioLayers.ForEach(
            //    scenarioLayer =>
            //    {
            //        var scenarioElements = scenarioLayer.ScenarioElements.Where(element => element.RoomsId.Contains(portal.RoomA) || element.RoomsId.Contains(portal.RoomB)).ToList();
            //        this.LayerRenderList.Add(
            //            new ScenarioLayer
            //            {
            //                LayerName = scenarioLayer.LayerName,
            //                ScenarioElements = scenarioElements
            //            });
            //    });
        }

        private List<IScenarioLayer> LayerRenderList { get; set; }

        /// <summary>
        ///     Devuelve la luz mas cercana a la posicion especificada
        /// </summary>
        private IPortal getClosestPortal(Microsoft.DirectX.Vector3 pos, List<IPortal> portalsList)
        {
            var minDist = float.MaxValue;
            IPortal minPortal = null;

            foreach (var portal in portalsList)
            {
                var distSq = Microsoft.DirectX.Vector3.LengthSq(pos - portal.Door.BoundingBox.calculateBoxCenter());
                if (distSq < minDist)
                {
                    minDist = distSq;
                    minPortal = portal;
                }
            }

            return minPortal;
        }

        /// <summary>
        /// Actualiza el personaje
        /// </summary>
        private void UpdateCharacter()
        {
            var character = ((TgcFpsCamera)this.Camara).Character;
            character.Item1.Transform = new Microsoft.DirectX.Matrix
            {
                M11 = character.Item2.InterpolationWorldTransform.M11,
                M12 = character.Item2.InterpolationWorldTransform.M12,
                M13 = character.Item2.InterpolationWorldTransform.M13,
                M14 = character.Item2.InterpolationWorldTransform.M14,
                M21 = character.Item2.InterpolationWorldTransform.M21,
                M22 = character.Item2.InterpolationWorldTransform.M22,
                M23 = character.Item2.InterpolationWorldTransform.M23,
                M24 = character.Item2.InterpolationWorldTransform.M24,
                M31 = character.Item2.InterpolationWorldTransform.M31,
                M32 = character.Item2.InterpolationWorldTransform.M32,
                M33 = character.Item2.InterpolationWorldTransform.M33,
                M34 = character.Item2.InterpolationWorldTransform.M34,
                M41 = character.Item2.InterpolationWorldTransform.M41,
                M42 = character.Item2.InterpolationWorldTransform.M42,
                M43 = character.Item2.InterpolationWorldTransform.M43,
                M44 = character.Item2.InterpolationWorldTransform.M44
            };

            var axisRadius = character.Item1.BoundingBox.calculateAxisRadius();
            var pmin = new Microsoft.DirectX.Vector3(
                character.Item2.InterpolationWorldTransform.Origin.X - axisRadius.X,
                character.Item2.InterpolationWorldTransform.Origin.Y - axisRadius.Y,
                character.Item2.InterpolationWorldTransform.Origin.Z - axisRadius.Z);
            var pmax = new Microsoft.DirectX.Vector3(
                character.Item2.InterpolationWorldTransform.Origin.X + axisRadius.X,
                character.Item2.InterpolationWorldTransform.Origin.Y + axisRadius.Y,
                character.Item2.InterpolationWorldTransform.Origin.Z + axisRadius.Z);
            character.Item1.BoundingBox.setExtremes(pmin, pmax);
        }

        /// <summary>
        /// Verifica si hay que dibujar los Bounding Box
        /// </summary>
        private void ActivateBoundingBox()
        {
            if (this.Input.keyPressed(Microsoft.DirectX.DirectInput.Key.B))
            {
                this.RenderBoundingBox = !this.RenderBoundingBox;
            }
        }

        /// <summary>
        /// Verifica si hay que dibujar el piso y el techo
        /// </summary>
        private void ActivateRoofAndFloor()
        {
            if (this.Input.keyPressed(Microsoft.DirectX.DirectInput.Key.R))
            {
                this.RenderFloorAndRoof = !this.RenderFloorAndRoof;
            }
        }

        /// <summary>
        /// Libera la memoria utilizada para el escenario
        /// </summary>
        private void DisposeScenario() => this.ScenarioCreator.ScenarioLayers
                .AsParallel()
                .SelectMany(
                    element =>
                        element.ScenarioElements)
                .ForAll(
                    element =>
                    {
                        element.RenderObject.dispose();
                    });

        /// <summary>
        /// Inicializa la camara
        /// </summary>
        private void InitCamara() => this.Camara = this.Container.Resolve<TgcFpsCamera>(
                            new NamedParameter("positionEye", this.Vector3Factory.CreateVector3(0, 0, 0)),
                            new NamedParameter("moveSpeed", 50),
                            new NamedParameter("jumpSpeed", 50),
                            new NamedParameter("input", this.Input),
                            new NamedParameter("dynamicsWorld", this.DynamicsWorld));

        /// <summary>
        /// Inicializa las luces
        /// </summary>
        private void InitLights()
        {
            this.LightMesh = TgcBox.fromSize(this.Vector3Factory.CreateVector3(100, 100, 100), this.Vector3Factory.CreateVector3(0, 0, 0));
            this.LightMesh.AutoTransformEnable = true;
            this.LightMesh.Position = this.Camara.Position;
            this.LightMesh.Color = Color.White;
            this.LightMesh.Enabled = true;
        }

        private IScenarioCreator ScenarioCreator { get; set; }

        /// <summary>
        /// Inicializa el escenario
        /// </summary>
        private void InitScenario()
        {
            this.ScenarioCreator = this.Container.Resolve<IScenarioCreator>();
            this.ScenarioCreator.CreateScenario(this.Container, this.MediaDir, this.DynamicsWorld);
        }

        /// <summary>
        /// Dibuja los elementos
        /// </summary>
        /// <param name="element"></param>
        private void RenderElement(IScenarioElement element)
        {
            if (this.RenderBoundingBox)
            {
                if (element.RenderObject is TgcMesh mesh)
                {
                    mesh.BoundingBox.render();
                }

                if (element.RenderObject is TgcBox box)
                {
                    box.BoundingBox.render();
                }
            }

            element.RenderObject?.render();
        }

        /// <summary>
        /// Dibujar las instrucciones del juego
        /// </summary>
        private void RenderInstructions()
        {
            this.DrawText.drawText("Mantenga presionado el boton izquierdo del mouse para mover la camara", 0, 20, Color.OrangeRed);
            this.DrawText.drawText("Presione WSAD para moverse", 0, 40, Color.OrangeRed);
            this.DrawText.drawText("Presione R para dibujar/eliminar el techo y el piso", 0, 60, Color.OrangeRed);
            this.DrawText.drawText("Presione F1, F2 o F3 para seleccionar distintas liternas", 0, 80, Color.OrangeRed);
            this.DrawText.drawText("Presione B para dibujar/eliminar los Bounding Box", 0, 100, Color.OrangeRed);
            this.DrawText.drawText("Presione F para activar/desactivar la iluminacion", 0, 120, Color.OrangeRed);
            this.DrawText.drawText("Presione LShift para correr", 0, 140, Color.OrangeRed);

            this.DrawText.drawText("Frustum-Portal: " + FrustumPortal, 500, 0, Color.Black);
        }

        /// <summary>
        /// Renderiza el escenario
        /// </summary>
        private void RenderScenario()
        {
            if (this.RenderFloorAndRoof)
            {
                RenderWithFloorAndRoof();
            }
            else
            {
                RenderWithoutFloorAndRoof();
            }
        }

        /// <summary>
        /// Renderiza el escenario con piso y techo
        /// </summary>
        private void RenderWithFloorAndRoof() => this.LayerRenderList
            .AsParallel()
            .SelectMany(
                 layer =>
                    layer.ScenarioElements)
            .ToList()
            .ForEach(
                element =>
                    RenderElement(element));

        /// <summary>
        /// Renderiza el escenario sin piso y techo
        /// </summary>
        private void RenderWithoutFloorAndRoof() => this.LayerRenderList
            .AsParallel()
            .Where(
                layer =>
                    !layer.LayerName.Equals("Floor") && !layer.LayerName.Equals("Roof"))
            .SelectMany(
                 layer =>
                    layer.ScenarioElements)
            .ToList()
            .ForEach(
                element =>
                    RenderElement(element));

        /// <summary>
        /// Configura el efecto del box
        /// </summary>
        /// <param name="box"></param>
        private void SetBoxEffect(TgcBox box)
        {
            box.Effect = TgcShaders.Instance.TgcMeshShader;
            if (this.LightMesh.Enabled)
            {
                this.LightIntensity = this.LightIntensity > 0.25f ? this.LightIntensity - this.LightIntensityVariation : 0.25f;
                box.Technique = TgcShaders.Instance.getTgcMeshTechnique(TgcMesh.MeshRenderType.DIFFUSE_MAP);
                box.Effect = TgcShaders.Instance.TgcMeshSpotLightShader;

                //Cargar variables shader de la luz
                var spotLightDir = ((TgcFpsCamera)this.Camara).LightDir;
                spotLightDir.Normalize();
                box.Effect.SetValue(nameof(spotLightDir), TgcParserUtils.vector3ToFloat3Array(spotLightDir));
                box.Effect.SetValue("spotLightAngleCos", FastMath.ToRad(0));
                box.Effect.SetValue("spotLightExponent", 40f);

                box.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
                box.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.White));
                box.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
                box.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.White));
                box.Effect.SetValue("materialSpecularExp", 20f);
                box.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(this.Camara.Position));
                box.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(this.Camara.Position));
                box.Effect.SetValue("lightColor", ColorValue.FromColor(this.LightMesh.Color));
                box.Effect.SetValue("lightIntensity", this.LightIntensity);
                box.Effect.SetValue("lightAttenuation", 0.01f);
            }
        }

        /// <summary>
        /// Configura el efecto del mesh
        /// </summary>
        /// <param name="mesh"></param>
        private void SetMeshEffect(TgcMesh mesh)
        {
            mesh.Effect = TgcShaders.Instance.TgcMeshShader;
            if (this.LightMesh.Enabled)
            {
                this.LightIntensity = this.LightIntensity > 0.25f ? this.LightIntensity - this.LightIntensityVariation : 0.25f;
                mesh.Technique = TgcShaders.Instance.getTgcMeshTechnique(mesh.RenderType);
                mesh.Effect = TgcShaders.Instance.TgcMeshSpotLightShader;

                var spotLightDir = ((TgcFpsCamera)this.Camara).LightDir;
                spotLightDir.Normalize();
                mesh.Effect.SetValue(nameof(spotLightDir), TgcParserUtils.vector3ToFloat3Array(spotLightDir));
                mesh.Effect.SetValue("spotLightAngleCos", FastMath.ToRad(0));
                mesh.Effect.SetValue("spotLightExponent", 40f);

                mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
                mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.White));
                mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
                mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.White));
                mesh.Effect.SetValue("materialSpecularExp", 20f);
                mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(this.Camara.Position));
                mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(this.Camara.Position));
                mesh.Effect.SetValue("lightColor", ColorValue.FromColor(this.LightMesh.Color));
                mesh.Effect.SetValue("lightIntensity", this.LightIntensity);
                mesh.Effect.SetValue("lightAttenuation", 0.01f);
            }
        }

        /// <summary>
        /// Actualiza las luces
        /// </summary>
        private void UpdateLights()
        {
            if (this.Input.keyPressed(Microsoft.DirectX.DirectInput.Key.F1))
            {
                this.LightIntensity = 1;
                this.LightIntensityVariation = 0.000001f;
                this.LightMesh.Color = Color.White;
            }

            if (this.Input.keyPressed(Microsoft.DirectX.DirectInput.Key.F2))
            {
                this.LightIntensity = 1;
                this.LightIntensityVariation = 0.000001f * 2.5f;
                this.LightMesh.Color = Color.Green;
            }

            if (this.Input.keyPressed(Microsoft.DirectX.DirectInput.Key.F3))
            {
                this.LightIntensity = 1;
                this.LightIntensityVariation = 0.000001f * 5;
                this.LightMesh.Color = Color.Blue;
            }

            if (this.Input.keyPressed(Microsoft.DirectX.DirectInput.Key.F))
            {
                this.LightMesh.Enabled = !this.LightMesh.Enabled;
            }

            this.LightMesh.updateValues();
        }

        private void UpdateScenario() => this.ScenarioCreator.ScenarioLayers
            .AsParallel()
            .SelectMany(
                element =>
                    element.ScenarioElements)
            .ForAll(
                element =>
                {
                    if (element.RenderObject is TgcMesh mesh)
                    {
                        if (element.Body is RigidBody rigidBody)
                        {
                            mesh.Transform = Microsoft.DirectX.Matrix.Scaling(mesh.Scale.X, mesh.Scale.Y, mesh.Scale.Z);
                            mesh.Transform *= new Microsoft.DirectX.Matrix
                            {
                                M11 = rigidBody.InterpolationWorldTransform.M11,
                                M12 = rigidBody.InterpolationWorldTransform.M12,
                                M13 = rigidBody.InterpolationWorldTransform.M13,
                                M14 = rigidBody.InterpolationWorldTransform.M14,
                                M21 = rigidBody.InterpolationWorldTransform.M21,
                                M22 = rigidBody.InterpolationWorldTransform.M22,
                                M23 = rigidBody.InterpolationWorldTransform.M23,
                                M24 = rigidBody.InterpolationWorldTransform.M24,
                                M31 = rigidBody.InterpolationWorldTransform.M31,
                                M32 = rigidBody.InterpolationWorldTransform.M32,
                                M33 = rigidBody.InterpolationWorldTransform.M33,
                                M34 = rigidBody.InterpolationWorldTransform.M34,
                                M41 = rigidBody.InterpolationWorldTransform.M41,
                                M42 = rigidBody.InterpolationWorldTransform.M42,
                                M43 = rigidBody.InterpolationWorldTransform.M43,
                                M44 = rigidBody.InterpolationWorldTransform.M44
                            };

                            var axisRadius = mesh.BoundingBox.calculateAxisRadius();
                            mesh.Transform *= Microsoft.DirectX.Matrix.Translation(0, -axisRadius.Y, 0);
                            var pmin = this.Vector3Factory.CreateVector3(
                                rigidBody.InterpolationWorldTransform.Origin.X - axisRadius.X,
                                rigidBody.InterpolationWorldTransform.Origin.Y - axisRadius.Y,
                                rigidBody.InterpolationWorldTransform.Origin.Z - axisRadius.Z);
                            var pmax = this.Vector3Factory.CreateVector3(
                                rigidBody.InterpolationWorldTransform.Origin.X + axisRadius.X,
                                rigidBody.InterpolationWorldTransform.Origin.Y + axisRadius.Y,
                                rigidBody.InterpolationWorldTransform.Origin.Z + axisRadius.Z);
                            mesh.BoundingBox.setExtremes(pmin, pmax);
                        }

                        this.SetMeshEffect(mesh);
                    }

                    if (element.RenderObject is TgcBox box)
                    {
                        if (element.Body is RigidBody rigidBody)
                        {
                            box.Transform = new Microsoft.DirectX.Matrix
                            {
                                M11 = rigidBody.InterpolationWorldTransform.M11,
                                M12 = rigidBody.InterpolationWorldTransform.M12,
                                M13 = rigidBody.InterpolationWorldTransform.M13,
                                M14 = rigidBody.InterpolationWorldTransform.M14,
                                M21 = rigidBody.InterpolationWorldTransform.M21,
                                M22 = rigidBody.InterpolationWorldTransform.M22,
                                M23 = rigidBody.InterpolationWorldTransform.M23,
                                M24 = rigidBody.InterpolationWorldTransform.M24,
                                M31 = rigidBody.InterpolationWorldTransform.M31,
                                M32 = rigidBody.InterpolationWorldTransform.M32,
                                M33 = rigidBody.InterpolationWorldTransform.M33,
                                M34 = rigidBody.InterpolationWorldTransform.M34,
                                M41 = rigidBody.InterpolationWorldTransform.M41,
                                M42 = rigidBody.InterpolationWorldTransform.M42,
                                M43 = rigidBody.InterpolationWorldTransform.M43,
                                M44 = rigidBody.InterpolationWorldTransform.M44
                            };

                            var axisRadius = box.BoundingBox.calculateAxisRadius();
                            var pmin = this.Vector3Factory.CreateVector3(
                                rigidBody.InterpolationWorldTransform.Origin.X - axisRadius.X,
                                rigidBody.InterpolationWorldTransform.Origin.Y - axisRadius.Y,
                                rigidBody.InterpolationWorldTransform.Origin.Z - axisRadius.Z);
                            var pmax = this.Vector3Factory.CreateVector3(
                                rigidBody.InterpolationWorldTransform.Origin.X + axisRadius.X,
                                rigidBody.InterpolationWorldTransform.Origin.Y + axisRadius.Y,
                                rigidBody.InterpolationWorldTransform.Origin.Z + axisRadius.Z);
                            box.BoundingBox.setExtremes(pmin, pmax);
                        }

                        this.SetBoxEffect(box);
                    }
                });
    }
}