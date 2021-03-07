using System;
using System.Windows.Forms;
using System.Drawing;

namespace RuneApp {
    // Maybe from http://stackoverflow.com/questions/395256/transparent-images-with-c-sharp-winforms
    public sealed class TransparentControl : Control {
        private readonly Timer refresher;
        private Image _image;
        private float gamma;
        private float scale;

        public TransparentControl() {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            gamma = 1;
            scale = 1;
            refresher = new Timer();
            refresher.Tick += TimerOnTick;
            refresher.Interval = 50;
            refresher.Enabled = true;
            refresher.Start();
        }

        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20;
                return cp;
            }
        }

        protected override void OnMove(EventArgs e) {
            RecreateHandle();
        }


        protected override void OnPaint(PaintEventArgs e) {
            if (_image == null) return;

            var attr = new System.Drawing.Imaging.ImageAttributes();

            attr.SetGamma(gamma);

            // slight change to how they drew it
            int width = (int)(_image.Width * scale);
            int height = (int)(_image.Height * scale);

            e.Graphics.DrawImage(_image, new Rectangle((Width / 2) - (width / 2), (Height / 2) - (height / 2), width, height), 0, 0, _image.Width, _image.Height, GraphicsUnit.Pixel, attr);
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            //Do not paint background
        }

        //Hack
        public void Redraw() {
            RecreateHandle();
        }

        private void TimerOnTick(object source, EventArgs e) {
            RecreateHandle();
            refresher.Stop();
        }

        public Image Image {
            get {
                return _image;
            }
            set {
                _image = value;
                RecreateHandle();
            }
        }

        public float Gamma {
            get {
                return gamma;
            }
            set {
                gamma = value;
            }
        }

        public float ImageScale {
            get {
                return scale;
            }
            set {
                scale = value;
            }
        }
    }
}
