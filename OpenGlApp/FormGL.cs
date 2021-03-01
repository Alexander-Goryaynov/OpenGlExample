using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpGL;

namespace OpenGlApp
{
    public partial class FormGL : Form
    {
        OpenGL gl;
        public FormGL()
        {
            InitializeComponent();
            gl = openGLControl.OpenGL;
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
            gl.Perspective(80, 4 / 3, .1, 200);
            // матрица вида (положение камеры x0, y0, z0; в какую точку x1, y1, z1 она смотрит;
            // величины поворота "головы" камеры x2, y2, z2)
            gl.LookAt(10, 5, 10, 0, 1, 0, 0, 1, 0);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
            // цвет фона
            gl.ClearColor(.1f, .1f, .3f, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
        }

        private void openGLControl_OpenGLDraw(object sender, RenderEventArgs args)
        {
            gl.Begin(OpenGL.GL_POLYGON);
            gl.Color(255, 0, 0);
            gl.Vertex(0, 0, 2.5);
            gl.Vertex(5, 0, 2.5);
            gl.Vertex(5, 2.5, 2.5);
            gl.Vertex(0, 2.5, 2.5);
            gl.End();
        }
    }
}
