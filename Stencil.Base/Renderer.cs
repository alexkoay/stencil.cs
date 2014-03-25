using System;
using Stencil.Core;
using Stencil.Elements;

namespace Stencil
{
	abstract public class Output
	{
		public Elements.Base node;
		public Unit left, up, width, height;
	}
	public class Renderer<T> where T : Output
	{
		public T Render(Template tpl, DataMap data = null) { return Render(tpl.elem, tpl, data); }

		protected T Render(Base elem, Template tpl, DataMap data)
		{
			if (elem is Box) { return RenderE((Box)elem, tpl, data); }
			if (elem is Graphic) { return RenderE((Graphic)elem, tpl, data); }
			if (elem is QR) { return RenderE((QR)elem, tpl, data); }
			if (elem is Text) { return RenderE((Text)elem, tpl, data); }
			if (elem is Value) { return RenderE((Value)elem, tpl, data); }

			throw new Exception("Unsupported element.");
		}

		protected virtual T RenderE(Box elem, Template tpl, DataMap data) { throw new Exception("Unsupported element."); }

		protected virtual T RenderE(Value elem, Template tpl, DataMap data) { throw new Exception("Unsupported element."); }
		protected virtual T RenderE(Graphic elem, Template tpl, DataMap data) { throw new Exception("Unsupported element."); }
		protected virtual T RenderE(QR elem, Template tpl, DataMap data) { throw new Exception("Unsupported element."); }
		protected virtual T RenderE(Text elem, Template tpl, DataMap data) { throw new Exception("Unsupported element."); }
	}
}
