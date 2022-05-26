
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

        public CalculateIDEA() : base("Calculate Connection", "Calculate IDEA Connection", "Calculate IDEA Connection. This is a connection file that has been saved and referenced", "KarambaIDEA", "5. IDEA Connection") {}

        public override GH_Exposure Exposure { get { return GH_Exposure.tertiary; } }

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

                IdeaConnection_2 conCopy = new IdeaConnection_2(ghCon.Value);

                //IdeaServiceModel service = new IdeaServiceModel();

                conCopy.CalculateConnection(mods.Select(x => x.Value).ToList(), UserFeeback);

                DA.SetData(0, new GH_IdeaConnection(conCopy));
            }
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.IDEAlogo_safe; } }
        public override Guid ComponentGuid {  get { return new Guid("8036C064-4B56-4AA5-91E8-D54E72605566"); }  }
    }

    public class RefConnectionByFilePath : GH_Component
    {
        public RefConnectionByFilePath() : base("Reference Connection", "RefCon", "Reference an existing connection which has been created manually by a filepath", "KarambaIDEA", "5. IDEA Connection") { }

        public override GH_Exposure Exposure { get { return GH_Exposure.secondary; } }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "Filepath of existing connection file to import (.ideaCon)", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Name/s of the Connection in the Connection Project File to Reference. If No Name is provided the First connection will be Used. If multiple is provided a seperate connection reference will be created for each item.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Load Info", "L", "XXX Not Yet Implemented XXX. Attempt to Load as much information as possible from the Connection file including Parameters, Project Items, Costs. Note: This requires an avaliable IdeaStatiCa License", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Connection", "C", "Referenced connection which can be put into Calculate Component", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = "";
            List<string> items = new List<string>();
            bool load = false;

            DA.GetData<string>(0, ref path);

            if (!path.EndsWith(".ideaCon"))
            {
                base.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Incorrect filepath extension.");
                return;
            }

            List<GH_IdeaConnection> ghCons = new List<GH_IdeaConnection>();
            DA.GetData<bool>(2, ref load);

            if (DA.GetDataList<string>(1, items))
            {
                foreach (var itemName in items)
                    ghCons.Add(new GH_IdeaConnection(new IdeaConnection_2(path, -1, load, itemName)));
            }
            else
            {
                ghCons.Add(new GH_IdeaConnection(new IdeaConnection_2(path, -1, load)));
            }

            DA.SetDataList(0, ghCons);
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.TemplateFromFilePath; } }
        public override Guid ComponentGuid { get { return new Guid("CAF6BB4D-4DB5-4856-BECE-BF744F9E9ED4"); } }
    }
}
