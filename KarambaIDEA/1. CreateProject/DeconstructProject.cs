using System;
using System.Collections.Generic;
using Rhino.Geometry;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Reflection;
using KarambaIDEA.Core;
using KarambaIDEA.IDEA;
using Grasshopper.Kernel.Parameters;

namespace KarambaIDEA.Grasshopper
{
    public class ProjectDeconstruct : GH_Component
    {
        public ProjectDeconstruct() : base("Deconstruct Project", "DP", "Deconstruct the KarambaIDEA Project. Currently only Joints and Brand Names are Provided. Contact us to Expand.", "KarambaIDEA", "1. Project") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {         
            pManager.AddGenericParameter("Project", "P", "KarambaIDEA Project to Deconstruct", GH_ParamAccess.item);
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.primary | GH_Exposure.obscure; } }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Joints", "J", "List of Joints that are created as apart of Project Creation", GH_ParamAccess.list);
            pManager.AddTextParameter("Brand Names", "B", "Brand Names avaliable in the Project", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_KarambaIdeaProject project = null;

            DA.GetData<GH_KarambaIdeaProject>(0, ref project);

            List<KarambaIdeaJoint> jointlist = new List<KarambaIdeaJoint>();
            
            foreach (Joint joint in project.Value.joints)
            {
                jointlist.Add(new KarambaIdeaJoint(joint));
            }

            DA.SetDataList(0, jointlist.Select(x => new GH_KarambaIdeaJoint(x)));
            DA.SetDataList(1, project.Value.GetBrandNames());
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.DeconstructProject; } }

        public override Guid ComponentGuid { get { return new Guid("FBF26E4F-3E5D-48FA-BD34-69099871CD98"); } }
    }
}
