using System.Numerics;

namespace CGALabs_N6_Edition
{
    public static class AdditionalMath
    {
        public static float ConvertDegreeToRadians(float degree)
        {
            return (float)(degree * Math.PI / 180f);
        }

        public static float ConvertRadiansToDegree(float radians)
        {
            return (float)(radians * 180f / Math.PI);
        }

        public static float Angle(Vector3 v1, Vector3 v2)
        {
            return (float)Math.Acos(Cross(v1, v2) / (v1.Length() * v2.Length()));
        }

        public static float Cross(Vector3 v1, Vector3 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }
    }
}