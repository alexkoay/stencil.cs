
namespace Stencil.Core
{
	public struct Vector
	{
		public Unit x, y;

		public Vector(params Unit[] v)
		{
			if (v.Length == 1) { x = v[0]; y = v[0]; }
			else if (v.Length > 1) { x = v[0]; y = v[1]; }
			else { x = 0; y = 0; }
		}
		public Vector(Vector v) { x = v.x; y = v.y; }

		public override string ToString()
		{
			return string.Format("{0}, {1}", x.ToString(), y.ToString());
		}

		public static Vector operator +(Vector a, Vector b) { return new Vector { x = a.x + b.x, y = a.y + b.y }; }
		public static Vector operator -(Vector a, Vector b) { return new Vector { x = a.x - b.x, y = a.y - b.y }; }
	}
}
