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
		DispatcherTimer timer;

		string tpl_file;
		FileSystemWatcher tpl_watch;
		DateTime tpl_last;
		Template template;

		string data_file;
		FileSystemWatcher data_watch;
		DateTime data_last;
		DataMap[] data;
		int pos;

		Render.BitmapRenderer ren = new Render.BitmapRenderer();
		Render.BitmapOutput bitmap;
		Render.BitmapPrinter prt = new Render.BitmapPrinter();

		TimeSpan parse_tpl, parse_data, render, time_last;
		string ts_format = "s\\.fffffff";

		public Viewer() { InitializeComponent(); }

		public bool OpenTemplate(string file)
		{
			if (file == null) { return false; }

			LoadTemplate(file);
			tpl_file = file;

			if (tpl_watch != null) { tpl_watch.EnableRaisingEvents = false; }
			tpl_watch = new FileSystemWatcher(Path.GetDirectoryName(file), Path.GetFileName(file));
			tpl_watch.NotifyFilter = NotifyFilters.LastWrite;
			tpl_watch.Changed += new FileSystemEventHandler(TemplateChanged);
			tpl_watch.EnableRaisingEvents = true;
			return true;
		}
		public bool OpenData(string file)
		{
			if (file == null) { return false; }

			LoadData(file);
			data_file = file;
			if (data_watch != null) { data_watch.EnableRaisingEvents = false; }
			data_watch = new FileSystemWatcher(Path.GetDirectoryName(file), Path.GetFileName(file));
			data_watch.NotifyFilter = NotifyFilters.LastWrite;
			data_watch.Changed += new FileSystemEventHandler(DataChanged);
			data_watch.EnableRaisingEvents = true;
			return true;
		}

		bool LoadTemplate(string file = null)
		{
			if (file == null) { file = tpl_file; }
			if (file == null) { return false; }
		rtpl:
			try
			{
				var sw = Stopwatch.StartNew();
				template = Stencil.Template.FromFile(file)[0];
				sw.Stop();
				parse_tpl = time_last = sw.Elapsed;
			}
			catch (IOException) { goto rtpl; }
			return true;
		}
		bool LoadData(string file = null)
		{
			if (file == null) { file = data_file; }
			if (file == null) { return false; }
		rdat:
			try
			{
				var sw = Stopwatch.StartNew();
				data = DataMap.FromFile(file).ToArray();
				if (pos >= data.Length) { pos = 0; }
				sw.Stop();
				parse_data = time_last = sw.Elapsed;
			}
			catch (IOException) { goto rdat; }
			return true;
		}

		void Render()
		{
			var dat = data != null && data.Length > 0 ? data[pos] : null;

			var sw = Stopwatch.StartNew();
			bitmap = template != null ? ren.Render(template, dat) : null;
			sw.Stop();
			render = time_last = sw.Elapsed;

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

			Title = "";
			if (data != null && data.Length > 1) { Title += String.Format("[{0}/{1}] ", pos + 1, data.Length); }
			Title += AppInfo.name;
			if (tpl_file != null) Title += ": " + Path.GetFileName(tpl_file);
			if (data_file != null) Title += " (" + Path.GetFileName(data_file) + ")";
		}

		void SetInfo(string txt, double delay = 0)
		{
			if (timer != null) { timer.Stop(); }
			if (delay > 0)
			{
				timer = new DispatcherTimer(
				TimeSpan.FromMilliseconds(delay),
				DispatcherPriority.Normal,
				new EventHandler((o, e) =>
				{
					info.Text = "";
					timer.Stop(); timer = null;
				}), Dispatcher);
			}
			info.Text = txt;
			status.Height = new GridLength(28, GridUnitType.Pixel);
		}

		void UINone()
		{
			try
			{
				var sw = Stopwatch.StartNew();
				Render();
				sw.Stop();
				SetInfo(string.Format("Draw. Time taken: {0} (img: {1}, left: {2})",
					sw.Elapsed.ToString(ts_format),
					render.ToString(ts_format),
					(sw.Elapsed - render).ToString(ts_format)));
			}
			catch (Exception e) { SetInfo(e.ToString()); }
		}
		void UITemplate()
		{
			try
			{
				var sw = Stopwatch.StartNew();
				LoadTemplate();
				Render();
				sw.Stop();
				SetInfo(string.Format("Loaded template & draw. Time taken: {0} (tpl: {1}, img: {2}, left: {3})",
					sw.Elapsed.ToString(ts_format),
					parse_tpl.ToString(ts_format),
					render.ToString(ts_format),
					(sw.Elapsed - parse_tpl - render).ToString(ts_format)));
			}
			catch (Exception e) { SetInfo(e.ToString()); }
		}
		void UIData()
		{
			try
			{
				var sw = Stopwatch.StartNew();
				LoadData();
				Render();
				sw.Stop();
				SetInfo(string.Format("Loaded data & draw. Time taken: {0} (dat: {1}, img: {2}, left: {3})",
					sw.Elapsed.ToString(ts_format),
					parse_data.ToString(ts_format),
					render.ToString(ts_format),
					(sw.Elapsed - parse_data - render).ToString(ts_format)));
			}
			catch (Exception e) { SetInfo(e.ToString()); }
		}
		void UIBoth()
		{
			try
			{
				var sw = Stopwatch.StartNew();
				LoadTemplate();
				LoadData();
				Render();
				sw.Stop();
				SetInfo(string.Format("Loaded files & draw. Time taken: {0} (tpl: {1}, dat: {2}, img: {3}, left: {4})",
					sw.Elapsed.ToString(ts_format),
					parse_tpl.ToString(ts_format),
					parse_data.ToString(ts_format),
					render.ToString(ts_format), 
					(sw.Elapsed - parse_tpl - parse_data - render).ToString(ts_format)));
			}
			catch (Exception e) { SetInfo(e.ToString()); }
		}

		void TemplateChanged(object sender, FileSystemEventArgs e)
		{
			DateTime last = File.GetLastWriteTime(e.FullPath);
			if (tpl_last != last)
				Dispatcher.Invoke(DispatcherPriority.Normal, (Action)UITemplate);
			tpl_last = last;
		}
		void DataChanged(object sender, FileSystemEventArgs e)
		{
			DateTime last = File.GetLastWriteTime(e.FullPath);
			if (data_last != last)
				Dispatcher.Invoke(DispatcherPriority.Normal, (Action)UIData);
			data_last = last;
		}

		void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (tpl_file == null) { SetInfo("No template loaded."); }
			else { UIBoth(); }
		}
		void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Left:
					if (data != null)
					{
						if (--pos < 0) pos += data.Length;
						if (data.Length > 1) UINone();
					}
					break;
				case Key.Right:
					if (data != null)
					{
						if (++pos >= data.Length) pos -= data.Length;
						if (data.Length > 1) UINone();
					}
					break;

				case Key.OemTilde: UINone(); break;
				case Key.D1: UITemplate(); break;
				case Key.D2: UIData(); break;
				case Key.D3: UIBoth(); break;

				case Key.O:
					if (bitmap != null)
					{
						var temp = new Bitmap(bitmap.render);
						for (int y = 0; y < temp.Height; ++y)
							for (int x = 0; x < temp.Width; ++x)
								if (temp.GetPixel(x, y).A == 0) { temp.SetPixel(x, y, Color.White); }

						var dlg = new Microsoft.Win32.SaveFileDialog();
						dlg.Filter = "Image file (*.png)|*.png";
						dlg.Title = "Save Image File";
						if (dlg.ShowDialog() ?? false)
						{
							temp.Save(Path.GetFullPath(dlg.FileName));
							SetInfo("Saved to " + Path.GetFullPath(dlg.FileName), 3000);
						}
					}
					break;
				case Key.P:
					if (bitmap != null)
					{
						try
						{
							var prt = new Render.BitmapPrinter();
							prt.Start(AppInfo.name);
							prt.Print(bitmap);
							prt.End();
							SetInfo("Printed label.", 3000);
						}
						catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
					}
					break;

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
						UIData();
						break;
					}
				case Key.F:
					{
						var dlg = new Microsoft.Win32.OpenFileDialog();
						dlg.Filter = "Template File (*.tpl)|*.tpl";
						dlg.Title = "Open Template File";
						if (dlg.ShowDialog() ?? false) { OpenTemplate(Path.GetFullPath(dlg.FileName)); }
						UITemplate();
						break;
					}

				case Key.C: System.GC.Collect(); SetInfo("Garbage collected.", 3000); break;
				case Key.Escape:
					if (MessageBox.Show("Exit the program?", "Quit", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
						== MessageBoxResult.Yes)
					{
						Close();
					}
					break;
			}
		}
	}
}
