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
    public class Template_WeldAllMembers : GH_Component
    {
        public Template_WeldAllMembers() : base("Template: Weld all members", "Template: Weld all members", "All members in joint will be connected by weld according to the hierarchy", "KarambaIDEA", "5. IDEA Templates")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("BrandNames", "BrandNames", "BrandNames to apply template to", GH_ParamAccess.list);
            // Assign default BrandName.
            Param_String param0 = (Param_String)pManager[1];
            param0.PersistentData.Append(new GH_String(""));
            
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("Message", "Message", "", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables            
            Project sourceProject = new Project();
            List<GH_String> brandNamesDirty = new List<GH_String>();
            List<string> brandNames = new List<string>();

            //Output variables
            List<string> messages = new List<string>();

            //Link input
            DA.GetData(0, ref sourceProject);
            DA.GetDataList(1, brandNamesDirty);

            //Clone project
            Project project = null;
            if (Project.copyProject == true) { project = sourceProject.Clone(); }
            else { project = sourceProject; }

            //process
            if (brandNamesDirty.Where(x=> x!=null&& !string.IsNullOrWhiteSpace(x.Value)).Count() > 0)
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
                            SetTemplate(joint);
                        }
                    }
                }
            }
            else
            {
                foreach (Joint joint in project.joints)
                {
                    SetTemplate(joint);
                }
            }

            messages = project.MakeTemplateJointMessage();

            //link output
            DA.SetData(0, project);
            DA.SetDataList(1, messages);
        }

        private static void SetTemplate(Joint joint)
        {
            joint.template = new Template();
            joint.template.workshopOperations = Template.WorkshopOperations.WeldAllMembers;
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.TempWeldAllMembers;

            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("4052f7f6-024f-47de-85fe-33f67cb31130"); }
        }


    }
}
