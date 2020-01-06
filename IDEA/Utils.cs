using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using System.Windows;

namespace KarambaIDEA.IDEA
{
    public static class Utils
    {
        public static Assembly IdeaResolveEventHandler(object sender, ResolveEventArgs args)
        {


            AssemblyName asmName = new AssemblyName(args.Name);

            String IdeaInstallDir = KarambaIDEA.IDEA.Properties.Settings.Default.IdeaInstallDir;

            string assemblyFileName = System.IO.Path.Combine(IdeaInstallDir, asmName.Name + ".dll");
            if (System.IO.File.Exists(assemblyFileName))
            {
                return Assembly.LoadFile(assemblyFileName);
            }


            return args.RequestingAssembly;
        }
    }
}

