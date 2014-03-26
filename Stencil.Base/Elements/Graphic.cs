using Stencil.Core;

namespace Stencil.Elements
{
	public class Graphic : Value
	{
		public Graphic(string value) : base(value) { }
		public Graphic(YamlElement node, ElementFactory fac, DataMap def = null)
			: base(node, fac, def ?? (fac.style.ContainsKey("graphic") ? fac.style["graphic"] : null))
		{
		}
	}
}
