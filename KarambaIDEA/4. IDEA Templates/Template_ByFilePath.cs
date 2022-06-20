// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
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
    public class IDEATemplateByFilePath : GH_Component
    {
        public IDEATemplateByFilePath() : base("Import IDEA Template", "ImptIDEATemp", "Import an IDEA Template by Filepath (.ideatemp)", "KarambaIDEA", "5. IDEA Templates") { }

        public override GH_Exposure Exposure { get { return GH_Exposure.primary; } }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "Filepath of ideaCon template file to import (.contemp)", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Template", "T", "Template which can be assigned to a joint.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = "";

            DA.GetData<string>(0, ref path);

            if (!path.EndsWith(".contemp"))
            {
                base.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Incorrect filepath extension.");
                DA.SetData(0, null);
            }
            if (File.Exists(path))
            {
                IdeaTemplate template = new IdeaTemplate(path);
                DA.SetData(0, new GH_IdeaTemplate(template));
            }
            else
            {
                base.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Template filepath does not exist.");
                DA.SetData(0, null);
            }
        }

        protected override System.Drawing.Bitmap Icon { get {  return Properties.Resources.TemplateByFilepath; } }
        public override Guid ComponentGuid { get { return new Guid("CC4A4623-5D4E-48C0-8E48-9D13BB44BD78"); } }

    }

    public class IDEATemplateDeconstruct : GH_Component
    {
        public IDEATemplateDeconstruct() : base("Deconstruct IDEA Template", "DecIDEATemp", "Deconstruct an Imported IDEA Template into its parts", "KarambaIDEA", "5. IDEA Templates") { }

        public override GH_Exposure Exposure { get { return GH_Exposure.primary; } }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Template", "T", "Imported Template to Deconstruct", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Parameters", "P", "List of Parameters assigned in Template", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_IdeaTemplate ghTemp = null;

            DA.GetData<GH_IdeaTemplate>(0, ref ghTemp);

            List<GH_IdeaParameter> conParams = new List<GH_IdeaParameter>();
            
            conParams = ghTemp.Value.GetParameters().ConvertAll(x=> new GH_IdeaParameter(x));
            if (conParams.Count == 0)
            {
                //throw new ArgumentNullException("No parameters found in template");
                base.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No parameters found in template");
            }
            DA.SetDataList(0, conParams);
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.DeconstructTemplate; } }
        public override Guid ComponentGuid { get { return new Guid("947B2E43-DFEC-43FF-8D50-B37E6194F452"); } }
    }

    public class TemplateAssignIdeaFull : GH_Component
    {
        public TemplateAssignIdeaFull() : base("IDEA Template: Full", "IDEATempAssign", "Assign a Template by referenced IdeaCon (.ideatemp) Template", "KarambaIDEA", "5. IDEA Templates") { }

        public override GH_Exposure Exposure { get { return GH_Exposure.secondary; } }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("IDEA Template", "T", "IDEA Template which has been referenced from a Idea Template file", GH_ParamAccess.item);
            pManager.AddGenericParameter("Parameter Mod", "P", "A list of IDEA Parameter modifications to be applied to the Connection after the template has been assigned", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Template Assign", "A", "Template Assign which will assign the Template to the applied Joint", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_IdeaTemplate IdeaTemplate = null;

            DA.GetData<GH_IdeaTemplate>(0, ref IdeaTemplate);

            GH_IdeaModification ghMod = null;

            IdeaModifyConnectionParameters paramsMod = new IdeaModifyConnectionParameters(new List<IIdeaParameter>());

            if (DA.GetData<GH_IdeaModification>(1, ref ghMod))
                if (ghMod.Value is IdeaModifyConnectionParameters pmod)
                    paramsMod = pmod;
                else
                    base.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Only parameter modifications can be provided");

            IdeaTemplateAssignFull template = new IdeaTemplateAssignFull(IdeaTemplate.Value, paramsMod);

            DA.SetData(0, new GH_JointTemplateAssign(template));
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.TemplateAssign; } }
        public override Guid ComponentGuid { get { return new Guid("0FA208EE-B4D9-43F1-8748-61F8723143E0"); } }

    }

    //TO FINISH
    public class TemplateAssignIdeaPartial : GH_Component
    {
        public TemplateAssignIdeaPartial() : base("IDEA Template: Partial", "IdeaPartTempAssign", "Assign a Partial Template by referenced IdeaCon (.ideatemp) Template and Member Indexs", "KarambaIDEA", "5. IDEA Templates") { }

        public override GH_Exposure Exposure { get { return GH_Exposure.secondary; } }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("IDEA Template", "T", "IDEA Template which has been referenced from a Idea Template file", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Support Member Id", "S", "The Id of the supporting member in the Template", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Connecting Member Ids", "C", "List of Id's of connection members int the Template", GH_ParamAccess.list);
            pManager.AddGenericParameter("Parameter Mod", "P", "A list of IDEA Parameter modifications to be applied to the Connection after the template has been assigned", GH_ParamAccess.item);
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Template Assign", "A", "Template Assign which will assign the Template to the applied Joint", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_IdeaTemplate IdeaTemplate = null;

            DA.GetData<GH_IdeaTemplate>(0, ref IdeaTemplate);

            GH_IdeaModification ghMod = null;

            IdeaModifyConnectionParameters paramsMod = new IdeaModifyConnectionParameters(new List<IIdeaParameter>());

            if (DA.GetData<GH_IdeaModification>(1, ref ghMod))
                if (ghMod.Value is IdeaModifyConnectionParameters pmod)
                    paramsMod = pmod;
                else
                    base.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Only parameter modifications can be provided");

            IdeaTemplateAssignFull template = new IdeaTemplateAssignFull(IdeaTemplate.Value, paramsMod);

            DA.SetData(0, new GH_JointTemplateAssign(template));
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.TemplateAssign; } }
        public override Guid ComponentGuid { get { return new Guid("E11068BB-B4C3-4FCB-82C7-154633749C94"); } }

    }

    public class AssignTemplateByFilePathSS_OBSOLETE : GH_Component
    {
        public AssignTemplateByFilePathSS_OBSOLETE() : base("Assign Template", "Assign a Template by Filepath", "Template by Filepath", "KarambaIDEA", "5. IDEA Templates")
        {

        }

        public override GH_Exposure Exposure { get { return GH_Exposure.hidden; } }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("BrandNames", "BrandNames", "BrandNames to apply template to", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Template file path", "Template filepath", "", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("Message", "Message", "", GH_ParamAccess.list);
            pManager.AddBrepParameter("Brep", "Brep", "", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables      
            Project sourceProject = new Project();
            string ideaTemplateLocation = null;
            List<GH_String> brandNamesDirty = new List<GH_String>();
            List<string> brandNames = new List<string>();

            //Output variables
            List<string> messages = new List<string>();
            List<Brep> breps = new List<Brep>();

            //Link input
            DA.GetData(0, ref sourceProject);
            DA.GetDataList(1, brandNamesDirty);
            DA.GetData(2, ref ideaTemplateLocation);

            //Check if path exists
            if (!(File.Exists(ideaTemplateLocation)))
            {
                throw new ArgumentNullException("Template filepath incorrect");
            }

            //Clone project
            Project project = null;
            if (Project.copyProject == true) { project = sourceProject.Clone(); }
            else { project = sourceProject; }

            //process
            if (brandNamesDirty.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Value)).Count() > 0)
            {
                List<string> brandNamesDirtyString = brandNamesDirty.Select(x => x.Value.ToString()).ToList();
                brandNames = ImportGrasshopperUtils.DeleteEnterCommandsInGHStrings(brandNamesDirtyString);
            }

            if (brandNames.Count != 0)
            {
                foreach (string brandName in brandNames)
                {
                    foreach (Joint joint in project.joints)
                    {
                        if (brandName == joint.brandName)
                        {
                            SetTemplate(ideaTemplateLocation, joint, breps);
                        }
                    }
                }
            }
            /*
            else
            {
                foreach (Joint joint in project.joints)
                {
                    SetTemplate(ideaTemplateLocation, joint, breps);
                }
            }
            */
            messages = project.MakeTemplateJointMessage(ideaTemplateLocation);

            //link output
            DA.SetData(0, project);
            DA.SetDataList(1, messages);
            DA.SetDataList(2, breps);
        }

        private static void SetTemplate(string ideaTemplateLocation, Joint joint, List<Brep> breps)
        {
            joint.ideaTemplateLocation = ideaTemplateLocation;
            joint.template = new Template();
            joint.template.workshopOperations = Template.WorkshopOperations.TemplateByFile;
            //BREP add sphere
            Point3d p = ImportGrasshopperUtils.CastPointToRhino(joint.centralNodeOfJoint);
            double radius = 200; //radius in mm
            Rhino.Geometry.Sphere sphere = new Sphere(p, radius / 1000);
            breps.Add(sphere.ToBrep());
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.TemplateFromFilePath;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("f48591d0-bdb2-45da-b347-2153af24f465"); }
        }


    }

}
