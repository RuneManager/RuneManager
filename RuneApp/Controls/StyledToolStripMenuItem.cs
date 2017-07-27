using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace RuneApp
{
	public partial class StyledToolStripButton : ToolStripMenuItem
	{
		public StyledToolStripButton()
		{
			InitializeComponent();
		}

		Image _image = null;
		Image _imageBlur = null;
		bool hover = false;

		public override Image Image
		{
			get
			{
				return _image;
			}

			set
			{
				_image = value;
				Int32 avgR = 0, avgG = 0, avgB = 0;
				Int32 blurPixelCount = 0;
				Bitmap srcPic = new Bitmap(_image);
				Bitmap blurPic = new Bitmap(srcPic.Width, srcPic.Height);

				for (int y = 0; y < srcPic.Height; y++)
				{
					for (int x = 0; x < srcPic.Width; x++)
					{
						Color pixel = srcPic.GetPixel(x, y);
						avgR += pixel.R;
						avgG += pixel.G;
						avgB += pixel.B;

						blurPixelCount++;
					}
				}

				avgR = avgR / blurPixelCount;
				avgG = avgG / blurPixelCount;
				avgB = avgB / blurPixelCount;

				for (int y = 0; y < srcPic.Height; y++)
				{
					for (int x = 0; x < srcPic.Width; x++)
					{
						blurPic.SetPixel(x, y, Color.FromArgb(avgR, avgG, avgB));
					}
				}
				_imageBlur = blurPic;
			}
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			hover = true;
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			hover = false;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			var attr = new ImageAttributes();

			
			
			float[][] ptsArray =
			{
					new float[] { 1, 0, 0, 0, 0 },
					new float[] { 0, 1, 0, 0, 0 },
					new float[] { 0, 0, 1, 0, 0 },
					new float[] { 0, 0, 0, 1, 0 },
					new float[] { ForeColor.R / (float)255, ForeColor.G / (float)255, ForeColor.B / (float)255, 0, 0 }
			};
			ColorMatrix clrMatrix = new ColorMatrix(ptsArray);
			attr.SetColorMatrix(clrMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			

			if (Image != null)
			{
				e.Graphics.DrawImage(hover ? _imageBlur : _image, 
					new Rectangle(e.ClipRectangle.X + 6, e.ClipRectangle.Y + 4, e.ClipRectangle.Width - 12, e.ClipRectangle.Height - 9),
					0, 0, Image.Width, Image.Height,
					GraphicsUnit.Pixel, attr);
			}
		}
	}
}
