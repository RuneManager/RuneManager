using System;
using System.Windows.Forms;
using System.Drawing;

namespace RuneApp
{
    // Maybe from http://stackoverflow.com/questions/395256/transparent-images-with-c-sharp-winforms
    public sealed class TransparentControl : Control
    {
        private readonly Timer refresher;
        private Image _image;
        private float gamma;

        public TransparentControl()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            gamma = 1;
            refresher = new Timer();
            refresher.Tick += TimerOnTick;
            refresher.Interval = 50;
            refresher.Enabled = true;
            refresher.Start();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20;
                return cp;
            }
        }

        protected override void OnMove(EventArgs e)
        {
            RecreateHandle();
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            if (_image == null) return;

            var attr = new System.Drawing.Imaging.ImageAttributes();

            attr.SetGamma(gamma);

            // slight change to how they drew it
            e.Graphics.DrawImage(_image, new Rectangle((Width / 2) - (_image.Width / 2), (Height / 2) - (_image.Height / 2), _image.Width, _image.Height), 0, 0, _image.Width, _image.Height, GraphicsUnit.Pixel, attr);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //Do not paint background
        }

        //Hack
        public void Redraw()
        {
            RecreateHandle();
        }

        private void TimerOnTick(object source, EventArgs e)
        {
            RecreateHandle();
            refresher.Stop();
        }

        public Image Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
                RecreateHandle();
            }
        }

        public float Gamma
        {
            get
            {
                return gamma;
            }
            set
            {
                gamma = value;
            }
        }
    }
}
