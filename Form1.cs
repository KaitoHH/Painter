using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace painter
{
    public partial class Form1 : Form
    {
        Paint pic;
        public Form1()
        {
            InitializeComponent();

        }

        //画图事件处理
        //***********************************************************************
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            pic.beginDraw(new Point(e.X, e.Y));
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            updateInfo(e.X, e.Y);
            updateState();
            if (e.Button == MouseButtons.Left)
            {
                pic.drawingProcess(new Point(e.X, e.Y));
            } else if (pic.mode == painter.Paint.MODE.ERASER || pic.mode == painter.Paint.MODE.DOTS)
            {
                pic.DotsMove(new Point(e.X, e.Y));
            }
            
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            pic.finshDraw();
            updateState();
        }
        //***********************************************************************

        //工具********************************************************************
        private void button1_Click(object sender, EventArgs e)
        {
            pic.mode = painter.Paint.MODE.LINES;
            updateInfo();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pic.mode = painter.Paint.MODE.DOTS;
            updateInfo();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            pic.mode = painter.Paint.MODE.CIRCLES;
            updateInfo();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            pic.mode = painter.Paint.MODE.RECTANGLE;
            updateInfo();
        }
        private void button7_Click(object sender, EventArgs e)
        {
            pic.mode = painter.Paint.MODE.FILL_CIRCLE;
            updateInfo();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            pic.mode = painter.Paint.MODE.FILL_RECTANGLE;
            updateInfo();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            pic.mode = painter.Paint.MODE.ERASER;
            updateInfo();
        }
        //***********************************************************************
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            pic.penWidth = trackBar1.Value;
            label1.Text = string.Format("线宽:{0,2}", trackBar1.Value);
        }


        //清屏按钮
        private void button5_Click(object sender, EventArgs e)
        {
            DialogResult ret = MessageBox.Show("保存当前画板？", "保存", MessageBoxButtons.YesNoCancel);
            if (ret == DialogResult.Yes)
            {
                pic.save();
            }else if (ret == DialogResult.Cancel)
            {
                return;
            }
            pic.clearPaint();
            updateState();
        }

        //选择颜色窗口
        private void button6_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = pic.penColor;
            colorDialog1.ShowDialog();
            pic.penColor = colorDialog1.Color;
            pictureBox2.BackColor = pic.penColor;
        }

        //撤销
        private void button10_Click(object sender, EventArgs e)
        {
            pic.op_undo();
            updateState();
        }

        //重做
        private void button11_Click(object sender, EventArgs e)
        {
            pic.op_redo();
            updateState();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pic = new Paint(pictureBox1.CreateGraphics(), pictureBox1.Width, pictureBox1.Height);
            pic.backColor = pictureBox1.BackColor;
            updateState();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
                pic.resize(pictureBox1.CreateGraphics(),pictureBox1.Width, pictureBox1.Height);
            updateInfo();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            pic.magiColor = checkBox1.Checked;
            trackBar2.Enabled = pic.magiColor;
        }

        //状态栏更新
        private void updateState()
        {
            button10.Enabled = pic.canUndo();
            button11.Enabled = pic.canRedo();
            pictureBox2.BackColor = pic.penColor;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            pic.magicX = trackBar2.Value;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //pic.rePaint();
        }
        private void updateInfo(int x = 0, int y = 0)
        {
            label2.Text = string.Format("当前坐标：({0,4},{1,4}) | 画布大小:({2,4},{3,4}) | 当前工具：{4}"
                                        , x, y,pictureBox1.Width,
                                        pictureBox1.Height,pic.mode);
        }

        //保存按钮
        private void button12_Click(object sender, EventArgs e)
        {
            pic.save();
        }
    }
}
