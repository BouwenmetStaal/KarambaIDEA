// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using Grasshopper.Kernel;
using KarambaIDEA.Core;
using System;
using System.Collections.Generic;
using Grasshopper;

using Grasshopper.Kernel.Data;

namespace KarambaIDEA
{
    public class TransportElements : GH_Component
    {
        public TransportElements() : base("Transport Elements Generator", "Transport Elements generator", "Generate 1D internal transport elements", "KarambaIDEA", "3. Project utilities")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("GroupNames", "GroupNames", "GroupNames/Hierarchies that will be asseset in algorithm", GH_ParamAccess.list);
            pManager.AddNumberParameter("Max Length", "Max Length", "Maximum length of 1D element", GH_ParamAccess.item);
            pManager.AddNumberParameter("Max Angle [rad]", "Max Angle [rad]", "Maximum angle in radians between elements", GH_ParamAccess.item);
            //TODO: add hierarchy, only members of the set hierarchies will be asseset

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Transport Element Lines", "Transport Element", "Lines of Transport elements", GH_ParamAccess.tree);
            pManager.AddLineParameter("All Element Lines", "All Elements", "Lines of all element", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Transport Element weights", "All Element weights", "Element weight", GH_ParamAccess.tree);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            Project project = new Project();
            List<string> groupNamesDirty = new List<string>();
            List<string> groupNames = new List<string>();

            double maxLength = 0.0;
            double maxAngle = 0.0;

            //Link input
            DA.GetData(0, ref project);
            DA.GetDataList(1, groupNamesDirty);
            DA.GetData(2, ref maxLength);
            DA.GetData(3, ref maxAngle);

            //Clean groupnames list from nextline ("\r\n") command produced by Karamba
            groupNames = ImportGrasshopperUtils.DeleteEnterCommandsInGHStrings(groupNamesDirty);

            //output variables
            //List<Rhino.Geometry.Line> lines = new List<Rhino.Geometry.Line>();

            DataTree<Rhino.Geometry.Line> lines = new DataTree<Rhino.Geometry.Line>();
            DataTree<double> weights = new DataTree<double>();
            //List<double> weights = new List<double>();

            List<List<Element>> data = new List<List<Element>>();

            int a = 0;
            int b = 0;
            GH_Path path = new GH_Path(a, b);

            //Sort list elements based on hierarchy
            foreach (Hierarchy hierarchy in project.hierarchylist)
            {
                List<Element> hierarchydata = new List<Element>();
                foreach (Element ele in project.elements)
                {
                    if (ele.numberInHierarchy == hierarchy.numberInHierarchy)
                    {
                        hierarchydata.Add(ele);
                    }
                }
                //Now we have a list with all elements of a certain hierarchy
                Element element = hierarchydata[0]; //start at first item of list 
                //add first element to list
                path = new GH_Path(a, b);
                lines.Add(ImportGrasshopperUtils.CastLineToRhino(element.line), path);
                List<Element> templist = new List<Element>();
                hierarchydata.RemoveAt(0);
                templist = hierarchydata;
                double length = element.line.Length;

                next:
                foreach (Element ele1 in templist)
                {
                    double angle = Vector.AngleBetweenVectors(element.line.Vector, ele1.line.Vector);

                    if (length > maxLength)//length is longer than maximum transport length, so cut element
                    {
                        double overLength = length - maxLength;
                        //where to start end of line or start of line?
                        //What if the length of the first line already exceeds the maxLenght?
                    }


                    if (element.line.end == ele1.line.start && angle < maxAngle)//Forward integration
                    {
                        //add element to list
                        path = new GH_Path(a, b);
                        lines.Add(ImportGrasshopperUtils.CastLineToRhino(ele1.line),path);

                        //continue with the found element;
                        element = ele1;

                        //remove found element from templist
                        templist.Remove(ele1);

                        goto next;
                    }
                    if (element.line.start == ele1.line.end && angle < maxAngle)//Backward integration
                    {
                        //add element to list
                        path = new GH_Path(a, b);
                        lines.Add(ImportGrasshopperUtils.CastLineToRhino(ele1.line), path);

                        //continue with the found element;
                        element = ele1;

                        //remove found element from templist
                        templist.Remove(ele1);
                        goto next;
                    }
                    else //goto next element
                    {
                        element = templist[0];
                        templist.Remove(ele1);
                        b = b + 1;
                        //add element to list
                        path = new GH_Path(a, b);
                        lines.Add(ImportGrasshopperUtils.CastLineToRhino(ele1.line), path);
                        goto next;
                    }
                }
                a = a + 1;
            }


            //
            //Forward integration
            //Backward integration
            //assembling of two snakes

            //Snake start point and Snake end point

            //Cut snakes by length


            //link output
            DA.SetDataTree(0, lines);
            DA.SetDataTree(1, weights); //visualize weldingvolume through brep
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.KarambaIDEA_logo;

            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("c9a16af6-596f-4633-a268-b493c136c0ba"); }
        }
    }
}
