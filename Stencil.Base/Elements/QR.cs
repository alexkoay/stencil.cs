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
		public QR(YamlElement node, ElementFactory fac, YamlElement def = null)
			: base(node, fac, def ?? (fac.style.ContainsKey("qr") ? fac.style["qr"] : null))
		{
		}
		public override void Configure(YamlElement node, ElementFactory fac)
		{
			base.Configure(node, fac);
			if (node.type != YamlElement.Types.Map) { return; }

			if (node.hasKey("error"))
			{
				Error err;
				if (Enum.TryParse(node.get("error"), out err)) { error = err; }
			}
			if (node.hasKey("resize"))
			{
				Resize rs;
				if (Enum.TryParse(node.get("resize"), out rs)) { resize = rs; }
			}
			if (node.hasKey("pitch"))
			{
				int pt;
				if (int.TryParse(node.get("pitch"), out pt)) { pitch = pt; }
			}
			if (node.hasKey("margin"))
			{
				int mg;
				if (int.TryParse(node.get("margin"), out mg)) { pitch = mg; }
			}
		}
	}
}
