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

            KarambaIDEA.MainWindow mainWindow = new MainWindow();
            Tester.GenerateTestJoint fj = new GenerateTestJoint();

            //hieronder testjoint definieren
            Joint joint = fj.Testjoint();
            //Joint joint = fj.Testjoint5();
            joint.project.CreateFolder(@"C:\Data\");
            joint.project.templatePath = @"C:\Data\template.contemp";
            //min lasafmeting uitzetten bij Grasshopper
            joint.project.minthroat = 1.0;

#warning: why is this test procedure not part of the tester?
            mainWindow.Test(joint);
        }
        
    }
}
