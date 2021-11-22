using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CGALabs_N6_Edition
{
    public class FastBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        private int[] Bits { get; set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        public int BitmapSize => Bits.Length;

        private GCHandle BitsHandle { get; set; }

        public static FastBitmap FromBitmap(Bitmap bitmap)
        {
            var fastBitmap = new FastBitmap(bitmap.Width, bitmap.Height);
            for (var i = 0; i < bitmap.Width; i++)
            {
                for (var g = 0; g < bitmap.Height; g++)
                {
                    fastBitmap.Bits[g * bitmap.Width + i] = bitmap.GetPixel(i, g).ToArgb();
                }
            }

            return fastBitmap;
        }

        public FastBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new int[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb,
                BitsHandle.AddrOfPinnedObject());
        }

        public void Clear()
        {
            var backgroundColor = Color.White.ToArgb();
            for (var i = 0; i < Bits.Length; i++)
            {
                Bits[i] = backgroundColor;
            }
        }

        public void SetPixel(int index, Color color)
        {
            var col = color.ToArgb();
            Bits[index] = col;
        }

        public void SetPixel(int index, int color)
        {
            Bits[index] = color;
        }

        public void SetPixel(int x, int y, int color)
        {
            Bits[x + (y * Width)] = color;
        }

        public void SetPixel(int x, int y, Color color)
        {
            Bits[x + (y * Width)] = color.ToArgb();
        }

        public Color GetPixel(int x, int y)
        {
            var index = x + (y * Width);
            var color = Color.FromArgb(Bits[index]);
            return color;
        }

        public void Dispose()
        {
            BitsHandle.Free();
        }
    }
}