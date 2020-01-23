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
    public class SaveData : GH_Component
    {
        public SaveData() : base("SaveData", "SaveData", "SaveData", "KarambaIDEA", "3. Project utilities")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Stiffness Calculated", "Stiffness Calculated", "Stiffness Calculated", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Refresh", "Refresh", "Refresh", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Stiffness Implemented", "Stiffness Implemented", "Stiffness implemented", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            List<double> stiffCalc = new List<double>();
            bool refresh = false;

            //Link input
            DA.GetDataList(0, stiffCalc);
            DA.GetData(1, ref refresh);


            //output variables
            List<double> stiffImp = new List<double>();

            if (refresh == true)
            {
                stiffImp = stiffCalc;
            }

            //link output
            DA.SetDataList(0, stiffImp);
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
            get { return new Guid("b5805f01-559b-4993-ac93-7ed81f6f140d"); }
        }
    }
}
