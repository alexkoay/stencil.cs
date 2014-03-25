using System;
using System.Text.RegularExpressions;

namespace Stencil.Core
{
	public class UnitFactory
	{
		public float ppi = 300;
		public Func<float, Unit> def = null;

		public UnitFactory(float p = 300, string type = "") { ppi = p; def = Factory(type); }

		public Unit Pixel(float val) { return new Unit(val); }
		public Unit Inch(float val) { return new Unit(val * ppi); }
		public Unit Milimeter(float val) { return Inch(val / 25.4f); }
		public Unit Centimeter(float val) { return Inch(val / 2.54f); }
		public Unit Point(float val) { return Inch(val / 72.0f); }

		public Func<float, Unit> Factory(string unit, Func<float, Unit> df = null)
		{
			switch (unit.ToLower())
			{
				case "px": return Pixel;
				case "in": return Inch;
				case "mm": return Milimeter;
				case "cm": return Centimeter;
				case "pt": return Point;
			}
			return df ?? def ?? Pixel;
		}

		static Regex rx = new Regex("^\\s*(?<num>NaN)|(?:(?<num>-?[0-9]+(?:\\.[0-9]*)?(?:e[+-]?[0-9]+)?)\\s*(?<unit>.+)?)\\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		public Unit Detect(string str, Func<float, Unit> df = null)
		{
			try
			{
				var match = rx.Match(str);
				Func<float, Unit> fn = Factory(match.Groups["unit"].Value, df);
				float num = float.Parse(match.Groups["num"].Value);
				return fn(num);
			}
			catch { return new Unit(float.NaN); }
		}
	}
}
