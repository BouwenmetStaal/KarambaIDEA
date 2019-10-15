// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	 

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
//using System.Windows;
using System.Text;
using System.Collections.Generic;


using System.Xml.Serialization;
using System.Linq;




using System.Xml;
using System.Runtime.Serialization;
using KarambaIDEA.Core;

namespace Tester
{
    public class GenerateTestJoint
    {
       public Joint Testjoint()
        {
            //info: Octopus-joint, all start points

            Project project2 = new Project("projectname");
            Point puntA = new Point(project2, -2, 0, 0);
            Point puntB = new Point(project2, 0, 0, 0);
            Point puntC = new Point(project2, 2, 0, 0);





            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S355);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA160", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new Element(project2, 0, new Line(0, puntA, puntB), project2.crossSections[1], "Column", 1,0.0);
            new Element(project2, 1, new Line(1, puntB, puntC), project2.crossSections[1], "Diagonal", 2, 0.0);
            



            List<Point> Points = new List<Point>();
            Points.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            LoadCase lc1 = new LoadCase(project2, 1);
            new LoadsPerLine(project2.elements[0], lc1, new Load(5, 5, 5, 5, 5, 5), new Load(10, 10, 10, 10, 10, 10));
            new LoadsPerLine(project2.elements[1], lc1, new Load(5, 5, 5, 5, 5, 5), new Load(10, 10, 10, 10, 10, 10));



            double tol = 1e-6;
            project2.CreateJoints(tol, 0, Points, project2.elements, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }

       public Joint Testjoint2()
        {
            //info: Octopus-joint, all start points

            Project project2 = new Project("projectname");
            Point puntA = new Point(project2, -2, 0, 0);
            Point puntB = new Point(project2, 0, 0, 0);
            Point puntC = new Point(project2, 2, 0, 0);
            Point puntD = new Point(project2, 0, 0, 2);
            Point puntE = new Point(project2, 0, 0, -2);





            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S355);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA160", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new Element(project2, 0, new Line(0, puntA, puntB), project2.crossSections[1], "Column", 1, 0.0);
            new Element(project2, 1, new Line(1, puntB, puntC), project2.crossSections[1], "Column", 1, 0.0);

            new Element(project2, 2, new Line(2, puntB, puntE), project2.crossSections[0], "Diagonal", 3, 0.0);
            new Element(project2, 3, new Line(3, puntB, puntD), project2.crossSections[0], "Diagonal", 3, 0.0);



            List<Point> Points = new List<Point>();
            Points.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            LoadCase lc1 = new LoadCase(project2, 1);
            new LoadsPerLine(project2.elements[0], lc1, new Load(5, 5, 5, 5, 5, 5), new Load(10, 10, 10, 10, 10, 10));
            new LoadsPerLine(project2.elements[1], lc1, new Load(5, 5, 5, 5, 5, 5), new Load(10, 10, 10, 10, 10, 10));
            new LoadsPerLine(project2.elements[2], lc1, new Load(5, 5, 5, 5, 5, 5), new Load(10, 10, 10, 10, 10, 10));
            new LoadsPerLine(project2.elements[3], lc1, new Load(5, 5, 5, 5, 5, 5), new Load(10, 10, 10, 10, 10, 10));



            double tol = 1e-6;
            project2.CreateJoints(tol, 0, Points, project2.elements, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }



    }
}

