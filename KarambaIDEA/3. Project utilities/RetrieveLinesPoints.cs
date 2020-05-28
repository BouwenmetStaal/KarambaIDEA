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
    public class RetrieveLinesPoints : GH_Component
    {
        public RetrieveLinesPoints() : base("RetrieveLinesPoints", "RetrieveLinesPoints", "Retrieve lines and points of project", "KarambaIDEA", "3. Project utilities")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "Lines", "Lines of project", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "Points", "Points of project", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            Project project = new Project();

            //Link input
            DA.GetData(0, ref project);

            //output variables
            List<Rhino.Geometry.Line> lines = new List<Rhino.Geometry.Line>();
            List<Rhino.Geometry.Point3d> points = new List<Rhino.Geometry.Point3d>();

            foreach(Element ele in project.elements)
            {
                lines.Add(ImportGrasshopperUtils.CastLineToRhino(ele.Line));
            }
            foreach(KarambaIDEA.Core.Point point in project.points)
            {
                points.Add(ImportGrasshopperUtils.CastPointToRhino(point));
            }
           

            //link output
            DA.SetDataList(0, lines);
            DA.SetDataList(1, points);
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
            get { return new Guid("4c035d94-5090-4ee2-876a-d5391c1c8ba7"); }
        }
    }
}
