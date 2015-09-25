using System.Drawing;
using System.Windows.Forms;

namespace Lobster
{
    public partial class SplashScreen : Form
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

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
