// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal, ABT bv. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	 
using IdeaRS.OpenModel.Message;
using IdeaRS.OpenModel;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Text;
using System.Collections.Generic;
using IdeaRS.Connections.Data;

using System.Xml.Serialization;
using System.Linq;

using IdeaRS.OpenModel.Connection;


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
            PointRAZ puntA = new PointRAZ(project2, -2, 0, 0);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, 2, 0, 0);
            PointRAZ puntD = new PointRAZ(project2, 0, 0, 2);
            PointRAZ puntE = new PointRAZ(project2, 0, 0, -2);





            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA160", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 0, new LineRAZ(0, puntA, puntB), project2.crossSections[1], "Column", 1);
            new ElementRAZ(project2, 1, new LineRAZ(1, puntB, puntC), project2.crossSections[1], "Column", 1);

            new ElementRAZ(project2, 2, new LineRAZ(2, puntB, puntE), project2.crossSections[0], "Diagonal", 3);
            new ElementRAZ(project2, 3, new LineRAZ(3, puntB, puntD), project2.crossSections[0], "Diagonal", 3);



            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            LoadcaseRAZ lc1 = new LoadcaseRAZ(project2, 1);
            new LoadsPerLineRAZ(project2.elementRAZs[0], lc1, new LoadsRAZ(5, 5, 5, 5, 5, 5), new LoadsRAZ(10, 10, 10, 10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[1], lc1, new LoadsRAZ(5, 5, 5, 5, 5, 5), new LoadsRAZ(10, 10, 10, 10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[2], lc1, new LoadsRAZ(5, 5, 5, 5, 5, 5), new LoadsRAZ(10, 10, 10, 10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[3], lc1, new LoadsRAZ(5, 5, 5, 5, 5, 5), new LoadsRAZ(10, 10, 10, 10, 10, 10));



            double tol = 1e-6;
            project2.CreateJoints(tol, 0, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }
    }
}

