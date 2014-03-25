using System;
using Stencil.Core;

namespace Stencil.Elements
{
	abstract public class Value : Base
	{
		public Placeholder data;
		public bool collapse = false;
		public Value(string value) : base() { data = value; }
		public Value(YamlElement node, ElementFactory fac, YamlElement def = null)
			: base(node, fac, def ?? (fac.style.ContainsKey("value") ? fac.style["value"] : null))
		{
			switch (node.type)
			{
				case YamlElement.Types.Scalar:
					data = node.val();
					break;
				case YamlElement.Types.Map:
					data = node.get("value", "");
					break;
			}
		}
		public override void Configure(YamlElement node, ElementFactory fac)
		{
			base.Configure(node, fac);
			if (node.type != YamlElement.Types.Map) { return; }

			if (node.hasKey("collapse"))
			{
				bool.TryParse(node.get("collapse"), out collapse);
			}
			if (node.hasKey("coding"))
			{
				Placeholder.Encoding en;
				if (Enum.TryParse(node.get("coding"), out en)) { data.coding = en; }
			}
		}
	}
}
