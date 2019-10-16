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



namespace KarambaIDEA
{
    public class WO_WeldAllMembers : GH_Component
    {
        public WO_WeldAllMembers() : base("Weld all members", "Weld all members", "Weld all members", "KarambaIDEA", "4. Workshop operations")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("BrandNames", "BrandNames", "BrandNames to apply template to", GH_ParamAccess.list);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Template", "Template", "This template is a miscellaneous of workshop operations", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables            
            List<Action> operations = new List<Action>();
            List<string> brandNamesDirty = new List<string>();
            List<string> brandNames = new List<string>();

            //Link input
            DA.GetData(0, ref brandNames);

            //Clean cross-section list from nextline ("\r\n") command produced by Karamba
            brandNames = ImportGrasshopperUtils.DeleteEnterCommandsInGHStrings(brandNamesDirty);

            //TODO: include brandName reference

            //operations.Add(WorkshopOperations.WeldAllMembers());

            //link output
            DA.SetDataList(0, operations);
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
            get { return new Guid("4052f7f6-024f-47de-85fe-33f67cb31130"); }
        }


    }
}
