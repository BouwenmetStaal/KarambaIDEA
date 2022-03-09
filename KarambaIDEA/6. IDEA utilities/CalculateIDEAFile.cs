
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
            pManager.AddTextParameter("File paths", "File paths", "File paths of .ideaCon files", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("RunIDEA", "RunIDEA", "Bool for running IDEA Statica Connection", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Analysis", "Analysis", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Plates", "Plates", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Bolts", "Bolts", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Welds", "Welds", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Buckling", "Buckling", "", GH_ParamAccess.tree);
            pManager.AddTextParameter("Summary", "Summary", "", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables
            GH_Structure<GH_String> filepaths = new GH_Structure<GH_String>();
            bool startIDEA = false;

            //Link input
            DA.GetDataTree(0, out filepaths);
            DA.GetData(1, ref startIDEA);

            //output variables
            DataTree<double?> analysis = new DataTree<double?>();
            DataTree<double?> plates = new DataTree<double?>();
            DataTree<double?> bolts = new DataTree<double?>();
            DataTree<double?> welds = new DataTree<double?>();
            DataTree<double?> buckling = new DataTree<double?>();
            DataTree<string> summary = new DataTree<string>();

            int index = 0;//TODO assign index based on branch path of filepathstree

            if (startIDEA == true)
            {
                foreach (GH_String filepath in filepaths)
                {
                                       
                    //Run HiddenCalculation
                    Joint joint = new Joint();
                    joint.JointFilePath = filepath.ToString();
                    HiddenCalculationV20.Calculate(joint, false);
                    //KarambaIDEA.IDEA.HiddenCalculation main = new HiddenCalculation(joint);

                    //Retrieve results
                    GH_Path path = new GH_Path(index);
                    analysis.Add(joint.ResultsSummary.analysis, path);
                    plates.Add(joint.ResultsSummary.plates, path);
                    bolts.Add(joint.ResultsSummary.bolts, path);
                    welds.Add(joint.ResultsSummary.welds, path);
                    buckling.Add(joint.ResultsSummary.buckling, path);
                    summary.Add(joint.ResultsSummary.summary, path);

                    index++;
                }
                
            }
            //link output
            DA.SetDataTree(0, analysis);
            DA.SetDataTree(1, plates);
            DA.SetDataTree(2, bolts);
            DA.SetDataTree(3, welds);
            DA.SetDataTree(4, buckling);
            DA.SetDataTree(5, summary);
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
