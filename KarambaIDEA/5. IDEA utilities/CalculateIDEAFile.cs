
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
using KarambaIDEA.IDEA.Parameters;
using KarambaIDEA.Grasshopper;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

namespace KarambaIDEA
{
    public class CalculateIDEA : GH_Component
    {
        private bool UserFeeback = false; 

        public CalculateIDEA() : base("Calculate Connection", "Calculate IDEA Connection", "Calculate IDEA Connection. This is a connection file that has been saved and referenced", "KarambaIDEA", "5. IDEA utilities")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Connection", "C", "Connections which have been created on disk and referenced", GH_ParamAccess.item);
            pManager.AddGenericParameter("Modifications", "M", "List of Modifications to apply to the conneciton file prior to Calculation", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Run IDEA", "R", "Run the Connection", GH_ParamAccess.item);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Connection", "C", "Calculated Connections with Modification Applied", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool startIDEA = false;
            DA.GetData(2, ref startIDEA);

            if (startIDEA)
            {
                GH_IdeaConnection ghCon = null;
                DA.GetData<GH_IdeaConnection>(0, ref ghCon);

                List<GH_IdeaModification> mods = new List<GH_IdeaModification>();

                DA.GetDataList<GH_IdeaModification>(1, mods);

                IdeaConnectionResult results = HiddenCalculationV20.Calculate(ghCon.Value.Filepath, mods.Select(x => x.Value).ToList(), UserFeeback);

                DA.SetData(0, new GH_IdeaConnection(new IdeaConnectionContainer(ghCon.Value, results)));
            }
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.IDEAlogo_safe; } }
        public override Guid ComponentGuid {  get { return new Guid("8036C064-4B56-4AA5-91E8-D54E72605566"); }  }
    }

}
