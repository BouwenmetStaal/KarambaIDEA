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
            VectorRAZ a = new VectorRAZ(1, 2, 3);
            VectorRAZ b = new VectorRAZ(0, 0, 1);

            double angle = 90.0;

            double scalar = VectorRAZ.DotProduct(a, b);
            VectorRAZ vector = VectorRAZ.CrossProduct(a, b);
            VectorRAZ xx = VectorRAZ.RotateVector(b, angle, a);

            System.Windows.Media.Media3D.Vector3D va = new System.Windows.Media.Media3D.Vector3D(a.X, a.Y, a.Z);
            System.Windows.Media.Media3D.Vector3D vb = new System.Windows.Media.Media3D.Vector3D(b.X, b.Y, b.Z);

            double dot = System.Windows.Media.Media3D.Vector3D.DotProduct(va, vb);
            System.Windows.Media.Media3D.Vector3D cross = System.Windows.Media.Media3D.Vector3D.CrossProduct(va, vb);

            System.Windows.Media.Media3D.AxisAngleRotation3D vec = new System.Windows.Media.Media3D.AxisAngleRotation3D();
            

            

            //System.Windows.Media.Media3D.Rotation3D rot = new System.Windows.Media.Media3D.Rotation3D();


            KarambaIDEA.MainWindow mainWindow = new MainWindow();
            Tester.GenerateTestJoint fj = new GenerateTestJoint();
            //KarambaIDEA.TestFrameworkJoint fj = new TestFrameworkJoint();

            //hieronder testjoint definieren
            Joint joint = fj.Testjoint();
            //Joint joint = fj.Testjoint5();
            joint.project.CreateFolder(@"C:\Data\");
            joint.project.templatePath = @"C:\Data\template.contemp";
            //min lasafmeting uitzetten bij Grasshopper
            joint.project.minthroat = 1.0;

            KarambaIDEA.IDEA.IdeaConnection idea = new KarambaIDEA.IDEA.IdeaConnection(joint);
        }
        
    }
}
