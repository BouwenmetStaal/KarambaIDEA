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
            TESTCreateAndCalculate();
       }

        static void TESTCalculate()
        {
            // Initialize idea references, before calling code.
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(KarambaIDEA.IDEA.Utils.IdeaResolveEventHandler);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(KarambaIDEA.IDEA.Utils.IdeaResolveEventHandler);


            Joint joint = new Joint();
            joint.JointFilePath = "C:\\Data\\20191115214919\\C12-brandname\\APIproduced File - NotCorrect.ideaCon";
            KarambaIDEA.IDEA.HiddenCalculation main = new HiddenCalculation(joint);
            //Results
            string results = joint.ResultsSummary.summary;
        }
        static void TESTCreateAndCalculate()
        {
            Tester.GenerateTestJoint testrun = new GenerateTestJoint();

            //Define testjoint
            Joint joint = testrun.Testjoint();


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
            KarambaIDEA.IDEA.HiddenCalculation main = new HiddenCalculation(joint);

            //Results
            string results = joint.ResultsSummary.summary;
        }





    }
}
