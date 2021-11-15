using System.Numerics;

namespace CGALabs_N6_Edition
{
    public class CameraManipulator
    {
        public CameraModel Camera { get; private set; }

        private readonly float _sensitivity = 0.01f;

        public CameraManipulator()
        {
            Camera = new CameraModel(
                new Vector3(0, 0, 500),
                new Vector3(0, 0, 0),
                new Vector3(0, 1, 0),
                AdditionalMath.ConvertDegreeToRadians(60)
            );
        }

        public void RotateY(int xOffset)
        {
            Camera.Eye = Vector3.Transform(Camera.Eye, Matrix4x4.CreateRotationY(_sensitivity * -xOffset));
        }

        public void RotateX(int yOffset)
        {
            Camera.Eye = Vector3.Transform(Camera.Eye, Matrix4x4.CreateRotationX(_sensitivity * yOffset));
            Camera.Up = Vector3.Transform(Camera.Up, Matrix4x4.CreateRotationX(_sensitivity * yOffset));
        }

        public void RotateZ(int zOffset)
        {
            Camera.Eye = Vector3.Transform(Camera.Eye, Matrix4x4.CreateRotationZ(_sensitivity * zOffset));
        }
    }
}
