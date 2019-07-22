/*
using System;
using System.Collections.Generic;

using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using KarambaIDEA.Core;

namespace KarambaIDEA.Grasshopper
{
    
    public class RotateVector : GH_Component
    {
        public RotateVector() : base("RV", "RV", "RV", "KarmabaIDEA", "KarambaIDEA")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("V", "V", "vector V", GH_ParamAccess.item);
            pManager.AddVectorParameter("N", "N", "vector N", GH_ParamAccess.item);
            pManager.AddNumberParameter("angle", "angle", "angle in degrees", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Rotated V", "Rotated V", "Rotated vector V", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables
            Vector3d v = new Vector3d();
            Vector3d n = new Vector3d();
            double angle = new double();

            //Link input
            DA.GetData(0, ref v);
            DA.GetData(1, ref n);
            DA.GetData(2, ref angle);

            //output variables
            Vector3d vrot = new Vector3d();


            VectorRAZ vraz = new VectorRAZ(v.X, v.Y, v.Z);
            VectorRAZ nraz = new VectorRAZ(n.X, n.Y, n.Z);

            VectorRAZ vrotraz = VectorRAZ.RotateVector(nraz, angle, vraz);

            vrot = new Vector3d(vrotraz.X, vrotraz.Y, vrotraz.Z);
            
            //link output
            DA.SetData(0, vrot);
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
            get { return new Guid("f68aa054-3a6a-4bf7-a670-999620653ca3"); }
        }
    }
}
*/
