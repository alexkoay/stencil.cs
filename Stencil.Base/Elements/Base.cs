using System.Linq;
using Stencil.Core;
using System;

namespace Stencil.Elements
{
	abstract public class Base
	{
		public enum Alignment {
			None = 0,
			Min, Left = Min, Top = Min, Up = Min,
			Mid, Center = Mid,
			Max, Right = Max, Bottom = Max, Down = Max };

		public Dimension rect;
		public Alignment alignX, alignY;

		public Base() { }
		public Base(YamlElement node, ElementFactory fac, YamlElement def = null)
		{
			if (def != null) { Configure(def, fac); }
			if (node.type != YamlElement.Types.Map) { return; }

			// position
			if (node.hasKey("pos"))
			{
				var pos = node.key("pos");
				if (pos.type == YamlElement.Types.Map)
				{
					rect.Up = fac.unit.Detect(pos.get("top", null));
					rect.Down = fac.unit.Detect(pos.get("bottom", null));
					rect.Left = fac.unit.Detect(pos.get("left", null));
					rect.Right = fac.unit.Detect(pos.get("right", null));
				}
				else if (pos.type == YamlElement.Types.Sequence) {
					var arr = pos.list().Select(i => (Unit)fac.unit.Detect(i.val(null)));
					rect = new Dimension(arr.ToArray());
				}
				else if (pos.type == YamlElement.Types.Scalar)
				{
					var arr = pos.val().Split(',').Select(i => fac.unit.Detect(i));
					rect = new Dimension(arr.ToArray());
				}
			}

			// size
			if (node.hasKey("size"))
			{
				var size = node.key("size");
				if (size.type == YamlElement.Types.Map)
				{
					rect.Width = fac.unit.Detect(size.get("width", null));
					rect.Height = fac.unit.Detect(size.get("height", null));
				}
				else if (size.type == YamlElement.Types.Sequence) {
					var arr = size.list().Select(i => fac.unit.Detect(i.val(null)));
					rect.Size = new Vector(arr.ToArray());
				}
				else if (size.type == YamlElement.Types.Scalar)
				{
					var arr = size.val().Split(',').Select(i => fac.unit.Detect(i));
					rect.Size = new Vector(arr.ToArray());
				}
			}

			// align
			if (node.hasKey("align"))
			{
				var align = node.key("align");
				string x = null, y = null;
				if (align.type == YamlElement.Types.Map)
				{
					x = align.get("h", null);
					y = align.get("v", null);
				}
				else if (align.type == YamlElement.Types.Sequence)
				{
					var arr = align.list().Select(i => i.ToString()).ToList();
					if (arr.Count == 1) { x = y = arr[0]; }
					else if (arr.Count > 1) { x = arr[0]; y = arr[1]; }
				}
				else if (align.type == YamlElement.Types.Scalar)
				{
					var arr = align.val().Split(',');
					if (arr.Length == 1) { x = y = arr[0]; }
					else if (arr.Length > 1) { x = arr[0]; y = arr[1]; }
				}

				Alignment align_en;
				if (Enum.TryParse(x, true, out align_en)) { alignX = align_en; }
				if (Enum.TryParse(y, true, out align_en)) { alignY = align_en; } 
			}
		}
		virtual public void Configure(YamlElement node, ElementFactory fac) { }
	}
}
