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


namespace KarambaIDEA.Grasshopper
{
    public class RetrieveLinesPoints : GH_Component
    {
        public RetrieveLinesPoints() : base("Retrieve Lines and Points", "RetLinPo", "Retrieve lines and points of project", "KarambaIDEA", "2. Project utilities")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "P", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "L", "Lines of project", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "P", "Points of project", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_KarambaIdeaProject ghProject = null;

            //Link input
            DA.GetData<GH_KarambaIdeaProject>(0, ref ghProject);

            Project project = ghProject.Value;

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

                return Properties.Resources.RetrieveLinesAndPoints;

            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("4c035d94-5090-4ee2-876a-d5391c1c8ba7"); }
        }
    }
}
