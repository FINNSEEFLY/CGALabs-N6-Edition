using CGALabs_N6_Edition.Interfaces;
using CGALabs_N6_Edition.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using static CGALabs_N6_Edition.Constants;

namespace CGALabs_N6_Edition
{
    public sealed class ObjectFileReader : IObjectFileReader
    {
        private bool _disposed = false;
        private bool _isPathSet = false;
        private string _path;
        private string[] _lines;
        private ParsedGraphicsObject _parsedGraphicsObject;
        private readonly ILogger _logger;
        private readonly CultureInfo _usCultureInfo = new("en-us");

        public ParsedGraphicsObject GetGraphicsObject()
        {
            if (_parsedGraphicsObject == null)
            {
                throw new ApplicationException("The object was not loaded");
            }

            return _parsedGraphicsObject;
        }

        public ObjectFileReader(ILogger<ObjectFileReader> logger)
        {
            _logger = logger;
        }

        public void SetObjectPath(string filePath)
        {
            if (_disposed) throw new ObjectDisposedException(ToString());
            if (!File.Exists(filePath)) throw new ArgumentException("File doesn't exist");

            _lines = File.ReadAllLines(filePath);

            _path = filePath;
            _isPathSet = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                // Освобождаем управляемые ресурсы
            }

            // освобождаем неуправляемые объекты
            _disposed = true;
        }

        ~ObjectFileReader()
        {
            Dispose(false);
        }

        private float ParseFloatUS(string floatString)
        {
            return float.Parse(floatString, _usCultureInfo);
        }

        private void ParseVertex(IReadOnlyList<string> parts, ParsedGraphicsObject parsedGraphicsObject)
        {
            parsedGraphicsObject.VertexList.Add(new Vector4(
                ParseFloatUS(parts[1]),
                ParseFloatUS(parts[2]),
                ParseFloatUS(parts[3]),
                parts.Count > 4 ? ParseFloatUS(parts[4]) : 1));
        }

        private void ParseVertexTexture(IReadOnlyList<string> parts, ParsedGraphicsObject parsedGraphicsObject)
        {
            parsedGraphicsObject.VertexTextureList.Add(new Vector3(
                ParseFloatUS(parts[1]),
                ParseFloatUS(parts[2]),
                parts.Count > 3 ? ParseFloatUS(parts[3]) : 0));
        }

        private void ParseVertexNormal(IReadOnlyList<string> parts, ParsedGraphicsObject parsedGraphicsObject)
        {
            parsedGraphicsObject.VertexNormalList.Add(new Vector3(
                ParseFloatUS(parts[1]),
                ParseFloatUS(parts[2]),
                ParseFloatUS(parts[3])));
        }

        private void ParsePolygonalFace(IReadOnlyList<string> parts, ParsedGraphicsObject parsedGraphicsObject)
        {
            var numOfVertex = parts.Count - 1;

            var element = new List<Vector3>();

            for (var i = 1; i <= numOfVertex; i++)
            {
                var polygonalParts = parts[i].Split("/");

                element.Add(new Vector3(
                    polygonalParts.Length > 0 && polygonalParts[0] != ""
                        ? int.Parse(polygonalParts[0])
                        : 0,
                    polygonalParts.Length > 1 && polygonalParts[1] != ""
                        ? int.Parse(polygonalParts[1])
                        : 0,
                    polygonalParts.Length > 2 && polygonalParts[2] != ""
                        ? int.Parse(polygonalParts[2])
                        : 0));

                parsedGraphicsObject.PolygonalIndexes.Add(element);
            }
        }

        public void LoadGraphicsObject()
        {
            if (_disposed) throw new ObjectDisposedException(ToString());
            if (!_isPathSet) throw new ApplicationException("File path must be setted up");

            _parsedGraphicsObject = new ParsedGraphicsObject
            {
                PublicName = Path.GetFileName(_path)
            };


            var startTime = DateTime.Now;

            foreach (var item in _lines)
            {
                var line = Regex.Replace(item, RegexPattern.Comment, "");

                if (line.Length == 0) continue;

                var parts = line.Split(new[] { ' ' }).Where(x => x != "").ToArray();

                if (parts.Length == 0) continue;

                switch (parts[0])
                {
                    case "v":
                        ParseVertex(parts, _parsedGraphicsObject);
                        break;
                    case "vt":
                        ParseVertexTexture(parts, _parsedGraphicsObject);
                        break;
                    case "vn":
                        ParseVertexNormal(parts, _parsedGraphicsObject);
                        break;
                    case "f":
                        ParsePolygonalFace(parts, _parsedGraphicsObject);
                        break;
                    default:
                        _logger.LogTrace($"{string.Join(' ', parts)} - were skipped from processing");
                        break;
                }
            }

            _logger.LogInformation(
                $"File {_parsedGraphicsObject.PublicName} parse has been completed in {(DateTime.Now - startTime).ToString("G")}");
        }
    }
}