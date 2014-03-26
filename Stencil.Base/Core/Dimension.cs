using System;
using System.Diagnostics;

namespace Stencil.Core
{
	[DebuggerDisplay("{u")]
	public struct Dimension
	{
		Nullable<Unit> u, d, l, r;

		public Dimension(params Unit[] pos)
		{
			u = d = l = r = null;
			try
			{
				u = pos[0].Valid ? (Nullable<Unit>)pos[0] : null;
				l = pos[1].Valid ? (Nullable<Unit>)pos[1] : null;
				d = pos[2].Valid ? (Nullable<Unit>)pos[2] : null;
				r = pos[3].Valid ? (Nullable<Unit>)pos[3] : null;
			}
			catch { }
		}

		// valid size
		public bool ValidX { get { return !(FluidNegX && FluidPosX); } }
		public bool ValidY { get { return !(FluidNegY && FluidPosY); } }
		public bool Valid { get { return ValidX && ValidY; } }

		// fluid size
		public bool FluidNegX { get { return l == null; } }
		public bool FluidPosX { get { return r == null; } }
		public bool FluidNegY { get { return u == null; } }
		public bool FluidPosY { get { return d == null; } }
		public bool FluidX { get { return FluidNegX || FluidPosX; } }
		public bool FluidY { get { return FluidNegY || FluidPosY; } }
		public bool Fluid { get { return FluidX || FluidY; } }

		// fixed size
		public bool FixedX { get { return !FluidX; } }
		public bool FixedY { get { return !FluidY; } }
		public bool Fixed { get { return !Fluid; } }

		// sane position
		public Unit Up
		{
			get { return u ?? 0; }
			set { u = value.Valid ? (Nullable<Unit>)value : null; }
		}
		public Unit Down
		{
			get { return d ?? 0; }
			set { d = value.Valid ? (Nullable<Unit>)value : null; }
		}
		public Unit Left
		{
			get { return l ?? 0; }
			set { l = value.Valid ? (Nullable<Unit>)value : null; }
		}
		public Unit Right
		{
			get { return r ?? 0; }
			set { r = value.Valid ? (Nullable<Unit>)value : null; }
		}

		// sane dimension
		public Unit Width
		{
			get { return FluidX ? 0 : (r.Value - l.Value); }
			set
			{
				if (!value.Valid || value <= 0) { return; }
				if (!ValidX) { l = 0; r = value; }
				else if (FluidNegX) { l = r.Value - value; }
				else { r = l.Value + value; }
			}
		}
		public Unit Height
		{
			get { return FluidY ? 0 : (d.Value - u.Value); }
			set
			{
				if (!value.Valid || value <= 0) { return; }
				if (!ValidY) { u = 0; d = value; }
				else if (FluidNegY) { u = d.Value - value; }
				else { d = u.Value + value; }
			}
		}
		public Vector Size
		{
			get { return new Vector(Width, Height); }
			set { Width = value.x; Height = value.y; }
		}

		// calculated values
		public Vector CalcUL(Vector sz) { return new Vector(ValidX ? (l ?? (r.Value - sz.x)) : 0, ValidY ? (u ?? (d.Value - sz.y)) : 0); }
		public Vector CalcUR(Vector sz) { return new Vector(ValidX ? (r ?? (l.Value + sz.x)) : sz.x, ValidY ? (u ?? (d.Value - sz.y)) : 0); }
		public Vector CalcDL(Vector sz) { return new Vector(ValidX ? (l ?? (r.Value - sz.x)) : 0, ValidY ? (d ?? (u.Value + sz.y)) : sz.y); }
		public Vector CalcDR(Vector sz) { return new Vector(ValidX ? (r ?? (l.Value + sz.x)) : sz.x, ValidY ? (d ?? (u.Value + sz.y)) : sz.y); }
		public Vector CalcWH(Vector sz) { return new Vector(FluidX ? sz.x : Width, FluidY ? sz.y : Height); }
	}
}
