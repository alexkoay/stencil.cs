using System;
using System.Collections.Specialized;
using Stencil.Core;
using Stencil.Elements;
using Y = YamlDotNet.RepresentationModel;
using System.Collections.Generic;
using System.IO;

namespace Stencil
{
	public class Template
	{
		public OrderedDictionary variables = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);

		public Base elem = null;
		public DataMap values = new DataMap();

		public Template(Base cont) { elem = cont; }
		public Template() : this(new Box()) { }

		public static List<Template> FromFile(string filename)
		{
			using (var txt = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var dat = new StreamReader(txt))
			{
				var yaml = new Y.YamlStream();
				yaml.Load(dat);

				var tpls = new System.Collections.Generic.List<Template>();
				foreach (var d in yaml.Documents)
				{
					var node = (YamlElement)d.RootNode;

					float ppi = 300;
					string unit = "";
		
					if (node.hasKey("dpi"))
					{
						float res;
						if (float.TryParse(node.get("dpi"), out res)) { ppi = res; }
					}
					if (node.hasKey("unit"))
					{
						unit = node.get("unit");
					}
					
					var fac = new ElementFactory(ppi, unit);
					var tpl = new Template(fac.Detect(node));
					tpls.Add(tpl);
				}
				return tpls;
			}
		}
	}
}
