using CGALabs_N6_Edition.Helpers;
using System.Numerics;

namespace CGALabs_N6_Edition.Camera
{
    public class CameraController
    {
        public Camera Camera { get; private set; }

        private const float Sensitivity = 0.01f;
        private const float ZoomCoefficient = 20;

        public CameraController()
        {
            Camera = new Camera(
                new Vector3(0, 0, 200),
                new Vector3(0, 0, 0),
                new Vector3(0, 1, 0),
                AdditionalMath.ConvertDegreeToRadians(60)
            );
        }

        public void RotateY(int xOffset)
        {
            Camera.Eye = Vector3.Transform(Camera.Eye, Matrix4x4.CreateRotationY(Sensitivity * -xOffset));
        }

        public void RotateX(int yOffset)
        {
            Camera.Eye = Vector3.Transform(Camera.Eye, Matrix4x4.CreateRotationX(Sensitivity * yOffset));
            Camera.Up = Vector3.Transform(Camera.Up, Matrix4x4.CreateRotationX(Sensitivity * yOffset));
        }

        public void Zoom(bool isNegative = false)
        {
            var newZValue = Camera.Eye.Z - ZoomCoefficient * (isNegative ? -1 : 1);
            Camera.Eye = new Vector3(Camera.Eye.X, Camera.Eye.Y, newZValue < 0 ? Camera.Eye.Z : newZValue);
        }
    }
}