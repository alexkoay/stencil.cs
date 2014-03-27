using System;
using Stencil.Core;

namespace Stencil.Elements
{
	abstract public class Value : Base
	{
		public Placeholder data;
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
