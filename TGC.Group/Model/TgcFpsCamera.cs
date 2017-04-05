using System.Drawing;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.Utils;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Camara en primera persona que utiliza matrices de rotacion, solo almacena las rotaciones en updown y costados.
    ///     Ref: http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series4/Mouse_camera.php
    ///     Autor: Rodrigo Garcia.
    /// </summary>
    public class TgcFpsCamera : TgcCamera
    {

        public TgcFpsCamera(TgcD3dInput input)
        {
            Input = input;
            PositionEye = new Vector3();
            MouseCenter = new Point(
                D3DDevice.Instance.Device.Viewport.Width / 2,
                D3DDevice.Instance.Device.Viewport.Height / 2);
            RotationSpeed = 0.1f;
            MovementSpeed = 500f;
            JumpSpeed = 500f;
            DirectionView = new Vector3(0, 0, -1);
            LeftrightRot = FastMath.PI_HALF;
            UpdownRot = -FastMath.PI / 10.0f;
            CameraRotation = Matrix.RotationX(UpdownRot) * Matrix.RotationY(LeftrightRot);
        }

        public TgcFpsCamera(Vector3 positionEye, TgcD3dInput input) : this(input)
        {
            PositionEye = positionEye;
        }

        public TgcFpsCamera(Vector3 positionEye, float moveSpeed, float jumpSpeed, TgcD3dInput input)
            : this(positionEye, input)
        {
            MovementSpeed = moveSpeed;
            JumpSpeed = jumpSpeed;
        }

        public TgcFpsCamera(Vector3 positionEye, float moveSpeed, float jumpSpeed, float rotationSpeed,
            TgcD3dInput input)
            : this(positionEye, moveSpeed, jumpSpeed, input)
        {
            RotationSpeed = rotationSpeed;
        }

        /// <summary>
        ///     Cuando se elimina esto hay que desbloquear la camera.
        /// </summary>
        ~TgcFpsCamera()
        {
            LockCam = false;
        }

        public float JumpSpeed { get; set; }

        public bool LockCam
        {
            get { return LockCamera; }
            set
            {
                if (!LockCamera && value)
                {
                    Cursor.Position = MouseCenter;

                    Cursor.Hide();
                }
                if (LockCamera && !value)
                    Cursor.Show();
                LockCamera = value;
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
            PositionEye = position;
            DirectionView = directionView;
        }

        public override void UpdateCamera(float elapsedTime)
        {
            var moveVector = new Vector3(0, 0, 0);
            // Forward
            if (Input.keyDown(Key.W))
            {
                moveVector += new Vector3(0, 0, -1) * MovementSpeed;
            }

            // Backward
            if (Input.keyDown(Key.S))
            {
                moveVector += new Vector3(0, 0, 1) * MovementSpeed;
            }

            // Strafe right
            if (Input.keyDown(Key.D))
            {
                moveVector += new Vector3(-1, 0, 0) * MovementSpeed;
            }

            // Strafe left
            if (Input.keyDown(Key.A))
            {
                moveVector += new Vector3(1, 0, 0) * MovementSpeed;
            }

            // Jump
            if (Input.keyDown(Key.Space))
            {
                moveVector += new Vector3(0, 1, 0) * JumpSpeed;
            }

            // Crouch
            if (Input.keyDown(Key.LeftControl))
            {
                moveVector += new Vector3(0, -1, 0) * JumpSpeed;
            }

            if (Input.keyPressed(Key.L) || Input.keyPressed(Key.Escape))
            {
                LockCam = !LockCamera;
            }

            // Solo rotar si se esta aprentando el boton izq del mouse
            if (LockCamera || Input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                LeftrightRot -= -Input.XposRelative * RotationSpeed;
                UpdownRot -= Input.YposRelative * RotationSpeed;

                // Se actualiza matrix de rotacion, para no hacer este calculo cada vez y solo cuando en verdad es necesario.
                CameraRotation = Matrix.RotationX(UpdownRot) * Matrix.RotationY(LeftrightRot);
            }

            if (LockCamera)
            {
                Cursor.Position = MouseCenter;
            }

            // Calculamos la nueva posicion del ojo segun la rotacion actual de la camara.
            var cameraRotatedPositionEye = Vector3.TransformNormal(moveVector * elapsedTime, CameraRotation);
            PositionEye += cameraRotatedPositionEye;

            // Calculamos el target de la camara, segun su direccion inicial y las rotaciones en screen space x,y.
            var cameraRotatedTarget = Vector3.TransformNormal(DirectionView, CameraRotation);
            var cameraFinalTarget = PositionEye + cameraRotatedTarget;

            var cameraOriginalUpVector = DEFAULT_UP_VECTOR;
            var cameraRotatedUpVector = Vector3.TransformNormal(cameraOriginalUpVector, CameraRotation);

            base.SetCamera(PositionEye, cameraFinalTarget, cameraRotatedUpVector);
        }
    }
}