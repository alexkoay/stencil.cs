using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Stencil.Core;
using Stencil.Elements;
using Y = YamlDotNet.RepresentationModel;

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

					if (node.hasKey("vars"))
					{
						var vars = node.key("vars");
						if (vars.type == YamlElement.Types.Sequence)
							foreach (var item in vars.list())
								switch (item.type)
								{
									case YamlElement.Types.Scalar:
										{
											var arr = item.val().Split(',');
											if (arr.Length > 1) { tpl.variables.Add(arr[0], arr[1]); }
											else if (arr.Length > 0) { tpl.variables.Add(arr[0], arr[0]); }
											break;
										}
									case YamlElement.Types.Sequence:
										{
											var arr = item.list().Select(i => i.str()).ToList();
											if (arr.Count > 1) { tpl.variables.Add(arr[0], arr[1]); }
											else if (arr.Count > 0) { tpl.variables.Add(arr[0], arr[0]); }
											break;
										}
								}
					}

					tpls.Add(tpl);
				}
				return tpls;
			}
		}
	}
}
