using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using RuneOptim.swar;

namespace RuneApp {
    // A really overloaded control to display runes cool-like with a million options
    // Mostly stolen from TransparentControl
    public sealed class RuneControl : Control {
        private readonly Timer refresher;

        // yeah, pictures
        private Image _imageSlot;
        private Image _imageSet;
        private Image _imageBack;
        private Image _imageStars;

        // allows runes to look "selected"
        private float gamma;

        // if to render fx
        private bool renderStars;
        private bool renderBack;

        // Number of stars
        private int grade;

        // Normal, Magic, Rare, Hero, Legend
        private int coolness;

        protected override Size DefaultSize {
            get {
                return new Size(171, 179);
            }
        }

        public Image SlotImage {
            get {
                return _imageSlot;
            }
            set {
                _imageSlot = value;
                RecreateHandle();
            }
        }

        public Image SetImage {
            get {
                return _imageSet;
            }
            set {
                _imageSet = value;
                RecreateHandle();
            }
        }

        public Image StarImage {
            get {
                return _imageStars;
            }
            set {
                _imageStars = value;
                RecreateHandle();
            }
        }

        public Image BackImage {
            get {
                return _imageBack;
            }
            set {
                _imageBack = value;
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

        public int Grade { get { return grade; } set { grade = value; } }
        public bool ShowBack { get { return renderBack; } set { renderBack = value; } }
        public bool ShowStars { get { return renderStars; } set { renderStars = value; } }
        public int Coolness { get { return coolness; } set { coolness = value; } }

        public RuneControl() {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            gamma = 1;
            refresher = new Timer();
            refresher.Tick += TimerOnTick;
            refresher.Interval = 50;
            refresher.Enabled = true;
            refresher.Start();
            grade = 1;
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

        public void SetCraft(Craft craft) {
            grade = 0;

            _imageSlot = runeSlotImages[(int)craft.Type + 6];
            _imageSet = runeSetImages[craft.Set];

            _imageBack = runeRarityImages[craft.Rarity - 1];
            coolness = craft.Rarity - 1;

            Refresh();
        }

        public void SetRune(RuneOptim.swar.Rune rune) {
            Tag = rune;
            if (rune == null) {
                _imageSlot = null;
                Refresh();
                return;
            }

            grade = rune.Grade;

            _imageSlot = runeSlotImages[rune.Slot];
            _imageSet = runeSetImages[rune.Set];

            _imageBack = runeRarityImages[rune.Rarity];
            coolness = rune.Rarity;

            _imageStars = Resources.Runes.star_unawakened;
            if (rune.Level == 15)
                _imageStars = Resources.Runes.star_awakened;

            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e) {
            if (_imageSlot != null) {
                var attr = new ImageAttributes();

                attr.SetGamma(gamma);

                int squarish = Math.Max(_imageSlot.Width, _imageSlot.Height);

                if (renderBack)
                    e.Graphics.DrawImage(_imageBack,
                        new Rectangle((Width / 2) - (squarish / 2), (Height / 2) - (squarish / 2), squarish, squarish),
                        new Rectangle(0, 0, _imageBack.Width, _imageBack.Height), GraphicsUnit.Pixel);

                e.Graphics.DrawImage(_imageSlot,
                    new Rectangle((Width / 2) - (_imageSlot.Width / 2), (Height / 2) - (_imageSlot.Height / 2), _imageSlot.Width, _imageSlot.Height),
                    0, 0, _imageSlot.Width, _imageSlot.Height,
                    GraphicsUnit.Pixel, attr);

                int smallish = (int)(squarish * 0.5);

                if (coolness != 0) {
                    // https://www.w3schools.com/colors/colors_converter.asp

                    float[] colour = new float[] { 0.13f, 0.76f, 0.32f, 0, 1 };

                    if (coolness == 2)
                        colour = new float[] { 0.32f, 1.0f, 0.92f, 0, 1 };
                    else if (coolness == 3)
                        colour = new float[] { 0.93f, 0.55f, 0.99f, 0, 1 };
                    else if (coolness == 4)
                        colour = new float[] { 0.98f, 0.68f, 0.48f, 0, 1 };
                    float[][] ptsArray =
                    {
                        new float[] { colour[0], 0, 0, 0, 0},
                        new float[] {0, colour[1], 0, 0, 0},
                        new float[] {0, 0, colour[2], 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0, 0, 0}
                    };
                    ColorMatrix clrMatrix = new ColorMatrix(ptsArray);
                    attr.SetColorMatrix(clrMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                }

                if (_imageSet != null) {
                    e.Graphics.DrawImage(_imageSet,
                        new Rectangle((Width / 2) - (smallish / 2), (Height / 2) - (smallish / 2), smallish, smallish),
                        0, 0, _imageSet.Width, _imageSet.Height,
                        GraphicsUnit.Pixel, attr);
                }

                // for int grade draw star
                if (renderStars) {
                    for (int i = 0; i < grade; i++) {
                        e.Graphics.DrawImage(_imageStars,
                            new Rectangle((Width / 2) - (squarish / 2) + 2 + (8 + 6 / grade) * i, (Height / 2) - (squarish / 2) + 3, 13, 13),
                            new Rectangle(0, 0, _imageStars.Width, _imageStars.Height),
                            GraphicsUnit.Pixel);
                    }
                }

            }
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

        static readonly Bitmap[] runeSlotImages =
        {
            null,
            Resources.Runes.rune1,
            Resources.Runes.rune2,
            Resources.Runes.rune3,
            Resources.Runes.rune4,
            Resources.Runes.rune5,
            Resources.Runes.rune6,
            Resources.Runes.enchant_gem_common,
            Resources.Runes.grindstone_common,
        };

        static readonly Bitmap[] runeRarityImages =
        {
            Resources.Runes.bg_normal,
            Resources.Runes.bg_magic,
            Resources.Runes.bg_rare,
            Resources.Runes.bg_hero,
            Resources.Runes.bg_legend,
        };

        static readonly System.Collections.Generic.Dictionary<RuneOptim.swar.RuneSet, Bitmap> runeSetImages = new System.Collections.Generic.Dictionary<RuneOptim.swar.RuneSet, Bitmap>()
        {
            { RuneOptim.swar.RuneSet.Null, null },
            { RuneOptim.swar.RuneSet.Blade, Resources.Runes.blade },
            { RuneOptim.swar.RuneSet.Despair, Resources.Runes.despair },
            { RuneOptim.swar.RuneSet.Destroy, Resources.Runes.destroy },
            { RuneOptim.swar.RuneSet.Endure, Resources.Runes.endure },
            { RuneOptim.swar.RuneSet.Energy, Resources.Runes.energy },
            { RuneOptim.swar.RuneSet.Fatal, Resources.Runes.fatal },
            { RuneOptim.swar.RuneSet.Focus, Resources.Runes.focus },
            { RuneOptim.swar.RuneSet.Guard, Resources.Runes.guard },
            { RuneOptim.swar.RuneSet.Nemesis, Resources.Runes.nemesis },
            { RuneOptim.swar.RuneSet.Rage, Resources.Runes.rage },
            { RuneOptim.swar.RuneSet.Revenge, Resources.Runes.revenge },
            { RuneOptim.swar.RuneSet.Shield, Resources.Runes.shield },
            { RuneOptim.swar.RuneSet.Swift, Resources.Runes.swift },
            { RuneOptim.swar.RuneSet.Vampire, Resources.Runes.vampire },
            { RuneOptim.swar.RuneSet.Violent, Resources.Runes.violent },
            { RuneOptim.swar.RuneSet.Will, Resources.Runes.will },
            { RuneOptim.swar.RuneSet.Fight, Resources.Runes.fight },
            { RuneOptim.swar.RuneSet.Determination, Resources.Runes.determination },
            { RuneOptim.swar.RuneSet.Enhance, Resources.Runes.enhance },
            { RuneOptim.swar.RuneSet.Accuracy, Resources.Runes.accuracy },
            { RuneOptim.swar.RuneSet.Tolerance, Resources.Runes.tolerance }
        };

    }
}
