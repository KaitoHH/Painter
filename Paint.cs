using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace painter
{
    /********************************************************************************

    ** 作者： Huang Hui

    ** 创始时间：2015-10-16

    ** 描述：

    **    封装了一个画图库，只需调入需要作为画板的Graphics类即可

    *********************************************************************************/

    class Paint
    {
        public enum MODE { ERASER, DOTS, LINES, CIRCLES, RECTANGLE, FILL_CIRCLE, FILL_RECTANGLE };
        private float _penWidth = 1;                        //画笔宽度
        private Color _penColor = Color.Black;              //画笔颜色
        private MODE _mode = MODE.DOTS;                     //当前绘画模式
        private Point _startPoint;                          //画笔起点
        private Color _backColor = Color.White;             //背景色
        private bool _magiColor = false;                    //随机颜色
        private bool _dColor = true;                        //随机颜色相关
        private int _magicX = 1;                            //随机颜色参数
        Random ran;
        Bitmap grph;                                        //画布
        Bitmap last;                                        //缓冲画布
        Graphics target;                                    //目标画板
        Stack undo;                                         //撤销
        Stack redo;                                         //重做

        //属性
        //****************************
        public float penWidth {
            get
            {
                return _penWidth;
            }
            set
            {
                _penWidth = value>0? value :_penWidth;
            }
        }
        public Color penColor
        {
            get
            {
                return _penColor;
            }
            set
            {
                _penColor = value;
            }
        }
        public MODE mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
            }
        }
        public Color backColor
        {
            get
            {
                return _backColor;
            }
            set
            {
                _backColor = value;
            }
        }
        public bool magiColor
        {
            get
            {
                return _magiColor;
            }
            set
            {
                _magiColor = value;
            }
        }
        public int magicX
        {
            set
            {
                _magicX = value;
            }
        }
        //***********************************************
        
        public Paint(Graphics _target,int _width,int _height)//构造函数
        {
            undo = new Stack();
            redo = new Stack();
            target = _target;
            grph = new Bitmap(_width, _height);
            last = (Bitmap)grph.Clone();
            clearPaint();
            ran = new Random();

        }

        public void clearPaint()//清空画板
        {
            target.Clear(_backColor);
            clearBitmap(grph);
            clearBitmap(last);
            undo.Clear();
            redo.Clear();
        }

        void clearBitmap(Bitmap _t)
        {
            Graphics __t = Graphics.FromImage(_t);
            __t.Clear(_backColor);
            __t.Dispose();
        }//清空bitmap

        //绘画操作处理
        //*************************************************
        public void beginDraw(Point __startPoint)
        {
            _startPoint = __startPoint;
            if (_mode == MODE.DOTS || _mode == MODE.ERASER)
            {
                drawingProcess(_startPoint);
            }
        }
        public void drawingProcess(Point endPoint)
        {
            if (magiColor) _penColor = nextColor(_penColor);
            if (_mode != MODE.DOTS && _mode != MODE.ERASER)
            {
                last.Dispose();
                last = (Bitmap)grph.Clone();
            }
            
            Graphics glast = Graphics.FromImage(last);
            Pen p = new Pen(_penColor, _penWidth);
            switch (_mode)
            {
                case MODE.LINES:
                    drawLine(endPoint,glast,p);
                    break;
                case MODE.CIRCLES:
                    drawEllipse(endPoint, glast, p);
                    break;
                case MODE.RECTANGLE:
                    drawRectangle(endPoint, glast, p);
                    break;
                case MODE.DOTS:
                    drawLine(endPoint, glast, p);
                    _startPoint = endPoint;
                    break;
                case MODE.FILL_RECTANGLE:
                    fillRectangle(endPoint, glast);
                    break;
                case MODE.FILL_CIRCLE:
                    fillEllipse(endPoint, glast);
                    break;
                case MODE.ERASER:
                    p.Color = _backColor;
                    drawLine(endPoint, glast, p);
                    _startPoint = endPoint;
                    break;
            }
            target.DrawImage(last, 0, 0);
            glast.Dispose();
        }
        public void finshDraw()
        {
            redo.Clear();
            undo.Push(grph.Clone());
            grph.Dispose();
            grph = (Bitmap)last.Clone();
            
        }
        //*************************************************

        
        //图像生成函数
        //*************************************************
        public void drawLine(Point endPoint, Graphics g, Pen p)//直线
        {
            g.FillEllipse(new SolidBrush(p.Color), _startPoint.X-_penWidth/2, _startPoint.Y - _penWidth / 2, _penWidth, _penWidth);
            g.DrawLine(p, _startPoint, endPoint);
            g.FillEllipse(new SolidBrush(p.Color), endPoint.X - _penWidth / 2, endPoint.Y - _penWidth / 2, _penWidth, _penWidth);

        }
        public void drawEllipse(Point endPoint, Graphics g, Pen p)//空心圆
        {
            g.DrawEllipse(p, _startPoint.X, _startPoint.Y, endPoint.X - _startPoint.X, endPoint.Y - _startPoint.Y);
        }
        public void drawRectangle(Point endPoint, Graphics g, Pen p)//空心矩形
        {
            g.DrawRectangle(p, Math.Min(_startPoint.X, endPoint.X), Math.Min(_startPoint.Y, endPoint.Y),
                            Math.Abs(_startPoint.X - endPoint.X), Math.Abs(_startPoint.Y - endPoint.Y));
        }
        public void fillRectangle(Point endPoint,Graphics g)//实心矩形
        {
            g.FillRectangle(new SolidBrush(_penColor), Math.Min(_startPoint.X, endPoint.X), Math.Min(_startPoint.Y, endPoint.Y),
                            Math.Abs(_startPoint.X - endPoint.X), Math.Abs(_startPoint.Y - endPoint.Y));
        }
        public void fillEllipse(Point endPoint,Graphics g)//实心圆
        {
            g.FillEllipse(new SolidBrush(_penColor), _startPoint.X, _startPoint.Y, endPoint.X - _startPoint.X, endPoint.Y - _startPoint.Y);
        }
        //*************************************************

        public void DotsMove(Point curPoint)//鼠标跟随
        {
            Bitmap temp = (Bitmap)last.Clone();
            Graphics gtemp = Graphics.FromImage(temp);
            gtemp.FillEllipse(new SolidBrush(mode==MODE.DOTS?_penColor:_backColor), 
                                curPoint.X - _penWidth / 2, curPoint.Y - _penWidth / 2, 
                                _penWidth, _penWidth);
            if (mode == MODE.ERASER)
            {
                gtemp.DrawEllipse(new Pen(Color.Black,1),
                                    curPoint.X - _penWidth / 2, curPoint.Y - _penWidth / 2,
                                    _penWidth, _penWidth);
            }
            target.DrawImage(temp, 0, 0);
            gtemp.Dispose();
            temp.Dispose();
        }
        public void rePaint()//重绘函数
        {
            target.DrawImage(grph, 0, 0);
        }
        public void resize(Graphics _target,int _width, int _height)
        {
            Bitmap temp = (Bitmap)grph.Clone();
            grph.Dispose();
            target = _target;
            grph = new Bitmap(_width, _height);
            target.Clear(_backColor);
            clearBitmap(grph);
            clearBitmap(last);
            Graphics g = Graphics.FromImage(grph);
            g.DrawImage(temp, 0, 0);
            g.Dispose();
            last = (Bitmap)grph.Clone();
            rePaint();
            temp.Dispose();
        }
        public Color nextColor(Color cur)
        {
            int[] c = new int[] { cur.R, cur.G, cur.B };
            int t = ran.Next(3);
            if (_dColor)
            {
                c[t]+=_magicX;
                if (c[t] > 0xff)
                {
                    c[t] -= 2 * _magicX;
                    _dColor = false;
                }
            }
            else
            {
                c[t] -= _magicX;
                if (c[t] < 0)
                {
                    c[t] += 2 * _magicX;
                    _dColor = true;
                }
            }
            return Color.FromArgb(0xff, c[0], c[1], c[2]);
        }//求下一个平滑随机颜色
        public void op_undo()//撤销
        {
            redo.Push(grph.Clone());
            clearBitmap(grph);
            Graphics.FromImage(grph).DrawImage((Bitmap)undo.Pop(),0,0);
            last.Dispose();
            last = (Bitmap)grph.Clone();
            rePaint();
        }
        public void op_redo()//重做
        {
            undo.Push(grph.Clone());
            clearBitmap(grph);
            Graphics.FromImage(grph).DrawImage((Bitmap)redo.Pop(), 0, 0);
            last.Dispose();
            last = (Bitmap)grph.Clone();
            rePaint();
        }

        public bool canUndo()
        {
            return undo.Count != 0;
        }
        public bool canRedo()
        {
            return redo.Count != 0;
        }

        public void save()
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            dir += @"\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bmp";
            grph.Save(dir);
            MessageBox.Show("bmp保存已至" + dir);
        }
    }
}
