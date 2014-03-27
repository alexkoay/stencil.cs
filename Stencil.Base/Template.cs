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
		
					if (node.Has("dpi"))
					{
						float res;
						if (float.TryParse(node.Get("dpi"), out res)) { ppi = res; }
					}
					if (node.Has("unit"))
					{
						unit = node.Get("unit");
					}
					
					var fac = new ElementFactory(ppi, unit);
					var tpl = new Template(fac.Detect(node));

					tpl.values.Set("desc", node.Get("desc", Path.GetFileName(filename)));
					tpl.values.Set("path", Path.GetDirectoryName(filename));
					tpl.values.Set("media", node.Get("media", "label"));

					if (node.Has("vars"))
					{
						var vars = node.Key("vars");
						if (vars.Type == YamlElement.Types.Sequence)
							foreach (var item in vars.List())
								switch (item.Type)
								{
									case YamlElement.Types.Scalar:
										{
											var arr = item.Val().Split(',');
											if (arr.Length > 1) { tpl.variables.Add(arr[0], arr[1]); }
											else if (arr.Length > 0) { tpl.variables.Add(arr[0], arr[0]); }
											break;
										}
									case YamlElement.Types.Sequence:
										{
											var arr = item.List().Select(i => i.Str()).ToList();
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
