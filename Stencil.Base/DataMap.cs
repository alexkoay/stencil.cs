using System;
using System.Collections.Generic;
using System.IO;
using Stencil.Core;
using Y = YamlDotNet.RepresentationModel;

namespace Stencil
{
	public class DataMap
	{
		Dictionary<string, string> data;
		public DataMap() : this(new Dictionary<string, string>()) { }
		public DataMap(Dictionary<string, string> dict) { data = dict ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); }
		public DataMap(YamlElement node) { data = node.ToData(); }
		public IEnumerator<KeyValuePair<string, string>> GetEnumerator() { return data.GetEnumerator(); }

		public static implicit operator DataMap(Dictionary<string, string> d) { return new DataMap(d); }
		public static implicit operator Dictionary<string, string>(DataMap d) { return d.data; }

		public static DataMap Zip(ICollection<string> keys, ICollection<string> values)
		{
			var data = new DataMap();
			var ki = keys.GetEnumerator();
			var vi = values.GetEnumerator();
			while (ki.MoveNext()) { data.Set(ki.Current, vi.MoveNext() ? vi.Current : ""); }
			return data;
		}

		public bool Has(string k) { return data.ContainsKey(k); }
		public void Set(string k, string v) { data[k] = v; }
		public string Get(string k) { return data[k]; }
		public string Get(string k, string def) { try { return data[k]; } catch { return def; } }

		public DataMap Join(Dictionary<string, string> dict) { foreach (var pair in dict) data[pair.Key] = pair.Value; return this; }
		public DataMap Copy() { return new DataMap().Join(data); }

		public static DataMap operator +(DataMap a, DataMap b) { return a.Copy().Join(b); }

		public static List<DataMap> FromFile(string filename)
		{
			using (var txt = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var dat = new StreamReader(txt))
			{
				var yaml = new Y.YamlStream();
				yaml.Load(dat);

				var dats = new System.Collections.Generic.List<DataMap>();
				foreach (var d in yaml.Documents)
				{
					var node = (YamlElement)d.RootNode;
					if (node.Type != YamlElement.Types.Map) { continue; }
					dats.Add(new DataMap(d.RootNode));
				}
				return dats;
			}
		}
	}
}
