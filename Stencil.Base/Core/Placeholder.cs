
namespace Stencil.Core
{
	public struct Placeholder
	{
		public enum Encoding { None, b64 }

		public string value;
		public int start, length;
		public Encoding coding;

		public Placeholder(string s)
		{
			value = s ?? "";
			coding = Encoding.None;
			start = 0;
			length = -1;
		}

		public string Parse(DataMap data) {
			if (data == null) { return value; }

			string hold = value;
			foreach (var pair in data) { hold = hold.Replace("[[" + pair.Key + "]]", pair.Value); }

			// substring
			if (start > 0 || length > 0)
			{
				hold = (length <= 0)
					? hold.Substring(start)
					: hold.Substring(start, length);
			}

			return Encode(coding, hold);
		}

		public static implicit operator Placeholder(string s) { return new Placeholder(s ?? ""); }
		public static implicit operator string(Placeholder p) { return p.value ?? ""; }

		public static string Encode(Encoding enc, string data)
		{
			switch (enc)
			{
				case Encoding.b64:
					var utf = System.Text.Encoding.UTF8.GetBytes(data);
					var code = System.Convert.ToBase64String(utf);
					return code.TrimEnd('=');
				default:
					return data;
			}
		}
	}
}
