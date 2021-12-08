using System.Numerics;


namespace CGALabs_N6_Edition.Models
{
    public class ParsedGraphicsObject
    {
        public string PublicName { get; set; } = "Default Object Name";
        public List<Vector4> VertexList { get; set; } = new();
        public List<Vector3> VertexTextureList { get; } = new();
        public List<Vector3> VertexNormalList { get; } = new();
        public List<List<Vector3>> PolygonalIndexes { get; } = new();
    }
}