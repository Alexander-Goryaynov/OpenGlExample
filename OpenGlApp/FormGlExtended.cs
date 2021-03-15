using SharpGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpGL.SceneGraph;
using System.Drawing.Imaging;

namespace OpenGlApp
{
    public partial class FormGlExtended : Form
    {
        private struct TextureStruct
        {
            public uint TextureValue;
            public double[,] X_YText;
            public double[,] CoordX_YText;
        }

        private const int gridLinesCount = 14;
        // длина ребра "1" - "2"
        private const float a = 4.0f;
        // высота ребра "5" - "6" над плоскостью xOy
        private const float b = 3.0f;
        // расстояние ребра "7" - "8" от плоскости yOz
        private const float c = 8.0f;
        private double camX;
        private double camY;
        // камера вращается вокруг точки 0,0,0 находясь на расстоянии camR
        private double camR;
        private double camAng;
        private const double degToRads = 0.0174;
        // шаг угла камеры
        private const double camAngDelta = 3.0;
        private Timer camTimer;
        // включить/выключить вращение камеры
        private bool isCamTimerActive;
        // меньше 100 не ставить, иначе картинка пропадает
        private const int camTimerInterval = 100;
        // обеспечивает возможность движения объекта вдоль оси Ox
        private double x;
        // шаг объекта при движении вдоль оси Ox
        private const double objStep = 1.0;
        // шаг радиуса камеры при зуме
        private const double zoomStep = 1.0;
        // включить/выключить освещение
        private bool isLightingEnabled;

        OpenGL gl;
        public FormGlExtended()
        {
            InitializeComponent();
            gl = openGLControl.OpenGL;
            camR = 10;
            camX = camR;
            camY = camR;
            camAng = 0;
            camTimer = new Timer();
            camTimer.Tick += new EventHandler(rotateCamera);
            camTimer.Interval = camTimerInterval;
            isCamTimerActive = false;
            isLightingEnabled = false;
            x = 0;
        }

        private void openGLControl_Load(object sender, EventArgs e)
        {
            openGLControl.Width = Width - 30;
            openGLControl.Height = Height - 100;
            // задаём проекционную матрицу
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            // загружаем матрицу в стек
            gl.LoadIdentity();
            // матрица проекции (угол обзора, соотношение сторон экрана,
            // расстояние до ближней плоскости, расстояние до дальней плоскости)
            gl.Perspective(90, 4 / 4, .1, 200);
            // матрица вида (положение камеры x0, y0, z0; в какую точку x1, y1, z1 она смотрит;
            // величины поворота "головы" камеры x2, y2, z2)
            gl.LookAt(camX, camY, 7, 0, 0, 0, 0, 0, 1);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
            // цвет фона
            gl.ClearColor(.1f, .1f, .3f, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            if (isLightingEnabled)
            {
                EnableLighting();
            }
        }

        private void openGLControl_OpenGLDraw(object sender, RenderEventArgs args)
        {
            DrawGrid();
            //DrawObject();
            //DrawSpheres();

            //DrawCylinder();
            displayTextureFromFile(@"C:\Users\Александр\Desktop\unknown.bmp");
        }

        private void DrawGrid()
        {
            gl.LineWidth(3.0f);
            gl.Begin(OpenGL.GL_LINES);
            for (int i = 0; i <= gridLinesCount; i++)
            {
                if (i == 0 || i == gridLinesCount / 2 || i == gridLinesCount)
                {
                    // белые линии сетки
                    gl.Color(1.0f, 1.0f, 1.0f, 1.0f);
                }
                else
                {
                    // серые линии сетки
                    gl.Color(0.5f, 0.5f, 0.5f, 1.0f);
                }
                // рисуем линии
                // линии параллельно Ox
                gl.Vertex(-14.0f + 2 * i, -14.0f, 0.0f);
                gl.Vertex(-14.0f + 2 * i, 14.0f, 0.0f);
                // линии параллельно Oy
                gl.Vertex(-14.0f, -14.0f + 2 * i, 0.0f);
                gl.Vertex(14.0f, -14.0f + 2 * i, 0.0f);
            }
            // ось Ox КРАСНАЯ
            gl.Color(1.0f, 0.0f, 0.0f, 1.0f);
            gl.Vertex(0.0f, 0.0f, 0.0f);
            gl.Vertex(10.0f, 0.0f, 0.0f);
            // ось Oy ЗЕЛЁНАЯ
            gl.Color(0.0f, 1.0f, 0.0f, 1.0f);
            gl.Vertex(0.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 10.0f, 0.0f);
            // ось Oz СИНЯЯ
            gl.Color(0.0f, 0.0f, 1.0f, 1.0f);
            gl.Vertex(0.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 0.0f, 10.0f);
            gl.End();
        }

        private void DrawSpheres()
        {
            // Зелёная сфера
            var i = gl.NewQuadric();
            gl.Color(Color.Green);
            gl.QuadricDrawStyle(i, OpenGL.GLU_LINE);
            gl.Sphere(i, 2, 25, 25);
            // Красная сфера
            i = gl.NewQuadric();
            gl.PushMatrix();
            gl.Translate(-5, 5, 5);
            gl.Rotate(90, 0, 0);
            gl.Color(Color.Red);
            gl.QuadricDrawStyle(i, OpenGL.GLU_FILL);
            gl.Sphere(i, 2, 25, 25);
            gl.PopMatrix();
            // Синяя сфера
            i = gl.NewQuadric();
            gl.PushMatrix();
            gl.Translate(5, 5, 5);
            gl.Rotate(90, 0, 0);
            gl.Color(Color.Blue);
            gl.QuadricDrawStyle(i, OpenGL.GLU_FILL);
            gl.Sphere(i, 2, 25, 25);
            gl.PopMatrix();
        }

        private void DrawCylinder()
        {
            var i = gl.NewQuadric();
            gl.PushMatrix();
            gl.Translate(1, -10, 1);
            gl.Rotate(50, 60, 0);
            gl.QuadricDrawStyle(i, OpenGL.GLU_FILL);
            gl.Cylinder(i, 5, 2, 10, 25, 25);
            gl.PopMatrix();
        }

        private void EnableLighting()
        {
            // цвет освещения по RGB
            float[] light0_diffuse3 = { 1f, 1f, 1f };
            float[] light0_direction3 = { 4.0f, 1f, 3.0f, 1.0f };
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT3);
            gl.Light(OpenGL.GL_LIGHT3, OpenGL.GL_DIFFUSE, light0_diffuse3);
            gl.Light(OpenGL.GL_LIGHT3, OpenGL.GL_POSITION, light0_direction3);
        }

        private void DisableLighting()
        {
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_LIGHT3);
        }

        private void displayTextureFromFile(string path)
        {
            TextureStruct Textures = new TextureStruct();
            Textures.CoordX_YText = new double[4, 3] { { 8, 0, 8 }, { 0, 0, 8 }, { 0, 0, 0 }, { 8, 0, 0 } };
            Textures.X_YText = new double[4, 2] { { 0, 0 }, { 1, 0 }, { 1, 1 }, { 0, 1 } };
            Bitmap BmpImage = new Bitmap(new Bitmap(path), 512, 512);
            Rectangle rect = new Rectangle(0, 0, BmpImage.Width, BmpImage.Height);
            BitmapData BmpData = BmpImage.LockBits(rect, ImageLockMode.ReadOnly, BmpImage.PixelFormat);
            gl.TexImage2D(
                OpenGL.GL_TEXTURE_2D,
                0,
                (int)OpenGL.GL_RGBA,
                BmpImage.Width,
                BmpImage.Height,
                0,
                OpenGL.GL_BGRA,
                OpenGL.GL_UNSIGNED_BYTE,
                BmpData.Scan0
            );
            BmpImage.UnlockBits(BmpData);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, Textures.TextureValue);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Begin(OpenGL.GL_QUADS);
            for (int i = 0; i < 4; i++)
            {
                gl.TexCoord(Textures.X_YText[i, 0], Textures.X_YText[i, 1]);
                gl.Vertex(Textures.CoordX_YText[i, 0], Textures.CoordX_YText[i, 1], Textures.CoordX_YText[i, 2]);
            }
            gl.End();
            gl.Disable(OpenGL.GL_TEXTURE_2D);
        }

        private void DrawObject()
        {
            gl.LineWidth(1.0f);
            // грань 1 красная
            gl.Begin(OpenGL.GL_POLYGON);
            gl.Color(255.0, 0.0, 0.0);
            gl.Vertex(.0f + x, .0f, .0f);
            gl.Vertex(.0f + x, a, .0f);
            gl.Vertex(.0f + x, a, a);
            gl.Vertex(.0f + x, .0f, a);
            gl.End();
            // грань 2 синяя
            gl.Begin(OpenGL.GL_POLYGON);
            gl.Color(0.0, 0.0, 255.0);
            gl.Vertex(.0f + x, .0f, .0f);
            gl.Vertex(.0f + x, a, .0f);
            gl.Vertex(c + x, a, a);
            gl.Vertex(c + x, .0f, a);
            gl.End();
            // грань 3 белая
            gl.Begin(OpenGL.GL_POLYGON);
            gl.Color(255.0, 255.0, 255.0);
            gl.Vertex(.0f + x, a, .0f);
            gl.Vertex(.0f + x, a, a);
            gl.Vertex(a + x, a, b);
            gl.Vertex(c + x, a, a);
            gl.End();
            // грань 4 жёлтая
            gl.Begin(OpenGL.GL_POLYGON);
            gl.Color(255.0, 128.0, 0.0);
            gl.Vertex(0 + x, 0, 0);
            gl.Vertex(0 + x, 0, a);
            gl.Vertex(a + x, 0, b);
            gl.Vertex(c + x, 0, a);
            gl.End();
            // грань 5 зелёная
            gl.Begin(OpenGL.GL_POLYGON);
            gl.Color(0.0, 255.0, 0.0);
            gl.Vertex(.0f + x, a, a);
            gl.Vertex(.0f + x, .0f, a);
            gl.Vertex(a + x, 0, b);
            gl.Vertex(a + x, a, b);
            gl.End();
            // грань 6 оранжевая
            gl.Begin(OpenGL.GL_POLYGON);
            gl.Color(0.0, 0.0, 0.0);
            gl.Vertex(a + x, 0, b);
            gl.Vertex(a + x, a, b);
            gl.Vertex(c + x, a, a);
            gl.Vertex(c + x, 0, a);
            gl.End();
        }

        private void rotateCamera(object sender, EventArgs e)
        {
            camAng += camAngDelta;
            camX = camR * Math.Cos(camAng * degToRads);
            camY = camR * Math.Sin(camAng * degToRads);
            openGLControl_Load(sender, e);
        }

        private void moveObject(Direction dir)
        {
            switch (dir)
            {
                case Direction.LEFT:
                    x += objStep;
                    break;
                case Direction.RIGHT:
                    x -= objStep;
                    break;
            }
        }

        private void scaleObject(Direction dir)
        {
            switch (dir)
            {
                case Direction.NEARER:
                    camR -= zoomStep;
                    break;
                case Direction.FARTHER:
                    camR += zoomStep;
                    break;
            }
            camX = camR * Math.Cos(camAng * degToRads);
            camY = camR * Math.Sin(camAng * degToRads);
        }

        private void openGLControl_Click(object sender, EventArgs e)
        {
            // Process right button click
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Right)
            {
                if (isLightingEnabled)
                {
                    DisableLighting();
                }
                else
                {
                    EnableLighting();
                }
                return;
            }
            if (isCamTimerActive)
            {
                camTimer.Stop();
            }
            else
            {
                camTimer.Start();
            }
            isCamTimerActive = !isCamTimerActive;
        }

        private void openGLControl_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    moveObject(Direction.LEFT);
                    break;
                case Keys.D:
                    moveObject(Direction.RIGHT);
                    break;
                case Keys.W:
                    scaleObject(Direction.NEARER);
                    break;
                case Keys.S:
                    scaleObject(Direction.FARTHER);
                    break;
            }
            openGLControl_Load(sender, e);
        }
    }
}
