using System.Drawing;
using Stencil.Core;

namespace Stencil.Render
{
	static class BitmapExtensions
	{
		public static Vector ToVector(this Size val) { return new Vector(val.Width, val.Height); }
		public static Vector ToVector(this SizeF val) { return new Vector(val.Width, val.Height); }
		public static Point ToPoint(this Vector val) { return new Point((int)val.x, (int)val.y); }
		public static Size ToSize(this Vector val) { return new Size((int)val.x, (int)val.y); }
	}
}
