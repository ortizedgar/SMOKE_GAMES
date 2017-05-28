namespace TGC.Group.Model
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using BulletSharp;
    using Microsoft.DirectX;
    using Microsoft.DirectX.Direct3D;
    using Microsoft.DirectX.DirectInput;
    using TGC.Core.Camara;
    using TGC.Core.Direct3D;
    using TGC.Core.Geometry;
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

#pragma warning disable CC0057 // Unused parameters
        public TgcFpsCamera(Vector3 positionEye, TgcD3dInput input) : this(input) => PositionEye = positionEye;
#pragma warning restore CC0057 // Unused parameters

        public TgcFpsCamera(Vector3 positionEye, float moveSpeed, float jumpSpeed, TgcD3dInput input) : this(positionEye, input)
        {
            this.MovementSpeed = moveSpeed;
            this.JumpSpeed = jumpSpeed;
        }

        public TgcFpsCamera(Vector3 positionEye, float moveSpeed, float jumpSpeed, TgcD3dInput input, DiscreteDynamicsWorld dynamicsWorld) : this(positionEye, input)
        {
            this.MovementSpeed = moveSpeed;
            this.JumpSpeed = jumpSpeed;
            this.DynamicsWorld = dynamicsWorld;
            this.InitCharacter();
        }

#pragma warning disable CC0057 // Unused parameters
        public TgcFpsCamera(Vector3 positionEye, float moveSpeed, float jumpSpeed, float rotationSpeed, TgcD3dInput input) : this(positionEye, moveSpeed, jumpSpeed, input) => RotationSpeed = rotationSpeed;
#pragma warning restore CC0057 // Unused parameters

        /// <summary>
        ///     Cuando se elimina esto hay que desbloquear la camera.
        /// </summary>
        ~TgcFpsCamera()
        {
            this.LockCam = false;
        }

        /// <summary>
        /// Mesh del personaje
        /// </summary>
        public Tuple<TgcBox, RigidBody> Character { get; set; }

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

        /// <summary>
        /// Mundo dinamico
        /// </summary>
        private DiscreteDynamicsWorld DynamicsWorld { get; set; }

        /// <summary>
        /// Debug, indica camara libre o no
        /// </summary>
        private bool FreeCamera { get; } = true;

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

        public Vector3 LightDir { get; set; }

        /// <summary>
        /// Actualiza la camara
        /// </summary>
        /// <param name="elapsedTime">Tiempo transcurrido desde el frame anterior</param>
        public override void UpdateCamera(float elapsedTime)
        {
            this.LightDir = new Vector3(-(this.PositionEye.X - this.LookAt.X), -(this.PositionEye.Y - this.LookAt.Y), -(this.PositionEye.Z - this.LookAt.Z));
            this.LightDir.Normalize();
            if (!this.FreeCamera)
            {
                Cursor.Hide();
                this.LeftrightRot -= -this.Input.XposRelative * this.RotationSpeed;
                this.UpdownRot -= this.Input.YposRelative * this.RotationSpeed;

                //Se actualiza matrix de rotacion, para no hacer este calculo cada vez y solo cuando en verdad es necesario.
                this.CameraRotation = Matrix.RotationX(this.UpdownRot) * Matrix.RotationY(this.LeftrightRot);
                Cursor.Position = this.MouseCenter;

                // Correr
                this.MovementSpeed = this.Input.keyDown(Key.LeftShift) ? 10 : 5;

                var lookVector = new Vector3(this.PositionEye.X - this.LookAt.X, 0, this.PositionEye.Z - this.LookAt.Z) * this.MovementSpeed;
                var moveVector = new Vector3(0, 0, 0);
                var moving = false;

                // Forward
                if (this.Input.keyDown(Key.W))
                {
                    moveVector -= lookVector;
                    moving = true;
                }

                // Backward
                if (this.Input.keyDown(Key.S))
                {
                    moveVector += lookVector;
                    moving = true;
                }

                // Strafe right
                if (this.Input.keyDown(Key.D))
                {
                    moveVector += lookVector;
                    moveVector.TransformCoordinate(Matrix.RotationYawPitchRoll(Geometry.DegreeToRadian(-90), 0, 0));
                    moving = true;
                }

                // Strafe left
                if (this.Input.keyDown(Key.A))
                {
                    moveVector += lookVector;
                    moveVector.TransformCoordinate(Matrix.RotationYawPitchRoll(Geometry.DegreeToRadian(90), 0, 0));
                    moving = true;
                }

                if (moving)
                {
                    this.Character.Item2.LinearVelocity = new BulletSharp.Math.Vector3(moveVector.X, moveVector.Y, moveVector.Z);
                }
                else
                {
                    this.Character.Item2.LinearVelocity = new BulletSharp.Math.Vector3(0, 0, 0);
                }

                this.PositionEye = new Vector3(
                   this.Character.Item2.InterpolationWorldTransform.Origin.X,
                   this.Character.Item2.InterpolationWorldTransform.Origin.Y + 2,
                   this.Character.Item2.InterpolationWorldTransform.Origin.Z);

                // Calculamos el target de la camara, segun su direccion inicial y las rotaciones en screen space x, y.
                var cameraRotatedTarget = Vector3.TransformNormal(this.DirectionView, this.CameraRotation);
                var cameraFinalTarget = this.PositionEye + cameraRotatedTarget;

                var cameraOriginalUpVector = this.DEFAULT_UP_VECTOR;
                var cameraRotatedUpVector = Vector3.TransformNormal(cameraOriginalUpVector, this.CameraRotation);

                base.SetCamera(this.PositionEye, cameraFinalTarget, cameraRotatedUpVector);
            }
            else
            {
                var moveVector = new Vector3(0, 0, 0);

                //Forward
                if (this.Input.keyDown(Key.W))
                {
                    moveVector += new Vector3(0, 0, -1) * this.MovementSpeed;
                }

                //Backward
                if (this.Input.keyDown(Key.S))
                {
                    moveVector += new Vector3(0, 0, 1) * this.MovementSpeed;
                }

                //Strafe right
                if (this.Input.keyDown(Key.D))
                {
                    moveVector += new Vector3(-1, 0, 0) * this.MovementSpeed;
                }

                //Strafe left
                if (this.Input.keyDown(Key.A))
                {
                    moveVector += new Vector3(1, 0, 0) * this.MovementSpeed;
                }

                //Jump
                if (this.Input.keyDown(Key.Space))
                {
                    moveVector += new Vector3(0, 1, 0) * this.JumpSpeed;
                }

                //Crouch
                if (this.Input.keyDown(Key.LeftControl))
                {
                    moveVector += new Vector3(0, -1, 0) * this.JumpSpeed;
                }

                if (this.Input.keyPressed(Key.L) || this.Input.keyPressed(Key.Escape))
                {
                    this.LockCam = !this.LockCam;
                }

                //Solo rotar si se esta aprentando el boton izq del mouse
                if (this.LockCam || this.Input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                {
                    this.LeftrightRot -= -this.Input.XposRelative * this.RotationSpeed;
                    this.UpdownRot -= this.Input.YposRelative * this.RotationSpeed;

                    //Se actualiza matrix de rotacion, para no hacer este calculo cada vez y solo cuando en verdad es necesario.
                    this.CameraRotation = Matrix.RotationX(this.UpdownRot) * Matrix.RotationY(this.LeftrightRot);
                }

                if (this.LockCam)
                {
                    Cursor.Position = this.MouseCenter;
                }

                //Calculamos la nueva posicion del ojo segun la rotacion actual de la camara.
                var cameraRotatedPositionEye = Vector3.TransformNormal(moveVector * elapsedTime, this.CameraRotation);
                this.PositionEye += cameraRotatedPositionEye;

                //Calculamos el target de la camara, segun su direccion inicial y las rotaciones en screen space x,y.
                var cameraRotatedTarget = Vector3.TransformNormal(this.DirectionView, this.CameraRotation);
                var cameraFinalTarget = this.PositionEye + cameraRotatedTarget;

                var cameraOriginalUpVector = this.DEFAULT_UP_VECTOR;
                var cameraRotatedUpVector = Vector3.TransformNormal(cameraOriginalUpVector, this.CameraRotation);

                base.SetCamera(this.PositionEye, cameraFinalTarget, cameraRotatedUpVector);
            }
        }

        /// <summary>
        /// Inicializa el personaje
        /// </summary>
        private void InitCharacter()
        {
            var characterMesh = TgcBox.fromSize(this.PositionEye, new Vector3(2.5f, 5, 2.5f));
            var boundingBoxAxisRadius = characterMesh.BoundingBox.calculateAxisRadius();
            var capsuleShape = new CapsuleShape(boundingBoxAxisRadius.X, boundingBoxAxisRadius.Y);
            var position = characterMesh.Position;
            var transform = BulletSharp.Math.Matrix.Translation(position.X + boundingBoxAxisRadius.X, position.Y + boundingBoxAxisRadius.Y, position.Z + boundingBoxAxisRadius.Z);
            const float Mass = 6f;
            var rigidBody = new RigidBody(new RigidBodyConstructionInfo(Mass, new DefaultMotionState(transform), capsuleShape, capsuleShape.CalculateLocalInertia(Mass)));
            rigidBody.SetSleepingThresholds(0, 0);
            rigidBody.AngularFactor = new BulletSharp.Math.Vector3(0, 0, 0);
            this.DynamicsWorld.AddRigidBody(rigidBody);
            this.Character = Tuple.Create(characterMesh, rigidBody);
        }
    }
}