// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	 
using KarambaIDEA.Core;
using KarambaIDEA.IDEA;

using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using KarambaIDEA;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tester
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //TESTCalculate();
            //TESTCreateAndCalculateTemplate();
            TESTCopyProject();

        }

        static void TESTCalculate()
        {
            // Initialize idea references, before calling code.
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(KarambaIDEA.IDEA.Utils.IdeaResolveEventHandler);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(KarambaIDEA.IDEA.Utils.IdeaResolveEventHandler);


            Joint joint = new Joint();
            joint.JointFilePath = "C:\\Data\\20191115214919\\C12-brandname\\APIproduced File - NotCorrect.ideaCon";
            HiddenCalculationV20.Calculate(joint);
            //Results
            string results = joint.ResultsSummary.summary;
        }
        static void TESTCreateAndCalculate()
        {
            Tester.GenerateTestJoint testrun = new GenerateTestJoint();

            //Define testjoint
            Joint joint = testrun.Testjoint2();


            //Define workshop operations
            joint.template = new Template();
            joint.template.workshopOperations = Template.WorkshopOperations.WeldAllMembers;

            //Set Project folder path
            string folderpath = @"C:\Data\";
            joint.project.CreateFolder(folderpath);

            //Set Joint folder path
            //string filepath = joint.project.projectFolderPath + ".ideaCon";
            //string fileName = joint.Name + ".ideaCon";
            //string jointFilePath = Path.Combine(joint.project.projectFolderPath, joint.Name, fileName);
            //joint.JointFilePath = jointFilePath;
            joint.JointFilePath = "xx";

            // Initialize idea references, before calling code.
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(KarambaIDEA.IDEA.Utils.IdeaResolveEventHandler);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(KarambaIDEA.IDEA.Utils.IdeaResolveEventHandler);

            //Create IDEA file
            IdeaConnection ideaConnection = new IdeaConnection(joint);

            //Calculate
            HiddenCalculationV20.Calculate(joint);
            //KarambaIDEA.IDEA.HiddenCalculation main = new HiddenCalculation(joint);

            //Results
            string results = joint.ResultsSummary.summary;
        }

        static void TESTCreateAndCalculateTemplate()
        {
            Tester.GenerateTestJoint testrun = new GenerateTestJoint();

            //Define testjoint
            Joint joint = testrun.Testjoint2();


            //Define Template location
            joint.ideaTemplateLocation = @"C:\SMARTconnection\BouwenmetStaal\KarambaIDEA\0_IDEA_Templates\TESTjointTester.contemp";

            //Set Project folder path
            string folderpath = @"C:\Data\";
            joint.project.CreateFolder(folderpath);

            //Set Joint folder path
            //string filepath = joint.project.projectFolderPath + ".ideaCon";
            //string fileName = joint.Name + ".ideaCon";
            //string jointFilePath = Path.Combine(joint.project.projectFolderPath, joint.Name, fileName);
            //joint.JointFilePath = jointFilePath;
            joint.JointFilePath = "xx";

            // Initialize idea references, before calling code.
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(KarambaIDEA.IDEA.Utils.IdeaResolveEventHandler);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(KarambaIDEA.IDEA.Utils.IdeaResolveEventHandler);

            //Create IDEA file
            IdeaConnection ideaConnection = new IdeaConnection(joint);

            //Calculate
            HiddenCalculationV20.Calculate(joint);
            

            //Results
            string results = joint.ResultsSummary.summary;
        }

        static void TESTCopyProject()
        {
            TestClass par = new TestClass();
            TestClass self = new TestClass() { parent = par, mypro = "sdsd" };

            par.children.Add(self);


            Project project = null;



            Project clone = project.CloneJson();
            //
            foreach (CrossSection c in clone.crossSections)
            {
                c.project = clone; //project copieren naar in elke child
                //kijken naar mogelijkheden om parent verwijzing te verwijderen
            }
        }

        public static void SetParent<T>(this T source, string propertyname, dynamic parent) where T : class
        {
            PropertyInfo pinfo = source.GetType().GetProperty(propertyname);
            if (pinfo != null)
            {
                pinfo.SetValue(source, parent);
            }
        }

        /// <summary>
        /// Perform a deep Copy of the object, using Json as a serialisation method. NOTE: Private members are not cloned using this method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T CloneJson<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }


    }
    public class TestClass
    {
        
        [NonSerialized]//toepassen project field
        public TestClass parent;
        public string mypro { get; set; }
        public List<TestClass> children { get; set; } = new List<TestClass>();
    }
}
