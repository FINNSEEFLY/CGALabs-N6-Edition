using CGALabs_N6_Edition.Camera;
using CGALabs_N6_Edition.Helpers;
using CGALabs_N6_Edition.Interfaces;
using CGALabs_N6_Edition.Models;
using CGALabs_N6_Edition.Rendering.Drawing;
using CGALabs_N6_Edition.Rendering.Light;
using Microsoft.Extensions.Logging;
using System.Numerics;
using Timer = System.Windows.Forms.Timer;

namespace CGALabs_N6_Edition
{
    public partial class Form1 : Form
    {
        private readonly ILogger _logger;
        private readonly IObjectFileReader _objectFileReader;
        private ParsedGraphicsObject _parsedGraphicsObject;
        private MatrixTransformer _transformer;
        private Rasterizer _rasterizer;
        private readonly CameraController _cameraController;
        private VisualizationModel _visualizationModel;
        private readonly int _timerInterval = 6; // 6 - 144 FPS | 16 - 60 FPS | 33 - 30 FPS
        private readonly Timer _timer;
        private bool _isMouseDown = false;
        private Point _mousePosition = new(0, 0);

        private TextureRasterizer _textureRasterizer;

        private readonly LightController _lightController;

        private List<Vector3> _points = new();

        private string _formTitle = "CGA-LABS";
        private bool _isCameraControl = true;
        private const string CameraControl = "Camera control";
        private const string LightControl = "Light control";

        public Form1()
        {
            InitializeComponent();
        }

        public Form1(ILogger<Form1> logger, IObjectFileReader objectFileReader)
        {
            _logger = logger;
            _objectFileReader = objectFileReader;
            InitializeComponent();

            _cameraController = new CameraController();
            _lightController = new LightController();
            _transformer = new MatrixTransformer(Size.Width, Size.Height);

            _textureRasterizer = new TextureRasterizer(Size.Width, Size.Height);
            _timer = new Timer
            {
                Interval = _timerInterval,
                Enabled = false
            };

            _timer.Tick += Timer_Tick;

            this.Text = $"{_formTitle} | {CameraControl}";
            // this.Text = $"{_formTitle}";
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadObject();
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();

            var startTime = DateTime.Now;

            _points = _transformer.ApplyTransformations(_cameraController.Camera, _visualizationModel);

            this.BackgroundImage = _textureRasterizer.GetBitmap(
                _points,
                _visualizationModel,
                _lightController.LightSourcePosition,
                _cameraController.Camera.Eye
            );

            var timeForDrawing = (DateTime.Now - startTime).TotalMilliseconds;
            var interval = (int) (_timerInterval - timeForDrawing);
            _timer.Interval = interval <= 0 ? 1 : interval;

            _timer.Start();
        }


        private void LoadObject()
        {
            var openFileDialog = new OpenFileDialog
            {
                DefaultExt = ".obj",
                Filter = "Object Files (OBJ)|*.obj",
            };

            if (openFileDialog.ShowDialog() == DialogResult.Abort)
            {
                return;
            }

            var filePath = openFileDialog.FileName;


            try
            {
                _objectFileReader.SetObjectPath(filePath);
                _objectFileReader.LoadGraphicsObject();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"{ex.Message}|{ex.StackTrace}");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}|{ex.StackTrace}");
                return;
            }

            var directory = Path.GetDirectoryName(filePath);
            _parsedGraphicsObject = _objectFileReader.GetGraphicsObject();
            _visualizationModel = new VisualizationModel(_parsedGraphicsObject);
            LoadTextureFiles(directory);


            MessageBox.Show("Object has been read");
        }

        private void LoadTextureFiles(string dirPath)
        {
            var diffusePath = Directory.EnumerateFiles(dirPath, "*.diffuse").FirstOrDefault();
            var reflectPath = Directory.EnumerateFiles(dirPath, "*.reflect").FirstOrDefault();
            var normalPath = Directory.EnumerateFiles(dirPath, "*.normal").FirstOrDefault();

            _visualizationModel.DiffuseTexture =
                (diffusePath != null && File.Exists(diffusePath)) ? new FastBitmap(diffusePath) : null;

            _visualizationModel.NormalsTexture =
                (normalPath != null && File.Exists(normalPath)) ? new FastBitmap(normalPath) : null;

            _visualizationModel.ReflectionTexture = 
                (reflectPath != null && File.Exists(reflectPath)) ? new FastBitmap(reflectPath) : null;

        }

        private void UpdateSize()
        {
            _transformer.Height = Size.Height;
            _transformer.Width = Size.Width;

            // _bitmapDrawer = new BitmapDrawer(Size.Width, Size.Height);

            // _phongBitmapDrawer = new PhongBitmapDrawer(Size.Width, Size.Height);
            // _lambertBitmapDrawer = new LambertBitmapDrawer(Size.Width, Size.Height);
            _textureRasterizer = new TextureRasterizer(Size.Width, Size.Height);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            UpdateSize();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            _isMouseDown = true;
            SaveMousePosition(e);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            _isMouseDown = false;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMouseDown) return;
            if (_isCameraControl)
            {
                var xOffset = e.X - _mousePosition.X;
                var yOffset = _mousePosition.Y - e.Y;
                SaveMousePosition(e);

                _cameraController.RotateX(yOffset);
                _cameraController.RotateY(xOffset);
            }
            else
            {
                var xOffset = e.X - _mousePosition.X;
                var yOffset = _mousePosition.Y - e.Y;
                SaveMousePosition(e);

                _lightController.RotateX(yOffset);
                _lightController.RotateY(xOffset);
            }
        }

        private void SaveMousePosition(MouseEventArgs e)
        {
            _mousePosition.X = e.X;
            _mousePosition.Y = e.Y;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Q:
                {
                    _isCameraControl = !_isCameraControl;
                    var formMode = _isCameraControl ? CameraControl : LightControl;
                    this.Text = $"{_formTitle} | {formMode}";
                    break;
                }
                case Keys.W:
                {
                    _cameraController.Zoom();
                    break;
                }
                case Keys.S:
                {
                    _cameraController.Zoom(true);
                    break;
                }
            }
        }
    }
}