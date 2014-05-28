using System;
using Stencil.Core;

namespace Stencil.Elements
{
	public class QR : Value
	{
		public enum Error { Low, Med, High, Max }
		public enum Resize { None, Up, Down, Even, Zero }

		public Error error = Error.Med;
		public Resize resize = Resize.Zero;
		public int pitch = 4, margin = 4;

		public QR(string value) : base(value) { }
		public QR(YamlElement node, ElementFactory fac, DataMap def = null)
			: base(node, fac, def ?? (fac.style.ContainsKey("qr") ? fac.style["qr"] : null))
		{
		}
		public override void Configure(DataMap node, ElementFactory fac)
		{
			base.Configure(node, fac);
			if (node.Has("error"))
			{
				Error err;
				if (Enum.TryParse(node.Get("error"), true, out err)) { error = err; }
			}
			if (node.Has("resize"))
			{
				Resize rs;
				if (Enum.TryParse(node.Get("resize"), true, out rs)) { resize = rs; }
			}
			if (node.Has("pitch"))
			{
				int pt;
				if (int.TryParse(node.Get("pitch"), out pt)) { pitch = pt; }
			}
			if (node.Has("margin"))
			{
				int mg;
				if (int.TryParse(node.Get("margin"), out mg)) { margin = mg; }
			}
		}
	}
}
