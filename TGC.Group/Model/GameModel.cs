using System.Collections.Generic;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
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
        /// Lista de <see cref="TgcPlane"/> que contiene el piso del escenario
        /// </summary>
        private List<TgcPlane> floor;

        private List<TgcPlane> walls;

        private List<TgcPlane> roof;

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
        /// </summary>
        public override void Init()
        {
            var camaraLocation = vector3Factory.CreateVector3(5, 2, -1);
            Camara.SetCamera(camaraLocation, vector3Factory.CreateVector3(camaraLocation.X, camaraLocation.Y, camaraLocation.Z + 1));

            CreateScenario();
        }

        private void CreateScenario()
        {
            CreateFloor();

            CreateWalls();

            CreateRoof();
        }

        private void CreateRoof()
        {
            roof = new List<TgcPlane>();
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"\roof.jpg");

            for (var i = 0; i < 10; i++)
            {
                var roofElement = tgcPlaneFactory.CreateTgcPlane();
                roofElement.setTexture(texture);
                roofElement.Origin = vector3Factory.CreateVector3(0, 10, 10 * i);
                roofElement.Size = vector3Factory.CreateVector3(10, 10, 10);
                roofElement.Orientation = TgcPlane.Orientations.XZplane;
                roofElement.AutoAdjustUv = false;
                roofElement.UTile = 1;
                roofElement.VTile = 1;
                roof.Add(roofElement);
            }
        }

        private void CreateWalls()
        {
            walls = new List<TgcPlane>();
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"\wall.jpg");

            for (var i = 0; i < 10; i++)
            {
                if (i % 3 != 0)
                {
                    var wallElement = tgcPlaneFactory.CreateTgcPlane();
                    wallElement.setTexture(texture);
                    wallElement.Origin = vector3Factory.CreateVector3(0, 0, 10 * i);
                    wallElement.Size = vector3Factory.CreateVector3(10, 10, 10);
                    wallElement.Orientation = TgcPlane.Orientations.YZplane;
                    wallElement.AutoAdjustUv = false;
                    wallElement.UTile = 1;
                    wallElement.VTile = 1;
                    walls.Add(wallElement);
                }
            }

            for (var i = 0; i < 10; i++)
            {
                if (i != 5)
                {
                    var wallElement = tgcPlaneFactory.CreateTgcPlane();
                    wallElement.setTexture(texture);
                    wallElement.Origin = vector3Factory.CreateVector3(10, 0, 10 * i);
                    wallElement.Size = vector3Factory.CreateVector3(10, 10, 10);
                    wallElement.Orientation = TgcPlane.Orientations.YZplane;
                    wallElement.AutoAdjustUv = false;
                    wallElement.UTile = 1;
                    wallElement.VTile = 1;
                    walls.Add(wallElement);
                }
            }
        }

        private void CreateFloor()
        {
            floor = new List<TgcPlane>();
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"\floor.bmp");

            for (var i = 0; i < 10; i++)
            {
                var floorElement = tgcPlaneFactory.CreateTgcPlane();
                floorElement.setTexture(texture);
                floorElement.Origin = vector3Factory.CreateVector3(0, 0, 10 * i);
                floorElement.Size = vector3Factory.CreateVector3(10, 10, 10);
                floorElement.Orientation = TgcPlane.Orientations.XZplane;
                floorElement.AutoAdjustUv = false;
                floorElement.UTile = 1;
                floorElement.VTile = 1;
                floor.Add(floorElement);
            }
        }

        /// <summary>
        ///     Se llama en cada frame.
        /// </summary>
        public override void Update()
        {
            PreUpdate();

            MoveCamara();
        }

        private void MoveCamara()
        {
            Vector3 cameraPosition;
            if (Input.keyDown(Key.W))
            {
                cameraPosition = vector3Factory.CreateVector3(Camara.Position.X, Camara.Position.Y, Camara.Position.Z + 10 * ElapsedTime);
                Camara.SetCamera(cameraPosition, vector3Factory.CreateVector3(Camara.Position.X, Camara.Position.Y, Camara.Position.Z + 1));
            }
            if (Input.keyDown(Key.S))
            {
                cameraPosition = vector3Factory.CreateVector3(Camara.Position.X, Camara.Position.Y, Camara.Position.Z - 10 * ElapsedTime);
                Camara.SetCamera(cameraPosition, vector3Factory.CreateVector3(Camara.Position.X, Camara.Position.Y, Camara.Position.Z + 1));
            }
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        /// </summary>
        public override void Render()
        {
            // Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            // Renderizar el piso
            foreach (var floorElement in floor)
            {
                floorElement.updateValues();
                floorElement.render();
            }

            // Renderizar las paredes
            foreach (var wallElement in walls)
            {
                wallElement.updateValues();
                wallElement.render();
            }

            // Renderizar el techo
            foreach (var roofElement in roof)
            {
                roofElement.updateValues();
                roofElement.render();
            }

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
            // Renderizar el piso
            foreach (var floorElement in floor)
            {
                floorElement.dispose();
            }
        }
    }
}