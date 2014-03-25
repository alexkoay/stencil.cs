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
		public Text(YamlElement node, ElementFactory fac, YamlElement def = null)
			: base(node, fac, def ?? (fac.style.ContainsKey("text") ? fac.style["text"] : null))
		{
		}
		public override void Configure(YamlElement node, ElementFactory fac)
		{
			base.Configure(node, fac);
			if (node.type != YamlElement.Types.Map) { return; }

			if (node.hasKey("family"))
			{
				family = node.get("family");
			}
			if (node.hasKey("fontsize"))
			{
				var sz = fac.unit.Detect(node.get("fontsize"), fac.unit.Point);
				if (sz.Valid) { fontsize = sz; }
			}
			if (node.hasKey("weight"))
			{
				Weight wt;
				if (Enum.TryParse(node.get("weight"), out wt)) { weight = wt; }
			}
			if (node.hasKey("style"))
			{
				Style st;
				if (Enum.TryParse(node.get("style"), out st)) { style = st; }
			}
		}
	}
}
