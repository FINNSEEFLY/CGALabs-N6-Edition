using System.Numerics;

namespace CGALabs_N6_Edition.Rendering.Light.Unused
{
    public class LambertLight
    {
        // Цвет рассеянного света
        private readonly Color objectColor;

        // Коэффициент рассеянного освещения
        private readonly float kd = 0.9f;

        public LambertLight(Color color)
        {
            objectColor = color;
        }

        public Color GetPointColor(Vector3 normalVector, Vector3 lightVector)
        {
            var normal = Vector3.Normalize(normalVector);
            var light = Vector3.Normalize(lightVector);

            double coefficient = Math.Max(
                Vector3.Dot(normal, light),
                0
            );
            var red =
                (Math.Round(objectColor.R * coefficient) <= 255)
                    ? (byte)Math.Round(objectColor.R * coefficient)
                    : (byte)255;
            var green =
                (Math.Round(objectColor.G * coefficient) <= 255)
                    ? (byte)Math.Round(objectColor.G * coefficient)
                    : (byte)255;
            var blue =
                (Math.Round(objectColor.B * coefficient) <= 255)
                    ? (byte)Math.Round(objectColor.B * coefficient)
                    : (byte)255;

            return Color.FromArgb(255, red, green, blue);
        }

        public static Color GetAverageColor(IEnumerable<Color> colors)
        {
            var sumR = 0;
            var sumG = 0;
            var sumB = 0;
            var sumA = 0;

            foreach (var color in colors)
            {
                sumR += color.R;
                sumG += color.G;
                sumB += color.B;
                sumA += color.A;
            }

            var count = colors.Count();

            var red = (byte)Math.Round((double)sumR / count);
            var green = (byte)Math.Round((double)sumG / count);
            var blue = (byte)Math.Round((double)sumB / count);
            var alpha = (byte)Math.Round((double)sumA / count);

            return Color.FromArgb(alpha, red, green, blue);
        }
    }
}
