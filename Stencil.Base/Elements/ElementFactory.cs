using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stencil.Core;

namespace Stencil.Elements
{
	public class ElementFactory
	{
		public UnitFactory unit;
		public Dictionary<string, YamlElement> style;

		public ElementFactory()
		{
			unit = new UnitFactory();
			style = new Dictionary<string, YamlElement>(StringComparer.OrdinalIgnoreCase);
		}
		public ElementFactory(float ppi, string type = "")
		{
			unit = new UnitFactory(ppi, type);
			style = new Dictionary<string, YamlElement>(StringComparer.OrdinalIgnoreCase);
		}
		public ElementFactory(ElementFactory other)
		{
			unit = other.unit;
			style = new Dictionary<string, YamlElement>(other.style, StringComparer.OrdinalIgnoreCase);
		}

		public Base Detect(YamlElement node)
		{
			if (node.type == YamlElement.Types.Scalar) { return new Text(node, this); }
			else if (node.type == YamlElement.Types.Sequence) { return new Box(node, this); }

			var type = node.get("type", "");
			switch (type)
			{
				case "text": return new Text(node, this);
				case "image": return new Graphic(node, this);
				case "qr": return new QR(node, this);
				case "flow": return new FlowBox(node, this, style.ContainsKey(type) ? style[type] : null);
				case "space":
				case "directed":
					{
						var elem = new FlowBox(node, this, style.ContainsKey(type) ? style[type] : null);
						elem.factor = 0;
						return elem;
					}
				case "stack":
					{
						var elem = new FlowBox(node, this, style.ContainsKey(type) ? style[type] : null);
						elem.factor = 1;
						return elem;
					}
				default: return new Box(node, this);
			}
		}
	}
}
