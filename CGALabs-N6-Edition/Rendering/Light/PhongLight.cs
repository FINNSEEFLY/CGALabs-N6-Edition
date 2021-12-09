using System.Numerics;
using CGALabs_N6_Edition.Helpers;
using CGALabs_N6_Edition.Models;

namespace CGALabs_N6_Edition.Rendering.Light
{
    public class PhongLight
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
        private const float AmbientCoefficient = 0.3f;
        // Коэффициент рассеянного освещения
        private const float DiffuseCoefficient = 0.9f;
        // Коэффициент зеркального освещения
        private const float ReflectiveCoefficient = 0.7f;

        public PhongLight(
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

            var Ia = AmbientCoefficient * _ambientColor.ToVector3();
            var Id = DiffuseCoefficient * Math.Max(Vector3.Dot(normalVector, lightVector), 0) * _objectColor.ToVector3();

            var reflectVector = Vector3.Normalize(Vector3.Reflect(-lightVector, normalVector));

            var Is = ReflectiveCoefficient
                     * (float)Math.Pow(
                         Math.Max(
                             0,
                             Vector3.Dot(reflectVector, viewVector)
                         ),
                         Alpha
                     )
                     * _lightColor.ToVector3();

            var color = Ia + Id + Is;

            var red = color.X <= 255 ? (byte)color.X : (byte)255;
            var green = color.Y <= 255 ? (byte)color.Y : (byte)255;
            var blue = color.Z <= 255 ? (byte)color.Z : (byte)255;

            return Color.FromArgb(255, red, green, blue);
        }

        public static Color GetPointColorWithTexture(
           Vector3 normal,
           Vector3 light,
           Vector3 view,
           VisualizationModel model,
           Vector3 texture
       )
        {
            var normalVector = Vector3.Normalize(normal);
            var lightVector = Vector3.Normalize(light);
            var viewVector = Vector3.Normalize(view);

            var x = texture.X * model.DiffuseTexture.Width;
            var y = (1 - texture.Y) * model.DiffuseTexture.Height;


            x = AdditionalMath.Clamp(x, model.DiffuseTexture.Width);
            y = AdditionalMath.Clamp(y, model.DiffuseTexture.Height);
            if (x < 0 || y < 0)
            {
                return Color.FromArgb(255, 0, 0, 0);
            }

            Vector3 pointNormal;
            if (model.NormalsTexture != null)
            {
                pointNormal = model.NormalsTexture.BilinearInterpolation(x, y);
                pointNormal.X -= 127.5f;
                pointNormal.Y -= 127.5f;
                pointNormal.Z -= 127.5f;
                pointNormal = Vector3.Normalize(pointNormal);
                pointNormal = Vector3.Normalize(
                    Vector3.TransformNormal(pointNormal, model.WorldMatrix)
                );
            }
            else
            {
                pointNormal = normalVector;
            }

            var interpolatedAmbient = model.DiffuseTexture.BilinearInterpolation(x, y) * AmbientCoefficient;
            var interpolatedDiffuse = model.DiffuseTexture.BilinearInterpolation(x, y)
                                      * DiffuseCoefficient * Math.Max(Vector3.Dot(pointNormal, lightVector), 0);
            Vector3 interpolatedReflection;

            if (model.ReflectionTexture != null)
            {
                var reflectionVector = Vector3.Normalize(
                    Vector3.Reflect(lightVector, pointNormal)
                );

                interpolatedReflection = model.ReflectionTexture.BilinearInterpolation(x, y)
                    * ReflectiveCoefficient
                    * (float)Math.Pow(Math.Max(0, Vector3.Dot(reflectionVector, viewVector)),
                        Alpha
                    );
            }
            else
            {
                interpolatedReflection = Vector3.Zero;
            }

            var color = interpolatedAmbient + interpolatedDiffuse + interpolatedReflection;

            var r = color.X <= 255 ? (byte)color.X : (byte)255;
            var g = color.Y <= 255 ? (byte)color.Y : (byte)255; 
            var b = color.Z <= 255 ? (byte)color.Z : (byte)255;

            return Color.FromArgb(255, r, g, b);
        }
    }
}
