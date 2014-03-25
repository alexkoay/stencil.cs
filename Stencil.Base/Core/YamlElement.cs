using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Y = YamlDotNet.RepresentationModel;

namespace Stencil.Core
{
	public class YamlElement
	{
		public enum Types { None, Scalar, Sequence, Map };

		public Y.YamlNode node;
		public Types type {
			get
			{
				var t = node.GetType();
				if (t == typeof(Y.YamlScalarNode)) { return Types.Scalar; }
				else if (t == typeof(Y.YamlSequenceNode)) { return Types.Sequence; }
				else if (t == typeof(Y.YamlMappingNode)) { return Types.Map; }
				else { return Types.None; }
			}
		}
		Y.YamlScalarNode scalar { get { return (Y.YamlScalarNode)node; } }
		Y.YamlSequenceNode seq { get { return (Y.YamlSequenceNode)node; } }
		Y.YamlMappingNode map { get { return (Y.YamlMappingNode)node; } }

		public YamlElement(Y.YamlNode n) { node = n; }
		public YamlElement(string s) { node = new Y.YamlScalarNode(s); }

		public static implicit operator YamlElement(Y.YamlNode n) { return new YamlElement(n); }
		public static implicit operator Y.YamlNode(YamlElement n) { return n.node; }

		// string
		public string str() { return node.ToString(); }

		// scalar
		public string val() { return scalar.Value; }
		public string val(string def) { try { return scalar.Value; } catch { return def; } }

		// sequence
		public IEnumerable<YamlElement> list() { return seq.Children.Select(i => (YamlElement)i); }
		public YamlElement pos(int idx) { return seq.Children[idx]; }
		public YamlElement pos(int idx, string def) { try { return pos(idx); } catch { return new YamlElement(def); } }

		// mapping
		public IEnumerable<KeyValuePair<string, YamlElement>> keys() { return map.Children.Select(i => new KeyValuePair<string, YamlElement>(i.Key.ToString(), i.Value)); }
		public bool hasKey(string k) { return map.Children.ContainsKey(new YamlElement(k)); }
		public YamlElement key(YamlElement k) { return map.Children[k]; }
		public YamlElement key(string k) { return key(new YamlElement(k)); }
		public YamlElement key(YamlElement k, YamlElement def) { try { return key(k); } catch { return def; } }
		public YamlElement key(YamlElement k, string def) { return key(k, new YamlElement(def)); }
		public YamlElement key(string k, string def) { return key(new YamlElement(k), def); }

		// direct-to-value
		public string get(int p) { return pos(p).val(); }
		public string get(int p, string def) { try { return get(p); } catch { return def; } }
		public string get(string k) { return key(k).val(); }
		public string get(string k, string def) { try { return get(k); } catch { return def; } }

		// as data
		public DataMap ToData() { return new DataMap(map.Children.ToDictionary(i => i.Key.ToString(), i => i.Value.ToString())); }
	}
}
