using System;
using System.Collections.Generic;
using System.Drawing;
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
    public class JointModify : GH_Component
    {
        public JointModify() : base("Modify Joint", "MJ", "Modify a KarambaIDEA Project Joint.", "KarambaIDEA", "1. Project") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Joint", "J", "KarambaIDEA Joint to Modify", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Optional Name to apply to the Joint", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Bearing Id","B", "Update the Bearing Member of a Joint. XXX Future Feature. Not yet Implemented", GH_ParamAccess.item);
            pManager.AddGenericParameter("Connection Data", "D", "Add bespoke IOM Connection Data to the Joint. XXX Future Feature. Not yet Implemented", GH_ParamAccess.list);
            pManager.AddGenericParameter("Template Assigns","A", "Templates to Assign to the Joint. You are allowed to assign multiple. Including Idea Full, Idea Partial and Visually Defined IOM definitions", GH_ParamAccess.list);
            pManager[1].Optional = pManager[2].Optional = pManager[3].Optional = pManager[4].Optional = true;
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.secondary; } }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Joint", "J", "Updated Joint", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_KarambaIdeaJoint ghJoint = null;

            DA.GetData<GH_KarambaIdeaJoint>(0, ref ghJoint);

            KarambaIdeaJoint jointCopy = new KarambaIdeaJoint(ghJoint.Value);

            string name = "";

            if (DA.GetData<string>(1, ref name))
                jointCopy.name = name;

            List<GH_JointTemplateAssign> ghTempList = new List<GH_JointTemplateAssign>();
            if (DA.GetDataList<GH_JointTemplateAssign>(4, ghTempList))
                foreach (var ghTemp in ghTempList)
                    jointCopy.AddTemplateAssign(ghTemp.Value);

            DA.SetData(0, new GH_KarambaIdeaJoint(jointCopy));
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.ModifyJoint; } }

        public override Guid ComponentGuid { get { return new Guid("5E8D5189-ECC4-4232-A613-FA209F3034E6"); } }
    }

    public class JointDeconstruct : GH_Component
    {
        public JointDeconstruct() : base("Deconstruct Joint", "DJ", "Deconstruct a KarambaIDEA Project Joint.", "KarambaIDEA", "1. Project") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Joint", "J", "KarambaIDEA Joint to Modify", GH_ParamAccess.item);
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.secondary; } }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Id", "Id", "Base Project Joint Id", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Joint Name", GH_ParamAccess.item);
            pManager.AddGenericParameter("Member Ids", "E", "Ids of Project Elements in KarambaIDEA Project", GH_ParamAccess.list);
            pManager.AddGenericParameter("Bearing Id", "B", "Id of the Bearing Member in KarambaIDEA Project", GH_ParamAccess.item);
            pManager.AddGenericParameter("Connection Data", "D", "Connection Data Associated with the Joint. XXX Future Feature. Not currently Implemented", GH_ParamAccess.item);
            pManager.AddTextParameter("Brand", "B", "Joint Brand", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_KarambaIdeaJoint ghJoint = null;

            DA.GetData<GH_KarambaIdeaJoint>(0, ref ghJoint);

            KarambaIdeaJoint joint = ghJoint.Value;

            DA.SetData(0, joint.id);
            DA.SetData(1, joint.Name);
            DA.SetDataList(2, joint.attachedMembers.Select(x => x.element.id).ToList());
            DA.SetData(3, joint.attachedMembers.OfType<BearingMember>().Select(x => x.element.id).First());
            //DA.SetData(4, joint.)
            DA.SetData(5, joint.brandName);
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.DeconstructJoint; } }

        public override Guid ComponentGuid { get { return new Guid("84039448-BEBE-4A48-B887-91C90F4C8A4C"); } }
    }


#if (DEBUG)
    public class ItemToXml : GH_Component
    {
        public ItemToXml() : base("To IOM XML", "IOM", "Returns the IOM XML definition of avaliable objects", "KarambaIDEA", "1. Project") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Item", "I", "Item which Can be represented as IOM Xml string", GH_ParamAccess.item);
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.tertiary | GH_Exposure.obscure; } }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("XML", "XML", "Output XML string", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IGH_XmlOutputObject ghIdeaItem = null;

            DA.GetData<IGH_XmlOutputObject>(0, ref ghIdeaItem);

            string xmlOutput = "";

            xmlOutput = ghIdeaItem.getXmlString();
            
            DA.SetData(0, xmlOutput);
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.GeneralIcon; } }

        public override Guid ComponentGuid { get { return new Guid("A50A8974-509F-4281-96E2-508FC46F502C"); } }
    }

#endif

}
