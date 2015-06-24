using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace LazyUrls
{
    public partial class ScreenForm : Form
    {
        private Graphics g;
        private Point ClickPosition, TopPosition, BottomPosition;
        private SolidBrush DrawPen, ErasePen;
        private Pen MarkingPen;
        private bool DrawSelection;
        private bool CrossMouse;

        public ScreenForm()
        {
            Program.Hook = true;
            InitializeComponent();

            this.TopMost = true;
            this.Focus();
            this.BringToFront();
            this.ShowInTaskbar = false;
            this.KeyUp += new KeyEventHandler(key_press);
            this.MouseDown += new MouseEventHandler(mouse_Click);
            this.MouseUp += new MouseEventHandler(mouse_Up);
            this.MouseMove += new MouseEventHandler(mouse_Move);

            this.g = this.CreateGraphics();
            this.DrawPen = new SolidBrush(Color.Black);
            this.ErasePen = new SolidBrush(Color.FromArgb(255, 255, 192));
            this.MarkingPen = new Pen(Color.Red, 2);
            this.DrawSelection = false;
            this.CrossMouse = true;

            g.FillRectangle(ErasePen, 0, 0, this.Width, this.Height);
        }
        
        public void key_press(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                CloseMe();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                int width = BottomPosition.X - TopPosition.X;
                int Height = BottomPosition.Y - TopPosition.Y;
                if (width == 0
                    || Height == 0)
                    CloseMe();
                var bmpScreenshot = new Bitmap(width, Height, PixelFormat.Format32bppArgb);
                var gfxScreenshot = Graphics.FromImage(bmpScreenshot);                
                gfxScreenshot.CopyFromScreen(TopPosition.X, TopPosition.Y, 0, 0, new Size(width, Height), CopyPixelOperation.SourceCopy);
                var str = "tmp-" + new Random().Next() + ".png";
                bmpScreenshot.Save(str, ImageFormat.Png);
                Program.UploadCapture(this, str);
            }
            else if (e.KeyCode == Keys.C)
            {
                CrossMouse = true;
            }
            else if (e.KeyCode == Keys.P)
            {
                CrossMouse = false;
            }
            else if (e.KeyCode == Keys.R)
            {
                MarkingPen.Color = Color.Red;
            }
            else if (e.KeyCode == Keys.G)
            {
                MarkingPen.Color = Color.Green;
            }
            else if (e.KeyCode == Keys.B)
            {
                MarkingPen.Color = Color.Blue;
            }
            else if (e.KeyCode == Keys.E)
            {
                MarkingPen.Color = DrawPen.Color;
            }
            else if (e.KeyCode == Keys.Z)
            {
                MarkingPen.Width++;
            }
            else if (e.KeyCode == Keys.X)
            {
                if (MarkingPen.Width > 0)
                    MarkingPen.Width--;
            }
        }

        private void CloseMe()
        {
            Program.ProgTray.ShowBalloonTip(1000, "Lazy urls", "Screen Capture Cancelled", ToolTipIcon.Info);
            
            Program.Hook = false;
            this.Close();
        }
        

        private void mouse_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (CrossMouse)
                {
                    g.FillRectangle(ErasePen, TopPosition.X, TopPosition.Y, BottomPosition.X - TopPosition.X, BottomPosition.Y - TopPosition.Y);

                    this.ClickPosition = MousePosition;
                    this.TopPosition = MousePosition;
                    this.BottomPosition = MousePosition;

                    DrawSelection = true;
                }
            }
        }

        private void mouse_Up(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (CrossMouse)
                    DrawSelection = false;
            }
        }

        private void mouse_Move(object sender, MouseEventArgs e)
        {
            if (!CrossMouse && e.Button == MouseButtons.Left)
            {
                if (MousePosition.X >= TopPosition.X
                    && MousePosition.X < BottomPosition.X
                    && MousePosition.Y >= TopPosition.Y
                    && MousePosition.Y < BottomPosition.Y)
                //means we can draw :D
                g.DrawRectangle(MarkingPen, MousePosition.X, MousePosition.Y, MarkingPen.Width, MarkingPen.Width);
            }

            if (DrawSelection)
            {
                g.FillRectangle(ErasePen, TopPosition.X, TopPosition.Y, BottomPosition.X - TopPosition.X, BottomPosition.Y - TopPosition.Y);

                if (MousePosition.X > ClickPosition.X)
                {
                    if (MousePosition.Y > ClickPosition.Y)
                    {
                        //4th
                        TopPosition = ClickPosition;
                        BottomPosition = MousePosition;
                    }
                    else
                    {
                        //1st
                        TopPosition = new Point(ClickPosition.X, MousePosition.Y);
                        BottomPosition = new Point(MousePosition.X, ClickPosition.Y);
                    }
                }
                else
                {
                    if (MousePosition.Y < ClickPosition.Y)
                    {
                        //2nd
                        BottomPosition = ClickPosition;
                        TopPosition = MousePosition;
                    }
                    else
                    {
                        //3rd
                        TopPosition = new Point(MousePosition.X, ClickPosition.Y);
                        BottomPosition = new Point(ClickPosition.X, MousePosition.Y);
                    }
                }

                g.FillRectangle(DrawPen, TopPosition.X, TopPosition.Y, BottomPosition.X - TopPosition.X, BottomPosition.Y - TopPosition.Y);
            }
        }
    }
}
