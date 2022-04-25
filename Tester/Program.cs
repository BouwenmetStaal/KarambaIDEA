// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	 
using KarambaIDEA.Core;
using KarambaIDEA.IDEA;
using KarambaIDEA.Grasshopper;

using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using KarambaIDEA;
using System.IO;
//using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Linq;
using System.Globalization;
using System.Windows.Threading;
using System.Threading;
using Eto.Forms;
using Eto.Drawing;
using Eto;
using Application = Eto.Forms.Application;
using System.Xml.Linq;
using System.Xml.Serialization;

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





            TESTCreateProgrammedTemplate();

          
          

            

        }

        static void TESTCreateProgrammedTemplate()
        {
            Tester.GenerateTestJoint testrun = new GenerateTestJoint();

            //Define testjoint
            Joint joint = testrun.Testjoint2();


            //Set Project folder path
            string folderpath = @"C:\Data\";
            joint.project.CreateFolder(folderpath);

            //Set Joint folder path
            //string filepath = joint.project.projectFolderPath + ".ideaCon";
            //string fileName = joint.Name + ".ideaCon";
            //string jointFilePath = Path.Combine(joint.project.projectFolderPath, joint.Name, fileName);
            //joint.JointFilePath = jointFilePath;
            joint.JointFilePath = "xx";

            joint.template = new Template();
            joint.template.workshopOperations = Template.WorkshopOperations.AddedMember;

            // Initialize idea references, before calling code.
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(KarambaIDEA.IDEA.Utils.IdeaResolveEventHandler);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(KarambaIDEA.IDEA.Utils.IdeaResolveEventHandler);

            //Create IDEA file

            IdeaConnection ideaConnection = new IdeaConnection(joint, true);

            //Calculate
            HiddenCalculationV20.Calculate(joint, true);


            //Results
            string results = joint.ResultsSummary.summary;
        }

        static void TESTCreateAndCalculateTemplate()
        {
            Tester.GenerateTestJoint testrun = new GenerateTestJoint();

            //Define testjoint
            Joint joint = testrun.Testjoint2();


            //Define Template location
            
            string filename = "template_plusjoint_extended";
            string pathfolder = "C:\\Users\\r.ajouz\\source\\repos\\KarambaIDEA\\0_IDEA_Templates\\";
            string extension = ".contemp";
            string path_1 = pathfolder + filename + extension;
            string path_2 = pathfolder + filename + "2" + extension;
            //string path = 
            //string pathTemplate = "C:\\Users\\r.ajouz\\source\\repos\\KarambaIDEA\\0_IDEA_Templates\\IDEA_NL.contemp";//This template does not work, contains multiple classes, which are not being serialized
            //string pathTemplate = "C:\\Users\\r.ajouz\\source\\repos\\KarambaIDEA\\0_IDEA_Templates\\template_plusjoint.contemp";//This template works, contains only CutBeamData
            //KarambaIDEA.IDEA.ConnectionTemplateGenerator con = new KarambaIDEA.IDEA.ConnectionTemplateGenerator(path_1);
            //con.UpdateTemplate();//check if 
            //con.SaveToXmlFile(path_2);

            joint.ideaTemplateLocation = path_2;

            if (!File.Exists(joint.ideaTemplateLocation))
            {
                Console.WriteLine("dddd");
            }

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
            
            IdeaConnection ideaConnection = new IdeaConnection(joint, true);

            //Calculate
            HiddenCalculationV20.Calculate(joint, true);


            //Results
            string results = joint.ResultsSummary.summary;
        }

        static void TESTCalculate()
        {
            // Initialize idea references, before calling code.
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(KarambaIDEA.IDEA.Utils.IdeaResolveEventHandler);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(KarambaIDEA.IDEA.Utils.IdeaResolveEventHandler);


            Joint joint = new Joint();
            joint.JointFilePath = "C:\\Data\\20191115214919\\C12-brandname\\APIproduced File - NotCorrect.ideaCon";
            HiddenCalculationV20.Calculate(joint, true);
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
            KarambaIDEA.Core.Point p_canvas = new KarambaIDEA.Core.Point(100, 100, 0);
            IdeaConnection ideaConnection = new IdeaConnection(joint, true);

            //Calculate
            HiddenCalculationV20.Calculate(joint, true);
            //KarambaIDEA.IDEA.HiddenCalculation main = new HiddenCalculation(joint);

            //Results
            string results = joint.ResultsSummary.summary;
        }


        static void TESTCopyProject()
        {
            TestClass par = new TestClass();
            TestClass self = new TestClass() { parent = par, mypro = "sdsd" };

            par.children.Add(self);




        }

        public static void SetParent<T>(this T source, string propertyname, dynamic parent) where T : class
        {
            PropertyInfo pinfo = source.GetType().GetProperty(propertyname);
            if (pinfo != null)
            {
                pinfo.SetValue(source, parent);
            }
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
