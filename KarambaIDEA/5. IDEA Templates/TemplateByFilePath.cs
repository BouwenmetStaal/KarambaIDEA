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
    public class TemplateByFilePath : GH_Component
    {
        public TemplateByFilePath() : base("Template by Filepath", "Template by Filepath", "Template by Filepath", "KarambaIDEA", "5. IDEA Templates") { }

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
                return;
            }

            IdeaTemplate template = new IdeaTemplate(path);

            DA.SetData(0, new GH_IdeaTemplate(template));
        }

        protected override System.Drawing.Bitmap Icon { get {  return Properties.Resources.TemplateFromFilePath; } }
        public override Guid ComponentGuid { get { return new Guid("CC4A4623-5D4E-48C0-8E48-9D13BB44BD78"); } }

    }

    public class DeconstructTemplate : GH_Component
    {
        public DeconstructTemplate() : base("Deconstruct Template", "DecTemplate", "Deconstruct an Imported Template", "KarambaIDEA", "5. IDEA Templates") { }

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

            DA.SetDataList(0, conParams);
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.TemplateFromFilePath; } }
        public override Guid ComponentGuid { get { return new Guid("947B2E43-DFEC-43FF-8D50-B37E6194F452"); } }
    }


    public class SetParameter : GH_Component
    {
        public SetParameter() : base("Set Param Value", "SetParam", "Set the Value of a Parameter", "KarambaIDEA", "5. IDEA Templates") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Parameter", "P", "Parameter", GH_ParamAccess.item);
            pManager.AddGenericParameter("Value", "V", "Value to Set to Parameter", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Parameter", "P", "Updated Parmater", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_IdeaParameter ghParam = null;

            if (DA.GetData<GH_IdeaParameter>(0, ref ghParam))
            {
                IGH_Goo value = null;
                DA.GetData<IGH_Goo>(1, ref value);

                string textvalue = value.ToString();

                IIdeaParameter param = ghParam.Value;

                if (param is IdeaParameterInt intparam)
                {
                    int id;
                    GH_Convert.ToInt32(value, out id, GH_Conversion.Both);
                    IdeaParameterInt clone = new IdeaParameterInt(intparam.Clone() as parameter);
                    clone.SetValue(id);
                    DA.SetData(0, new GH_IdeaParameter(clone));
                    return;
                }
                else if (param is IdeaParameterFloat floatparam)
                {
                    double number;
                    GH_Convert.ToDouble(value, out number, GH_Conversion.Both);
                    IdeaParameterFloat clone = new IdeaParameterFloat(floatparam.Clone() as parameter);
                    clone.SetValue(number);
                    DA.SetData(0, new GH_IdeaParameter(clone));
                    return;
                }
                else
                {
                    IdeaParameterString clone = new IdeaParameterString(param.Clone() as parameter);
                    clone.SetValue(textvalue);
                    DA.SetData(0, new GH_IdeaParameter(clone));
                    return;
                }
            }
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.TemplateFromFilePath; } }
        public override Guid ComponentGuid { get { return new Guid("65D37365-755F-4998-99CD-FDA73C8E2788"); } }
    }


    public class AssignTemplateByFilePathSS_OBSOLETE : GH_Component
    {
        public AssignTemplateByFilePathSS_OBSOLETE() : base("Assign Template by Filepath", "Assign a Template by Filepath", "Template by Filepath", "KarambaIDEA", "5. IDEA Templates")
        {

        }

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
            Rhino.Geometry.Sphere sphere = new Sphere(p, radius/1000);
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
