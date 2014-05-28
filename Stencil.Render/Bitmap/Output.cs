using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stencil.Core;
using Stencil.Elements;

namespace Stencil.Render
{
	public class BitmapOutput : Output, IDisposable
	{
		public Bitmap render;
		internal BitmapOutput(Base n, Bitmap r)
		{
			var wh = r.Size.ToVector();
			width = wh.x;
			height = wh.y;
			render = r;
			node = n;
		}
		internal BitmapOutput(Base n, Vector v, Bitmap r)
			: this(n, r)
		{
			left = v.x;
			up = v.y;
		}

		public byte[,] ToGRF(float threshold = 1.00f)
		{
			if (threshold <= 0) { threshold = 0.01f; }
			else if (threshold > 1) { threshold = 1.00f; }

			var stride = (int)Math.Ceiling(render.Width / 8.0);
			var size = stride * render.Height;

			var grf = new byte[stride, render.Height];
			for (int y = 0; y < render.Height; ++y)
			{
				var pos = 0;
				for (int x = 0; x < render.Width; ++x)
				{
					int bit = 7 - (x % 8);
					var px = render.GetPixel(x, y);
					if (px.A > 0 && px.GetBrightness() < threshold) { grf[pos, y] |= (byte)(0x1 << bit); }
					if (bit == 0) { ++pos; }
				}
			}
			return grf;
		}
		public void Dispose() { render.Dispose(); }
	}
}
