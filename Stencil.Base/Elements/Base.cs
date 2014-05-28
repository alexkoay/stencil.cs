using System.Linq;
using Stencil.Core;
using System;

namespace Stencil.Elements
{
	abstract public class Base
	{
		public enum Alignment
		{
			None = 0,
			Min, Left = Min, Top = Min, Up = Min,
			Mid, Center = Mid,
			Max, Right = Max, Bottom = Max, Down = Max
		};

		public Dimension rect;
		public Alignment alignX, alignY;

		public Base() { }
		public Base(YamlElement node, ElementFactory fac, DataMap def = null)
		{
			if (def != null) { Configure(def, fac); }
			if (node.Type != YamlElement.Types.Map) { return; }

			Configure(node.ToData(), fac);

			// position
			if (node.Has("pos"))
			{
				var pos = node.Key("pos");
				if (pos.Type == YamlElement.Types.Map)
				{
					rect.Up = fac.unit.Detect(pos.Get("top", null));
					rect.Down = fac.unit.Detect(pos.Get("bottom", null));
					rect.Left = fac.unit.Detect(pos.Get("left", null));
					rect.Right = fac.unit.Detect(pos.Get("right", null));
				}
				else if (pos.Type == YamlElement.Types.Sequence)
				{
					var arr = pos.List().Select(i => (Unit)fac.unit.Detect(i.Val(null)));
					rect = new Dimension(arr.ToArray());
				}
				else if (pos.Type == YamlElement.Types.Scalar)
				{
					var arr = pos.Val().Split(',').Select(i => fac.unit.Detect(i));
					rect = new Dimension(arr.ToArray());
				}
			}

			// size
			if (node.Has("size"))
			{
				var size = node.Key("size");
				if (size.Type == YamlElement.Types.Map)
				{
					rect.Width = fac.unit.Detect(size.Get("width", null));
					rect.Height = fac.unit.Detect(size.Get("height", null));
				}
				else if (size.Type == YamlElement.Types.Sequence)
				{
					var arr = size.List().Select(i => fac.unit.Detect(i.Val(null)));
					rect.Size = new Vector(arr.ToArray());
				}
				else if (size.Type == YamlElement.Types.Scalar)
				{
					var arr = size.Val().Split(',').Select(i => fac.unit.Detect(i));
					rect.Size = new Vector(arr.ToArray());
				}
			}

			// align
			if (node.Has("align"))
			{
				var align = node.Key("align");
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
				if (Enum.TryParse(x, true, out align_en)) { alignX = align_en; }
				if (Enum.TryParse(y, true, out align_en)) { alignY = align_en; }
			}
		}
		virtual public void Configure(DataMap node, ElementFactory fac) { }
	}
}
