using System;
using System.Drawing;
using System.Numerics;

namespace CGALabs_N6_Edition
{
    public class PhongLighting
    {
        // Цвет рассеянного света
        private readonly Color _objectColor;
        // Цвет зеркального света
        private readonly Color _lightColor;
        // Цвет фонового освещения
        private readonly Color _ambientColor;
        // Коэффициент блеска поверхности
        private const int Alpha = 32;

        // Коэффициент фонового освещения
        private const float Ka = 0.3f;
        // Коэффициент рассеянного освещения
        private const float Kd = 0.9f;
        // Коэффициент зеркального освещения
        private const float Ks = 0.3f; 

        public PhongLighting(
            Color objectColor,
            Color lightColor,
            Color ambientColor)
        {
            this._objectColor = objectColor;
            this._lightColor = lightColor;
            this._ambientColor = ambientColor;
        }

        public Color GetPointColor(Vector3 normal, Vector3 light, Vector3 view)
        {
            var normalVector = Vector3.Normalize(normal);
            var lightVector = Vector3.Normalize(light);
            var viewVector = Vector3.Normalize(view);

            var Ia = Ka * _ambientColor.ToVector3();
            var Id = Kd * Math.Max(Vector3.Dot(normalVector, lightVector), 0) * _objectColor.ToVector3();

            var reflectVector = Vector3.Normalize(Vector3.Reflect(-lightVector, normalVector));
            
            var Is = Ks
                     * (float)Math.Pow(
                         Math.Max(
                             0, 
                             Vector3.Dot(reflectVector, viewVector)
                         ), 
                         Alpha
                     )
                     * _lightColor.ToVector3();

            var color = Ia + Id + Is;

            var red = 
                color.X <= 255
                    ? (byte)color.X
                    : (byte)255;
            var green = 
                color.Y <= 255
                    ? (byte)color.Y
                    : (byte)255;
            var blue = 
                color.Z <= 255 
                    ? (byte)color.Z
                    : (byte)255;

            return Color.FromArgb(255, red, green, blue);
        }
    }
}
