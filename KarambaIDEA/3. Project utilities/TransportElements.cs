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
            DataTree<Rhino.Geometry.Line> lines = new DataTree<Rhino.Geometry.Line>();
            DataTree<double> weights = new DataTree<double>();

            //List<List<Element>> data = new List<List<Element>>();

            int a = 0;
            int b = 0;
            GH_Path path = new GH_Path(a, b);

            //Sort list elements based on hierarchy
            foreach (Hierarchy hierarchy in project.hierarchylist)
            {
                foreach (string groupName in groupNames)
                {
                    if (hierarchy.groupname == groupName)
                    {
                        List<Line> hierarchydata = new List<Line>();
                        foreach (Element ele in project.elements)
                        {
                            if (ele.numberInHierarchy == hierarchy.numberInHierarchy)
                            {
                                hierarchydata.Add(ele.line);
                            }
                        }
                        //Now we have a list with all elements of a certain hierarchy
                        Line line = hierarchydata[0]; //start at first item of list 
                        AddLineToTree(a, b, line, lines);
                        List<Line> templist = new List<Line>();
                        
                        templist = hierarchydata;
                        
                        //templist.Remove(line);
                        double length = line.Length;
                        next:
                        templist.Remove(line);
                        /*
                        
                        }
                        */
                        foreach (Line L1 in templist)
                        {
                            double angle = Vector.AngleBetweenVectors(line.Vector, L1.Vector);
                            if (line.end == L1.start && angle < maxAngle)//Forward integration
                            {
                                //add element to list
                                AddLineToTree(a, b, L1, lines);

                                //continue with the found element;
                                line = L1;

                                //remove found element from templist
                                templist.Remove(L1);
                                length = length + line.Length;

                                goto next;
                            }
                            if (line.start == L1.end && angle < maxAngle)//Backward integration
                            {
                                //add element to list
                                AddLineToTree(a, b, L1, lines);

                                //continue with the found element;
                                line = L1;

                                //remove found element from templist
                                templist.Remove(L1);
                                length = length + line.Length;
                                goto next;
                            }
                            
                        }
                        if (templist.Count>0)
                        {
                            line = templist[0];
                            //templist.Remove(L1);
                            b = b + 1;
                            //add element to list
                            AddLineToTree(a, b, line, lines);
                            length = length + line.Length;
                            goto next;
                        }
                        a = a + 1;
                    }
                    else
                    {
                        //These elements will not be assesed in algorithm
                        //and will therefor not be composed
                    }

                }


                //
                //Forward integration
                //Backward integration
                //assembling of two snakes

                //Snake start point and Snake end point

                //Cut snakes by length

                DataTree<Rhino.Geometry.Line> lines2 = new DataTree<Rhino.Geometry.Line>();
                lines2 = lines;

                weights.Add(125, new GH_Path(0));

                //link output
                DA.SetDataTree(0, lines2);
                DA.SetDataTree(2, weights); //visualize weldingvolume through brep
            }
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

        public void AddLineToTree(int a, int b, Line line, DataTree<Rhino.Geometry.Line> lines)
        {
            GH_Path path = new GH_Path(a, b);
            lines.Add(ImportGrasshopperUtils.CastLineToRhino(line), path);
        }

        public void SplitIfNeeded(int a, int b, double length, double maxLength, Line line, DataTree<Rhino.Geometry.Line> lines, List<Line> templist)
        {
            if (length > maxLength)//length is longer than maximum transport length, so cut element
            {
                double overLength = length - maxLength;

                if (Math.Abs(overLength - line.Length) < 1e-6)
                {
                    //add element to list
                    b = b + 1;
                    AddLineToTree(a, b, line, lines);
                    //remove found element from templist
                    templist.Remove(line);
                    length = length + line.Length;
                    //goto next;
                }
                else
                {
                    List<Line> linesFromSplit = Line.SplitLine(line, overLength);
                    Line aline = linesFromSplit[0];
                    Line bline = linesFromSplit[1];

                    //add element to list
                    AddLineToTree(a, b, aline, lines);


                    //where to start end of line or start of line?
                    //What if the length of the first line already exceeds the maxLenght?
                    b = b + 1;
                    //add element to list
                    AddLineToTree(a, b, bline, lines);

                    //remove found element from templist
                    templist.Remove(line);

                    //TODO: make script for lines instead of elements
                    length = overLength;//Next part starts with the overlength part
                                        //goto next;
                }
            }
        }
    }
}
