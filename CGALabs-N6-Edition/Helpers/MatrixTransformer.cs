using System.Collections.Concurrent;
using System.Numerics;
using CGALabs_N6_Edition.Camera;
using CGALabs_N6_Edition.Models;

namespace CGALabs_N6_Edition.Helpers
{
    public class MatrixTransformer
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public MatrixTransformer(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public List<Vector3> ApplyTransformations(CameraModel camera, VisualizationModel model)
        {
            // Мировые координаты
            var worldMatrix = CreateWorldSpace(model);
            model.WorldMatrix = worldMatrix;

            // Координаты наблюдателя
            var viewMatrix = CreateViewSpace(camera);

            // Координаты перспективной проекции
            var projectionMatrix = CreatePerspectiveSpace(camera.Fov, Width, Height);

            // Матрица трансформации
            var transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

            // Координаты окна
            var vertexes = GetWindowSpace(transformMatrix, model.Vertexes);

            return vertexes;
        }

        private Matrix4x4 CreateWorldSpace(VisualizationModel model)
        {
            return Matrix4x4.CreateScale(model.Scale)
                   * CreateRotation(model.Rotation)
                   * CreateTranslation(model.Position);
        }

        private static Matrix4x4 CreateTranslation(Vector3 vector)
        {
            return Matrix4x4.CreateTranslation(vector);
        }

        private static Matrix4x4 CreateRotation(Vector3 vector)
        {
            return Matrix4x4.CreateRotationY(vector.Y)
                   * Matrix4x4.CreateRotationX(vector.X)
                   * Matrix4x4.CreateRotationZ(vector.Z);
        }

        private static Matrix4x4 CreateViewSpace(CameraModel camera)
        {
            return Matrix4x4.CreateLookAt(camera.Eye, camera.Target, camera.Up);
        }

        private static Matrix4x4 CreatePerspectiveSpace(float fov, float width, float height)
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(fov, width / height, 0.1f, 200.0f);
        }

        private static Matrix4x4 CreateViewPortSpace
        (
            float width,
            float height,
            float xMin = 0,
            float yMin = 0)
        {
            return new Matrix4x4(
                width / 2, 0, 0, 0,
                0, -height / 2, 0, 0,
                0, 0, 1, 0,
                xMin + width / 2, yMin + height / 2, 0, 1
            );
        }

        private List<Vector3> GetWindowSpace(Matrix4x4 transformMatrix, List<Vector4> vertexes)
        {
            var windowPoints = new Vector3[vertexes.Count];

            // Координаты в соответствии с шириной и высотой экрана
            var viewPortMatrix = CreateViewPortSpace(Width, Height);

            Parallel.ForEach(Partitioner.Create(0, vertexes.Count), range =>
            {
                for (var i = range.Item1; i < range.Item2; i++)
                {
                    var transformedPoint = Vector4.Transform(vertexes[i], transformMatrix);
                    transformedPoint /= transformedPoint.W;
                    var displayedPoint = Vector4.Transform(transformedPoint, viewPortMatrix);
                    windowPoints[i] = new Vector3(
                        displayedPoint.X,
                        displayedPoint.Y,
                        displayedPoint.Z
                    );
                }
            });

            return windowPoints.ToList();
        }
    }
}