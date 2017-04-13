namespace TGC.Group.Form
{
    using System;
    using System.Threading;
    using System.Windows.Forms;
    using TGC.Core.Direct3D;
    using TGC.Core.Example;
    using TGC.Core.Input;
    using TGC.Core.Shaders;
    using TGC.Core.Sound;
    using TGC.Core.Textures;
    using TGC.Group.Model;

#pragma warning disable S110 // Inheritance tree of classes should not be too deep
    /// <summary>
    ///     GameForm es el formulario de entrada, el mismo invocara a nuestro modelo  que extiende TgcExample, e inicia el
    ///     render loop.
    /// </summary>
    public partial class GameForm : Form
#pragma warning restore S110 // Inheritance tree of classes should not be too deep
    {
        /// <summary>
        ///     Constructor de la ventana.
        /// </summary>
        public GameForm() => InitializeComponent();

        /// <summary>
        ///     Obtener o parar el estado del RenderLoop.
        /// </summary>
        private bool ApplicationRunning { get; set; }

        /// <summary>
        ///     Permite manejar el sonido.
        /// </summary>
        private TgcDirectSound DirectSound { get; set; }

        /// <summary>
        ///     Permite manejar los inputs de la computadora.
        /// </summary>
        private TgcD3dInput Input { get; set; }

        /// <summary>
        ///     Ejemplo del juego a correr
        /// </summary>
        private TgcExample Modelo { get; set; }

        /// <summary>
        ///     Indica si la aplicacion esta activa.
        ///     Busca si la ventana principal tiene foco o si alguna de sus hijas tiene.
        /// </summary>
        public bool ApplicationActive()
        {
            if (this.ContainsFocus)
            {
                return true;
            }

            foreach (var form in this.OwnedForms)
            {
                if (form.ContainsFocus)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Arranca a ejecutar un ejemplo.
        ///     Para el ejemplo anterior, si hay alguno.
        /// </summary>
        public void ExecuteModel()
        {
            // Ejecutar Init
            try
            {
                this.Modelo.ResetDefaultConfig();
                this.Modelo.DirectSound = this.DirectSound;
                this.Modelo.Input = this.Input;
                this.Modelo.Init();
                this.panel3D.Focus();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error en Init() del juego", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        ///     Inicio todos los objetos necesarios para cargar el ejemplo y directx.
        /// </summary>
        public void InitGraphics()
        {
            // Se inicio la aplicación
            this.ApplicationRunning = true;

            // Inicio Device
            D3DDevice.Instance.InitializeD3DDevice(this.panel3D);

            // Inicio inputs
            this.Input = new TgcD3dInput();
            this.Input.Initialize(this, this.panel3D);

            // Inicio sonido
            this.DirectSound = new TgcDirectSound();
            this.DirectSound.InitializeD3DDevice(this.panel3D);

            // Directorio actual de ejecución
            var currentDirectory = Environment.CurrentDirectory + "\\";

            // Cargar shaders del framework
            TgcShaders.Instance.loadCommonShaders(currentDirectory + Game.Default.ShadersDirectory);

            // Juego a ejecutar, si quisiéramos tener diferentes modelos aquí podemos cambiar la instancia e invocar a otra clase
            this.Modelo = new GameModel(
                currentDirectory + Game.Default.MediaDirectory,
                currentDirectory + Game.Default.ShadersDirectory,
                new IocConfigurator().BuildAutofacContainer());

            // Cargar juego
            ExecuteModel();
        }

        /// <summary>
        ///     Comienzo el loop del juego.
        /// </summary>
        public void InitRenderLoop()
        {
            while (this.ApplicationRunning)
            {
                // Renderizo si es que hay un ejemplo activo.
                if (this.Modelo != null)
                {
                    // Solo renderizamos si la aplicacion tiene foco, para no consumir recursos innecesarios.
                    if (ApplicationActive())
                    {
                        this.Modelo.Update();
                        this.Modelo.Render();
                    }
                    else
                    {
                        // Si no tenemos el foco, dormir cada tanto para no consumir gran cantidad de CPU.
                        Thread.Sleep(100);
                    }
                }

                // Process application messages.
                Application.DoEvents();
            }
        }

        /// <summary>
        ///     Finalizar aplicacion
        /// </summary>
        public void ShutDown()
        {
            this.ApplicationRunning = false;

            StopCurrentExample();

            // Liberar Device al finalizar la aplicacion
            D3DDevice.Instance.Dispose();
            TexturesPool.Instance.clearAll();
        }

        /// <summary>
        ///     Deja de ejecutar el ejemplo actual
        /// </summary>
        public void StopCurrentExample()
        {
            if (this.Modelo != null)
            {
                this.Modelo.Dispose();
                this.Modelo = null;
            }
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.ApplicationRunning)
            {
                ShutDown();
            }
        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            // Iniciar graficos.
            InitGraphics();

            // Titulo de la ventana principal.
            this.Text = this.Modelo.Name + " - " + this.Modelo.Description;

            // Focus panel3D.
            this.panel3D.Focus();

            // Inicio el ciclo de Render.
            InitRenderLoop();
        }
    }
}