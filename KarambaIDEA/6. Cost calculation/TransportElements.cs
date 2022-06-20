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
        public TransportElements() : base("Transport Elements Generator", "Transport Elements generator", "Generate 1D internal transport elements", "KarambaIDEA", "4. Cost calculation")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("GroupNames", "GroupNames", "GroupNames/Hierarchies that will be asseset in algorithm", GH_ParamAccess.list," ");
            pManager.AddNumberParameter("Max Length", "Max Length", "Maximum length of 1D element", GH_ParamAccess.item);
            pManager.AddNumberParameter("Max Angle [rad]", "Max Angle [rad]", "Maximum angle in radians between elements", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Transport Element Lines", "Transport Element", "Lines of Transport elements", GH_ParamAccess.tree);
            pManager.AddLineParameter("All Element Lines", "All Elements", "Lines of all element", GH_ParamAccess.tree);
            //pManager.AddNumberParameter("Transport Element weights", "All Element weights", "Element weight", GH_ParamAccess.tree);
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
            DataTree<Rhino.Geometry.Line> transportLines = new DataTree<Rhino.Geometry.Line>();
            DataTree<Rhino.Geometry.Line> allLines = new DataTree<Rhino.Geometry.Line>();
            //DataTree<double> weights = new DataTree<double>();

            //variables for tree path
            int a = 0;
            int b = 0;
            GH_Path path = new GH_Path(a, b);

            //PROCESS
            //Sort list elements based on hierarchy
            foreach (Hierarchy hierarchy in project.hierarchylist)
            {
                List<Line> hierarchydata = new List<Line>();
                foreach (Element ele in project.elements)
                {
                    if (ele.numberInHierarchy == hierarchy.numberInHierarchy)
                    {
                        hierarchydata.Add(ele.Line);
                    }
                }
                if (groupNames.Contains(hierarchy.groupname))
                {
                    Line line = hierarchydata[0]; //start at first item of list 
                    List<Line> templist = new List<Line>();//create temporary list
                    templist = hierarchydata;//copy data
                    templist.Remove(line);//remove first item
                    double length = line.Length;//set length
                    if (maxLength == 0.0)
                    {
                        break;
                    }

                next:
                    double overLength = length - maxLength;
                    if (overLength > 1e-6)//overlength is positive
                    {
                        //Splitline definition for forwards integration
                        double alength = line.Length - overLength;
                        List<Line> linesFromSplit = Line.SplitLine(line, alength);
                        Line aline = linesFromSplit[0];//first piece
                        Line bline = linesFromSplit[1];//second piece
                        AddLineToTree(a, b, aline, transportLines);//add first piece
                        AddLineToTree(a, b, aline, allLines);//add first piece
                        b = b + 1;//go to next branch
                        length = overLength;//Next part starts with the overlength part
                        line = bline;//second piece become the line
                        goto next;
                        //TODO:add splitline definition for backwards integration
                    }
                    if (overLength < -1e-6)//overlength is negative
                    {
                        AddLineToTree(a, b, line, transportLines);//add element to list
                        AddLineToTree(a, b, line, allLines);
                    }
                    else//line is equal to overlength
                    {
                        AddLineToTree(a, b, line, transportLines);//add element to list
                        AddLineToTree(a, b, line, allLines);
                        templist.Remove(line);//remove found element from templist
                        length = 0;//reset length
                        b = b + 1;//got to next branch
                    }
                    foreach (Line L1 in templist)
                    {
                        double angle = Vector.AngleBetweenVectors(line.Vector, L1.Vector);
                        if (line.end == L1.start && angle < maxAngle)//Forward integration
                        {
                            line = L1;//continue with the found element;                
                            length = length + line.Length;//add length
                            templist.Remove(L1);//remove found element from templist
                            goto next;
                        }
                        if (line.start == L1.end && angle < maxAngle)//Backward integration
                        {
                            line = L1;//continue with the found element;
                            length = length + line.Length;//add length
                            templist.Remove(L1);//remove found element from templist
                            goto next;
                        }
                    }
                    if (templist.Count > 0)
                    {
                        line = templist[0];//get first element of templist
                        templist.Remove(line);//remove found element from templist
                        b = b + 1;//change branch, make a new snake
                        length = line.Length;//reset length
                        goto next;
                    }
                }
                else
                {
                    foreach (Line line in hierarchydata)
                    {
                        AddLineToTree(a, b, line, allLines);
                        b = b + 1;
                    }
                    //These elements will not be assesed in algorithm
                    //and will therefor not be composed
                }
                a = a + 1;//different group, change brach
                b = 0;//reset b
            }
            //Forward integration
            //Backward integration
            //assembling of two snakes
            //Snake start point and Snake end point
            //where to start end of line or start of line?
            //What if the length of the first line already exceeds the maxLenght?

            //link output
            DA.SetDataTree(0, transportLines);
            DA.SetDataTree(1, allLines);
            //DA.SetDataTree(2, weights); 
        }
        
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.TransportElements_trans_01;

            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("c9a16af6-596f-4633-a268-b493c136c0ba"); }
        }

        public void AddLineToTree(int a, int b, Line line, DataTree<Rhino.Geometry.Line> transportLines)
        {
            GH_Path path = new GH_Path(a, b);
            transportLines.Add(ImportGrasshopperUtils.CastLineToRhino(line), path);
        }

            
        
    }
}
