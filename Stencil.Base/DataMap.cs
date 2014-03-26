using System;
using System.Collections.Generic;
using Stencil.Core;

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

		public static DataMap Zip(IEnumerable<string> keys, IEnumerable<string> values)
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
	}
}
