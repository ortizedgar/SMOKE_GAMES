using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Group.Interfaces;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Survial horror game
    /// </summary>
    public class GameModel : TgcExample
    {
        /// <summary>
        /// Fabrica de <see cref="TgcPlane"/>
        /// </summary>
        private readonly ITgcPlaneFactory tgcPlaneFactory;

        /// <summary>
        /// Fabrica de <see cref="Vector3"/>
        /// </summary>
        private readonly IVector3Factory vector3Factory;

        /// <summary>
        /// Plano que contiene el piso del escenario
        /// </summary>
        private TgcPlane floor;

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
        /// <param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
        public GameModel(string mediaDir, string shadersDir, ITgcPlaneFactory tgcPlaneFactory, IVector3Factory vector3Factory) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
            this.tgcPlaneFactory = tgcPlaneFactory;
            this.vector3Factory = vector3Factory;
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        public override void Init()
        {
            floor = tgcPlaneFactory.CreateTgcPlane();
            floor.setTexture(TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"\stones.bmp"));
            floor.Origin = vector3Factory.CreateVector3(0, 0, 0);
            floor.Size = vector3Factory.CreateVector3(1000, 1000, 1000);
            floor.Orientation = TgcPlane.Orientations.XZplane;
            floor.AutoAdjustUv = false;
            floor.UTile = 1;
            floor.VTile = 1;

            Camara.SetCamera(vector3Factory.CreateVector3(500, 200, -100), vector3Factory.CreateVector3(500, 150, 0));
        }

        /// <summary>
        ///     Se llama en cada frame.
        /// </summary>
        public override void Update()
        {
            PreUpdate();
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        /// </summary>
        public override void Render()
        {
            // Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            // Actualizar valores de pared
            floor.updateValues();

            // Renderizar pared
            floor.render();

            // Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            floor.dispose();
        }
    }
}