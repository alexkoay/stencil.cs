using System;
using Stencil.Core;

namespace Stencil.Elements
{
	public class Text : Value
	{
		public enum Weight { Normal, Bold };
		public enum Style { Normal, Italic };

		public string family = "Wingdings";
		public float fontsize = new UnitFactory().Point(12);
		public Weight weight = Weight.Normal;
		public Style style = Style.Normal;

		public Text(string value) : base(value) { }
		public Text(YamlElement node, ElementFactory fac, DataMap def = null)
			: base(node, fac, def ?? (fac.style.ContainsKey("text") ? fac.style["text"] : null))
		{
		}
		public override void Configure(DataMap node, ElementFactory fac)
		{
			base.Configure(node, fac);
			if (node.Has("family"))
			{
				family = node.Get("family");
			}
			if (node.Has("fontsize"))
			{
				var sz = fac.unit.Detect(node.Get("fontsize"), fac.unit.Point);
				if (sz.Valid) { fontsize = sz; }
			}
			if (node.Has("weight"))
			{
				Weight wt;
				if (Enum.TryParse(node.Get("weight"), true, out wt)) { weight = wt; }
			}
			if (node.Has("style"))
			{
				Style st;
				if (Enum.TryParse(node.Get("style"), true, out st)) { style = st; }
			}
		}
	}
}
