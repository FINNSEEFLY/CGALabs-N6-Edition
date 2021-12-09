using System.Numerics;

namespace CGALabs_N6_Edition.Camera
{
    public class CameraModel
    {
        public Vector3 Eye { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; }
        public float Fov { get; set; }

        public CameraModel(
            Vector3 eye,
            Vector3 target,
            Vector3 up,
            float fov)
        {
            Eye = eye;
            Target = target;
            Up = up;
            Fov = fov;
        }
    }
}
