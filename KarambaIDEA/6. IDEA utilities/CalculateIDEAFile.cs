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
    public class CalculateIDEAFile : GH_Component
    {
        public CalculateIDEAFile() : base("Calculate IDEA File", "Calculate IDEA File", "Calculate IDEA file", "KarambaIDEA", "6. IDEA utilities")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File path", "File path", "File path of .ideaCon file", GH_ParamAccess.item);
            pManager.AddBooleanParameter("RunIDEA", "RunIDEA", "Bool for running IDEA Statica Connection", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Analysis", "Analysis", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Plates", "Plates", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bolts", "Bolts", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Welds", "Welds", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Buckling", "Buckling", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Summary", "Summary", "", GH_ParamAccess.item);
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
            double analysis = new double();
            double plates = new double();
            double bolts = new double();
            double welds = new double();
            double buckling = new double();
            string summary = string.Empty;
                                  
            if (startIDEA == true)
            {
                //Run HiddenCalculation
                Joint joint = new Joint();
                joint.JointFilePath = filepath;
                KarambaIDEA.IDEA.HiddenCalculation main = new HiddenCalculation(joint);

                //Retrieve results
                analysis = joint.ResultsSummary.analysis;
                plates = joint.ResultsSummary.plates;
                bolts = joint.ResultsSummary.bolts;
                welds = joint.ResultsSummary.welds;
                buckling = joint.ResultsSummary.buckling;
                summary = joint.ResultsSummary.summary;
            }
            //link output
            DA.SetData(0, analysis);
            DA.SetData(1, plates);
            DA.SetData(2, bolts);
            DA.SetData(3, welds);
            DA.SetData(4, buckling);
            DA.SetData(5, summary);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.IDEAlogo_safe;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("b563fd17-0555-4194-8308-a785e127d406"); }
        }


    }

}
