using System;
using System.Linq;
using System.Collections.Generic;

using Rhino.Geometry;
using KarambaIDEA.IDEA.Parameters;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using KarambaIDEA.Core;
using KarambaIDEA.IDEA;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using System.IO;

namespace KarambaIDEA.Grasshopper
{
    public class ConnectionModifyParameters : GH_Component
    {
        public ConnectionModifyParameters() : base("Modify Con Parameters", "ModParams", "Provide a list of Parameters to Modify in the Connection Model", "KarambaIDEA", "6. IDEA Connection") { }

        public override GH_Exposure Exposure { get { return GH_Exposure.quarternary | GH_Exposure.obscure; } }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Parameters", "P", "Parameter", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Modification", "M", "Connection Modification", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_IdeaParameter> ghParams = new List<GH_IdeaParameter>();

            if (DA.GetDataList<GH_IdeaParameter>(0, ghParams))
            {
                IdeaModifyConnectionParameters conModification = new IdeaModifyConnectionParameters(ghParams.Select(x => x.Value).ToList());

                DA.SetData(0, new GH_IdeaModification(conModification));
            }
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.ModifyConParameters; } }
        public override Guid ComponentGuid { get { return new Guid("09821E93-72F6-4E04-825D-986858ABD996"); } }
    }
}
