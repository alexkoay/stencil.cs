using System.Collections.Generic;
using System.Linq;
using Stencil.Core;

namespace Stencil.Elements
{
	public class Box : Base
	{
		public enum Direction { X, Y };

		public bool invert = false;
		public List<Base> nodes = new List<Base>();

		public Box(List<Base> n = null, bool inv = false)
		{
			nodes = n ?? new List<Base>();
			invert = inv;
		}
		public Box(YamlElement node, ElementFactory fac, DataMap def = null)
			: base(node, fac, def ?? (fac.style.ContainsKey("box") ? fac.style["box"] : null))
		{
			switch (node.Type)
			{
				case YamlElement.Types.Sequence:
					nodes = node.List().Select(i => fac.Detect(i)).ToList();
					break;
				case YamlElement.Types.Map:
					if (node.Has("elements"))
					{
						var elem = node.Key("elements");
						if (elem.Type == YamlElement.Types.Sequence)
						{
							var cfac = new ElementFactory(fac);
							foreach (var pair in node.Keys())
							{
								if (pair.Value.Type != YamlElement.Types.Map) { continue; }
								DataMap dat = null;
								if (!cfac.style.ContainsKey(pair.Key)) { cfac.style[pair.Key] = dat = new DataMap(); }
								else { cfac.style[pair.Key] = dat = cfac.style[pair.Key].Copy(); }
								dat.Join(pair.Value.ToData());
							}
							nodes = elem.List().Select(i => cfac.Detect(i)).ToList();
						}
					}
					break;
			}
		}
		public override void Configure(DataMap node, ElementFactory fac)
		{
			base.Configure(node, fac);
			if (node.Has("invert"))
			{
				bool.TryParse(node.Get("invert"), out invert);
			}
		}

		// positioning
		public virtual void Process(List<Output> nodes)
		{
			var max = CalcWH(nodes);
			foreach (var o in nodes)
			{
				if (o.node.alignX != Alignment.None)
					CalcAlign(ref o.left, o.node.alignX, o.width, max.x);

				if (o.node.alignY != Alignment.None) 
					CalcAlign(ref o.up, o.node.alignY, o.height, max.y);
			}
		}

		// helpers
		public Vector CalcBR(IEnumerable<Output> nodes)
		{
			var max = new Vector();
			foreach (var i in nodes)
			{
				var X = i.left + i.width;
				if (max.x < X) { max.x = X; }

				var Y = i.up + i.height;
				if (max.y < Y) { max.y = Y; }
			}
			return rect.CalcWH(max);
		}
		public Vector CalcWH(IEnumerable<Output> nodes)
		{
			var max = new Vector();
			foreach (var i in nodes)
			{
				var X = i.width;
				if (max.x < X) { max.x = X; }

				var Y = i.height;
				if (max.y < Y) { max.y = Y; }
			}
			return rect.CalcWH(max);
		}

		public void CalcAlign(ref Unit orig, Alignment align, Unit size, Unit max)
		{
			switch (align)
			{
				case Alignment.Min: orig = .0f; break;
				case Alignment.Mid: orig = (max - size) / 2; break;
				case Alignment.Max: orig = max - size; break;
			}
		}
	}
}