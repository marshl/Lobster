using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Lobster
{
    public partial class SplashScreen : Form
    {
        private bool painted;

        public SplashScreen()
        {
            InitializeComponent();
        //this.TransparencyKey = Color.Magenta;
    }


        /*protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.painted) return;
            e.Graphics.DrawImage(BackgroundImage, new Point(0, 0));
            painted = true;
        }*/

        protected override void OnPaint(PaintEventArgs e)
        {
            //  Do nothing here!
        }
  
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Graphics gfx = e.Graphics;
  
            gfx.DrawImage(this.BackgroundImage, new Rectangle(0, 0, this.Width, this.Height));
  
        }
}
}
