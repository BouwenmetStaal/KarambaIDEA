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
    public class Template_BoltedEndPlateConnection : GH_Component
    {
        public Template_BoltedEndPlateConnection() : base("Bolted endplate connection", "Bolted endplate connection", "Bolted endplate connection", "KarambaIDEA", "5. IDEA Templates")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //pManager.AddTextParameter("BrandNames", "BrandNames", "BrandNames to apply template to", GH_ParamAccess.list);
            pManager.AddNumberParameter("Thickness endplate", "Thickness endplate", "", GH_ParamAccess.item);
            // Assign default BrandName.
            Param_String param0 = (Param_String)pManager[0];
            param0.PersistentData.Append(new GH_String(""));
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Template: Bolted endplate connection", "Template: Bolted endplate connection", "Bolted endplate connection", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables            
            double tplate = new double();
            List<GH_String> brandNamesDirty = new List<GH_String>();
            List<string> brandNames = new List<string>();

            //Link input
            //DA.GetDataList(0, brandNamesDirty);
            DA.GetData(0, ref tplate);

            //process
            if (brandNamesDirty.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Value)).Count() > 0)
            {
                List<string> brandNamesDirtyString = brandNamesDirty.Select(x => x.Value.ToString()).ToList();
                brandNames = ImportGrasshopperUtils.DeleteEnterCommandsInGHStrings(brandNamesDirtyString);
            }

            EnumWorkshopOperations operation = EnumWorkshopOperations.BoltedEndPlateConnection;
            //IDEA.Templates.BoltedEndplateConnection(var, var, 10);

            //link output
            DA.SetData(0, operation);
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
            get { return new Guid("c87e4243-ed21-492f-9d25-a599454de06f"); }
        }


    }
}
