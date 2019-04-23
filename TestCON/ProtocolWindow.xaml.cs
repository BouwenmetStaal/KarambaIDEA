using System.Diagnostics;
using System.IO.Compression;
using System.Windows;

namespace ConnectionLinkTestApp
{
	/// <summary>
	/// Interaction logic for ProtocolWindow.xaml
	/// </summary>
	public partial class ProtocolWindow : Window
	{
		private string reportDir = null;

		public ProtocolWindow()
		{
			InitializeComponent();
		}

		public ProtocolWindow(string archiveName)
		{
			InitializeComponent();

			try
			{
				string fileNameToShow = string.Empty;
				string fileNameExt = System.IO.Path.GetExtension(archiveName);
				if (fileNameExt.Equals(".zip", System.StringComparison.InvariantCultureIgnoreCase))
				{
					string dirPath = System.IO.Path.GetDirectoryName(archiveName);
					string dirName = System.IO.Path.GetFileNameWithoutExtension(archiveName);

					reportDir = System.IO.Path.Combine(dirPath, dirName);
					System.IO.Directory.CreateDirectory(reportDir);

					ZipFile.ExtractToDirectory(archiveName, reportDir);
					fileNameToShow = string.Format("{0}\\CombinedReport.html", reportDir);
				}
				else
				{
					fileNameToShow = archiveName;
				}

				webBrowser.Navigate(fileNameToShow);
			}
			catch
			{
				Debug.Assert(false);
			}
		}
	}
}