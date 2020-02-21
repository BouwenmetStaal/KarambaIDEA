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
using KarambaIDEA.IDEA;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

namespace KarambaIDEA
{
    public class CheckLocalCoordinateSystem : GH_Component
    {
        public CheckLocalCoordinateSystem() : base("Local Coordinate System", "LCS", "Local Coordinate System of element", "KarambaIDEA", "3. Project utilities")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Line of elements", "Lines", "", GH_ParamAccess.list);
            pManager.AddVectorParameter("Local X vectors", "X vecs", "", GH_ParamAccess.list);
            pManager.AddVectorParameter("Local Y vectors", "Y vecs", "", GH_ParamAccess.list);
            pManager.AddVectorParameter("Local Z vectors", "Z vecs", "", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables
            Project project = new Project();

            //Link input
            DA.GetData(0, ref project);

            //output variables
            List<Vector3d> locXvecs = new List<Vector3d>();
            List<Vector3d> locYvecs = new List<Vector3d>();
            List<Vector3d> locZvecs = new List<Vector3d>();
            List<Rhino.Geometry.Line> lines = new List<Rhino.Geometry.Line>();
           
            foreach (Element ele in project.elements)
            {
                Core.Line line = ele.Line;
                Rhino.Geometry.Line rhiline = ImportGrasshopperUtils.CastLineToRhino(line);
                lines.Add(rhiline);

                LocalCoordinateSystem lcs = ele.localCoordinateSystem;
                locXvecs.Add(new Vector3d(lcs.X.X, lcs.X.Y, lcs.X.Z));
                locYvecs.Add(new Vector3d(lcs.Y.X, lcs.Y.Y, lcs.Y.Z));
                locZvecs.Add(new Vector3d(lcs.Z.X, lcs.Z.Y, lcs.Z.Z));
            }

            //link output
            DA.SetDataList(0, lines);
            DA.SetDataList(1, locXvecs);
            DA.SetDataList(2, locYvecs); 
            DA.SetDataList(3, locZvecs);
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
            get { return new Guid("b30aa8fd-8240-40b0-8f7a-c33e19572fab"); }
        }


    }
}

