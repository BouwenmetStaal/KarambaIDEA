// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Collections.Generic;

using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using KarambaIDEA.Core;



namespace KarambaIDEA.Grasshopper
{
    public class LinesSCIA : GH_Component
    {
        public LinesSCIA() : base("SCIA Lines", "SL", "Create lines from SCIA PointNames", "KarambaIDEA", "0. Utilities")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Point names", "PN", "Names of point", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list);

            pManager.AddTextParameter("Name StartPoint", "N SP", "Name of startpoint", GH_ParamAccess.list);
            pManager.AddTextParameter("Name EndPoint", "N EP", "Name of startpoint", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "L", "Created Lines", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables
            List<string> pointName = new List<string>();
            List<Point3d> points = new List<Point3d>();
            List<string> startpointName = new List<string>();
            List<string> endpointName = new List<string>();


            //Link input
            DA.GetDataList(0, pointName);
            DA.GetDataList(1, points);
            DA.GetDataList(2, startpointName);
            DA.GetDataList(3, endpointName);

            //output variables
            List<Rhino.Geometry.Line> lines = new List<Rhino.Geometry.Line>();

            //Link pointName with points
            List<Core.Point> plist = new List<Core.Point>();
            for (int i=0; i<points.Count; i++)
            {
                Core.Point p = new Core.Point(pointName[i], points[i].X, points[i].Y, points[i].Z);
                plist.Add(p);
            }
            //Create lines bases on startpointName and endpointName
            for (int b = 0; b < startpointName.Count; b++)
            {
                Core.Point start = plist.Find(a => a.name == startpointName[b]);
                Core.Point end = plist.Find(a => a.name == endpointName[b]);
                Rhino.Geometry.Line line = new Rhino.Geometry.Line(start.X, start.Y, start.Z, end.X, end.Y, end.Z);
                lines.Add(line);
            }
            //link output
            DA.SetDataList(0, lines);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.KarambaIDEA_logo_LinesFromNodes;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("efc0b433-35c1-4ec1-8c12-7303043f9e9c"); }
        }
    }
}
