namespace CGALabs_N6_Edition
{
    public static class LineDrawer
    {
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