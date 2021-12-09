using System.Numerics;
using CGALabs_N6_Edition.Models;
using CGALabs_N6_Edition.Rendering.Light;
using CGALabs_N6_Edition.Rendering.Light.Unused;

namespace CGALabs_N6_Edition.Rendering.Drawing
{
    public class LambertRasterizer : Rasterizer
    {
        private LambertLight Light { get; set; }

        private int Width => _bitmap.Width;

        private int Height => _bitmap.Height;

        public LambertRasterizer(int width, int height)
        {
            _bitmap = new FastBitmap(width, height);
            ZBuffer = new ZBuffer(_bitmap.Width, _bitmap.Height);
            Light = new LambertLight(ActiveColor);
        }

        public Bitmap GetBitmap(List<Vector3> windowVertices, VisualizationModel model, Vector3 lightVector)
        {
            var width = Width;
            var height = Height;
            _bitmap = new FastBitmap(width, height);
            ZBuffer = new ZBuffer(_bitmap.Width, _bitmap.Height);

            this._windowVertices = windowVertices;
            this.VisualizationModel = model;

            DrawPixels(lightVector);

            return _bitmap.Bitmap;
        }

        private void DrawPixels(Vector3 lightVector)
        {
            var polygonsList = VisualizationModel.Polygons;

            polygonsList.AsParallel().ForAll(polygon =>
            {
                if (IsPolygonVisible(polygon))
                {
                    DrawPolygon(polygon, lightVector);
                }
            });
        }

        private void DrawPolygon(List<Vector3> vertexIndexes, Vector3 lightVector)
        {
            var sidesList = new List<Pixel>();
            var color = GetColorForPolygon(vertexIndexes, lightVector);

            for (var i = 0; i < vertexIndexes.Count - 1; i++)
            {
                DrawLine(i, i + 1, vertexIndexes, color, sidesList);
            }

            DrawLine(vertexIndexes.Count - 1, 0, vertexIndexes, color, sidesList);

            DrawPixelForRasterization(sidesList, color);
        }

        private Color GetColorForPolygon(List<Vector3> polygon, Vector3 lightVector)
        {
            var colors = polygon.Select(index => (int) index.Z - 1)
                                          .Select(normalIndex => Light
                                                                    .GetPointColor(VisualizationModel.Normals[normalIndex], 
                                                                                    lightVector))
                                          .ToList();

            return LambertLight.GetAverageColor(colors);
        }

        private void DrawLine(
            int from,
            int to,
            IReadOnlyList<Vector3> indexes,
            Color color,
            ICollection<Pixel> sidesList)
        {
            var indexFrom = (int) indexes[from].X - 1;
            var indexTo = (int) indexes[to].X - 1;

            var vertexFrom = _windowVertices[indexFrom];
            var vertexTo = _windowVertices[indexTo];

            var pixelFrom = new Pixel()
            {
                Point = new Vector3(
                    (int) Math.Round(vertexFrom.X),
                    (int) Math.Round(vertexFrom.Y),
                    vertexFrom.Z)
            };
            var pixelTo = new Pixel()
            {
                Point = new Vector3(
                    (int) Math.Round(vertexTo.X),
                    (int) Math.Round(vertexTo.Y),
                    vertexTo.Z)
            };

            var drawnPixels = LineCreator.DrawLinePoints(pixelFrom, pixelTo);

            foreach (var pixel in drawnPixels)
            {
                sidesList.Add(pixel);

                DrawPixel(pixel, color);
            }
        }

        private void DrawPixelForRasterization(List<Pixel> sidesList, Color color)
        {
            SearchMinAndMaxY(sidesList, out var minY, out var maxY);

            for (var y = minY + 1; y < maxY; y++)
            {
                SearchStartAndEndXByY(sidesList, y, out var pixelFrom, out var pixelTo);

                var drawnPixels = LineCreator.DrawLinePoints(pixelFrom, pixelTo);

                foreach (var pixel in drawnPixels)
                {
                    DrawPixel(pixel, color);
                }
            }
        }

        private void DrawPixel(Pixel pixel, Color color)
        {
            var point = pixel.Point;

            if (point.X > 0
                && point.X < ZBuffer.Width
                && point.Y > 0
                && point.Y < ZBuffer.Height)
            {
                if (point.Z <= ZBuffer[(int) point.X, (int) point.Y])
                {
                    ZBuffer[(int) point.X, (int) point.Y] = point.Z;
                    _bitmap.SetPixel((int) point.X, (int) point.Y, color);
                }
            }
        }
    }
}