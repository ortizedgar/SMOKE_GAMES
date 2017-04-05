using System;
using System.Collections.Generic;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Group.Interfaces;

namespace TGC.Group.Model
{
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
        public GameModel(string mediaDir, string shadersDir, ITgcPlaneFactory tgcPlaneFactory, IVector3Factory vector3Factory, IScenarioCreator scenarioCreator) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
            TgcPlaneFactory = tgcPlaneFactory;
            Vector3Factory = vector3Factory;
            ScenarioCreator = scenarioCreator;
        }

        /// <summary>
        /// Tamaño del plano de las paredes, techo y piso
        /// </summary>
        private float PlaneSize { get; } = 10;

        /// <summary>
        /// Creador de escenario
        /// </summary>
        private IScenarioCreator ScenarioCreator { get; set; }

        /// <summary>
        /// Lista de <see cref="IRenderObject"/> que contiene las paredes, pisos, techos y objetos del escenario
        /// </summary>
        private List<Tuple<string, List<IRenderObject>>> ScenarioElements { get; set; }

        /// <summary>
        /// Fabrica de <see cref="TgcPlane"/>
        /// </summary>
        private ITgcPlaneFactory TgcPlaneFactory { get; set; }

        /// <summary>
        /// Fabrica de <see cref="Vector3"/>
        /// </summary>
        private IVector3Factory Vector3Factory { get; set; }

        /// <summary>
        /// Indica si el techo y el piso deben renderizarse
        /// </summary>
        private bool RenderFloorAndRoof { get; set; } = false;

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            // Limpiar el escenario
            foreach (var scenarioElement in ScenarioElements)
            {
                foreach (var element in scenarioElement.Item2)
                {
                    element.dispose();
                }
            }
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        /// </summary>
        public override void Init()
        {
            Camara = new TgcFpsCamera(Vector3Factory.CreateVector3(5, 5, 5), 10, 50, Input);

            ScenarioElements = ScenarioCreator.CreateScenario(MediaDir, Vector3Factory, TgcPlaneFactory, PlaneSize);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        /// </summary>
        public override void Render()
        {
            // Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            // Renderizar el escenario
            foreach (var scenarioElement in ScenarioElements)
            {
                foreach (var element in scenarioElement.Item2)
                {
                    if (element is TgcPlane)
                    {
                        ((TgcPlane)element).updateValues();
                    }

                    if (RenderFloorAndRoof || !(scenarioElement.Item1.Equals("Floor") || scenarioElement.Item1.Equals("Roof")))
                    {
                        element.render();
                    }
                }
            }

            // Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        /// <summary>
        ///     Se llama en cada frame.
        /// </summary>
        public override void Update()
        {
            PreUpdate();

            if (Input.keyPressed(Key.F))
            {
                RenderFloorAndRoof = !RenderFloorAndRoof;
            }
        }
    }
}