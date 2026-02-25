using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace perpixelOpacity
{
    public partial class Form1 : Form
    {
        private Timer animTimer;
        private float angle = 0f;
        private float pulse = 0f;
        private float speed, pspeed = 0f;
        private bool pulseUp = true;

        private readonly Color[] googleColors = new Color[]
        {
            Color.FromArgb(66, 133, 244),
            Color.FromArgb(234, 67, 53),
            Color.FromArgb(251, 188, 5),
            Color.FromArgb(52, 168, 83)
        };

        public Form1()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            Width = 400;
            Height = 400;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            animTimer = new Timer();
            animTimer.Interval = 16; // ~60 FPS
            animTimer.Tick += (s, ev) => Animate();
            animTimer.Start();
        }
        private void Animate()
        {
            speed = 2f + 6f * pulse;
            angle += 0.8f * speed + 0.2f * pspeed;
            pspeed = speed;

            if (angle >= 360f) angle = 0f;

            if (pulseUp)
            {
                pulse += (float)Math.Sqrt(1f - Math.Min(1,pulse*pulse)) * 0.07f;
                if (pulse >= 1f) pulseUp = false;
            }
            else
            {
                pulse -= (float)Math.Sqrt(1.2f - Math.Min(1, pulse * pulse)) * 0.07f;
                if (pulse <= 0f) pulseUp = true;
            }

            DrawFrame();
        }
        private void DrawFrame()
        {
            Bitmap bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                float centerX = Width / 2f;
                float centerY = Height / 2f;

                float baseRadius = 30f + 60f * pulse;
                float dotRadius = 18f + pulse * 12f;

                for (int i = 0; i < 4; i++)
                {
                    float a = angle + i * 90f;
                    float rad = (float)(a * Math.PI / 180f);

                    float x = centerX + (float)Math.Cos(rad) * baseRadius;
                    float y = centerY + (float)Math.Sin(rad) * baseRadius;

                    using (SolidBrush b = new SolidBrush(googleColors[i]))
                    {
                        g.FillEllipse(
                            b,
                            x - dotRadius,
                            y - dotRadius,
                            dotRadius * 2,
                            dotRadius * 2
                        );
                    }
                }
            }

            SetBitmap(bmp);
        }


        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00080000;
                return cp;
            }
        }

        private void SetBitmap(Bitmap bitmap)
        {
            IntPtr screenDc = NativeMethods.GetDC(IntPtr.Zero);
            IntPtr memDc = NativeMethods.CreateCompatibleDC(screenDc);
            IntPtr hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
            IntPtr oldBitmap = NativeMethods.SelectObject(memDc, hBitmap);

            NativeMethods.SIZE size = new NativeMethods.SIZE(bitmap.Width, bitmap.Height);
            NativeMethods.POINT pointSource = new NativeMethods.POINT(0, 0);
            NativeMethods.POINT topPos = new NativeMethods.POINT(Left, Top);

            NativeMethods.BLENDFUNCTION blend = new NativeMethods.BLENDFUNCTION();
            blend.BlendOp = 0;
            blend.BlendFlags = 0;
            blend.SourceConstantAlpha = 255;
            blend.AlphaFormat = 1; // AC_SRC_ALPHA

            NativeMethods.UpdateLayeredWindow(
                Handle,
                screenDc,
                ref topPos,
                ref size,
                memDc,
                ref pointSource,
                0,
                ref blend,
                2);

            NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
            NativeMethods.SelectObject(memDc, oldBitmap);
            NativeMethods.DeleteObject(hBitmap);
            NativeMethods.DeleteDC(memDc);
        }

        private class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct POINT { public int x, y; public POINT(int x, int y) { this.x = x; this.y = y; } }

            [StructLayout(LayoutKind.Sequential)]
            public struct SIZE { public int cx, cy; public SIZE(int cx, int cy) { this.cx = cx; this.cy = cy; } }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct BLENDFUNCTION
            {
                public byte BlendOp;
                public byte BlendFlags;
                public byte SourceConstantAlpha;
                public byte AlphaFormat;
            }

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool UpdateLayeredWindow(
                IntPtr hwnd,
                IntPtr hdcDst,
                ref POINT pptDst,
                ref SIZE psize,
                IntPtr hdcSrc,
                ref POINT pprSrc,
                int crKey,
                ref BLENDFUNCTION pblend,
                int dwFlags);

            [DllImport("gdi32.dll", SetLastError = true)]
            public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr GetDC(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

            [DllImport("gdi32.dll", SetLastError = true)]
            public static extern bool DeleteDC(IntPtr hdc);

            [DllImport("gdi32.dll", SetLastError = true)]
            public static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

            [DllImport("gdi32.dll", SetLastError = true)]
            public static extern bool DeleteObject(IntPtr hObject);
        }
    }
}
