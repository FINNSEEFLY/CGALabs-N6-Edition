using CGALabs_N6_Edition.Interfaces;
using CGALabs_N6_Edition.Models;
using Microsoft.Extensions.Logging;
using System.Numerics;
using Timer = System.Windows.Forms.Timer;

namespace CGALabs_N6_Edition
{
    public partial class Form1 : Form
    {
        private readonly ILogger _logger;
        private readonly IObjectFileReader _objectFileReader;
        private GraphicsObject _graphicsObject;
        private MatrixTransformer _transformer;
        private BitmapDrawer _bitmapDrawer;
        private CameraManipulator _cameraManipulator;
        private WatchModel _watchModel;
        private readonly int _timerInterval = 6; // 6 - 144 FPS | 16 - 60 FPS | 33 - 30 FPS
        private readonly Timer _timer;
        private bool _isMouseDown = false;
        private Point _mousePosition = new(0, 0);

        private List<Vector3> _points = new();

        private string _formTitle = "CGA-LABS";
        // private const string CameraControl = "Camera control";
        // private const string ObjectControl = "Object control";

        public Form1()
        {
            InitializeComponent();
        }

        public Form1(ILogger<Form1> logger, IObjectFileReader objectFileReader)
        {
            _logger = logger;
            _objectFileReader = objectFileReader;
            InitializeComponent();

            _cameraManipulator = new CameraManipulator();
            _transformer = new MatrixTransformer(Size.Width, Size.Height);
            _bitmapDrawer = new BitmapDrawer(Size.Width, Size.Height);
            _timer = new Timer
            {
                Interval = _timerInterval,
                Enabled = false
            };

            _timer.Tick += Timer_Tick;

            // this.Text = $"{_formTitle} | {CameraControl}";
            this.Text = $"{_formTitle}";
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadObject();
            _watchModel = new WatchModel(_graphicsObject);
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();

            var startTime = DateTime.Now;

            _points = _transformer.Transform(_cameraManipulator.Camera, _watchModel);
            this.BackgroundImage = _bitmapDrawer.GetBitmap(_points, _watchModel);

            var timeForDrawing = (DateTime.Now - startTime).TotalMilliseconds;
            var interval = (int)(_timerInterval - timeForDrawing);
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

            _graphicsObject = _objectFileReader.GetGraphicsObject();


            MessageBox.Show("Object has been read");
        }

        private void UpdateSize()
        {
            _transformer.Height = Size.Height;
            _transformer.Width = Size.Width;

            _bitmapDrawer = new BitmapDrawer(Size.Width, Size.Height);
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
            var xOffset = e.X - _mousePosition.X;
            var yOffset = _mousePosition.Y - e.Y;
            SaveMousePosition(e);

            _cameraManipulator.RotateX(yOffset);
            _cameraManipulator.RotateY(xOffset);
        }

        private void SaveMousePosition(MouseEventArgs e)
        {
            _mousePosition.X = e.X;
            _mousePosition.Y = e.Y;
        }
    }
}