namespace TGC.Group.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autofac;
    using Microsoft.DirectX.DirectInput;
    using TGC.Core.Example;
    using TGC.Core.Geometry;
    using TGC.Core.SceneLoader;

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
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
            Container = container;
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
        public override void Dispose()
        {
            // Limpiar el escenario
            DisposeScenario();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        /// </summary>
        public override void Init()
        {
            InitCamara();

            InitScenario();
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        /// </summary>
        public override void Render()
        {
            // Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            // Renderizar el escenario
            ScenarioRender();

            // Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        /// <summary>
        ///     Se llama en cada frame.
        /// </summary>
        public override void Update()
        {
            PreUpdate();

            ActivateRoofAndFloor();
        }

        /// <summary>
        /// Renderiza un elemento y lo actualiza si es un <see cref="TgcPlane"/>
        /// </summary>
        /// <param name="element"></param>
        private static void RenderElement(IRenderObject element)
        {
            if (element is TgcPlane)
            {
                ((TgcPlane)element).updateValues();
            }

            element.render();
        }

        /// <summary>
        /// Verifica si hay que dibujar el piso y el techo
        /// </summary>
        private void ActivateRoofAndFloor()
        {
            if (Input.keyPressed(Key.F))
            {
                RenderFloorAndRoof = !RenderFloorAndRoof;
            }
        }

        /// <summary>
        /// Libera la memoria utilizada para el escenario
        /// </summary>
        private void DisposeScenario()
        {
            ScenarioElements
                .SelectMany(
                    element =>
                        element.Item2)
                .AsParallel()
                .ToList()
                .ForEach(
                    element =>
                        element.dispose());
        }

        /// <summary>
        /// Inicializa el escenario
        /// </summary>
        private void InitCamara()
        {
            Camara = Container.Resolve<TgcFpsCamera>(
                            new NamedParameter("positionEye", Container.Resolve<Vector3Factory>().CreateVector3(5, 5, 5)),
                            new NamedParameter("moveSpeed", 50),
                            new NamedParameter("jumpSpeed", 50),
                            new NamedParameter("input", Input));
        }

        /// <summary>
        /// Inicializa la camara
        /// </summary>
        private void InitScenario()
        {
            ScenarioElements = Container.Resolve<ScenarioCreator>().CreateScenario(MediaDir, Container);
        }

        /// <summary>
        /// Renderiza el escenario con piso y techo
        /// </summary>
        private void RenderWithFloorAndRoof()
        {
            ScenarioElements
                .SelectMany(
                    element =>
                        element.Item2)
                .AsParallel()
                .ToList()
                .ForEach(
                    element =>
                    {
                        RenderElement(element);
                    });
        }

        /// <summary>
        /// Renderiza el escenario sin piso y techo
        /// </summary>
        private void RenderWithoutFloorAndRoof()
        {
            ScenarioElements
                .Where(
                    element =>
                        !element.Item1.Equals("Floor") && !element.Item1.Equals("Roof"))
                .AsParallel()
                .SelectMany(element => element.Item2)
                .ToList()
                .ForEach(
                    element =>
                    {
                        RenderElement(element);
                    });
        }

        /// <summary>
        /// Renderiza el escenario
        /// </summary>
        private void ScenarioRender()
        {
            if (RenderFloorAndRoof)
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