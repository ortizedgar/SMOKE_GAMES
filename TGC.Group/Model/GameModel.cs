namespace TGC.Group.Model
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Autofac;
    using Microsoft.DirectX.Direct3D;
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
        /// Mesh para la linterna
        /// </summary>
        private TgcBox MeshLinterna;

        /// <summary>
        /// Mesh para la linterna
        /// </summary>
        private TgcBox MeshLampara;

        /// <summary>
        /// Mesh para la linterna
        /// </summary>
        private TgcBox MeshVela;

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
        }

        /// <summary>
        /// Container IOC
        /// </summary>
        private IContainer Container { get; set; }

        /// <summary>
        /// Indica si el techo y el piso deben renderizarse
        /// </summary>
        private bool RenderFloorAndRoof { get; set; } = false;

        /// <summary>
        /// Lista de <see cref="IRenderObject"/> que contiene las paredes, pisos, techos y objetos del escenario
        /// </summary>
        private List<Tuple<string, List<IRenderObject>>> ScenarioElements { get; set; }

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
            DisposeScenario();
            this.MeshLinterna.dispose();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        /// </summary>
        public override void Init()
        {
            InitCamara();
            InitScenario();
            InitLights();

            ////Se crea una caja de tamaño 20 con rotaciones y origien en 10,100,10 y 1kg de masa.
            //var boxShape = new BoxShape(10, 10, 10);
            //var boxTransform = Matrix.RotationYawPitchRoll(MathUtil.SIMD_HALF_PI, MathUtil.SIMD_QUARTER_PI, MathUtil.SIMD_2_PI).ToBsMatrix;
            //boxTransform.Origin = new Vector(10, 100, 10).ToBsVector;
            //DefaultMotionState boxMotionState = new DefaultMotionState(boxTransform);

            ////Es importante calcular la inercia caso contrario el objeto no rotara.
            //var boxLocalInertia = boxShape.CalculateLocalInertia(1f);
            //var boxInfo = new RigidBodyConstructionInfo(1f, boxMotionState, boxShape, boxLocalInertia);
            //boxBody = new RigidBody(boxInfo);
            //dynamicsWorld.AddRigidBody(boxBody);
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
            RenderLights();

            // Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        /// <summary>
        ///     Se llama en cada frame.
        /// </summary>
        public override void Update()
        {
            PreUpdate();

            if (this.ElapsedTime >= 1 / 30)
            {
                ActivateRoofAndFloor();
                UpdateLights();

                if (this.Input.keyPressed(Microsoft.DirectX.DirectInput.Key.B))
                {
                    this.RenderBoundingBox = !this.RenderBoundingBox;
                }
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
        private void DisposeScenario() => this.ScenarioElements
            .AsParallel()
            .SelectMany(
                element =>
                    element.Item2)
            .ForAll(
                element =>
                    element.dispose());

        /// <summary>
        /// Inicializa el escenario
        /// </summary>
        private void InitCamara() => this.Camara = this.Container.Resolve<TgcFpsCamera>(
                            new NamedParameter("positionEye", this.Vector3Factory.CreateVector3(5, 5, 5)),
                            new NamedParameter("moveSpeed", 50),
                            new NamedParameter("jumpSpeed", 50),
                            new NamedParameter("input", this.Input));

        /// <summary>
        /// Inicializa las luces
        /// </summary>
        private void InitLights()
        {
            InitLinterna();
            InitLampara();
            InitVela();
        }

        private void InitLinterna()
        {
            this.MeshLinterna = TgcBox.fromSize(this.Vector3Factory.CreateVector3(1, 1, 1));
            this.MeshLinterna.AutoTransformEnable = true;
            this.MeshLinterna.Position = this.Camara.Position;
            this.MeshLinterna.Color = Color.White;
            this.MeshLinterna.Enabled = true;
        }

        private void InitLampara()
        {
            this.MeshLampara = TgcBox.fromSize(this.Vector3Factory.CreateVector3(1, 1, 1));
            this.MeshLampara.AutoTransformEnable = true;
            this.MeshLampara.Position = this.Camara.Position;
            this.MeshLampara.Color = Color.Blue;
            this.MeshLampara.Enabled = false;
        }

        private void InitVela()
        {
            this.MeshVela = TgcBox.fromSize(this.Vector3Factory.CreateVector3(1, 1, 1));
            this.MeshVela.AutoTransformEnable = true;
            this.MeshVela.Position = this.Camara.Position;
            this.MeshVela.Color = Color.Red;
            this.MeshVela.Enabled = false;
        }

        /// <summary>
        /// Inicializa la camara
        /// </summary>
        private void InitScenario() => this.ScenarioElements = this.Container.Resolve<IScenarioCreator>().CreateScenario(this.MediaDir, this.Container);

        /// <summary>
        /// Dibujar las instrucciones del juego
        /// </summary>
        private void RenderInstructions()
        {
            this.DrawText.drawText("Presione R para dibujar/eliminar el techo y el piso", 0, 20, Color.OrangeRed);
            this.DrawText.drawText("Presione Shift izquierdo para prender/apagar la literna", 0, 40, Color.OrangeRed);
            this.DrawText.drawText("Presione WSAD para moverse", 0, 60, Color.OrangeRed);
            this.DrawText.drawText("Mantenga presionado el boton izquierdo del mouse para mover la camara", 0, 80, Color.OrangeRed);
        }

        private float lightIntensity { get; set; } = 1;

        /// <summary>
        /// Dibuja las luces
        /// </summary>
        private void RenderLights() => this.ScenarioElements
            .AsParallel()
            .SelectMany(
                element =>
                    element.Item2)
            .ToList()
            .ForEach(
                element =>
                {
                    this.lightIntensity = this.lightIntensity > 0.25f ? this.lightIntensity - 0.000001f : 0.25f;
                    var mesh = element as TgcMesh;
                    mesh.Technique = TgcShaders.Instance.getTgcMeshTechnique(mesh.RenderType);
                    mesh.Effect = TgcShaders.Instance.TgcMeshShader;

                    if (this.MeshLinterna.Enabled)
                    {
                        SetLinterna(mesh);
                    }
                    if (this.MeshLampara.Enabled)
                    {
                        SetLampara(mesh);
                    }
                    if (this.MeshVela.Enabled)
                    {
                        SetVela(mesh);
                    }

                    // Cargar variables de shader de Material
                    mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
                    mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.White));
                    mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
                    mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.White));
                    mesh.Effect.SetValue("materialSpecularExp", 1f);
                    mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(this.Camara.Position));
                    mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(this.Camara.Position));
                });

        private void SetLinterna(TgcMesh mesh)
        {
            mesh.Effect = TgcShaders.Instance.TgcMeshPointLightShader;

            // Cargar variables shader de la luz
            mesh.Effect.SetValue("lightColor", ColorValue.FromColor(this.MeshLinterna.Color));
            mesh.Effect.SetValue(nameof(lightIntensity), this.lightIntensity);
            mesh.Effect.SetValue("lightAttenuation", 0.5f);
        }

        private void SetLampara(TgcMesh mesh)
        {
            mesh.Effect = TgcShaders.Instance.TgcMeshPointLightShader;

            // Cargar variables shader de la luz
            mesh.Effect.SetValue("lightColor", ColorValue.FromColor(this.MeshLampara.Color));
            mesh.Effect.SetValue(nameof(lightIntensity), this.lightIntensity);
            mesh.Effect.SetValue("lightAttenuation", 0.5f);
        }

        private void SetVela(TgcMesh mesh)
        {
            mesh.Effect = TgcShaders.Instance.TgcMeshPointLightShader;

            // Cargar variables shader de la luz
            mesh.Effect.SetValue("lightColor", ColorValue.FromColor(this.MeshVela.Color));
            mesh.Effect.SetValue(nameof(lightIntensity), this.lightIntensity);
            mesh.Effect.SetValue("lightAttenuation", 0.5f);
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
        private void RenderWithFloorAndRoof() => this.ScenarioElements
            .AsParallel()
            .SelectMany(
                element =>
                    element.Item2)
            .ToList()
            .ForEach(
                element =>
                    RenderElements(element as TgcMesh));

        private bool RenderBoundingBox { get; set; } = false;

        private void RenderElements(TgcMesh element)
        {
            if (this.RenderBoundingBox)
            {
                element.BoundingBox.render();
            }

            element.render();
        }

        /// <summary>
        /// Renderiza el escenario sin piso y techo
        /// </summary>
        private void RenderWithoutFloorAndRoof() => this.ScenarioElements
            .AsParallel()
            .Where(
                element =>
                    !element.Item1.Equals("Floor") && !element.Item1.Equals("Roof"))
            .SelectMany(
                element =>
                    element.Item2)
            .ToList()
            .ForEach(
                element =>
                    RenderElements(element as TgcMesh));

        /// <summary>
        /// Actualiza las luces
        /// </summary>
        private void UpdateLights()
        {
            if (this.Input.keyPressed(Microsoft.DirectX.DirectInput.Key.F1))
            {
                this.lightIntensity = 1;
                this.MeshLinterna.Enabled = true;
                this.MeshLampara.Enabled = false;
                this.MeshVela.Enabled = false;
            }

            if (this.Input.keyPressed(Microsoft.DirectX.DirectInput.Key.F2))
            {
                this.lightIntensity = 1;
                this.MeshLinterna.Enabled = false;
                this.MeshLampara.Enabled = true;
                this.MeshVela.Enabled = false;
            }

            if (this.Input.keyPressed(Microsoft.DirectX.DirectInput.Key.F3))
            {
                this.lightIntensity = 1;
                this.MeshLinterna.Enabled = false;
                this.MeshLampara.Enabled = false;
                this.MeshVela.Enabled = true;
            }

            this.MeshLinterna.updateValues();
            this.MeshLampara.updateValues();
            this.MeshVela.updateValues();
        }
    }
}