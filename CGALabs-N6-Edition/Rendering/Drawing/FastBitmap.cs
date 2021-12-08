using System.Drawing.Imaging;
using System.Numerics;
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

        public FastBitmap(string filepath)
        {
            var tmpBitmap = new Bitmap(filepath, true);
            Width = tmpBitmap.Width;
            Height = tmpBitmap.Height;
            Bits = new int[Width * Height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb,
                BitsHandle.AddrOfPinnedObject());
            var sourceData =
                tmpBitmap.LockBits(new Rectangle(0, 0, tmpBitmap.Width, tmpBitmap.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(sourceData.Scan0, Bits, 0, Bits.Length);
            tmpBitmap.UnlockBits(sourceData);
            tmpBitmap.Dispose();
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
            Bits[index] = color.ToArgb();
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
            var color = Color.FromArgb(Bits[index-1]);
            return color;
        }

        public void Dispose()
        {
            BitsHandle.Free();
        }

        public Vector3 GetPixelRGBVector(int x, int y)
        {
            /*if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return Vector3.Zero;
            }*/

            var color = GetPixel(x, y);
            return new Vector3(color.R, color.G, color.B);
        }

        public Vector3 Bilinear(float x, float y)
        {
            int x1 = (int) x;
            int y1 = (int) y;

            float deltaX = x - x1;
            float deltaY = y - y1;

            if (deltaX == 0 && deltaY == 0)
            {
                return GetPixelRGBVector(x1, y1);
            }

            if (deltaX == 0)
            {
                return (-deltaY + 1) * GetPixelRGBVector(x1, y1)
                       + deltaY * GetPixelRGBVector(x1, y1 + 1);
            }

            if (deltaY == 0)
            {
                return (-deltaX + 1) * GetPixelRGBVector(x1, y1)
                       + deltaX * GetPixelRGBVector(x1 + 1, y1);
            }

            Vector3 y1Vector = (-deltaX + 1) * GetPixelRGBVector(x1, y1)
                               + deltaX * GetPixelRGBVector(x1 + 1, y1);
            Vector3 y2Vector = (-deltaX + 1) * GetPixelRGBVector(x1, y1 + 1)
                               + deltaX * GetPixelRGBVector(x1 + 1, y1 + 1);
            return (-deltaX + 1) * y1Vector
                   + deltaX * y2Vector;
        }
    }
}