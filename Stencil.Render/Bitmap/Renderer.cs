using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Stencil.Core;
using Stencil.Elements;

namespace Stencil.Render
{
	public class BitmapRenderer : Renderer<BitmapOutput>
	{
		static class Helper
		{
			static Bitmap _empty;
			static Graphics _context;

			public static Bitmap Create(Unit width, Unit height)
			{
				if (width <= 0 || height <= 0) { return Create(1, 1); }
				return new Bitmap((int)width, (int)height);
			}
			public static Bitmap Create(Vector size) { return Create(size.x, size.y); }
			public static Bitmap Empty() { return _empty ?? (_empty = Create(1, 1)); }
			public static Graphics Context() { return _context ?? (_context = Graphics.FromImage(Empty())); }

		}

		// elements ////////////////////////////////////////////////////////////

		protected override BitmapOutput RenderE(Box elem, Template tpl, DataMap data)
		{
			var childs = elem.nodes.Select(i => (Output)Render(i, tpl, data)).ToList();
			elem.Process(childs);

			var size = elem.CalcBR(childs);
			var img = Helper.Create(size);

			var mat = new ColorMatrix();
			mat.Matrix44 = 1;
			mat.Matrix00 = mat.Matrix11 = mat.Matrix22 = (elem.invert ? -1 : 1);
			mat.Matrix40 = mat.Matrix41 = mat.Matrix42 = (elem.invert ? 1 : 0);
			var attr = new ImageAttributes(); attr.SetColorMatrix(mat);

			using (var grf = Graphics.FromImage(img))
			{
				if (elem.invert) grf.Clear(Color.Black);
				foreach (BitmapOutput o in childs)
				{
					grf.DrawImage(
						o.render, new Rectangle((int)o.left, (int)o.up, (int)o.width, (int)o.height),
						0, 0, o.width, o.height,
						GraphicsUnit.Pixel, attr);
					o.Dispose();
				}
			}

			return new BitmapOutput(elem, elem.rect.CalcUL(size), img);
		}

		////////////////////////////////////////////////////////////////////////

		protected override BitmapOutput RenderE(Graphic elem, Template tpl, DataMap data)
		{
			var path = elem.data.Parse(data);
			if (!Path.IsPathRooted(path) && tpl.values.Has("path")) { path = Path.Combine(tpl.values.Get("path"), path); }

			System.Drawing.Image file = null;
			var size = elem.rect.Size;
			try
			{
				file = System.Drawing.Image.FromFile(path);
				size = elem.rect.CalcWH(file.Size.ToVector());
				if (elem.rect.FluidX && elem.rect.FixedY) { size.x = size.y / file.Height * file.Width; }
				if (elem.rect.FixedX && elem.rect.FluidY) { size.y = size.x / file.Width * file.Height; }
			}
			catch
			{
				if (elem.collapse || elem.rect.Fluid) { size = new Vector(0, 0); }
			}

			var img = Helper.Create(size);
			if (file != null)
			{
				using (var grf = Graphics.FromImage(img))
				{
					grf.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
					grf.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
					grf.DrawImage(file, 0, 0, size.x, size.y);
				}
				file.Dispose();
			}
			return new BitmapOutput(elem, elem.rect.CalcUL(size), img);
		}

		////////////////////////////////////////////////////////////////////////

		protected override BitmapOutput RenderE(QR elem, Template tpl, DataMap data)
		{
			var text = elem.data.Parse(data);

			// generate QR

			var writer = new ZXing.QrCode.QRCodeWriter();
			var hints = new Dictionary<ZXing.EncodeHintType, object>();
			hints[ZXing.EncodeHintType.MARGIN] = elem.margin;
			switch (elem.error)
			{
				case QR.Error.Low: hints[ZXing.EncodeHintType.ERROR_CORRECTION] = ZXing.QrCode.Internal.ErrorCorrectionLevel.L; break;
				case QR.Error.Med: hints[ZXing.EncodeHintType.ERROR_CORRECTION] = ZXing.QrCode.Internal.ErrorCorrectionLevel.M; break;
				case QR.Error.High: hints[ZXing.EncodeHintType.ERROR_CORRECTION] = ZXing.QrCode.Internal.ErrorCorrectionLevel.Q; break;
				case QR.Error.Max: hints[ZXing.EncodeHintType.ERROR_CORRECTION] = ZXing.QrCode.Internal.ErrorCorrectionLevel.H; break;
			}

			if (text == "") { text = "\0"; }
			var code = writer.encode(text, ZXing.BarcodeFormat.QR_CODE, 0, 0, hints);
			var qr = Helper.Create(code.Dimension, code.Dimension);
			for (int y = 0; y < qr.Height; ++y)
			{
				var row = code.getRow(y, null);
				for (int x = 0; x < qr.Width; ++x)
				{
					var val = row.isRange(x, x + 1, true);
					qr.SetPixel(x, y, val ? Color.Black : Color.White);
				}
			}

			// generate bitmap

			var size = elem.rect.CalcWH(new Vector(-1, -1));
			if (size.x < 0) { size.x = size.y; }
			else if (size.y < 0) { size.y = size.x; }

			if (size.x < 0) { size.x = size.y = code.Height * elem.pitch; }
			else if (elem.resize == QR.Resize.None) { /* do nothing */ }
			else if (size.x < code.Height) { size.x = size.y = code.Height; }
			else
			{
				var scale = size.x / (double)code.Height;
				var ratio = (float)scale;
				switch (elem.resize)
				{
					case QR.Resize.Up: ratio = (float)Math.Ceiling(scale); break;
					case QR.Resize.Down: ratio = (float)Math.Floor(scale); break;
					case QR.Resize.Even: ratio = (float)Math.Round(scale, MidpointRounding.ToEven); break;
					case QR.Resize.Zero: ratio = (float)Math.Round(scale, MidpointRounding.AwayFromZero); break;
				}
				size.x = size.y = code.Height * ratio;
			}

			var img = qr;
			if (size.x != code.Height)
			{
				img = Helper.Create(size);
				using (var grf = Graphics.FromImage(img))
				{
					grf.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
					grf.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
					grf.DrawImage(qr, 0, 0, size.x, size.x);
				}
				qr.Dispose();
			}
			return new BitmapOutput(elem, elem.rect.CalcUL(size), img);
		}

		////////////////////////////////////////////////////////////////////////

		protected override BitmapOutput RenderE(Text elem, Template tpl, DataMap data)
		{
			var text = elem.data.Parse(data);
			var mtext = text;
			if (!elem.collapse && text == "") { mtext = "x"; }
			var font = new Font(elem.family, elem.fontsize);
			if (elem.weight == Text.Weight.Bold) { font = new Font(font, font.Style | FontStyle.Bold); }
			if (elem.style == Text.Style.Italic) { font = new Font(font, font.Style | FontStyle.Italic); }
			var size = elem.rect.CalcWH(Helper.Context().MeasureString(mtext, font, (int)elem.rect.Width, StringFormat.GenericTypographic).ToVector());

			var img = Helper.Create(size);
			using (var grf = Graphics.FromImage(img))
			{
				grf.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
				grf.DrawString(text, font, Brushes.Black, new RectangleF(new PointF(), new SizeF(size.x, size.y + 1)), StringFormat.GenericTypographic);
			}
			return new BitmapOutput(elem, elem.rect.CalcUL(size), img);
		}
	}
}
