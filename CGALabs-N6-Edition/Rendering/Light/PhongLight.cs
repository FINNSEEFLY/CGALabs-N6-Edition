using System.Numerics;

namespace CGALabs_N6_Edition
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
        private const float Ka = 0.3f;
        // Коэффициент рассеянного освещения
        private const float Kd = 0.9f;
        // Коэффициент зеркального освещения
        private const float Ks = 0.3f;

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

        public Color GetPointColorWithTexture(
           Vector3 normal,
           Vector3 light,
           Vector3 view,
           VisualizationModel model,
           Vector3 texture
       )
        {
            Vector3 normalVector = Vector3.Normalize(normal);
            Vector3 lightVector = Vector3.Normalize(light);
            Vector3 viewVector = Vector3.Normalize(view);

            float x = texture.X * model.DiffuseTexture.Width;
            float y = (1 - texture.Y) * model.DiffuseTexture.Height;


            x = AdditionalMath.Clamp(x, model.DiffuseTexture.Width);
            y = AdditionalMath.Clamp(y, model.DiffuseTexture.Height);
            if (x < 0 || y < 0)
            {
                return Color.FromArgb(255, 0, 0, 0);
            }

            Vector3 pointNormal;
            if (model.NormalsTexture != null)
            {
                pointNormal = model.NormalsTexture.Bilinear(x, y);
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

            Vector3 Ia = model.DiffuseTexture.Bilinear(x, y)
                * Ka;
            Vector3 Id = model.DiffuseTexture.Bilinear(x, y)
                * Kd
                * Math.Max(Vector3.Dot(pointNormal, lightVector), 0);
            Vector3 Is;

            if (model.ReflectionTexture != null)
            {
                Vector3 reflectionVector = Vector3.Normalize(
                    Vector3.Reflect(lightVector, pointNormal)
                );

                Is = model.ReflectionTexture.Bilinear(x, y)
                    * Ks
                    * (float)Math.Pow(
                        Math.Max(0, Vector3.Dot(reflectionVector, viewVector)),
                        Alpha
                    );
            }
            else
            {
                Is = Vector3.Zero;
            }

            Vector3 color = Ia + Id + Is;

            byte r = color.X <= 255
                ? (byte)color.X
                : (byte)255;
            byte g = color.Y <= 255
                ? (byte)color.Y
                : (byte)255;
            byte b = color.Z <= 255
                ? (byte)color.Z
                : (byte)255;

            return Color.FromArgb(255, r, g, b);
        }
    }
}
