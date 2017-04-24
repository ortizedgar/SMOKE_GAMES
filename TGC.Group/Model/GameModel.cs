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

            if (this.ElapsedTime >= 1 / 30)
            {
                ActivateRoofAndFloor();
                ActivateBoundingBox();
                UpdateLights();
            }
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
            this.LightMesh = TgcBox.fromSize(this.Vector3Factory.CreateVector3(1, 1, 1));
            this.LightMesh.AutoTransformEnable = true;
            this.LightMesh.Position = this.Camara.Position;
            this.LightMesh.Color = Color.Red;
            this.LightMesh.Enabled = true;
        }

        /// <summary>
        /// Inicializa la camara
        /// </summary>
        private void InitScenario() => this.ScenarioElements = this.Container.Resolve<IScenarioCreator>().CreateScenario(this.MediaDir, this.Container);

        /// <summary>
        /// Dibuja los elementos
        /// </summary>
        /// <param name="element"></param>
        private void RenderElements(TgcMesh element)
        {
            SetMeshEffect(element);

            if (this.RenderBoundingBox)
            {
                element.BoundingBox.render();
            }

            element.render();
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
            this.DrawText.drawText("Presione LShift para activar/desactivar la iluminacion", 0, 120, Color.OrangeRed);
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
        /// Configura el efecto del mesh
        /// </summary>
        /// <param name="mesh"></param>
        private void SetMeshEffect(TgcMesh mesh)
        {
            this.LightIntensity = this.LightIntensity > 0.25f ? this.LightIntensity - this.LightIntensityVariation : 0.25f;
            mesh.Technique = TgcShaders.Instance.getTgcMeshTechnique(mesh.RenderType);
            mesh.Effect = TgcShaders.Instance.TgcMeshShader;
            if (this.LightMesh.Enabled)
            {
                mesh.Effect = TgcShaders.Instance.TgcMeshPointLightShader;
                mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
                mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.White));
                mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
                mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.White));
                mesh.Effect.SetValue("materialSpecularExp", 1f);
                mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(this.Camara.Position));
                mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(this.Camara.Position));
                mesh.Effect.SetValue("lightColor", ColorValue.FromColor(this.LightMesh.Color));
                mesh.Effect.SetValue("lightIntensity", this.LightIntensity);
                mesh.Effect.SetValue("lightAttenuation", 0.5f);
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
                this.LightMesh.Color = Color.Red;
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

            if (this.Input.keyPressed(Microsoft.DirectX.DirectInput.Key.LeftShift))
            {
                this.LightMesh.Enabled = !this.LightMesh.Enabled;
            }

            this.LightMesh.updateValues();
        }
    }
}