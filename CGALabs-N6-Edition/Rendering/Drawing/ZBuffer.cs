using System.Collections;

namespace CGALabs_N6_Edition.Rendering.Drawing
{
    public class ZBuffer : IEnumerable<double>
    {
        private readonly double[] _buffer;

        public int Width { get; set; }
        public int Height { get; set; }

        public ZBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            _buffer = Enumerable
                .Repeat(double.MaxValue, width * height)
                .ToArray();
        }

        private int GetAddress(int x, int y)
        {
            return (y - 1) * Width + (x - 1);
        }

        public double this[int x, int y]
        {
            get
            {
                if (IsValidParams(x, y))
                {
                    var address = GetAddress(x, y);
                    return _buffer[address];
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
            set
            {
                if (IsValidParams(x, y))
                {
                    var address = GetAddress(x, y);
                    _buffer[address] = value;
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        public IEnumerator<double> GetEnumerator()
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    yield return this[x, y];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _buffer.GetEnumerator();
        }

        private bool IsValidParams(int x, int y) => !(x < 0 || x > Width || y < 0 || y > Height);
    }
}