namespace TGC.Group.Model
{
    using System.Drawing;
    using System.Windows.Forms;
    using Microsoft.DirectX;
    using Microsoft.DirectX.DirectInput;
    using TGC.Core.Camara;
    using TGC.Core.Direct3D;
    using TGC.Core.Input;
    using TGC.Core.Utils;

    /// <summary>
    ///     Camara en primera persona que utiliza matrices de rotacion, solo almacena las rotaciones en updown y costados.
    ///     Ref: http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series4/Mouse_camera.php
    ///     Autor: Rodrigo Garcia.
    /// </summary>
    public class TgcFpsCamera : TgcCamera
    {
        public TgcFpsCamera(TgcD3dInput input)
        {
            this.Input = input;
            this.PositionEye = new Vector3();
            this.MouseCenter = new Point(
                D3DDevice.Instance.Device.Viewport.Width / 2,
                D3DDevice.Instance.Device.Viewport.Height / 2);
            this.RotationSpeed = 0.1f;
            this.MovementSpeed = 500f;
            this.JumpSpeed = 500f;
            this.DirectionView = new Vector3(0, 0, -1);
            this.LeftrightRot = FastMath.PI_HALF;
            this.UpdownRot = -FastMath.PI / 10.0f;
            this.CameraRotation = Matrix.RotationX(this.UpdownRot) * Matrix.RotationY(this.LeftrightRot);
        }

        public TgcFpsCamera(Vector3 positionEye, TgcD3dInput input) : this(input) => PositionEye = positionEye;

        public TgcFpsCamera(Vector3 positionEye, float moveSpeed, float jumpSpeed, TgcD3dInput input) : this(positionEye, input)
        {
            this.MovementSpeed = moveSpeed;
            this.JumpSpeed = jumpSpeed;
        }

        public TgcFpsCamera(Vector3 positionEye, float moveSpeed, float jumpSpeed, float rotationSpeed, TgcD3dInput input) : this(positionEye, moveSpeed, jumpSpeed, input) => RotationSpeed = rotationSpeed;

        /// <summary>
        ///     Cuando se elimina esto hay que desbloquear la camera.
        /// </summary>
        ~TgcFpsCamera()
        {
            this.LockCam = false;
        }

        public float JumpSpeed { get; set; }

        public bool LockCam
        {
            get => this.LockCamera;
            set
            {
                if (!this.LockCamera && value)
                {
                    Cursor.Position = this.MouseCenter;

                    Cursor.Hide();
                }

                if (this.LockCamera && !value)
                {
                    Cursor.Show();
                }

                this.LockCamera = value;
            }
        }

        public float MovementSpeed { get; set; }

        public float RotationSpeed { get; set; }

        // Se mantiene la matriz rotacion para no hacer este calculo cada vez.
        private Matrix CameraRotation { get; set; }

        // Direction view se calcula a partir de donde se quiere ver con la camara inicialmente. por defecto se ve en -Z.
        private Vector3 DirectionView { get; set; }

        private TgcD3dInput Input { get; }

        // No hace falta la base ya que siempre es la misma, la base se arma segun las rotaciones de esto costados y updown.
        private float LeftrightRot { get; set; }

        private bool LockCamera { get; set; }

        // Centro de mouse 2D para ocultarlo.
        private Point MouseCenter { get; set; }
        private Vector3 PositionEye { get; set; }
        private float UpdownRot { get; set; }

        /// <summary>
        ///     se hace override para actualizar las posiones internas, estas seran utilizadas en el proximo update.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="directionView"> debe ser normalizado.</param>
        public override void SetCamera(Vector3 position, Vector3 directionView)
        {
            this.PositionEye = position;
            this.DirectionView = directionView;
        }

        public override void UpdateCamera(float elapsedTime)
        {
            var moveVector = new Vector3(0, 0, 0);
            
            // Forward
            if (this.Input.keyDown(Key.W))
            {
                moveVector += new Vector3(0, 0, -1) * this.MovementSpeed;
            }

            // Backward
            if (this.Input.keyDown(Key.S))
            {
                moveVector += new Vector3(0, 0, 1) * this.MovementSpeed;
            }

            // Strafe right
            if (this.Input.keyDown(Key.D))
            {
                moveVector += new Vector3(-1, 0, 0) * this.MovementSpeed;
            }

            // Strafe left
            if (this.Input.keyDown(Key.A))
            {
                moveVector += new Vector3(1, 0, 0) * this.MovementSpeed;
            }

            // Jump
            if (this.Input.keyDown(Key.Space))
            {
                moveVector += new Vector3(0, 1, 0) * this.JumpSpeed;
            }

            // Crouch
            if (this.Input.keyDown(Key.LeftControl))
            {
                moveVector += new Vector3(0, -1, 0) * this.JumpSpeed;
            }

            if (this.Input.keyPressed(Key.L) || this.Input.keyPressed(Key.Escape))
            {
                this.LockCam = !this.LockCamera;
            }

            // Solo rotar si se esta aprentando el boton izq del mouse
            if (this.LockCamera || this.Input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                this.LeftrightRot -= -this.Input.XposRelative * this.RotationSpeed;
                this.UpdownRot -= this.Input.YposRelative * this.RotationSpeed;

                // Se actualiza matrix de rotacion, para no hacer este calculo cada vez y solo cuando en verdad es necesario.
                this.CameraRotation = Matrix.RotationX(this.UpdownRot) * Matrix.RotationY(this.LeftrightRot);
            }

            if (this.LockCamera)
            {
                Cursor.Position = this.MouseCenter;
            }

            // Calculamos la nueva posicion del ojo segun la rotacion actual de la camara.
            var cameraRotatedPositionEye = Vector3.TransformNormal(moveVector * elapsedTime, this.CameraRotation);
            this.PositionEye += cameraRotatedPositionEye;

            // Calculamos el target de la camara, segun su direccion inicial y las rotaciones en screen space x,y.
            var cameraRotatedTarget = Vector3.TransformNormal(this.DirectionView, this.CameraRotation);
            var cameraFinalTarget = this.PositionEye + cameraRotatedTarget;

            var cameraOriginalUpVector = this.DEFAULT_UP_VECTOR;
            var cameraRotatedUpVector = Vector3.TransformNormal(cameraOriginalUpVector, this.CameraRotation);

            base.SetCamera(this.PositionEye, cameraFinalTarget, cameraRotatedUpVector);
        }
    }
}