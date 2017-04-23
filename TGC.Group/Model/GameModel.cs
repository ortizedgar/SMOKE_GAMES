namespace TGC.Group.Model
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Autofac;
    using Microsoft.DirectX;
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
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose() => DisposeScenario();

        private TgcBox lightMesh;

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        /// </summary>
        public override void Init()
        {
            InitCamara();

            InitScenario();

            InitLights();
        }

        private void InitLights()
        {
            //Mesh para la luz
            this.lightMesh = TgcBox.fromSize(new Vector3(1, 1, 1));

            //Pongo al mesh en posicion, activo e AutoTransform
            this.lightMesh.AutoTransformEnable = true;
            this.lightMesh.Position = new Vector3(10, 10, 10);
            this.lightMesh.Color = Color.White;
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

        private void RenderLights()
        {
            var currentShader = this.lightMesh.Enabled ? TgcShaders.Instance.TgcMeshPointLightShader : TgcShaders.Instance.TgcMeshShader;

            // Renderizar meshes
            this.ScenarioElements
            .AsParallel()
            .SelectMany(
                element =>
                    element.Item2)
            .ToList()
            .ForEach(
                element =>
                {
                    if (element is TgcMesh mesh)
                    {
                        mesh.Effect = currentShader;

                        // El Technique depende del tipo RenderType del mesh
                        mesh.Technique = TgcShaders.Instance.getTgcMeshTechnique(mesh.RenderType);

                        if (this.lightMesh.Enabled)
                        {
                            // Cargar variables shader de la luz
                            mesh.Effect.SetValue("lightColor", ColorValue.FromColor(this.lightMesh.Color));
                            mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(this.lightMesh.Position));
                            mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(this.Camara.Position));
                            mesh.Effect.SetValue("lightIntensity", 1f);
                            mesh.Effect.SetValue("lightAttenuation", 0.5f);

                            // Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
                            mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
                            mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.White));
                            mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
                            mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.White));
                            mesh.Effect.SetValue("materialSpecularExp", 1f);
                        }
                    }
                });
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
            }
        }

        private void UpdateLights()
        {
            if (this.Input.keyPressed(Microsoft.DirectX.DirectInput.Key.LeftShift))
            {
                this.lightMesh.Enabled = !this.lightMesh.Enabled;
            }

            this.lightMesh.updateValues();
        }

        /// <summary>
        /// Renderiza un elemento y lo actualiza si es un <see cref="TgcPlane"/>
        /// </summary>
        /// <param name="element"></param>
        private static void RenderElement(IRenderObject element)
        {
            if (element is TgcPlane tgcPlane)
            {
                tgcPlane.updateValues();
            }

            element.render();
        }

        /// <summary>
        /// Verifica si hay que dibujar el piso y el techo
        /// </summary>
        private void ActivateRoofAndFloor()
        {
            if (this.Input.keyPressed(Microsoft.DirectX.DirectInput.Key.F))
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
                            new NamedParameter("positionEye", this.Container.Resolve<IVector3Factory>().CreateVector3(5, 5, 5)),
                            new NamedParameter("moveSpeed", 50),
                            new NamedParameter("jumpSpeed", 50),
                            new NamedParameter("input", this.Input));

        /// <summary>
        /// Inicializa la camara
        /// </summary>
        private void InitScenario() => this.ScenarioElements = this.Container.Resolve<IScenarioCreator>().CreateScenario(this.MediaDir, this.Container);

        /// <summary>
        /// Dibujar las instrucciones del juego
        /// </summary>
        private void RenderInstructions()
        {
            this.DrawText.drawText("Presione F para dibujar/eliminar el techo y el piso", 0, 20, Color.OrangeRed);
            this.DrawText.drawText("Presione WSAD para moverse", 0, 40, Color.OrangeRed);
            this.DrawText.drawText("Mantenga presionado el boton izquierdo del mouse para mover la camara", 0, 60, Color.OrangeRed);
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
                    RenderElement(element));

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
                    RenderElement(element));

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
    }
}