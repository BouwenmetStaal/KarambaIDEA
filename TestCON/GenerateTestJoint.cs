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

namespace TestCON
{
    public class TestFrameworkJoint2
    {
        public Joint TestjointCornerA()
        {
            //info: Pratt K-joint of Topchord with Grasshopper values: diagonal rechts-beneden

            Project project2 = new Project("projectname");
            PointRAZ puntA = new PointRAZ(project2, 0, 0, -1.5);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, 2.5, 0, 0);
            PointRAZ puntD = new PointRAZ(project2, 2.5, 0, -1.5);



            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEB240", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEB140", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "buis", KarambaIDEA.Core.CrossSection.Shape.HollowSection, steel, 200, 100, 10, 5, 6);

            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 0, new LineRAZ(0, puntA, puntB), project2.crossSections[2], "Column", 0);
            new ElementRAZ(project2, 1, new LineRAZ(1, puntB, puntC), project2.crossSections[0], "Topchord", 1);
            new ElementRAZ(project2, 2, new LineRAZ(2, puntB, puntD), project2.crossSections[1], "Diagonal", 4);


            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            //new LoadcaseRAZ(1,);
            LoadcaseRAZ lc1 = new LoadcaseRAZ(project2, 1);
            //LoadcaseRAZ lc2 = new LoadcaseRAZ(project2, 2);

            new LoadsPerLineRAZ(project2.elementRAZs[0], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-92.28, -1.96, 0));
            new LoadsPerLineRAZ(project2.elementRAZs[1], lc1, new LoadsRAZ(-138.02, -8.40, 0), new LoadsRAZ(0, 0, 0));
            new LoadsPerLineRAZ(project2.elementRAZs[2], lc1, new LoadsRAZ(163.40, 0, 0), new LoadsRAZ(0, 0, 0));


            //new LoadsPerLineRAZ(project2.elementRAZs[0], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[1], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[2], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[3], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));


            double tol = 1e-6;
            project2.CreateJoints(tol, 0, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }

        public Joint TestjointCornerB()
        {
            //info: Pratt K-joint of Topchord with Grasshopper values: diagonal rechts-beneden

            Project project2 = new Project("projectname");
            PointRAZ puntA = new PointRAZ(project2, 0, 0, -1.5);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, -2.5, 0, 0);
            PointRAZ puntD = new PointRAZ(project2, -2.5, 0, -1.5);



            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEB240", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEB140", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "buis", KarambaIDEA.Core.CrossSection.Shape.HollowSection, steel, 200, 100, 10, 5, 6);

            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 0, new LineRAZ(0, puntA, puntB), project2.crossSections[2], "Column", 0);
            new ElementRAZ(project2, 1, new LineRAZ(1, puntB, puntC), project2.crossSections[0], "Topchord", 1);
            new ElementRAZ(project2, 2, new LineRAZ(2, puntB, puntD), project2.crossSections[1], "Diagonal", 4);


            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            //new LoadcaseRAZ(1,);
            LoadcaseRAZ lc1 = new LoadcaseRAZ(project2, 1);
            //LoadcaseRAZ lc2 = new LoadcaseRAZ(project2, 2);

            new LoadsPerLineRAZ(project2.elementRAZs[0], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-92.28, -1.96, 0));
            new LoadsPerLineRAZ(project2.elementRAZs[1], lc1, new LoadsRAZ(-138.02, -8.40, 0), new LoadsRAZ(0, 0, 0));
            new LoadsPerLineRAZ(project2.elementRAZs[2], lc1, new LoadsRAZ(163.40, 0, 0), new LoadsRAZ(0, 0, 0));


            //new LoadsPerLineRAZ(project2.elementRAZs[0], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[1], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[2], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[3], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));


            double tol = 1e-6;
            project2.CreateJoints(tol, 0, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }
        public Joint TestjointA()
        {
            //info: Pratt K-joint of Topchord with Grasshopper values: diagonal rechts-beneden

            Project project2 = new Project("projectname");
            PointRAZ puntA = new PointRAZ(project2, -3.125, 0, 0);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, 3.125, 0, 0);
            PointRAZ puntD = new PointRAZ(project2, 0, 0, -1.5);
            PointRAZ puntE = new PointRAZ(project2, 3.125, 0, -1.5);


            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEB240", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA160", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "buis", KarambaIDEA.Core.CrossSection.Shape.HollowSection, steel, 200, 100, 10, 5, 6);

            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 0, new LineRAZ(0, puntA, puntB), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 1, new LineRAZ(1, puntB, puntC), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 2, new LineRAZ(2, puntD, puntB), project2.crossSections[3], "Post", 2);
            new ElementRAZ(project2, 3, new LineRAZ(3, puntB, puntE), project2.crossSections[3], "Diagonal", 3);

            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            //new LoadcaseRAZ(1,);
            LoadcaseRAZ lc1 = new LoadcaseRAZ(project2, 1);
            //LoadcaseRAZ lc2 = new LoadcaseRAZ(project2, 2);

            new LoadsPerLineRAZ(project2.elementRAZs[0], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-527.01, 37.28, 4.98));
            new LoadsPerLineRAZ(project2.elementRAZs[1], lc1, new LoadsRAZ(-911.43, -41.2, 12.64), new LoadsRAZ(0, 0, 0));
            new LoadsPerLineRAZ(project2.elementRAZs[2], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-259.8, 6.27, -3.06));
            new LoadsPerLineRAZ(project2.elementRAZs[3], lc1, new LoadsRAZ(419.3, 0.31, -2.66), new LoadsRAZ(0, 0, 0));

            //new LoadsPerLineRAZ(project2.elementRAZs[0], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[1], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[2], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[3], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));


            double tol = 1e-6;
            project2.CreateJoints(tol, 0, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }
        public Joint TestjointB()
        {
            //info: Pratt K-joint of Topchord with Grasshopper values: diagonal rechts-beneden

            Project project2 = new Project("projectname");
            PointRAZ puntA = new PointRAZ(project2, -3.125, 0, 0);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, 3.125, 0, 0);
            PointRAZ puntD = new PointRAZ(project2, 0, 0, -1.5);
            PointRAZ puntE = new PointRAZ(project2, -3.125, 0, -1.5);


            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEB240", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA160", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "buis", KarambaIDEA.Core.CrossSection.Shape.HollowSection, steel, 200, 100, 10, 5, 6);

            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 0, new LineRAZ(0, puntA, puntB), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 1, new LineRAZ(1, puntB, puntC), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 2, new LineRAZ(2, puntD, puntB), project2.crossSections[3], "Post", 2);
            new ElementRAZ(project2, 3, new LineRAZ(3, puntB, puntE), project2.crossSections[3], "Diagonal", 3);

            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            //new LoadcaseRAZ(1,);
            LoadcaseRAZ lc1 = new LoadcaseRAZ(project2, 1);
            //LoadcaseRAZ lc2 = new LoadcaseRAZ(project2, 2);

            new LoadsPerLineRAZ(project2.elementRAZs[0], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-527.01, 37.28, 4.98));
            new LoadsPerLineRAZ(project2.elementRAZs[1], lc1, new LoadsRAZ(-911.43, -41.2, 12.64), new LoadsRAZ(0, 0, 0));
            new LoadsPerLineRAZ(project2.elementRAZs[2], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-259.8, 6.27, -3.06));
            new LoadsPerLineRAZ(project2.elementRAZs[3], lc1, new LoadsRAZ(419.3, 0.31, -2.66), new LoadsRAZ(0, 0, 0));

            //new LoadsPerLineRAZ(project2.elementRAZs[0], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[1], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[2], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[3], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));


            double tol = 1e-6;
            project2.CreateJoints(tol, 0, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }
        public Joint TestjointC()
        {
            //info: Pratt K-joint of Topchord with Grasshopper values: diagonal rechts-beneden

            Project project2 = new Project("projectname");
            PointRAZ puntA = new PointRAZ(project2, -3.125, 0, 0);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, 3.125, 0, 0);
            PointRAZ puntD = new PointRAZ(project2, 0, 0, 1.5);
            PointRAZ puntE = new PointRAZ(project2, -3.125, 0, 1.5);


            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEB240", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA160", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "buis", KarambaIDEA.Core.CrossSection.Shape.HollowSection, steel, 200, 100, 10, 5, 6);

            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 0, new LineRAZ(0, puntA, puntB), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 1, new LineRAZ(1, puntB, puntC), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 2, new LineRAZ(2, puntD, puntB), project2.crossSections[3], "Post", 2);
            new ElementRAZ(project2, 3, new LineRAZ(3, puntB, puntE), project2.crossSections[3], "Diagonal", 3);

            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            //new LoadcaseRAZ(1,);
            LoadcaseRAZ lc1 = new LoadcaseRAZ(project2, 1);
            //LoadcaseRAZ lc2 = new LoadcaseRAZ(project2, 2);

            new LoadsPerLineRAZ(project2.elementRAZs[0], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-527.01, 37.28, 4.98));
            new LoadsPerLineRAZ(project2.elementRAZs[1], lc1, new LoadsRAZ(-911.43, -41.2, 12.64), new LoadsRAZ(0, 0, 0));
            new LoadsPerLineRAZ(project2.elementRAZs[2], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-259.8, 6.27, -3.06));
            new LoadsPerLineRAZ(project2.elementRAZs[3], lc1, new LoadsRAZ(419.3, 0.31, -2.66), new LoadsRAZ(0, 0, 0));

            //new LoadsPerLineRAZ(project2.elementRAZs[0], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[1], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[2], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[3], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));


            double tol = 1e-6;
            project2.CreateJoints(tol, 0, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }
        public Joint TestjointD()
        {
            //info: Pratt K-joint of Topchord with Grasshopper values: diagonal rechts-beneden

            Project project2 = new Project("projectname");
            PointRAZ puntA = new PointRAZ(project2, -3.125, 0, 0);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, 3.125, 0, 0);
            PointRAZ puntD = new PointRAZ(project2, 0, 0, 1.5);
            PointRAZ puntE = new PointRAZ(project2, 3.125, 0, 1.5);


            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEB240", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA160", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "buis", KarambaIDEA.Core.CrossSection.Shape.HollowSection, steel, 200, 100, 10, 5, 6);

            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 0, new LineRAZ(0, puntA, puntB), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 1, new LineRAZ(1, puntB, puntC), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 2, new LineRAZ(2, puntD, puntB), project2.crossSections[3], "Post", 2);
            new ElementRAZ(project2, 3, new LineRAZ(3, puntB, puntE), project2.crossSections[3], "Diagonal", 3);

            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            //new LoadcaseRAZ(1,);
            LoadcaseRAZ lc1 = new LoadcaseRAZ(project2, 1);
            //LoadcaseRAZ lc2 = new LoadcaseRAZ(project2, 2);

            new LoadsPerLineRAZ(project2.elementRAZs[0], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-527.01, 37.28, 4.98));
            new LoadsPerLineRAZ(project2.elementRAZs[1], lc1, new LoadsRAZ(-911.43, -41.2, 12.64), new LoadsRAZ(0, 0, 0));
            new LoadsPerLineRAZ(project2.elementRAZs[2], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-259.8, 6.27, -3.06));
            new LoadsPerLineRAZ(project2.elementRAZs[3], lc1, new LoadsRAZ(419.3, 0.31, -2.66), new LoadsRAZ(0, 0, 0));

            //new LoadsPerLineRAZ(project2.elementRAZs[0], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[1], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[2], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[3], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));


            double tol = 1e-6;
            project2.CreateJoints(tol, 0, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }

        public Joint TestjointDinverse()
        {
            //info: Pratt K-joint of Topchord with Grasshopper values: diagonal rechts-beneden

            Project project2 = new Project("projectname");
            PointRAZ puntA = new PointRAZ(project2, -2, 0, 2);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, 2, 0, -2);
            PointRAZ puntD = new PointRAZ(project2, -1, 0, 0);
            PointRAZ puntE = new PointRAZ(project2, 0, 0, -1);


            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEB240", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA160", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "buis", KarambaIDEA.Core.CrossSection.Shape.HollowSection, steel, 200, 100, 10, 5, 6);

            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 1, new LineRAZ(1, puntB, puntA), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 0, new LineRAZ(0, puntC, puntB), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 2, new LineRAZ(2, puntD, puntB), project2.crossSections[3], "Post", 2);
            new ElementRAZ(project2, 3, new LineRAZ(3, puntB, puntE), project2.crossSections[3], "Diagonal", 3);

            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            //new LoadcaseRAZ(1,);
            LoadcaseRAZ lc1 = new LoadcaseRAZ(project2, 1);
            //LoadcaseRAZ lc2 = new LoadcaseRAZ(project2, 2);

            new LoadsPerLineRAZ(project2.elementRAZs[0], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-527.01, 37.28, 4.98));
            new LoadsPerLineRAZ(project2.elementRAZs[1], lc1, new LoadsRAZ(-911.43, -41.2, 12.64), new LoadsRAZ(0, 0, 0));
            new LoadsPerLineRAZ(project2.elementRAZs[2], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-259.8, 6.27, -3.06));
            new LoadsPerLineRAZ(project2.elementRAZs[3], lc1, new LoadsRAZ(419.3, 0.31, -2.66), new LoadsRAZ(0, 0, 0));

            //new LoadsPerLineRAZ(project2.elementRAZs[0], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[1], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[2], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[3], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));


            double tol = 1e-6;
            project2.CreateJoints(tol, 0, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }

        public Joint TestjointColumnA()
        {
            //info: Pratt K-joint of Topchord with Grasshopper values: diagonal rechts-beneden

            Project project2 = new Project("projectname");
            PointRAZ puntA = new PointRAZ(project2, 0, 0, -2);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, 0, 0, 2);
            PointRAZ puntD = new PointRAZ(project2, 2, 0, 0);
            PointRAZ puntE = new PointRAZ(project2, 3.125, 0, 1.5);


            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEB240", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA160", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "buis", KarambaIDEA.Core.CrossSection.Shape.HollowSection, steel, 200, 100, 10, 5, 6);

            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 0, new LineRAZ(0, puntA, puntB), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 1, new LineRAZ(1, puntB, puntC), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 2, new LineRAZ(2, puntD, puntB), project2.crossSections[1], "Post", 2);
            new ElementRAZ(project2, 3, new LineRAZ(3, puntB, puntE), project2.crossSections[3], "Diagonal", 3);

            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            //new LoadcaseRAZ(1,);
            LoadcaseRAZ lc1 = new LoadcaseRAZ(project2, 1);
            //LoadcaseRAZ lc2 = new LoadcaseRAZ(project2, 2);

            new LoadsPerLineRAZ(project2.elementRAZs[0], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-527.01, 37.28, 4.98));
            new LoadsPerLineRAZ(project2.elementRAZs[1], lc1, new LoadsRAZ(-911.43, -41.2, 12.64), new LoadsRAZ(0, 0, 0));
            new LoadsPerLineRAZ(project2.elementRAZs[2], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-259.8, 6.27, -3.06));
            new LoadsPerLineRAZ(project2.elementRAZs[3], lc1, new LoadsRAZ(419.3, 0.31, -2.66), new LoadsRAZ(0, 0, 0));

            //new LoadsPerLineRAZ(project2.elementRAZs[0], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[1], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[2], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[3], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));


            double tol = 1e-6;
            project2.CreateJoints(tol, 0, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }

        public Joint TestjointColumnB()
        {
            //info: Pratt K-joint of Topchord with Grasshopper values: diagonal rechts-beneden

            Project project2 = new Project("projectname");
            PointRAZ puntA = new PointRAZ(project2, 0, 0, -2);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, 0, 0, 2);
            PointRAZ puntD = new PointRAZ(project2, 2, 0, 0);
            PointRAZ puntE = new PointRAZ(project2, 3.125, 0, -1.5);


            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEB240", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA160", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "buis", KarambaIDEA.Core.CrossSection.Shape.HollowSection, steel, 200, 100, 10, 5, 6);

            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 0, new LineRAZ(0, puntA, puntB), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 1, new LineRAZ(1, puntB, puntC), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 2, new LineRAZ(2, puntD, puntB), project2.crossSections[3], "Post", 2);
            new ElementRAZ(project2, 3, new LineRAZ(3, puntB, puntE), project2.crossSections[3], "Diagonal", 3);

            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            //new LoadcaseRAZ(1,);
            LoadcaseRAZ lc1 = new LoadcaseRAZ(project2, 1);
            //LoadcaseRAZ lc2 = new LoadcaseRAZ(project2, 2);

            new LoadsPerLineRAZ(project2.elementRAZs[0], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-527.01, 37.28, 4.98));
            new LoadsPerLineRAZ(project2.elementRAZs[1], lc1, new LoadsRAZ(-911.43, -41.2, 12.64), new LoadsRAZ(0, 0, 0));
            new LoadsPerLineRAZ(project2.elementRAZs[2], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-259.8, 6.27, -3.06));
            new LoadsPerLineRAZ(project2.elementRAZs[3], lc1, new LoadsRAZ(419.3, 0.31, -2.66), new LoadsRAZ(0, 0, 0));

            //new LoadsPerLineRAZ(project2.elementRAZs[0], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[1], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[2], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[3], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));


            double tol = 1e-6;
            project2.CreateJoints(tol, 0, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }

        public Joint TestjointColumnC()
        {
            //info: Pratt K-joint of Topchord with Grasshopper values: diagonal rechts-beneden

            Project project2 = new Project("projectname");
            PointRAZ puntA = new PointRAZ(project2, 0, 0, -2);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, 0, 0, 2);
            PointRAZ puntD = new PointRAZ(project2, -2, 0, 0);
            PointRAZ puntE = new PointRAZ(project2, -3.125, 0, -1.5);


            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEB240", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA160", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "buis", KarambaIDEA.Core.CrossSection.Shape.HollowSection, steel, 200, 100, 10, 5, 6);

            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 0, new LineRAZ(0, puntA, puntB), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 1, new LineRAZ(1, puntB, puntC), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 2, new LineRAZ(2, puntD, puntB), project2.crossSections[3], "Post", 2);
            new ElementRAZ(project2, 3, new LineRAZ(3, puntB, puntE), project2.crossSections[3], "Diagonal", 3);

            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            //new LoadcaseRAZ(1,);
            LoadcaseRAZ lc1 = new LoadcaseRAZ(project2, 1);
            //LoadcaseRAZ lc2 = new LoadcaseRAZ(project2, 2);

            new LoadsPerLineRAZ(project2.elementRAZs[0], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-527.01, 37.28, 4.98));
            new LoadsPerLineRAZ(project2.elementRAZs[1], lc1, new LoadsRAZ(-911.43, -41.2, 12.64), new LoadsRAZ(0, 0, 0));
            new LoadsPerLineRAZ(project2.elementRAZs[2], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-259.8, 6.27, -3.06));
            new LoadsPerLineRAZ(project2.elementRAZs[3], lc1, new LoadsRAZ(419.3, 0.31, -2.66), new LoadsRAZ(0, 0, 0));

            //new LoadsPerLineRAZ(project2.elementRAZs[0], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[1], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[2], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[3], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));


            double tol = 1e-6;
            project2.CreateJoints(tol, 0, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }

        public Joint TestjointColumnD()
        {
            //info: Pratt K-joint of Topchord with Grasshopper values: diagonal rechts-beneden

            Project project2 = new Project("projectname");
            PointRAZ puntA = new PointRAZ(project2, 0, 0, -2);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, 0, 0, 2);
            PointRAZ puntD = new PointRAZ(project2, -2, 0, 0);
            PointRAZ puntE = new PointRAZ(project2, -3.125, 0, 1.5);


            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEB240", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA160", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "buis", KarambaIDEA.Core.CrossSection.Shape.HollowSection, steel, 200, 100, 10, 5, 6);

            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 0, new LineRAZ(0, puntA, puntB), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 1, new LineRAZ(1, puntB, puntC), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 2, new LineRAZ(2, puntD, puntB), project2.crossSections[3], "Post", 2);
            new ElementRAZ(project2, 3, new LineRAZ(3, puntB, puntE), project2.crossSections[3], "Diagonal", 3);

            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            //new LoadcaseRAZ(1,);
            LoadcaseRAZ lc1 = new LoadcaseRAZ(project2, 1);
            //LoadcaseRAZ lc2 = new LoadcaseRAZ(project2, 2);

            new LoadsPerLineRAZ(project2.elementRAZs[0], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-527.01, 37.28, 4.98));
            new LoadsPerLineRAZ(project2.elementRAZs[1], lc1, new LoadsRAZ(-911.43, -41.2, 12.64), new LoadsRAZ(0, 0, 0));
            new LoadsPerLineRAZ(project2.elementRAZs[2], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-259.8, 6.27, -3.06));
            new LoadsPerLineRAZ(project2.elementRAZs[3], lc1, new LoadsRAZ(419.3, 0.31, -2.66), new LoadsRAZ(0, 0, 0));

            //new LoadsPerLineRAZ(project2.elementRAZs[0], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[1], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[2], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[3], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));


            double tol = 1e-6;
            project2.CreateJoints(tol, 0, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }

        public Joint Testjoint1()
        {
            //info: Pratt K-joint of Topchord with Grasshopper values

            Project project2 = new Project("projectname");
            PointRAZ puntA = new PointRAZ(project2, -3.125, 0, 0);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, 3.125, 0, 0);
            PointRAZ puntD = new PointRAZ(project2, 0, 0, -1.5);
            PointRAZ puntE = new PointRAZ(project2, -3.125, 0, -1.5);


            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEB240", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA160", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "buis", KarambaIDEA.Core.CrossSection.Shape.HollowSection, steel, 400, 200, 20, 20, 6);

            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 0, new LineRAZ(0, puntA, puntB), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 1, new LineRAZ(1, puntB, puntC), project2.crossSections[0], "Bottomchord", 1);
            new ElementRAZ(project2, 2, new LineRAZ(2, puntD, puntB), project2.crossSections[1], "Post", 2);
            new ElementRAZ(project2, 3, new LineRAZ(3, puntE, puntB), project2.crossSections[3], "Diagonal", 3);

            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            //new LoadcaseRAZ(1,);
            LoadcaseRAZ lc1 = new LoadcaseRAZ(project2, 1);
            //LoadcaseRAZ lc2 = new LoadcaseRAZ(project2, 2);

            new LoadsPerLineRAZ(project2.elementRAZs[0], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-527.01, 37.28, 4.98));
            new LoadsPerLineRAZ(project2.elementRAZs[1], lc1, new LoadsRAZ(-911.43, -41.2, 12.64), new LoadsRAZ(0, 0, 0));
            new LoadsPerLineRAZ(project2.elementRAZs[2], lc1, new LoadsRAZ(0, 0, 0), new LoadsRAZ(-259.8, 6.27, -3.06));
            new LoadsPerLineRAZ(project2.elementRAZs[3], lc1, new LoadsRAZ(419.3, 0.31, -2.66), new LoadsRAZ(0, 0, 0));

            //new LoadsPerLineRAZ(project2.elementRAZs[0], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[1], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[2], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[3], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));


            double tol = 1e-6;
            project2.CreateJoints(tol, 0, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }


        public Joint Testjoint2()
        {
            //info: Pratt K-joint of Topchord with eccentricity, no loads

            Project project2 = new Project("projectname");
            PointRAZ puntA = new PointRAZ(project2, -3, 0, 0);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, 3, 0, 0);
            PointRAZ puntD = new PointRAZ(project2, 0, 0, -3);
            PointRAZ puntE = new PointRAZ(project2, 3, 0, -3);
            PointRAZ puntF = new PointRAZ(project2, 0.2, 0, 0);


            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA200", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA300", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);

            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 1, new LineRAZ(1, puntA, puntB), project2.crossSections[1], "Bottomchord", 1);
            new ElementRAZ(project2, 2, new LineRAZ(2, puntB, puntF), project2.crossSections[1], "Bottomchord", 1);
            new ElementRAZ(project2, 3, new LineRAZ(3, puntF, puntC), project2.crossSections[1], "Bottomchord", 1);
            new ElementRAZ(project2, 4, new LineRAZ(4, puntB, puntD), project2.crossSections[0], "Post", 2);
            new ElementRAZ(project2, 5, new LineRAZ(5, puntF, puntE), project2.crossSections[0], "Diagonal", 3);

            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));


            double tol = 1e-6;
            //eccentricty is 0.2meter
            project2.CreateJoints(tol, 0.2, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }


        public Joint Testjoint3()
        {
            //info: Pratt K-joint of Topchord, all loads positive value

            Project project2 = new Project("projectname");
            PointRAZ puntA = new PointRAZ(project2, -3.125, 0, 0);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, 3.125, 0, 0);
            PointRAZ puntD = new PointRAZ(project2, 0, 0, -1.5);
            PointRAZ puntE = new PointRAZ(project2, 3.125, 0, -1.5);


            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA160", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 0, new LineRAZ(0, puntA, puntB), project2.crossSections[1], "Bottomchord", 1);
            new ElementRAZ(project2, 1, new LineRAZ(1, puntB, puntC), project2.crossSections[1], "Bottomchord", 1);
            new ElementRAZ(project2, 2, new LineRAZ(2, puntD, puntB), project2.crossSections[0], "Post", 2);
            new ElementRAZ(project2, 3, new LineRAZ(3, puntB, puntE), project2.crossSections[0], "Diagonal", 3);

            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            //new LoadcaseRAZ(1,);
            LoadcaseRAZ lc1 = new LoadcaseRAZ(project2, 1);
            //LoadcaseRAZ lc2 = new LoadcaseRAZ(project2, 2);

            new LoadsPerLineRAZ(project2.elementRAZs[0], lc1, new LoadsRAZ(10, 10, 10), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[1], lc1, new LoadsRAZ(10, 10, 10), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[2], lc1, new LoadsRAZ(10, 10, 10), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[3], lc1, new LoadsRAZ(10, 10, 10), new LoadsRAZ(10, 10, 10));

            //new LoadsPerLineRAZ(project2.elementRAZs[0], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[1], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[2], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));
            //new LoadsPerLineRAZ(project2.elementRAZs[3], lc2, new LoadsRAZ(10, 40, 20), new LoadsRAZ(10, 40, 20));


            double tol = 1e-6;
            project2.CreateJoints(tol, 0, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }

        public Joint Testjoint4()
        {
            //info: Octopus-joint

            Project project2 = new Project("projectname");
            PointRAZ puntA = new PointRAZ(project2, -2, 0, 0);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, 2, 0, 0);
            PointRAZ puntD = new PointRAZ(project2, 0, 0, 2);
            PointRAZ puntE = new PointRAZ(project2, 0, 0, -2);

            PointRAZ puntF1 = new PointRAZ(project2, -2, 0, 1);
            PointRAZ puntF2 = new PointRAZ(project2, -1, 0, 2);
            PointRAZ puntF3 = new PointRAZ(project2, 2, 0, 1);
            PointRAZ puntF4 = new PointRAZ(project2, 1, 0, 2);

            PointRAZ puntG1 = new PointRAZ(project2, -2, 0, -1);
            PointRAZ puntG2 = new PointRAZ(project2, -1, 0, -2);
            PointRAZ puntG3 = new PointRAZ(project2, 2, 0, -1);
            PointRAZ puntG4 = new PointRAZ(project2, 1, 0, -2);




            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA160", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 0, new LineRAZ(0, puntA, puntB), project2.crossSections[1], "Bottomchord", 1);//endpoint
            new ElementRAZ(project2, 1, new LineRAZ(1, puntB, puntC), project2.crossSections[1], "Bottomchord", 1);
            new ElementRAZ(project2, 2, new LineRAZ(2, puntD, puntB), project2.crossSections[0], "Post", 2);//endpoint
            new ElementRAZ(project2, 3, new LineRAZ(3, puntE, puntB), project2.crossSections[0], "Diagonal", 3);//endpoint

            new ElementRAZ(project2, 4, new LineRAZ(4, puntB, puntF1), project2.crossSections[0], "Diagonal", 3);
            new ElementRAZ(project2, 5, new LineRAZ(5, puntB, puntF2), project2.crossSections[0], "Diagonal", 3);
            new ElementRAZ(project2, 6, new LineRAZ(6, puntB, puntF3), project2.crossSections[0], "Diagonal", 3);
            new ElementRAZ(project2, 7, new LineRAZ(7, puntF4, puntB), project2.crossSections[0], "Diagonal", 3);//endpoint
            new ElementRAZ(project2, 8, new LineRAZ(8, puntB, puntG1), project2.crossSections[0], "Diagonal", 3);
            new ElementRAZ(project2, 9, new LineRAZ(9, puntG2, puntB), project2.crossSections[0], "Diagonal", 3);//endpoint
            new ElementRAZ(project2, 10, new LineRAZ(10, puntB, puntG3), project2.crossSections[0], "Diagonal", 3);
            new ElementRAZ(project2, 11, new LineRAZ(11, puntB, puntG4), project2.crossSections[0], "Diagonal", 3);

            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            LoadcaseRAZ lc1 = new LoadcaseRAZ(project2, 1);
            new LoadsPerLineRAZ(project2.elementRAZs[0], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[1], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[2], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[3], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[4], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[5], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[6], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[7], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[8], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[9], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[10], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[11], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));


            double tol = 1e-6;
            project2.CreateJoints(tol, 0, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }

        public Joint Testjoint5()
        {
            //info: Octopus-joint, all end points

            Project project2 = new Project("projectname");
            PointRAZ puntA = new PointRAZ(project2, -2, 0, 0);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, 2, 0, 0);
            PointRAZ puntD = new PointRAZ(project2, 0, 0, 2);
            PointRAZ puntE = new PointRAZ(project2, 0, 0, -2);

            PointRAZ puntF1 = new PointRAZ(project2, -2, 0, 1);
            PointRAZ puntF2 = new PointRAZ(project2, -1, 0, 2);
            PointRAZ puntF3 = new PointRAZ(project2, 2, 0, 1);
            PointRAZ puntF4 = new PointRAZ(project2, 1, 0, 2);

            PointRAZ puntG1 = new PointRAZ(project2, -2, 0, -1);
            PointRAZ puntG2 = new PointRAZ(project2, -1, 0, -2);
            PointRAZ puntG3 = new PointRAZ(project2, 2, 0, -1);
            PointRAZ puntG4 = new PointRAZ(project2, 1, 0, -2);




            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA160", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 0, new LineRAZ(0, puntA, puntB), project2.crossSections[1], "Bottomchord", 1);//endpoint
            new ElementRAZ(project2, 1, new LineRAZ(1, puntC, puntB), project2.crossSections[1], "Bottomchord", 1);//endpoint
            new ElementRAZ(project2, 2, new LineRAZ(2, puntD, puntB), project2.crossSections[0], "Post", 2);//endpoint
            new ElementRAZ(project2, 3, new LineRAZ(3, puntE, puntB), project2.crossSections[0], "Diagonal", 3);//endpoint

            new ElementRAZ(project2, 4, new LineRAZ(4, puntF1, puntB), project2.crossSections[0], "Diagonal", 3);//endpoint
            new ElementRAZ(project2, 5, new LineRAZ(5, puntF2, puntB), project2.crossSections[0], "Diagonal", 3);//endpoint
            new ElementRAZ(project2, 6, new LineRAZ(6, puntF3, puntB), project2.crossSections[0], "Diagonal", 3);//endpoint
            new ElementRAZ(project2, 7, new LineRAZ(7, puntF4, puntB), project2.crossSections[0], "Diagonal", 3);//endpoint
            new ElementRAZ(project2, 8, new LineRAZ(8, puntG1, puntB), project2.crossSections[0], "Diagonal", 3);//endpoint
            new ElementRAZ(project2, 9, new LineRAZ(9, puntG2, puntB), project2.crossSections[0], "Diagonal", 3);//endpoint
            new ElementRAZ(project2, 10, new LineRAZ(10, puntG3, puntB), project2.crossSections[0], "Diagonal", 3);//endpoint
            new ElementRAZ(project2, 11, new LineRAZ(11, puntG4, puntB), project2.crossSections[0], "Diagonal", 3);//endpoint

            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            LoadcaseRAZ lc1 = new LoadcaseRAZ(project2, 1);
            new LoadsPerLineRAZ(project2.elementRAZs[0], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[1], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[2], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[3], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[4], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[5], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[6], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[7], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[8], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[9], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[10], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[11], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));


            double tol = 1e-6;
            project2.CreateJoints(tol, 0, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }

        public Joint Testjoint6()
        {
            //info: Octopus-joint, all start points

            Project project2 = new Project("projectname");
            PointRAZ puntA = new PointRAZ(project2, -2, 0, 0);
            PointRAZ puntB = new PointRAZ(project2, 0, 0, 0);
            PointRAZ puntC = new PointRAZ(project2, 2, 0, 0);
            PointRAZ puntD = new PointRAZ(project2, 0, 0, 2);
            PointRAZ puntE = new PointRAZ(project2, 0, 0, -2);

            PointRAZ puntF1 = new PointRAZ(project2, -2, 0, 1);
            PointRAZ puntF2 = new PointRAZ(project2, -1, 0, 2);
            PointRAZ puntF3 = new PointRAZ(project2, 2, 0, 1);
            PointRAZ puntF4 = new PointRAZ(project2, 1, 0, 2);

            PointRAZ puntG1 = new PointRAZ(project2, -2, 0, -1);
            PointRAZ puntG2 = new PointRAZ(project2, -1, 0, -2);
            PointRAZ puntG3 = new PointRAZ(project2, 2, 0, -1);
            PointRAZ puntG4 = new PointRAZ(project2, 1, 0, -2);




            KarambaIDEA.Core.MaterialSteel steel = new MaterialSteel(project2, MaterialSteel.SteelGrade.S275);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA100", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            KarambaIDEA.Core.CrossSection.CreateNewOrExisting(project2, "HEA160", KarambaIDEA.Core.CrossSection.Shape.ISection, steel, 20, 20, 20, 20, 20);
            //CrossSection cross = new CrossSection(project2, "HEA100", "I", steel, 20, 20, 20, 20, 20);

            new ElementRAZ(project2, 0, new LineRAZ(0, puntB, puntA), project2.crossSections[1], "Bottomchord", 1);
            new ElementRAZ(project2, 1, new LineRAZ(1, puntB, puntC), project2.crossSections[1], "Bottomchord", 1);
            new ElementRAZ(project2, 2, new LineRAZ(2, puntB, puntD), project2.crossSections[0], "Post", 2);
            new ElementRAZ(project2, 3, new LineRAZ(3, puntB, puntE), project2.crossSections[0], "Diagonal", 3);

            new ElementRAZ(project2, 4, new LineRAZ(4, puntB, puntF1), project2.crossSections[0], "Diagonal", 3);
            new ElementRAZ(project2, 5, new LineRAZ(5, puntB, puntF2), project2.crossSections[0], "Diagonal", 3);
            new ElementRAZ(project2, 6, new LineRAZ(6, puntB, puntF3), project2.crossSections[0], "Diagonal", 3);
            new ElementRAZ(project2, 7, new LineRAZ(7, puntB, puntF4), project2.crossSections[0], "Diagonal", 3);
            new ElementRAZ(project2, 8, new LineRAZ(8, puntB, puntG1), project2.crossSections[0], "Diagonal", 3);
            new ElementRAZ(project2, 9, new LineRAZ(9, puntB, puntG2), project2.crossSections[0], "Diagonal", 3);
            new ElementRAZ(project2, 10, new LineRAZ(10, puntB, puntG3), project2.crossSections[0], "Diagonal", 3);
            new ElementRAZ(project2, 11, new LineRAZ(11, puntB, puntG4), project2.crossSections[0], "Diagonal", 3);

            List<PointRAZ> pointRAZs = new List<PointRAZ>();
            pointRAZs.Add(puntB);

            project2.hierarchylist.Add(new Hierarchy(0, "Column"));
            project2.hierarchylist.Add(new Hierarchy(1, "Topchord"));
            project2.hierarchylist.Add(new Hierarchy(2, "Bottomchord"));
            project2.hierarchylist.Add(new Hierarchy(3, "Post"));
            project2.hierarchylist.Add(new Hierarchy(4, "Diagonal"));

            LoadcaseRAZ lc1 = new LoadcaseRAZ(project2, 1);
            new LoadsPerLineRAZ(project2.elementRAZs[0], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[1], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[2], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[3], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[4], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[5], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[6], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[7], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[8], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[9], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[10], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));
            new LoadsPerLineRAZ(project2.elementRAZs[11], lc1, new LoadsRAZ(5, 5, 5), new LoadsRAZ(10, 10, 10));


            double tol = 1e-6;
            project2.CreateJoints(tol, 0, pointRAZs, project2.elementRAZs, project2.hierarchylist);


            Joint joint = project2.joints[0];
            return joint;
        }
    }
}
