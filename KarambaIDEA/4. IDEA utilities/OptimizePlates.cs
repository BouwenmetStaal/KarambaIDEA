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
    public class OptimizePlates : GH_Component
    {
        public OptimizePlates() : base("Optimize plates", "Optimize plates", "Optimize plates of IDEA file", "KarambaIDEA", "4. IDEA utilities")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File path", "File path", "File path of .ideaCon file", GH_ParamAccess.item);
            pManager.AddBooleanParameter("RunIDEA", "RunIDEA", "Bool for running IDEA Statica Connection", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Message", "Message", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables
            string filepath = null;
            bool startIDEA = false;

            //Link input
            DA.GetData(0, ref filepath);
            DA.GetData(1, ref startIDEA);

            //output variables
            string message = string.Empty;

            //process
            if (startIDEA == true)
            {
                //Run HiddenCalculation
                Joint joint = new Joint();
                joint.JointFilePath = filepath;
                KarambaIDEA.IDEA.HiddenCalculation main = new HiddenCalculation(joint);
                

                //TODO: add plate optmization process
            }

            //link output
            DA.SetData(0, message);
           
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
            get { return new Guid("2ac4d20f-82e8-4a75-82c1-fee709a83455"); }
        }


    }

}
