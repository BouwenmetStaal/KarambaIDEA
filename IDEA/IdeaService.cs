using System;
using IdeaRS.OpenModel;
using IdeaRS.OpenModel.Result;

using IdeaStatiCa.Plugin;

using System.IO;
using System.Reflection;
using KarambaIDEA.Core;
using Microsoft.Win32;
using System.Linq;
using System.Globalization;
using System.Windows.Threading;
using System.Collections.Generic;
using IdeaRS.OpenModel.Connection;

namespace KarambaIDEA.IDEA
{
    public class IdeaStatiCaVersion
    {
        public static string GetLatestVersionPath()
        {
            try
            {
                string programsFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string[] ideaInstalls = Directory.GetDirectories(Path.Combine(programsFolder, "IDEA StatiCa"));
                List<string> orderedInstalls = ideaInstalls.OrderByDescending(x => Double.Parse(Path.GetFileName(x).Split(' ')[1], CultureInfo.InvariantCulture.NumberFormat)).ToList();

                return orderedInstalls[0];
            }
            catch
            {
                throw new ArgumentNullException("IDEA StatiCa installation could not be found in Program Files Directory");
            }

#warning Update this to find the version through Programs folder.
            try
            {
                //Find most recent version of IDEA StatiCa in registry
                RegistryKey staticaRoot = Registry.LocalMachine.OpenSubKey("SOFTWARE\\IDEAStatiCa");
                string[] SubKeyNames = staticaRoot.GetSubKeyNames();
                Dictionary<double?, string> versions = new Dictionary<double?, string>();
                foreach (string SubKeyName in SubKeyNames)
                {
                    versions.Add(double.Parse(SubKeyName, CultureInfo.InvariantCulture.NumberFormat), SubKeyName);
                }
                double[] staticaVersions = staticaRoot.GetSubKeyNames().Select(x => double.Parse(x, CultureInfo.InvariantCulture.NumberFormat)).OrderByDescending(x => x).ToArray();
                double? lastverion = staticaVersions.FirstOrDefault();
                string versionString = versions[lastverion];
                if (lastverion == null) { throw new ArgumentNullException("IDEA StatiCa installation cannot be found"); }
                string path = $@"{versionString.Replace(",", ".")}\IDEAStatiCa\Designer";


                return staticaRoot.OpenSubKey(path).GetValue("InstallDir64").ToString();
            }
            catch
            {
                throw new ArgumentNullException("IDEA StatiCa installation cannot be found");
            }
        }
    }

    //Attempt to use the singleton design pattern to access the connection client.

    //public sealed class Singleton
    //{
    //    private static Singleton instance = null;
    //    private static readonly object padlock = new object();

    //    Singleton()
    //    {
    //    }

    //    public static Singleton Instance
    //    {
    //        get
    //        {
    //            lock (padlock)
    //            {
    //                if (instance == null)
    //                {
    //                    instance = new Singleton();
    //                }
    //                return instance;
    //            }
    //        }
    //    }
    //}

    public sealed class IdeaServiceModel
    {
        private static IdeaServiceModel instance = null;
        private static readonly object padlock = new object();

        IdeaServiceModel()
        {
            InitCalcFactory();
        }

        public static IdeaServiceModel Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new IdeaServiceModel();
                    }
                    return instance;
                }
            }
        }


        ConnHiddenClientFactory CalcFactory { get; set; }
        ConnectionHiddenCheckClient IdeaConnectionClient { get; set; }

        ConnectionHiddenCheckClient service;

        private ConnectionHiddenCheckClient Service
        {
            get => service;
            set
            {
                service = value;
            }
        }


        public void InitCalcFactory()
        {
            if (CalcFactory == null)
            {
                CalcFactory = new ConnHiddenClientFactory(IdeaStatiCaVersion.GetLatestVersionPath());
            }
        }


        public ConnectionHiddenCheckClient GetConnectionService()
        {
            if (Service != null && Service.State != System.ServiceModel.CommunicationState.Faulted)
            {
                return Service;
            }

            IdeaConnectionClient = CalcFactory.Create();
            Service = IdeaConnectionClient;
            return Service;
        }

        //Add more methods for simply closing the project.
        public void CloseProject()
        {
            IdeaConnectionClient.CloseProject();
        }

        
        public void CloseConnectionService()
        {
            if (Service == null)
            {
                return;
            }

            IdeaConnectionClient.CloseProject();
            IdeaConnectionClient.Close();
            IdeaConnectionClient = null;
            Service = null;
        }


        private static Assembly IdeaResolveEventHandler(object sender, ResolveEventArgs args)
        {
            AssemblyName asmName = new AssemblyName(args.Name);
            string assemblyFileName = System.IO.Path.Combine(IdeaStatiCaVersion.GetLatestVersionPath(), asmName.Name + ".dll");
            if (System.IO.File.Exists(assemblyFileName))
            {
                return Assembly.LoadFile(assemblyFileName);
            }
            return args.RequestingAssembly;
        }

    }
}
