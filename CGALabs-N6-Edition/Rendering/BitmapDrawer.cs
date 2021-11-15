using System.Numerics;

namespace CGALabs_N6_Edition
{
    public class BitmapDrawer
    {
        private FastBitmap _bitmap;
        private List<Vector3> _windowVertices;
        private readonly Color _activeColor = Color.Black;

        private int _activeColorArgb;

        public int Width => _bitmap.Width;
        public int Height => _bitmap.Height;

        public BitmapDrawer(int width, int height)
        {
            _bitmap = new FastBitmap(width, height);

            _activeColorArgb = _activeColor.ToArgb();
        }

        public Bitmap GetBitmap(List<Vector3> windowVertices, WatchModel watchModel)
        {
            var width = Width;
            var height = Height;
            _bitmap.Clear();
            _bitmap = new FastBitmap(width, height);

            _windowVertices = windowVertices;

            watchModel.Polygons.AsParallel().ForAll(DrawLines);

            return _bitmap.Bitmap;
        }

        private void DrawLines(List<Vector3> vertexIndexes)
        {
            for (var i = 0; i < vertexIndexes.Count - 1; i++)
            {
                DrawLine(i, i + 1, vertexIndexes);
            }

            DrawLine(0, vertexIndexes.Count - 1, vertexIndexes);
        }

        private void DrawLine(int from, int to, List<Vector3> indexes)
        {
            var indexFrom = (int)indexes[from].X - 1;
            var indexTo = (int)indexes[to].X - 1;

            var vertexFrom = _windowVertices[indexFrom];
            var vertexTo = _windowVertices[indexTo];

            var pointFrom = GetPoint(vertexFrom);
            var pointTo = GetPoint(vertexTo);

            LineDrawer.DrawLinePoints(pointFrom, pointTo, this);
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

        private Point GetPoint(Vector3 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }
    }
}
