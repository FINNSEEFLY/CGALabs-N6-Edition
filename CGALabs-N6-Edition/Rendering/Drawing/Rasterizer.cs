using System.Numerics;
using CGALabs_N6_Edition.Models;

namespace CGALabs_N6_Edition.Rendering.Drawing
{
    public class Rasterizer
    {
        protected FastBitmap _bitmap;
        protected List<Vector3> _windowVertices;
        protected readonly Color ActiveColor = Color.AliceBlue;
        protected ZBuffer ZBuffer { get; set; }

        private int _activeColorArgb;

        protected VisualizationModel VisualizationModel;

        private int Width => _bitmap.Width;
        private int Height => _bitmap.Height;

        protected bool IsPolygonVisible(List<Vector3> polygon)
        {
            var result = true;

            var normal = GetPolygonNormal(polygon);

            if (normal.Z >= 0)
            {
                result = false;
            }

            return result;
        }

        protected static void SearchMinAndMaxY(List<Pixel> sidesList, out int min, out int max)
        {
            var list = sidesList.OrderBy(x => (int)x.Point.Y).ToList();
            min = (int)list[0].Point.Y;
            max = (int)list[sidesList.Count - 1].Point.Y;
        }

        public Bitmap GetBitmap(List<Vector3> windowVertices, VisualizationModel visualizationModel)
        {
            var width = Width;
            var height = Height;
            _bitmap.Clear();
            _bitmap = new FastBitmap(width, height);

            _windowVertices = windowVertices;

            visualizationModel.Polygons.AsParallel().ForAll(DrawLines);

            return _bitmap.Bitmap;
        }

        protected static void SearchStartAndEndXByY(List<Pixel> sidesList, int y, out Pixel pixelFrom, out Pixel pixelTo)
        {
            if (sidesList == null) throw new ArgumentNullException(nameof(sidesList));
            var sameYList = sidesList
                .Where(x => (int)x.Point.Y == y)
                .OrderBy(x => (int)x.Point.X)
                .ToList();

            pixelFrom = sameYList[0];
            pixelTo = sameYList[^1];
        }

        private void DrawLines(List<Vector3> vertexIndexes)
        {
            for (var i = 0; i < vertexIndexes.Count - 1; i++)
            {
                DrawLine(i, i + 1, vertexIndexes);
            }

            DrawLine(0, vertexIndexes.Count - 1, vertexIndexes);
        }

        private void DrawLine(int from, int to, IReadOnlyList<Vector3> indexes)
        {
            var indexFrom = (int)indexes[from].X - 1;
            var indexTo = (int)indexes[to].X - 1;

            var vertexFrom = _windowVertices[indexFrom];
            var vertexTo = _windowVertices[indexTo];

            var pointFrom = GetPoint(vertexFrom);
            var pointTo = GetPoint(vertexTo);

            LineCreator.DrawLinePoints(pointFrom, pointTo, this);
        }

        public void DrawPoint(Point point)
        {
            if (point.X > 0
                && point.X < _bitmap.Width
                && point.Y > 0
                && point.Y < _bitmap.Height)
            {
                _bitmap.SetPixel(point.X, point.Y, _activeColorArgb);
            }
        }

        private static Point GetPoint(Vector3 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }

        private Vector3 GetPolygonNormal(IReadOnlyList<Vector3> Polygon)
        {
            var indexPoint1 = (int)Math.Round(Polygon[0].X - 1);
            var indexPoint2 = (int)Math.Round(Polygon[1].X - 1);
            var indexPoint3 = (int)Math.Round(Polygon[2].X - 1);

            var point1 = _windowVertices[indexPoint1];
            var point2 = _windowVertices[indexPoint2];
            var point3 = _windowVertices[indexPoint3];

            var vector1 = point2 - point1;
            var vector2 = point3 - point1;
            var vector1XYZ = new Vector3(vector1.X, vector1.Y, vector1.Z);
            var vector2XYZ = new Vector3(vector2.X, vector2.Y, vector2.Z);

            return Vector3.Normalize(Vector3.Cross(vector1XYZ, vector2XYZ));
        }
    }
}
