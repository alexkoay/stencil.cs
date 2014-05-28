using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Stencil.Tools.Editor
{
	public partial class Viewer : Window
	{
		string tplname, dataname;
		FileSystemWatcher twatch, dwatch;
		DateTime lasttpl, lastdata;
		DispatcherTimer timer;
		TimeSpan parsetpl, parsedat, render, last;

		Render.BitmapRenderer ren = new Render.BitmapRenderer();
		Render.BitmapPrinter prt = new Render.BitmapPrinter();
		Template tpl;
		DataMap[] dat;
		int pos;

		Render.BitmapOutput bitmap;

		public Viewer() { InitializeComponent(); }

		public bool OpenTemplate(string file)
		{
			if (file == null) { return false; }

			try { ReloadTemplate(file); }
			catch { return false; }

			tplname = file;

			if (twatch != null) { twatch.EnableRaisingEvents = false; }
			twatch = new FileSystemWatcher(Path.GetDirectoryName(file), Path.GetFileName(file));
			twatch.NotifyFilter = NotifyFilters.LastWrite;
			twatch.Changed += new FileSystemEventHandler(TemplateChanged);
			twatch.EnableRaisingEvents = true;
			return true;
		}

		public bool OpenData(string file)
		{
			if (file != null)
			{
				try { ReloadData(file); }
				catch { return false; }
			}

			dataname = file;
			if (dwatch != null)
			{
				dwatch.EnableRaisingEvents = false;
				dwatch = null;
			}
			if (file != null)
			{
				dwatch = new FileSystemWatcher(Path.GetDirectoryName(file), Path.GetFileName(file));
				dwatch.NotifyFilter = NotifyFilters.LastWrite;
				dwatch.Changed += new FileSystemEventHandler(DataChanged);
				dwatch.EnableRaisingEvents = true;
			}
			return true;
		}

		void ReloadTemplate(string file = null)
		{
			if (file == null) { file = tplname; }
		rtpl:
			try
			{
				var sw = Stopwatch.StartNew();
				tpl = Stencil.Template.FromFile(file)[0];
				sw.Stop();
				parsetpl = last = sw.Elapsed;
			}
			catch (IOException) { goto rtpl; }
			catch (Exception) { throw; }
		}
		void ReloadData(string file = null)
		{
			if (file == null) { file = dataname; }
		rdat:
			try
			{
				var sw = Stopwatch.StartNew();
				dat = DataMap.FromFile(file).ToArray();
				if (pos >= dat.Length) { pos = 0; }
				sw.Stop();
				parsedat = last = sw.Elapsed;
			}
			catch (IOException) { goto rdat; }
		}
		void Redraw()
		{
			var sw = Stopwatch.StartNew();
			bitmap = tpl != null ? ren.Render(tpl, dat != null && dat.Length > 0 ? dat[pos] : null) : null;
			sw.Stop();
			render = last = sw.Elapsed;

			if (bitmap == null) { preview.Source = null; }
			else
			{
				BitmapImage bmpi = new BitmapImage();
				using (var mem = new MemoryStream())
				{
					bitmap.render.Save(mem, System.Drawing.Imaging.ImageFormat.Png);
					mem.Position = 0;
					bmpi.BeginInit();
					bmpi.StreamSource = mem;
					bmpi.CacheOption = BitmapCacheOption.OnLoad;
					bmpi.EndInit();
				}
				preview.Source = bmpi;
			}
		}

		void Save()
		{
			var temp = new Bitmap(bitmap.render);
			for (int y = 0; y < temp.Height; ++y)
				for (int x = 0; x < temp.Width; ++x)
					if (temp.GetPixel(x, y).A == 0) { temp.SetPixel(x, y, Color.White); }
			temp.Save("test.png");
		}
		void Print()
		{
			try
			{
				prt.Start("LabelViewer");
				prt.Print(bitmap);
				prt.End();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
		}

		void SetInfo(string txt, double delay = 0)
		{
			if (timer != null) { timer.Stop(); }
			if (delay > 0)
				timer = new DispatcherTimer(
					TimeSpan.FromMilliseconds(delay),
					DispatcherPriority.Normal,
					new EventHandler((o, e) =>
					{
						info.Text = "";
						timer.Stop(); timer = null;
					}), Dispatcher);
			info.Text = txt;
		}
		void UINone()
		{
			try
			{
				var sw = Stopwatch.StartNew();
				Redraw();
				sw.Stop();
				SetInfo(string.Format("Draw. Time taken: {0} (img: {1}, left: {2})", sw.Elapsed, render, sw.Elapsed - render));
			}
			catch (Exception e) { SetInfo(e.ToString()); }
		}
		void UITemplate()
		{
			try
			{
				var sw = Stopwatch.StartNew();
				ReloadTemplate();
				Redraw();
				sw.Stop();
				SetInfo(string.Format("Loaded template & draw. Time taken: {0} (tpl: {1}, img: {2}, left: {3})", sw.Elapsed, parsetpl, render, sw.Elapsed - parsetpl - render));
			}
			catch (Exception e) { SetInfo(e.ToString()); }
		}
		void UIDraw()
		{
			try
			{
				var sw = Stopwatch.StartNew();
				ReloadData();
				Redraw();
				sw.Stop();
				SetInfo(string.Format("Loaded data & draw. Time taken: {0} (dat: {1}, img: {2}, left: {3})", sw.Elapsed, parsedat, render, sw.Elapsed - parsedat - render));
			}
			catch (Exception e) { SetInfo(e.ToString()); }
		}
		void UIBoth()
		{
			if (tplname == null) { return; }
			try
			{
				var sw = Stopwatch.StartNew();
				ReloadTemplate();
				ReloadData();
				Redraw();
				sw.Stop();
				SetInfo(string.Format("Loaded files & draw. Time taken: {0} (tpl: {1}, dat: {2}, img: {3}, left: {4})", sw.Elapsed, parsetpl, parsedat, render, sw.Elapsed - parsetpl - parsedat - render));
			}
			catch (Exception e) { SetInfo(e.ToString()); }
		}

		void TemplateChanged(object sender, FileSystemEventArgs e)
		{
			DateTime last = File.GetLastWriteTime(e.FullPath);
			if (lasttpl != last)
				Dispatcher.Invoke(DispatcherPriority.Normal, (Action)UITemplate);
			lasttpl = last;
		}
		void DataChanged(object sender, FileSystemEventArgs e)
		{
			DateTime last = File.GetLastWriteTime(e.FullPath);
			if (lastdata != last)
				Dispatcher.Invoke(DispatcherPriority.Normal, (Action)UIDraw);
			lastdata = last;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) { UIBoth(); }
		void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Left:
					if (--pos < 0) pos += dat.Length;
					if (dat.Length > 1) UINone();
					break;
				case Key.Right:
					if (++pos >= dat.Length) pos -= dat.Length;
					if (dat.Length > 1) UINone();
					break;
				case Key.E: UINone(); break;
				case Key.R: UIBoth(); break;
				case Key.T: UITemplate(); break;
				case Key.Y: UIDraw(); break;
				case Key.O: Save(); SetInfo("Saved as picture.", 3000); break;
				case Key.P: Print(); SetInfo("Printed label.", 3000); break;
				case Key.A: Topmost = !Topmost; SetInfo(Topmost ? "Fixed to top" : "Unfixed from top", 5000); break;
				case Key.S:
					if (preview.Stretch == System.Windows.Media.Stretch.Uniform)
					{
						switch (preview.StretchDirection)
						{
							case System.Windows.Controls.StretchDirection.Both: preview.StretchDirection = System.Windows.Controls.StretchDirection.DownOnly; SetInfo("Scale down only", 3000); break;
							case System.Windows.Controls.StretchDirection.DownOnly: preview.StretchDirection = System.Windows.Controls.StretchDirection.UpOnly; SetInfo("Scale up only", 3000); break;
							case System.Windows.Controls.StretchDirection.UpOnly: preview.Stretch = System.Windows.Media.Stretch.None; SetInfo("Scale off", 3000); break;
						}
					}
					else
					{
						preview.Stretch = System.Windows.Media.Stretch.Uniform;
						preview.StretchDirection = System.Windows.Controls.StretchDirection.Both;
						SetInfo("Scale on", 3000);
					}
					break;
				case Key.D:
					{
						var dlg = new Microsoft.Win32.OpenFileDialog();
						dlg.Filter = "Data File (*.dat)|*.dat";
						dlg.Title = "Open Data File";
						if (dlg.ShowDialog() ?? false) { OpenData(Path.GetFullPath(dlg.FileName)); }
						else { OpenData(null); }
						Redraw();
						break;
					}
				case Key.F:
					{
						var dlg = new Microsoft.Win32.OpenFileDialog();
						dlg.Filter = "Template File (*.tpl)|*.tpl";
						dlg.Title = "Open Template File";
						if (dlg.ShowDialog() ?? false) { OpenTemplate(Path.GetFullPath(dlg.FileName)); }
						else { OpenTemplate(null); }
						Redraw();
						break;
					}
				case Key.C: System.GC.Collect(); SetInfo("Garbage collected.", 3000); break;
			}
		}
	}
}
