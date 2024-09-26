using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SplitText
{
    public partial class Form1 : Form
    {
        string path = string.Empty;
        public enum MouseDirection
        {
            Herizontal,//水平方向拖动，只改变窗体的宽度
            Vertical,//垂直方向拖动，只改变窗体的高度
            Declining,//倾斜方向，同时改变窗体的宽度和高度
            None//不做标志，即不拖动窗体改变大小
        }
        Point mouseOff;//鼠标移动位置变量
        bool leftFlag;//标签是否为左键
        bool isMouseDown = false; //表示鼠标当前是否处于按下状态，初始值为否
        MouseDirection direction = MouseDirection.None;//表示拖动的方向，起始为None，表示不拖动

        public Form1()
        {
            InitializeComponent();
        }

        private void richTextBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int sel = richTextBox1.SelectionStart;
            richTextBox1.Text = richTextBox1.Text.Substring(sel);
            richTextBox1.SaveFile(path, RichTextBoxStreamType.PlainText);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            num.Text = richTextBox1.Text.Length.ToString();
            Lines.Text = richTextBox1.Lines.Length.ToString();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFile = new OpenFileDialog())
            {
                openFile.Filter = "文本文件|*.txt|所有文件|*.*";
                if (DialogResult.Cancel == openFile.ShowDialog())
                {
                    return;
                }
                richTextBox1.LoadFile(openFile.FileName, RichTextBoxStreamType.PlainText);
                path = openFile.FileName;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            richTextBox1.SaveFile(path, RichTextBoxStreamType.PlainText);
        }

        private void label3_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtSP.Text))
            {
                richTextBox1.Text = richTextBox1.Text.Replace(txtSP.Text, txtSP.Text + "\r\n");
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string line in richTextBox1.Lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine(line.Trim());
                }
            }
            richTextBox1.Text = sb.ToString();
        }

        private void label6_Click(object sender, EventArgs e)
        {
            using (FontDialog fd = new FontDialog())
            {
                fd.ShowColor = true;
                fd.Font = richTextBox1.Font;
                fd.Color= richTextBox1.ForeColor;
                if (DialogResult.OK == fd.ShowDialog())
                {
                    richTextBox1.Font = fd.Font;
                    richTextBox1.ForeColor = fd.Color;
                }
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            //如果鼠标按下，同时有方向箭头那么直接调整大小,这里是改进的地方，不然斜角拉的过程中，会有问题
            if (isMouseDown && direction != MouseDirection.None)
            {
                //设定好方向后，调用下面方法，改变窗体大小
                ResizeWindow();
                return;
            }
            //鼠标移动过程中，坐标时刻在改变
            //当鼠标移动时横坐标距离窗体右边缘5像素以内且纵坐标距离下边缘也在5像素以内时，要将光标变为倾斜的箭头形状，同时拖拽方向direction置为MouseDirection.Declining
            if (e.Location.X >= this.Width - 5 && e.Location.Y > this.Height - 5)
            {
                this.Cursor = Cursors.SizeNWSE;
                direction = MouseDirection.Declining;
            }
            //当鼠标移动时横坐标距离窗体右边缘5像素以内时，要将光标变为倾斜的箭头形状，同时拖拽方向direction置为MouseDirection.Herizontal
            else if (e.Location.X >= this.Width - 5)
            {
                this.Cursor = Cursors.SizeWE;
                direction = MouseDirection.Herizontal;
            }
            //同理当鼠标移动时纵坐标距离窗体下边缘5像素以内时，要将光标变为倾斜的箭头形状，同时拖拽方向direction置为MouseDirection.Vertical
            else if (e.Location.Y >= this.Height - 5)
            {
                this.Cursor = Cursors.SizeNS;
                direction = MouseDirection.Vertical;
            }
            //否则，以外的窗体区域，鼠标星座均为单向箭头（默认） 按下时可移动界面
            else
            {
                this.Cursor = Cursors.Arrow;
                if (leftFlag)
                {
                    Point mouseSet = Control.MousePosition;
                    mouseSet.Offset(mouseOff.X, mouseOff.Y); //设置移动后的位置
                    Location = mouseSet;
                }
            }
        }
        private void ResizeWindow()
        {
            //这个判断很重要，只有在鼠标按下时才能拖拽改变窗体大小，如果不作判断，那么鼠标弹起和按下时，窗体都可以改变
            if (!isMouseDown)
                return;
            //MousePosition的参考点是屏幕的左上角，表示鼠标当前相对于屏幕左上角的坐标this.left和this.top的参考点也是屏幕，属性MousePosition是该程序的重点
            if (direction == MouseDirection.Declining)
            {
                //此行代码在mousemove事件中已经写过，在此再写一遍，并不多余，一定要写
                this.Cursor = Cursors.SizeNWSE;
                //下面是改变窗体宽和高的代码，不明白的可以仔细思考一下
                this.Width = MousePosition.X - this.Left;
                this.Height = MousePosition.Y - this.Top;
            }
            //以下同理
            if (direction == MouseDirection.Herizontal)
            {
                this.Cursor = Cursors.SizeWE;
                this.Width = MousePosition.X - this.Left;
            }
            else if (direction == MouseDirection.Vertical)
            {
                this.Cursor = Cursors.SizeNS;
                this.Height = MousePosition.Y - this.Top;
            }
            //即使鼠标按下，但是不在窗口右和下边缘，那么也不能改变窗口大小
            else
                this.Cursor = Cursors.Arrow;
        }
        //添加窗体的MouseDown事件，并编写如下代码
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            isMouseDown = true;
            if (e.Button == MouseButtons.Left)
            {
                mouseOff = new Point(-e.X, -e.Y); //得到变量的值
                leftFlag = true; //点击左键按下时标注为true;
            }
        }
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
            direction = MouseDirection.None;
            if (leftFlag)
            {
                leftFlag = false;//释放鼠标后标注为false;
            }
        }

        private void label7_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    this.WindowState = FormWindowState.Minimized;
                    this.Hide();
                    break;
                case Keys.Q:
                    int sel = richTextBox1.SelectionStart;
                    richTextBox1.Text = richTextBox1.Text.Substring(sel);
                    richTextBox1.SaveFile(path, RichTextBoxStreamType.PlainText);
                    break;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            richTextBox1.Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Left = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
            this.Top = Screen.PrimaryScreen.WorkingArea.Height - this.Height;
        }
    }
}
