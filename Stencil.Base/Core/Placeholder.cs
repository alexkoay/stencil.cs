
namespace Stencil.Core
{
	public struct Placeholder
	{
		public enum Encoding { None, b64 }

		public Encoding coding;
		string value;
		public Placeholder(string s, Encoding en = Encoding.None)
		{
			value = s ?? "";
			coding = en;
		}

		public string parse(DataMap data) {
			if (data == null) { return value; }

			string hold = value;
			foreach (var pair in data) { hold = hold.Replace("[[" + pair.Key + "]]", pair.Value); }
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
