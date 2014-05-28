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
		public Types Type
		{
			get
			{
				var t = node.GetType();
				if (t == typeof(Y.YamlScalarNode)) { return Types.Scalar; }
				else if (t == typeof(Y.YamlSequenceNode)) { return Types.Sequence; }
				else if (t == typeof(Y.YamlMappingNode)) { return Types.Map; }
				else { return Types.None; }
			}
		}
		Y.YamlScalarNode Scalar { get { return (Y.YamlScalarNode)node; } }
		Y.YamlSequenceNode Seq { get { return (Y.YamlSequenceNode)node; } }
		Y.YamlMappingNode Map { get { return (Y.YamlMappingNode)node; } }

		public YamlElement(Y.YamlNode n) { node = n; }
		public YamlElement(string s) { node = new Y.YamlScalarNode(s); }

		public static implicit operator YamlElement(Y.YamlNode n) { return new YamlElement(n); }
		public static implicit operator Y.YamlNode(YamlElement n) { return n.node; }

		// string
		public string Str() { return node.ToString(); }

		// Scalar
		public string Val() { return Scalar.Value; }
		public string Val(string def) { try { return Scalar.Value; } catch { return def; } }

		// sequence
		public IEnumerable<YamlElement> List() { return Seq.Children.Select(i => (YamlElement)i); }
		public YamlElement Pos(int idx) { return Seq.Children[idx]; }
		public YamlElement Pos(int idx, string def) { try { return Pos(idx); } catch { return new YamlElement(def); } }

		// mapping
		public IEnumerable<KeyValuePair<string, YamlElement>> Keys() { return Map.Children.Select(i => new KeyValuePair<string, YamlElement>(i.Key.ToString(), i.Value)); }
		public bool Has(string k) { return Map.Children.ContainsKey(new YamlElement(k)); }
		public YamlElement Key(YamlElement k) { return Map.Children[k]; }
		public YamlElement Key(string k) { return Key(new YamlElement(k)); }
		public YamlElement Key(YamlElement k, YamlElement def) { try { return Key(k); } catch { return def; } }
		public YamlElement Key(YamlElement k, string def) { return Key(k, new YamlElement(def)); }
		public YamlElement Key(string k, string def) { return Key(new YamlElement(k), def); }

		// direct-to-value
		public string Get(int p) { return Pos(p).Val(); }
		public string Get(int p, string def) { try { return Get(p); } catch { return def; } }
		public string Get(string k) { return Key(k).Val(); }
		public string Get(string k, string def) { try { return Get(k); } catch { return def; } }

		// as data
		public DataMap ToData() { return new DataMap(Map.Children.ToDictionary(i => i.Key.ToString(), i => i.Value.ToString())); }
	}
}
