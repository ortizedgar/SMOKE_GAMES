using System.Collections.Generic;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Group.Interfaces;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Survial horror game
    /// </summary>
    public class GameModel : TgcExample
    {
        private const short planeSize = 10;

        /// <summary>
        /// Creador de escenario
        /// </summary>
        private readonly IScenarioCreator scenarioCreator;

        /// <summary>
        /// Lista de <see cref="TgcPlane"/> que contiene las paredes, pisos y techos del escenario
        /// </summary>
        private List<List<TgcPlane>> scenarioElements;
        /// <summary>
        /// Fabrica de <see cref="TgcPlane"/>
        /// </summary>
        private readonly ITgcPlaneFactory tgcPlaneFactory;

        /// <summary>
        /// Fabrica de <see cref="Vector3"/>
        /// </summary>
        private readonly IVector3Factory vector3Factory;

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
            this.tgcPlaneFactory = tgcPlaneFactory;
            this.vector3Factory = vector3Factory;
            this.scenarioCreator = scenarioCreator;
        }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            // Limpiar el escenario
            foreach (var scenarioElement in scenarioElements)
            {
                foreach (var element in scenarioElement)
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
            Camara = new TgcRotationalCamera(Input);

            scenarioElements = scenarioCreator.CreateScenario(MediaDir, vector3Factory, tgcPlaneFactory, planeSize);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        /// </summary>
        public override void Render()
        {
            // Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            // Renderizar el escenario
            foreach (var scenarioElement in scenarioElements)
            {
                foreach (var element in scenarioElement)
                {
                    element.updateValues();
                    element.render();
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
        }
    }
}