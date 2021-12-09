using CGALabs_N6_Edition.Models;
using CGALabs_N6_Edition.Rendering.Light;
using System.Numerics;

namespace CGALabs_N6_Edition.Rendering.Drawing
{
    public class TextureRasterizer : Rasterizer
    {
        private PhongLight Light { get; set; }

        private int Width => _bitmap.Width;

        private int Height => _bitmap.Height;

        public TextureRasterizer(int width, int height)
        {
            _bitmap = new FastBitmap(width, height);
            ZBuffer = new ZBuffer(_bitmap.Width, _bitmap.Height);
            Light = new PhongLight(ActiveColor, Color.Wheat, Color.Azure);
        }

        public Bitmap GetBitmap(
            List<Vector3> windowVertices,
            VisualizationModel model,
            Vector3 lightVector,
            Vector3 viewVector
        )
        {
            var width = Width;
            var height = Height;
            _bitmap = new FastBitmap(width, height);
            ZBuffer = new ZBuffer(_bitmap.Width, _bitmap.Height);

            this._windowVertices = windowVertices;
            this.VisualizationModel = model;

            RasterizePolygons(lightVector, viewVector);


            return _bitmap.Bitmap;
        }

        private void RasterizePolygons(Vector3 lightVector, Vector3 viewVector)
        {
            var polygonsList = VisualizationModel.Polygons;

            polygonsList.AsParallel().ForAll(polygon =>
            {
                if (IsPolygonVisible(polygon))
                {
                    RasterizePolygon(polygon, lightVector, viewVector);
                }
            });
        }

        private void RasterizePolygon(IReadOnlyList<Vector3> vertexIndexes, Vector3 lightVector, Vector3 viewVector)
        {
            var sidesList = new List<Pixel>();

            for (var i = 0; i < vertexIndexes.Count - 1; i++)
            {
                DrawLine(i, i + 1, vertexIndexes, sidesList, lightVector, viewVector);
            }

            DrawLine(vertexIndexes.Count - 1, 0, vertexIndexes, sidesList, lightVector, viewVector);

            RasterizePixels(sidesList, lightVector, viewVector);
        }

        private void DrawLine(
            int from,
            int to,
            IReadOnlyList<Vector3> indexes,
            ICollection<Pixel> sidesList,
            Vector3 lightVector,
            Vector3 viewVector)
        {
            var indexFrom = (int)indexes[from].X - 1;
            var indexTo = (int)indexes[to].X - 1;

            var indexNormalFrom = (int)indexes[from].Z - 1;
            var indexNormalTo = (int)indexes[to].Z - 1;

            var textureIndexFrom = (int)indexes[from].Y - 1;
            var textureIndexTo = (int)indexes[to].Y - 1;

            var vertexFrom = _windowVertices[indexFrom];
            var vertexTo = _windowVertices[indexTo];

            var pixelFrom = new Pixel()
            {
                Point = new Vector3(
                    (int)Math.Round(vertexFrom.X),
                    (int)Math.Round(vertexFrom.Y),
                    vertexFrom.Z
                ),
                Normal = VisualizationModel.Normals[indexNormalFrom],
                World = VisualizationModel.Vertexes[indexFrom],
                Texture = VisualizationModel.Textures[textureIndexFrom]
            };
            var pixelTo = new Pixel()
            {
                Point = new Vector3(
                    (int)Math.Round(vertexTo.X),
                    (int)Math.Round(vertexTo.Y),
                    vertexTo.Z
                ),
                Normal = VisualizationModel.Normals[indexNormalTo],
                World = VisualizationModel.Vertexes[indexTo],
                Texture = VisualizationModel.Textures[textureIndexTo]
            };

            var drawnPixels = LineCreator.CreateLinePoints(pixelFrom, pixelTo);

            foreach (var pixel in drawnPixels)
            {
                sidesList.Add(pixel);

                DrawPixel(pixel, lightVector, viewVector);
            }
        }

        private void RasterizePixels(List<Pixel> sidesList, Vector3 lightVector, Vector3 viewVector)
        {
            SearchMinAndMaxY(sidesList, out var minY, out var maxY);

            for (var y = minY + 1; y < maxY; y++)
            {
                SearchStartAndEndXByY(sidesList, y, out var pixelFrom, out var pixelTo);

                var drawnPixels = LineCreator.CreateLinePoints(pixelFrom, pixelTo);

                foreach (var pixel in drawnPixels)
                {
                    DrawPixel(pixel, lightVector, viewVector);
                }
            }
        }

        private void DrawPixel(Pixel pixel, Vector3 lightVector, Vector3 viewVector)
        {
            var point = pixel.Point;

            if (!(point.X > 0) || !(point.X < ZBuffer.Width) || !(point.Y > 0) || !(point.Y < ZBuffer.Height)) 
                return;

            if (!(point.Z <= ZBuffer[(int) point.X, (int) point.Y])) return;


            var world4 = pixel.World / pixel.World.W;
            var world3 = new Vector3(world4.X, world4.Y, world4.Z);

            var color = PhongLight.CalculatePixelColorForTexture(
                pixel,
                lightVector,
                viewVector - world3,
                VisualizationModel
            );

            ZBuffer[(int)point.X, (int)point.Y] = point.Z;
            _bitmap.SetPixel((int)point.X, (int)point.Y, color);
        }
    }
}