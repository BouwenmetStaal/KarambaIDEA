//using log4net;
using System;
using System.Reflection;
using System.Windows;

// Configure log4net using the .config file
// [assembly: log4net.Config.XmlConfigurator(Watch = true)]
// This will cause log4net to look for a configuration file
// called ConsoleApp.exe.config in the application base
// directory (i.e. the directory containing ConsoleApp.exe)

namespace ConnectionLinkTestApp
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static string IdeaInstallDir;
// public static readonly ILog LinkLogger = LogManager.GetLogger("ConnectionLinkLogger");

        static App()
		{
            IdeaInstallDir = KarambaIDEA.Properties.Settings.Default.IdeaInstallDir;
            //IdeaInstallDir = CONOPT.Properties.Settings.Default.IdeaInstallDir;

			//AppDomain currentDomain = AppDomain.CurrentDomain;
			//currentDomain.AssemblyResolve += new ResolveEventHandler(IdeaResolveEventHandler);
		}

		private static Assembly IdeaResolveEventHandler(object sender, ResolveEventArgs args)
		{
			AssemblyName asmName = new AssemblyName(args.Name);

			string assemblyFileName = System.IO.Path.Combine(IdeaInstallDir, asmName.Name + ".dll");
			if (System.IO.File.Exists(assemblyFileName))
			{
				return Assembly.LoadFile(assemblyFileName);
			}

			return args.RequestingAssembly;
		}
	}
}