using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace RevitShutteringAddin
{
	public static class RibbonIcons
	{
		[DllImport("gdi32.dll", SetLastError = true)]
		private static extern bool DeleteObject(IntPtr hObject);

		/// <summary>
		/// Creates a 2D icon representing concrete shuttering & scaffolding formwork (32x32 or 16x16).
		/// </summary>
		public static BitmapSource CreateShutteringIcon(int size)
		{
			try
			{
				BitmapSource embedded = LoadEmbeddedPng($"RevitShutteringAddin.Resources.shuttering_{size}.png");
				if (embedded != null)
				{
					return embedded;
				}

				using (Bitmap bmp = GenerateShutteringBitmap(size))
				{
					return ToBitmapSource(bmp);
				}
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Creates a 2D icon representing "Select Similar" elements tool (32x32 or 16x16).
		/// </summary>
		public static BitmapSource CreateSelectSimilarIcon(int size)
		{
			try
			{
				BitmapSource embedded = LoadEmbeddedPng($"RevitShutteringAddin.Resources.select_similar_{size}.png");
				if (embedded != null)
				{
					return embedded;
				}

				using (Bitmap bmp = GenerateSelectSimilarBitmap(size))
				{
					return ToBitmapSource(bmp);
				}
			}
			catch
			{
				return null;
			}
		}

		public static Bitmap GenerateShutteringBitmap(int size)
		{
			Bitmap bmp = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.Clear(Color.Transparent);

				float scale = size / 32f;

				// 1. Concrete Slab / Beam (Top - Slate Grey)
				using (SolidBrush concreteBrush = new SolidBrush(Color.FromArgb(255, 130, 145, 160)))
				using (Pen concretePen = new Pen(Color.FromArgb(255, 85, 100, 115), Math.Max(1f, 1f * scale)))
				{
					RectangleF concreteRect = new RectangleF(3 * scale, 3 * scale, 26 * scale, 7 * scale);
					g.FillRectangle(concreteBrush, concreteRect);
					g.DrawRectangle(concretePen, concreteRect.X, concreteRect.Y, concreteRect.Width, concreteRect.Height);
				}

				// Concrete aggregate texture accents
				if (size >= 24)
				{
					using (SolidBrush dotBrush = new SolidBrush(Color.FromArgb(180, 200, 215, 230)))
					{
						g.FillEllipse(dotBrush, 7 * scale, 5 * scale, 1.5f * scale, 1.5f * scale);
						g.FillEllipse(dotBrush, 16 * scale, 6 * scale, 1.5f * scale, 1.5f * scale);
						g.FillEllipse(dotBrush, 23 * scale, 5 * scale, 1.5f * scale, 1.5f * scale);
					}
				}

				// 2. Plywood Formwork Panel (DOKA Vibrant Yellow)
				using (SolidBrush plywoodBrush = new SolidBrush(Color.FromArgb(255, 245, 185, 30)))
				using (Pen plywoodPen = new Pen(Color.FromArgb(255, 190, 135, 10), Math.Max(1f, 1f * scale)))
				{
					RectangleF plywoodRect = new RectangleF(2 * scale, 11 * scale, 28 * scale, 4 * scale);
					g.FillRectangle(plywoodBrush, plywoodRect);
					g.DrawRectangle(plywoodPen, plywoodRect.X, plywoodRect.Y, plywoodRect.Width, plywoodRect.Height);
				}

				// 3. Timber Primary Joist H20 (Warm Ochre / Orange)
				using (SolidBrush joistBrush = new SolidBrush(Color.FromArgb(255, 220, 135, 25)))
				{
					g.FillRectangle(joistBrush, 4 * scale, 15.5f * scale, 24 * scale, 2.5f * scale);
				}

				// 4. Scaffolding Props (Left and Right - Steel Blue)
				using (SolidBrush propBrush = new SolidBrush(Color.FromArgb(255, 25, 110, 195)))
				using (Pen propPen = new Pen(Color.FromArgb(255, 15, 65, 130), Math.Max(1f, 1f * scale)))
				{
					RectangleF leftProp = new RectangleF(6.5f * scale, 18 * scale, 3.5f * scale, 10 * scale);
					g.FillRectangle(propBrush, leftProp);
					g.DrawRectangle(propPen, leftProp.X, leftProp.Y, leftProp.Width, leftProp.Height);

					RectangleF rightProp = new RectangleF(22f * scale, 18 * scale, 3.5f * scale, 10 * scale);
					g.FillRectangle(propBrush, rightProp);
					g.DrawRectangle(propPen, rightProp.X, rightProp.Y, rightProp.Width, rightProp.Height);
				}

				// 5. Diagonal X-Brace (Construction Orange)
				using (Pen bracePen = new Pen(Color.FromArgb(255, 235, 100, 20), Math.Max(1f, 1.5f * scale)))
				{
					g.DrawLine(bracePen, 10 * scale, 19 * scale, 22 * scale, 27 * scale);
					g.DrawLine(bracePen, 10 * scale, 27 * scale, 22 * scale, 19 * scale);
				}

				// 6. Base Plates / Screw Jacks (Dark Steel)
				using (SolidBrush baseBrush = new SolidBrush(Color.FromArgb(255, 45, 55, 65)))
				{
					g.FillRectangle(baseBrush, 4.5f * scale, 28 * scale, 7.5f * scale, 2.5f * scale);
					g.FillRectangle(baseBrush, 20f * scale, 28 * scale, 7.5f * scale, 2.5f * scale);
				}
			}
			return bmp;
		}

		public static Bitmap GenerateSelectSimilarBitmap(int size)
		{
			Bitmap bmp = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.Clear(Color.Transparent);

				float scale = size / 32f;

				// Three matching element squares (blue & yellow shuttering elements)
				using (SolidBrush elemBrush = new SolidBrush(Color.FromArgb(255, 41, 128, 185)))
				using (Pen elemPen = new Pen(Color.FromArgb(255, 31, 97, 141), Math.Max(1f, 1f * scale)))
				using (SolidBrush formworkBrush = new SolidBrush(Color.FromArgb(255, 243, 156, 18)))
				{
					// Top-left element
					g.FillRectangle(elemBrush, 3 * scale, 3 * scale, 10 * scale, 8 * scale);
					g.FillRectangle(formworkBrush, 3 * scale, 11 * scale, 10 * scale, 2 * scale);
					g.DrawRectangle(elemPen, 3 * scale, 3 * scale, 10 * scale, 10 * scale);

					// Top-right element
					g.FillRectangle(elemBrush, 18 * scale, 3 * scale, 10 * scale, 8 * scale);
					g.FillRectangle(formworkBrush, 18 * scale, 11 * scale, 10 * scale, 2 * scale);
					g.DrawRectangle(elemPen, 18 * scale, 3 * scale, 10 * scale, 10 * scale);

					// Bottom-left element
					g.FillRectangle(elemBrush, 3 * scale, 18 * scale, 10 * scale, 8 * scale);
					g.FillRectangle(formworkBrush, 3 * scale, 26 * scale, 10 * scale, 2 * scale);
					g.DrawRectangle(elemPen, 3 * scale, 18 * scale, 10 * scale, 10 * scale);
				}

				// Green selection checkmarks on each matching item
				using (Pen checkPen = new Pen(Color.FromArgb(255, 46, 204, 113), Math.Max(1.5f, 2f * scale)))
				{
					g.DrawLine(checkPen, 5 * scale, 8 * scale, 7.5f * scale, 10.5f * scale);
					g.DrawLine(checkPen, 7.5f * scale, 10.5f * scale, 11.5f * scale, 5.5f * scale);

					g.DrawLine(checkPen, 20 * scale, 8 * scale, 22.5f * scale, 10.5f * scale);
					g.DrawLine(checkPen, 22.5f * scale, 10.5f * scale, 26.5f * scale, 5.5f * scale);

					g.DrawLine(checkPen, 5 * scale, 23 * scale, 7.5f * scale, 25.5f * scale);
					g.DrawLine(checkPen, 7.5f * scale, 25.5f * scale, 11.5f * scale, 20.5f * scale);
				}

				// White pointer selection cursor at bottom-right
				PointF[] cursor = new PointF[]
				{
					new PointF(17 * scale, 15 * scale),
					new PointF(17 * scale, 28 * scale),
					new PointF(20.5f * scale, 24.5f * scale),
					new PointF(24.5f * scale, 29 * scale),
					new PointF(27 * scale, 27 * scale),
					new PointF(23 * scale, 22.5f * scale),
					new PointF(27 * scale, 22.5f * scale)
				};
				using (SolidBrush cursorBrush = new SolidBrush(Color.White))
				using (Pen cursorPen = new Pen(Color.FromArgb(255, 40, 50, 60), Math.Max(1f, 1.5f * scale)))
				{
					g.FillPolygon(cursorBrush, cursor);
					g.DrawPolygon(cursorPen, cursor);
				}
			}
			return bmp;
		}

		private static BitmapSource LoadEmbeddedPng(string resourceName)
		{
			try
			{
				Assembly asm = Assembly.GetExecutingAssembly();
				using (Stream stream = asm.GetManifestResourceStream(resourceName))
				{
					if (stream != null)
					{
						PngBitmapDecoder decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
						return decoder.Frames[0];
					}
				}
			}
			catch
			{
			}
			return null;
		}

		public static BitmapSource ToBitmapSource(Bitmap bitmap)
		{
			IntPtr hBitmap = bitmap.GetHbitmap();
			try
			{
				return Imaging.CreateBitmapSourceFromHBitmap(
					hBitmap,
					IntPtr.Zero,
					Int32Rect.Empty,
					BitmapSizeOptions.FromEmptyOptions());
			}
			finally
			{
				DeleteObject(hBitmap);
			}
		}
	}
}
