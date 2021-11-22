using System.Numerics;

namespace CGALabs_N6_Edition
{
    public static class LineDrawer
    {
        public static List<Pixel> DrawLinePoints(Pixel pixel1, Pixel pixel2)
        {
            var points = new List<Pixel>();

            var point1 = pixel1.Point;
            var point2 = pixel2.Point;

            var normal1 = pixel1.Normal;
            var world1 = pixel1.World;

            int signX = 1, signY = 1;

            var deltaX = Math.Abs((int)point2.X - (int)point1.X);
            var deltaY = Math.Abs((int)point2.Y - (int)point1.Y);

            var deltaZ = point2.Z - point1.Z;

            var deltaN = pixel2.Normal - pixel1.Normal;
            var deltaW = pixel2.World - pixel1.World;


            if (deltaX > deltaY)
            {
                deltaZ /= deltaX;
                deltaN /= deltaX;
                deltaW /= deltaX;
            }
            else
            {
                deltaZ /= deltaY;
                deltaN /= deltaY;
                deltaW /= deltaY;
            }

            if (point1.X > point2.X)
            {
                signX = -1;
            }
            if (point1.Y > point2.Y)
            {
                signY = -1;
            }

            var error = deltaX - deltaY;

            while (point1.X != point2.X || point1.Y != point2.Y)
            {
                points.Add(new Pixel()
                {
                    Point = new Vector3(point1.X, point1.Y, point1.Z),
                    Normal = normal1,
                    World = world1
                });

                var error2 = error * 2;

                if (error2 > -deltaY)
                {
                    error -= deltaY;
                    point1.X += signX;
                }
                if (error2 < deltaX)
                {
                    error += deltaX;
                    point1.Y += signY;
                }

                point1.Z += deltaZ;
                normal1 += deltaN;
                world1 += deltaW;
            }

            points.Add(new Pixel()
            {
                Point = new Vector3(point1.X, point1.Y, point1.Z),
                Normal = normal1,
                World = world1
            });

            return points;
        }
        public static void DrawLinePoints(Point point1, Point point2, BitmapDrawer bitmapDrawer)
        {
            var signX = 1;
            var signY = 1;

            var deltaX = Math.Abs(point2.X - point1.X);
            var deltaY = Math.Abs(point2.Y - point1.Y);

            if (point1.X > point2.X)
            {
                signX = -1;
            }

            if (point1.Y > point2.Y)
            {
                signY = -1;
            }

            var error = deltaX - deltaY;

            while (point1.X != point2.X || point1.Y != point2.Y)
            {
                bitmapDrawer.DrawPoint(new Point(point1.X, point1.Y));

                var error2 = error * 2;

                if (error2 > -deltaY)
                {
                    error -= deltaY;
                    point1.X += signX;
                }
                
                if (error2 >= deltaX) continue;
                error += deltaX;
                point1.Y += signY;
            }

            bitmapDrawer.DrawPoint(new Point(point2.X, point2.Y));
        }
    }
}