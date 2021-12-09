using System.Collections.Concurrent;
using System.Numerics;
using CGALabs_N6_Edition.Models;
using CGALabs_N6_Edition.Rendering.Light;

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
            Light = new PhongLight(ActiveColor, Color.Yellow, Color.DodgerBlue);
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

            DrawAllPixels(lightVector, viewVector);


            return _bitmap.Bitmap;
        }

        private void DrawAllPixels(Vector3 lightVector, Vector3 viewVector)
        {
            var polygonsList = VisualizationModel.Polygons;

            /*polygonsList.AsParallel().ForAll(polygon =>
            {
                if (IsPolygonVisible(polygon))
                {
                    DrawPoligon(polygon, lightVector, viewVector);
                }
            });*/

            Parallel.ForEach(
                Partitioner.Create(0, polygonsList.Count),
                range =>
                {
                    for (var i = range.Item1; i < range.Item2; i++)
                    {
                        var polygon = polygonsList[i];
                        if (IsPolygonVisible(polygon))
                        {
                            DrawPoligon(polygon, lightVector, viewVector);
                        }
                    }
                }
            );
        }

        protected void DrawPoligon(List<Vector3> vertexIndexes, Vector3 lightVector, Vector3 viewVector)
        {
            var sidesList = new List<Pixel>();

            for (var i = 0; i < vertexIndexes.Count - 1; i++)
            {
                DrawLine(i, i + 1, vertexIndexes, sidesList, lightVector, viewVector);
            }

            DrawLine(vertexIndexes.Count - 1, 0, vertexIndexes, sidesList, lightVector, viewVector);

            DrawPixelForRasterization(sidesList, lightVector, viewVector);
        }

        private void DrawLine(
            int from,
            int to,
            List<Vector3> indexes,
            List<Pixel> sidesList,
            Vector3 lightVector,
            Vector3 viewVector)
        {
            var indexFrom = (int) indexes[from].X - 1;
            var indexTo = (int) indexes[to].X - 1;

            var indexNormalFrom = (int) indexes[from].Z - 1;
            var indexNormalTo = (int) indexes[to].Z - 1;

            var textureIndexFrom = (int) indexes[from].Y - 1;
            var textureIndexTo = (int) indexes[to].Y - 1;

            var vertexFrom = _windowVertices[indexFrom];
            var vertexTo = _windowVertices[indexTo];

            var pixelFrom = new Pixel()
            {
                Point = new Vector3(
                    (int) Math.Round(vertexFrom.X),
                    (int) Math.Round(vertexFrom.Y),
                    vertexFrom.Z
                ),
                Normal = VisualizationModel.Normals[indexNormalFrom],
                World = VisualizationModel.Vertexes[indexFrom],
                Texture = VisualizationModel.Textures[textureIndexFrom]
            };
            var pixelTo = new Pixel()
            {
                Point = new Vector3(
                    (int) Math.Round(vertexTo.X),
                    (int) Math.Round(vertexTo.Y),
                    vertexTo.Z
                ),
                Normal = VisualizationModel.Normals[indexNormalTo],
                World = VisualizationModel.Vertexes[indexTo],
                Texture = VisualizationModel.Textures[textureIndexTo]
            };

            var drawnPixels = LineCreator.DrawLinePoints(pixelFrom, pixelTo);

            foreach (var pixel in drawnPixels)
            {
                sidesList.Add(pixel);

                DrawPixel(pixel, lightVector, viewVector);
            }
        }

        protected void DrawPixelForRasterization(List<Pixel> sidesList, Vector3 lightVector, Vector3 viewVector)
        {
            int minY, maxY;
            Pixel pixelFrom, pixelTo;
            SearchMinAndMaxY(sidesList, out minY, out maxY);

            for (var y = minY + 1; y < maxY; y++)
            {
                SearchStartAndEndXByY(sidesList, y, out pixelFrom, out pixelTo);

                var drawnPixels = LineCreator.DrawLinePoints(pixelFrom, pixelTo);

                foreach (var pixel in drawnPixels)
                {
                    DrawPixel(pixel, lightVector, viewVector);
                }
            }
        }

        protected void DrawPixel(Pixel pixel, Vector3 lightVector, Vector3 viewVector)
        {
            var point = pixel.Point;

            if (point.X > 0
                && point.X < ZBuffer.Width
                && point.Y > 0
                && point.Y < ZBuffer.Height)
            {
                if (point.Z <= ZBuffer[(int) point.X, (int) point.Y])
                {
                    var world4 = pixel.World / pixel.World.W;
                    var world3 = new Vector3(world4.X, world4.Y, world4.Z);

                    var color = PhongLight.GetPointColorWithTexture(
                        pixel.Normal,
                        lightVector,
                        viewVector - world3,
                        VisualizationModel,
                        pixel.Texture
                    );

                    ZBuffer[(int) point.X, (int) point.Y] = point.Z;
                    _bitmap.SetPixel((int) point.X, (int) point.Y, color);
                }
            }
        }
    }
}