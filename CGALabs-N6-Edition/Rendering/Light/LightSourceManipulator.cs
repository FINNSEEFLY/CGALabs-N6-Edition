using System.Numerics;

namespace CGALabs_N6_Edition
{
    public class LightSourceManipulator
    {
        public Vector3 LightSource { get; private set; }

        private const float Sensitivity = 0.1f;

        public LightSourceManipulator()
        {
            LightSource = new Vector3(0, 500, 0);
        }

        public void RotateY(int xOffset)
        {
            LightSource = Vector3.Transform(LightSource, Matrix4x4.CreateRotationY(Sensitivity * -xOffset));
        }

        public void RotateX(int yOffset)
        {
            LightSource = Vector3.Transform(LightSource, Matrix4x4.CreateRotationX(Sensitivity * yOffset));
        }
    }
}
