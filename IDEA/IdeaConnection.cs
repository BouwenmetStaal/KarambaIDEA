// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using IdeaRS.OpenModel;
using IdeaRS.OpenModel.Result;

using IdeaStatiCa.Plugin;//V20

using System.IO;
using System.Reflection;
using KarambaIDEA.Core;
using Microsoft.Win32;
using System.Linq;
using System.Globalization;
using System.Windows.Threading;
using System.Collections.Generic;

namespace KarambaIDEA.IDEA
{
    public class IdeaConnection
    {
        public OpenModelGenerator openModelGenerator;
        public Joint joint;
        public string filePath = "";
        public static string ideaStatiCaDir;
        bool windowWPF = true;

        /// <summary>
        /// Constructor for an IdeaConnection based on a joint
        /// </summary>
        /// <param name="_joint">Joint object</param>
        public IdeaConnection(Joint _joint)
        {
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
                ideaStatiCaDir = staticaRoot.OpenSubKey(path).GetValue("InstallDir64").ToString();
            }
            catch
            {
                throw new ArgumentNullException("IDEA StatiCa installation cannot be found");
            }

            ProgressWindow pop = new ProgressWindow();
            if (windowWPF)
            {                
                pop.Show();
                pop.AddMessage(string.Format("IDEA StatiCa installation was found in '{0}'", ideaStatiCaDir));
            }




            //1.set joint
            joint = _joint;

            //throw new ArgumentException("dddddd");

            //2.create folder for joint
            string folder = this.joint.project.projectFolderPath;
            filePath = Path.Combine(folder, this.joint.Name);
            if (!Directory.Exists(this.filePath))
            {
                Directory.CreateDirectory(this.filePath);
            }


            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(IdeaResolveEventHandler);

            openModelGenerator = new OpenModelGenerator();
            openModelGenerator.CreateOpenModel(joint, filePath);

            //3. create IOM and results
            OpenModel openModel = openModelGenerator.openModel;
            OpenModelResult openModelResult = openModelGenerator.openModelResult;
            if (windowWPF)
            {
                pop.AddMessage(string.Format("Creating Openmodel and OpenmodelResult for '{0}'", joint.Name));
            }
            
            
            if (joint.template!= null)
            {
                Templates.ApplyProgrammedIDEAtemplate(openModel , joint);
            }

            string iomFileName = Path.Combine(folder, joint.Name, "IOM.xml");
            string iomResFileName = Path.Combine(folder, joint.Name, "IOMresults.xml");
            if (windowWPF)
            {
                pop.AddMessage(string.Format("Saving Openmodel and OpenmodelResult to XML for '{0}'", joint.Name));
            }
            

            // save to the files
            openModel.SaveToXmlFile(iomFileName);
            openModelResult.SaveToXmlFile(iomResFileName);

            if (windowWPF)
            {
                pop.AddMessage(string.Format("Creating IDEA StatiCa File '{0}'", joint.Name));
            }

            string filename = joint.Name + ".ideaCon";            
            //var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var fileConnFileNameFromLocal = Path.Combine(folder,joint.Name, filename);
            
            var calcFactory = new ConnHiddenClientFactory(ideaStatiCaDir);//V20
            //string newBoltAssemblyName = "M16 8.8";
            
            var client = calcFactory.Create();
            try
            {
                // it creates connection project from IOM 
                client.CreateConProjFromIOM(iomFileName, iomResFileName, fileConnFileNameFromLocal);
                if (windowWPF)
                {
                    pop.AddMessage(string.Format("Joint '{0}' was saved to:\n {1}", joint.Name, fileConnFileNameFromLocal));
                }
                

            }
            catch (Exception e)
            {
                
                throw new Exception(string.Format("Error '{0}'", e.Message));
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }
            }
            if (windowWPF)
            {
                pop.Close();
            }
            
        }

        

        private static Assembly IdeaResolveEventHandler(object sender, ResolveEventArgs args)
        {
            AssemblyName asmName = new AssemblyName(args.Name);
            string assemblyFileName = System.IO.Path.Combine(ideaStatiCaDir, asmName.Name + ".dll");
            if (System.IO.File.Exists(assemblyFileName))
            {
                return Assembly.LoadFile(assemblyFileName);
            }
            return args.RequestingAssembly;
        }
    }
}
        