using System;
using System.Collections;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using System.Linq;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace KarambaIDEA.Grasshopper
{
    public class JointVector : GH_Component
    {
        public JointVector() : base("JointVector", "Id", "Define normal vector of the joint", "KarmabaIDEA", "KarambaIDEA")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "L", "Lines of geometry", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "P", "Points of connections", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Vec", "Vec", "JointVector", GH_ParamAccess.list);

        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            List<Line> lines = new List<Line>();
            List<Point3d> points = new List<Point3d>();

            //Temp variables
            DataTree<Line> tree = new DataTree<Line>();

            //output variables
            List<Vector3d> JointVectors = new List<Vector3d>();    

            //Link input
            DA.GetDataList(0, lines);
            DA.GetDataList(1, points);

            //tolerance needed to cover rounding errors
            double tol = 1e-6;


            //loop over data and create joints
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
                    }
                    //if endpoint is equal to current point 
                    if (Math.Abs(ptree.X - linetree.To.X) < tol && Math.Abs(ptree.Y - linetree.To.Y) < tol && Math.Abs(ptree.Z - linetree.To.Z) < tol)
                    {
                        tree.Add(linetree, path);
                    }
                }
            }

            //define vector per joint
            for (int i = 0; i < tree.Branches.Count; i++)
            {
                List<Vector3d> vecs = new List<Vector3d>();
                Vector3d vector = new Vector3d();

                foreach (Line lijn in tree.Branch(i))
                {
                    if (tree.Branch(i).Count == 1)
                    {
                        double xvec = lijn.Direction.X;
                        double yvec = lijn.Direction.Y;
                        double zvec = lijn.Direction.Z;
                        
                        //Define LCS (local-y in XY plane) 
                        if (xvec == 0.0 && yvec == 0.0)
                        {
                            Vector3d loodrecht1 = new Vector3d((-zvec), 0.0, (xvec));
                            vecs.Add(loodrecht1);
                        }
                        else
                        {
                            Vector3d loodrecht2 = new Vector3d((-zvec * xvec), (-zvec * yvec), ((xvec * xvec) + (yvec * yvec)));
                            vecs.Add(loodrecht2);
                        }
                    }
                    else
                    {
                        Vector3d v1 = lijn.Direction;
                        Vector3d a = new Vector3d(v1.X / v1.Length, v1.Y / v1.Length, v1.Z / v1.Length);

                        foreach (Line lijn2 in tree.Branch(i))
                        {
                            Vector3d v2 = lijn2.Direction;
                            Vector3d b = new Vector3d(v2.X / v2.Length, v2.Y / v2.Length, v2.Z / v2.Length);

                            //Kruisproduct
                            Vector3d loodrecht = new Vector3d((a.Y * b.Z - a.Z * b.Y), (a.Z * b.X - a.X * b.Z), (a.X * b.Y - a.Y * b.X));

                            //Filter vectoren met een z-coordinaat groter dan nul
                            //afronding kan voor ruis zorgen, bij centrale knoop
                            if (loodrecht.Z > 0.0001)
                            {
                                vecs.Add(loodrecht);
                            }
                        }
                    }
                }

                //Gemiddelde waardes x,y,z van set vectoren vinden
                double xcor = new double();
                double ycor = new double();
                double zcor = new double();

                int lengte = vecs.Count;

                foreach (Vector3d vec in vecs)
                {
                    xcor = xcor + vec.X;
                    ycor = ycor + vec.Y;
                    zcor = zcor + vec.Z;
                }

                vector = new Vector3d(xcor / lengte, ycor / lengte, zcor / lengte);
                JointVectors.Add(vector);
            }
            
            //link output   
            DA.SetDataList(0, JointVectors);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;

                return Properties.Resources.KarambaIDEA_logo_JointVector;

            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("157eeca2-26a5-422f-96d8-76ab78eef843"); }
        }
    }
}
