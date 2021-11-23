using System.Numerics;

namespace CGALabs_N6_Edition
{
    internal static class ColorVectorExtension
    {
        public static Vector3 ToVector3(this Color color)
        {
            return new Vector3(color.R, color.G, color.B);
        }

        public static Vector4 ToVector4(this Color color)
        {
            return new Vector4(color.R, color.G, color.B, color.A);
        }
    }
}