using System.Collections.Generic;
using System.Linq;
using Stencil.Core;
using System;

namespace Stencil.Elements
{
	public class FlowBox : Box
	{
		public Direction dir = Direction.X;
		public Alignment contentX = Alignment.None;
		public Alignment contentY = Alignment.None;
		public Unit spacing = .0f;
		public float factor = .0f;

		public FlowBox(List<Base> n = null, bool inv = false) : base(n, inv) { }
		public FlowBox(YamlElement node, ElementFactory fac, YamlElement def = null)
			: base(node, fac, def ?? (fac.style.ContainsKey("flowbox") ? fac.style["flowbox"] : null))
		{
		}
		public override void Configure(YamlElement node, ElementFactory fac)
		{
			if (node.type != YamlElement.Types.Map) { return; } 
			base.Configure(node, fac);
			if (node.hasKey("dir"))
			{
				Direction dr;
				if (Enum.TryParse(node.get("dir"), true, out dr)) { dir = dr; }
			}
			if (node.hasKey("factor"))
			{
				float fct;
				if (float.TryParse(node.get("factor"), out fct) && !float.IsNaN(fct)) { factor = fct; }
			}
			if (node.hasKey("factor"))
			{
				var sp = fac.unit.Detect(node.get("spacing"));
				if (sp.Valid) { spacing = sp; }
			}			
		}

		public override void Process(List<Output> nodes)
		{
			var max = CalcWH(nodes);
			Unit cur = .0f;
			switch (dir)
			{
				case Direction.X:
					if (rect.FixedX && contentX != Alignment.None)
					{
						var size = nodes.Select(o => o.left + (o.width * factor) + spacing).Sum() - spacing;
						CalcAlign(ref cur, contentX, size, max.x);
					}

					foreach (var o in nodes)
					{
						o.left += cur;
						cur = o.left + (o.width * factor) + spacing;
						if (contentY != Alignment.None)
							CalcAlign(ref o.up, contentY, o.height, max.y);
					}
					break;

				case Direction.Y:
					if (rect.FixedY && contentY != Alignment.None)
					{
						var size = nodes.Select(o => o.up + (o.height * factor) + spacing).Sum() - spacing;
						CalcAlign(ref cur, contentY, size, max.y);
					}

					foreach (var o in nodes)
					{
						o.up += cur;
						cur = o.up + (o.height * factor) + spacing;
						if (contentY != Alignment.None)
							CalcAlign(ref o.left, contentX, o.width, max.x);
					}
					break;
			}
		}
	}
}
