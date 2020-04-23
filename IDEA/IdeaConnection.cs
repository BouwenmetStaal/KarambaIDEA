// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Collections.Generic;
using System.Linq;


using IdeaRS.OpenModel.Connection;
using IdeaRS.OpenModel;
using IdeaRS.OpenModel.Result;

using IdeaStatiCa.Plugin;//V20

using System.Diagnostics;
using System.IO;
using System.Reflection;


using System.Xml;
using System.Runtime.Serialization;

using System.Xml.Serialization;
using KarambaIDEA.Core;

namespace KarambaIDEA.IDEA
{
    public class IdeaConnection
    {
        public Lazy<dynamic> dynLinkLazy;
        public OpenModelGenerator openModelGenerator;
        public Joint joint;
        private Guid ideaConnectionIdentifier = Guid.Empty;


        public string filePath = "";
        public static string IdeaInstallDir;

        /// <summary>
        /// Constructor for an IdeaConnection based on a joint
        /// </summary>
        /// <param name="_joint">Joint object</param>
        public IdeaConnection(Joint _joint)
        {
            //1.set joint
            joint = _joint;
            
            //2.create folder for joint
            string folder = this.joint.project.projectFolderPath;
            filePath = Path.Combine(folder, this.joint.Name);
            if (!Directory.Exists(this.filePath))
            {
                Directory.CreateDirectory(this.filePath);
            }

            //IdeaInstallDir = IDEA.Properties.Settings.Default.IdeaInstallDir;
            IdeaInstallDir = @"C:\Release_20_UT_x64_2020-04-20_23-28_20.0.139";
            if (!Directory.Exists(IdeaInstallDir))
            {
                Console.WriteLine("IDEA StatiCa installation was not found in '{0}'", IdeaInstallDir);
                return;
            }
            Console.WriteLine("IDEA StatiCa installation directory is '{0}'", IdeaInstallDir);
            Console.WriteLine("Start generate example of IOM...");


            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(IdeaResolveEventHandler);

            openModelGenerator = new OpenModelGenerator();
            openModelGenerator.CreateOpenModel(joint, filePath);

            // create IOM and results
            OpenModel example = openModelGenerator.openModel;
            OpenModelResult result = openModelGenerator.openModelResult;

            //add template to openmodel
            if(joint.template!= null)
            {
                if (joint.template.workshopOperations == Template.WorkshopOperations.NoOperation)
                {

                }
                if (joint.template.workshopOperations == Template.WorkshopOperations.BoltedEndPlateConnection)
                {
                    if (joint.template.plates.First() != null)
                    {
                        double platethickness = joint.template.plates[0].thickness/1000;
                        Templates.BoltedEndplateConnection(example, joint, platethickness);
                    }
                    
                }
                if (joint.template.workshopOperations == Template.WorkshopOperations.WeldAllMembers)
                {
                    Templates.WeldAllMembers(example);
                }
            }

            string iomFileName = Path.Combine(folder, joint.Name, "IOM.xml");
            string iomResFileName = Path.Combine(folder, joint.Name, "IOMresults.xml");

            // save to the files
            example.SaveToXmlFile(iomFileName);
            result.SaveToXmlFile(iomResFileName);

            string filename = joint.Name + ".ideaCon";

            //var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var fileConnFileNameFromLocal = Path.Combine(folder,joint.Name, filename);

            var calcFactory = new ConnHiddenClientFactory(IdeaInstallDir);//V20

            var client = calcFactory.Create();
            try
            {
                // it creates connection project from IOM 
                Console.WriteLine("Creating Idea connection project ");
                client.CreateConProjFromIOM(iomFileName, iomResFileName, fileConnFileNameFromLocal);
                Console.WriteLine("Generated project was saved to the file '{0}'", fileConnFileNameFromLocal);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error '{0}'", e.Message);
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }
            }
            /*
            string ideaConLinkFullPath = System.IO.Path.Combine(IdeaInstallDir, "IdeaStatiCa.IOMToConnection.dll");
			var conLinkAssembly = Assembly.LoadFrom(ideaConLinkFullPath);
			object obj = conLinkAssembly.CreateInstance("IdeaStatiCa.IOMToConnection.IOMToConnection");
			dynamic d = obj;

			// Initializtion
			var initMethod = (obj).GetType().GetMethod("Init");
			initMethod.Invoke(obj, null);

			Console.WriteLine("Generating IDEA Connection project locally");

			// Invoking method Import by reflection
			var methodImport = (obj).GetType().GetMethod("Import");
			object[] array = new object[3];
			array[0] = example;
			array[1] = result;
			array[2] = fileConnFileNameFromLocal;
			methodImport.Invoke(obj, array);
            */
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
        