using System;
using System.Collections.Generic;

using Rhino.Geometry;
using System.Linq;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;


using KarambaIDEA.Core;


namespace KarambaIDEA.Grasshopper
{
    public class JointViewer : GH_Component
    {
        public JointViewer() : base("JointViewer", "JV", "Get insight in how joints are created from a list of lines", "KarmabaIDEA", "KarambaIDEA")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "L", "Lines of geometry", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "P", "Points of connections", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("tree", "tr", "tree with lines", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("IDS", "IDS", "IDS", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables
            List<Line> lines = new List<Line>();
            List<Point3d> points = new List<Point3d>();           
            
            //Link input
            DA.GetDataList(0, lines);
            DA.GetDataList(1, points);
           
            //output variables
            DataTree<int> IDS = new DataTree<int>();
            DataTree<Line> tree = new DataTree<Line>();
            
            //tolerance needed to cover rounding errors
            double tol = 1e-6;

            //loop over data
            for (int i = 0; i < points.Count; i++)
            {
                Point3d ptree = points[i];
                GH_Path path = new GH_Path(i);
                foreach (Line linetree in lines)
                {
                    //if startpoint is equal to current point 
                    if (Math.Abs(ptree.X - linetree.From.X) < tol && Math.Abs(ptree.Y - linetree.From.Y) < tol && Math.Abs(ptree.Z - linetree.From.Z) < tol)
                    {
                        tree.Add(linetree, path);
                        IDS.Add(lines.IndexOf(linetree), path);
                    }
                    //if endpoint is equal to current point 
                    if (Math.Abs(ptree.X - linetree.To.X) < tol && Math.Abs(ptree.Y - linetree.To.Y) < tol && Math.Abs(ptree.Z - linetree.To.Z) < tol)
                    {
                        tree.Add(linetree, path);
                        IDS.Add(lines.IndexOf(linetree), path);
                    }
                }               
            }

            //link output         
            DA.SetDataTree(0, tree);
            DA.SetDataTree(1, IDS);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                
                return Properties.Resources.KarambaIDEAviewer_logo;

            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("0b746c51-0b7f-40b2-9112-0da3e1903fef"); }
        }
    }
}
