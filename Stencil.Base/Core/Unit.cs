using System;

namespace Stencil.Core
{
	public struct Unit
	{
		public static implicit operator Unit(float val) { return new Unit(val); }
		public static explicit operator int(Unit u) { return (int)Math.Ceiling(u.value); }
		public static implicit operator float(Unit u) { return u.value; }

		internal float value;
		public bool Valid { get { return !float.IsNaN(value); } }
		public Unit(float v) { value = v; }
	}
}
