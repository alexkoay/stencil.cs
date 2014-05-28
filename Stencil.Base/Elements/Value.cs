using System;
using System.Linq;
using Stencil.Core;

namespace Stencil.Elements
{
	abstract public class Value : Base
	{
		public Placeholder data = new Placeholder("");
		public bool collapse = false;
		public Value(string value) : base() { data = value; }
		public Value(YamlElement node, ElementFactory fac, DataMap def = null)
			: base(node, fac, def)
		{
			string str = "";
			switch (node.Type)
			{
				case YamlElement.Types.Scalar:
					str = node.Val();
					break;
				case YamlElement.Types.Map:
					str = node.Key("value", "").Str();
					if (node.Has("substring"))
					{
						int pos;
						var subs = node.Key("substring");
						if (subs.Type == YamlElement.Types.Map)
						{
							if (subs.Has("length") && int.TryParse(subs.Get("length"), out pos)) { data.length = pos; }
							if (subs.Has("start") && int.TryParse(subs.Get("start"), out pos)) { data.start = pos; }
						}
						else if (subs.Type == YamlElement.Types.Sequence)
						{
							var arr = subs.List().Select(i => i.Str()).ToList();
							if (arr.Count > 1 && int.TryParse(arr[1], out pos)) { data.length = pos; }
							if (arr.Count > 0 && int.TryParse(arr[0], out pos)) { data.start = pos; }
						}
						else if (subs.Type == YamlElement.Types.Scalar)
						{
							var arr = subs.Val().Split(',');
							if (arr.Length > 1 && int.TryParse(arr[1], out pos)) { data.length = pos; }
							if (arr.Length > 0 && int.TryParse(arr[0], out pos)) { data.start = pos; }
						}
					}
					break;
			}
			data.value = str.Replace("[ [ ", "[[").Replace(" ] ]", "]]");
		}
		public override void Configure(DataMap node, ElementFactory fac)
		{
			base.Configure(node, fac);
			if (node.Has("collapse"))
			{
				bool.TryParse(node.Get("collapse"), out collapse);
			}
			if (node.Has("coding"))
			{
				Placeholder.Encoding en;
				if (Enum.TryParse(node.Get("coding"), out en)) { data.coding = en; }
			}
		}
	}
}
