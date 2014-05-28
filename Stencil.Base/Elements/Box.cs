using System.Collections.Generic;
using System.Linq;
using Stencil.Core;
using System;

namespace Stencil.Elements
{
	public class Box : Base
	{
		public enum Direction { X, Y };

		public bool invert = false;
		public Alignment contentX = Alignment.None;
		public Alignment contentY = Alignment.None;
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
					if (node.Has("contentalign"))
					{
						var align = node.Key("contentalign");
						string x = null, y = null;
						if (align.Type == YamlElement.Types.Map)
						{
							x = align.Get("h", null);
							y = align.Get("v", null);
						}
						else if (align.Type == YamlElement.Types.Sequence)
						{
							var arr = align.List().Select(i => i.ToString()).ToList();
							if (arr.Count == 1) { x = y = arr[0]; }
							else if (arr.Count > 1) { x = arr[0]; y = arr[1]; }
						}
						else if (align.Type == YamlElement.Types.Scalar)
						{
							var arr = align.Val().Split(',');
							if (arr.Length == 1) { x = y = arr[0]; }
							else if (arr.Length > 1) { x = arr[0]; y = arr[1]; }
						}

						Alignment align_en;
						if (Enum.TryParse(x, true, out align_en)) { contentX = align_en; }
						if (Enum.TryParse(y, true, out align_en)) { contentY = align_en; }
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
				if (contentX != Alignment.None)
					CalcAlign(ref o.left, contentX, o.width, max.x);
				else if (o.node.alignX != Alignment.None)
					CalcAlign(ref o.left, o.node.alignX, o.width, max.x);

				if (contentY != Alignment.None)
					CalcAlign(ref o.up, contentY, o.height, max.y);
				else if (o.node.alignY != Alignment.None)
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