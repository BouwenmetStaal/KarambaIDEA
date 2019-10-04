// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Collections.Generic;
using System.Linq;


using IdeaRS.OpenModel.Connection;
using IdeaRS.Connections.Data;
using IdeaRS.OpenModel;
using IdeaRS.OpenModel.Result;


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


        public string filepath = "";
        private static string IdeaInstallDir;

        /// <summary>
        /// Constructor for an IdeaConnection based on a joint
        /// </summary>
        /// <param name="_joint">Joint object</param>
        public IdeaConnection(Joint _joint)
        {
            //1.set joint
            joint = _joint;

            //TODO: make sure only one folder is created now two folders are created.

            //2.create folder
            string folder = this.joint.project.filepath;
            filepath = Path.Combine(folder, this.joint.Name);
            if (!Directory.Exists(this.filepath))
            {
                Directory.CreateDirectory(this.filepath);
            }
            


            //IdeaInstallDir = KarambaIDEA.Properties.Settings.Default.IdeaInstallDir;
            IdeaInstallDir = @"C:\Program Files\IDEA StatiCa\StatiCa 10.1";

            Console.WriteLine("IDEA StatiCa installation directory is '{0}'", IdeaInstallDir);

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(IdeaResolveEventHandler);

            Console.WriteLine("Start generate example of IOM...");


            openModelGenerator = new OpenModelGenerator();
            openModelGenerator.CreateOpenModelGenerator(joint, filepath);

            // create IOM and results
            OpenModel example = openModelGenerator.openModel;
            OpenModelResult result = openModelGenerator.openModelResult;

            

            // save to the files
            result.SaveToXmlFile("example.xmlR");
            example.SaveToXmlFile("example.xml");

            var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var fileConnFileNameFromLocal = Path.Combine(desktopDir, "connectionFromIOM-local.ideaCon");

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

			Console.WriteLine("Writing Idea connection project to file '{0}'", fileConnFileNameFromLocal);

            // end console application
            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey();

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
        