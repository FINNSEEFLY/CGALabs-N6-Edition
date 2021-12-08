using CGALabs_N6_Edition.Models;

namespace CGALabs_N6_Edition.Interfaces
{
    public interface IObjectFileReader : IDisposable
    {
        public void LoadGraphicsObject();
        public void SetObjectPath(string filePath);
        public ParsedGraphicsObject GetGraphicsObject();
    }
}