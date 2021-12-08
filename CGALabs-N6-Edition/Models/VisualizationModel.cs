using CGALabs_N6_Edition.Models;
using System.Numerics;

namespace CGALabs_N6_Edition
{
    public class VisualizationModel
    {
        public readonly List<Vector4> Vertexes;
        public List<Vector3> Textures;
        public List<Vector3> Normals;
        public readonly List<List<Vector3>> Polygons;

        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public int Scale = 1;

        public VisualizationModel(ParsedGraphicsObject parsedGraphicsObject)
        {
            Position = Vector3.Zero;
            Rotation = Vector3.Zero;

            Vertexes = parsedGraphicsObject.VertexList;
            Textures = parsedGraphicsObject.VertexTextureList;
            Normals = parsedGraphicsObject.VertexNormalList;
            Polygons = parsedGraphicsObject.PolygonalIndexes;
            var max = GetMax();
            Scale = 500 / (max * 3);
        }

        private int GetMax()
        {
            var max = int.MinValue;
            foreach (var vertex in Vertexes)
            {
                var max1 = (int)Math.Max(vertex.X, vertex.Y);
                var max2 = (int)Math.Max(vertex.Z, vertex.W);
                max1 = (int)Math.Max(max1, max2);
                if (max1 > max)
                {
                    max = max1;
                }
            }
            return max;
        }
    }
}
