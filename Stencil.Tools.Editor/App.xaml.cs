using System.Windows;

namespace Stencil.Tools.Editor
{
	public static class AppInfo
	{
		public const string name = "Label Editor";
		public const string version = "1.0.0";
	}

	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			Viewer v = new Viewer();
			if (e.Args.Length > 0) { v.OpenTemplate(e.Args[0]); }
			if (e.Args.Length > 1) { v.OpenData(e.Args[1]); }
			v.Show();
		}
	}
}
