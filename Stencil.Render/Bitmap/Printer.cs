using System;
using System.Runtime.InteropServices;
using System.Text;
using Stencil.Core;

namespace Stencil.Render
{
	public class BitmapPrinter
	{
		public enum Type { Detect, Normal, Zebra };
		public string Name { get; private set; }
		public Printer Prt { get; private set; }
		Type type;

		public BitmapPrinter() { }
		public BitmapPrinter(string n, Type t = Type.Detect)
		{
			Name = n;
			if (t == Type.Detect)
			{
				if (Name.Contains("ZPL")
					|| Name.Contains("ZDesigner")
					|| Name.Contains("Zebra"))
				{ t = Type.Zebra; }
				else { t = Type.Normal; }
			}

			type = t;
			switch (type)
			{
				case Type.Detect: throw new Exception("Unable to detect printer Type.");
				case Type.Normal: break;
				case Type.Zebra: Prt = new ZPL(Name);  break;
			}
		}

		public void Start(string doc = "Stencil.Render.BitmapPrinter") { Prt.Start(doc); Prt.Setup(); }
		public void End() { Prt.End(); }
		public void Print(BitmapOutput bmp, int qty = 1) { Prt.Print(bmp, qty); }

		public interface Printer
		{
			void Open();
			void Close();
			void Start(string doc);
			void End();
			void Setup();
			void Print(BitmapOutput bmp, int qty);
		}
		public class ZPL : Printer
		{
			public string Name { get; private set; }

			public bool Opened { get { return handle != IntPtr.Zero; } }
			public bool Started { get { return docinfo != null; } }
			public bool Paged { get; private set; }

			IntPtr handle = IntPtr.Zero;
			Win32.DOCINFOA docinfo = null;

			public ZPL(string printer) { Name = printer; }
			~ZPL() { Close(); }

			public void Open()
			{
				if (!Opened && !Win32.OpenPrinter(Name.Normalize(), out handle, IntPtr.Zero))
				{
					handle = IntPtr.Zero;
					throw new Exception("Could not open printer.");
				}
			}
			public void Close()
			{
				if (Opened)
				{
					End();
					Win32.ClosePrinter(handle);
					handle = IntPtr.Zero;
				}
			}

			public void Start(string document)
			{
				if (!Opened) { Open(); }
				if (Started) { End(); }

				docinfo = new Win32.DOCINFOA();
				docinfo.pDocName = document;
				docinfo.pDataType = "RAW";

				if (!Win32.StartDocPrinter(handle, 1, docinfo))
				{
					docinfo = null;
					throw new Exception("Could not start document.");
				}
			}
			public void End()
			{
				if (Started)
				{
					Win32.EndDocPrinter(handle);
					docinfo = null;
				}
			}

			public void Setup()
			{
				Spool("^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^JUS^LRN^CI0^XZ");
			}
			public void Print(BitmapOutput bmp, int qty = 1)
			{
				if (qty == 0) { return; }
				var grf = bmp.ToGRF(1.0f);

				var sb = new StringBuilder();
				for (var y = 0; y < grf.GetLength(1); ++y)
					for (var x = 0; x < grf.GetLength(0); ++x)
						sb.AppendFormat("{0:X2}", grf[x, y]);

				Spool(String.Format("~DG000,{0},{1},", grf.GetLength(0) * grf.GetLength(1), grf.GetLength(0)));
				Spool(sb.ToString());
				Spool(String.Format("^XA^MMT^PW{0}^LL0{1}^LS0^FT0,0^XG000.GRF,1,1^FS^PQ{2},0,{2},Y^XZ", grf.GetLength(0) * 8, grf.GetLength(1), qty));
				Flush();
			}

			public void Page(IntPtr bytes, Int32 count)
			{
				if (!Paged && !Win32.StartPagePrinter(handle)) throw new Exception("Could not start page.");

				Int32 written = 0;
				if (!Win32.WritePrinter(handle, bytes, count, out written)) throw new Exception(String.Format("Error writing to printer ({0}).", Marshal.GetLastWin32Error()));
			}
			public void Page(byte[] bytes) { Page(Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0), bytes.Length); }

			public void Spool(byte[] bytes) { Page(bytes); }
			public void Spool(string str) { Spool(Encoding.GetEncoding("Latin1").GetBytes(str + "\n")); }
			public void Flush()
			{
				if (Paged)
				{
					Win32.EndPagePrinter(handle);
					Paged = false;
				}
			}
		}

		class Win32
		{
			// Structure and API declarions:
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
			public class DOCINFOA
			{
				[MarshalAs(UnmanagedType.LPStr)]
				public string pDocName;
				[MarshalAs(UnmanagedType.LPStr)]
				public string pOutputFile;
				[MarshalAs(UnmanagedType.LPStr)]
				public string pDataType;
			}
			[DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
			public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

			[DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
			public static extern bool ClosePrinter(IntPtr hPrinter);

			[DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
			public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

			[DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
			public static extern bool EndDocPrinter(IntPtr hPrinter);

			[DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
			public static extern bool StartPagePrinter(IntPtr hPrinter);

			[DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
			public static extern bool EndPagePrinter(IntPtr hPrinter);

			[DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
			public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);
		}
	}
}
