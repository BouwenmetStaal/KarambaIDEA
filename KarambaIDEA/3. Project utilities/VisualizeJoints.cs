// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Linq;
using System.Collections.Generic;

using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using KarambaIDEA.Core;


namespace KarambaIDEA
{
    public class VisualizeJoints: GH_Component
    {
        public VisualizeJoints() : base("Visualize Joints", "Visualize Joints", "Visualize all joints in project", "KarambaIDEA", "3. Project utilities")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Joint types", "Joint types", "Types of joint in project", GH_ParamAccess.tree);
            pManager.AddTextParameter("Brandnames", "Brandnames", "Defined brandname per joint", GH_ParamAccess.tree);
            pManager.AddPointParameter("Centerpoints", "Centerpoints", "Centerpoint per joint", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            Project project = new Project();

            //Link input
            DA.GetData(0, ref project);

            //output variables
            DataTree<Rhino.Geometry.Line> linetree = new DataTree<Rhino.Geometry.Line>();
            DataTree<string> brandNames = new DataTree<string>();
            DataTree<Rhino.Geometry.Point3d> centerPoints = new DataTree<Rhino.Geometry.Point3d>();

            //Define Brandnames and assemble tree
            //project.SetBrandNames(project);

            //select unique brandName   
            var lstBrand = project.joints.Select(a => a.brandName).Distinct().ToList();

            //Create tree based on unique brandNames
            for (int a = 0; a < lstBrand.Count; a++)
            {
                int d = 0;
                for (int b = 0; b < project.joints.Count; b++)
                {
                    if (lstBrand[a] == project.joints[b].brandName)
                    {
                        for (int c = 0; c < project.joints[b].attachedMembers.Count; c++)
                        {
                            Core.Line oriline = project.joints[b].attachedMembers[c].ideaLine;
                            Core.Point centerpoint = project.joints[b].centralNodeOfJoint;
                            Core.Line line = Core.Line.MoveLineToOrigin(centerpoint, oriline);
                            GH_Path path = new GH_Path(a, d);
                            Point3d start = new Point3d(line.start.X, line.start.Y, line.start.Z);
                            Point3d end = new Point3d(line.end.X, line.end.Y, line.end.Z);
                            Rhino.Geometry.Line rhinoline = new Rhino.Geometry.Line(start, end);
                            linetree.Add(rhinoline, path);
                        }
                        brandNames.Add(project.joints[b].brandName, new GH_Path(a, d));
                        centerPoints.Add(new Point3d(0.0,0.0,0.0), new GH_Path(a, d));
                        //Go to next joint [d]
                        d = d + 1;
                    }
                }
            }

            //link output
            DA.SetDataTree(0, linetree);
            DA.SetDataTree(1, brandNames);
            DA.SetDataTree(2, centerPoints);
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
            get { return new Guid("3c292c35-ff3f-43fa-a0b6-d2d3e00a3aba"); }
        }
    }
}
