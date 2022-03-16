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

            IdeaTemplate template = new IdeaTemplate();

            //DA.


        }

        protected override System.Drawing.Bitmap Icon { get {  return Properties.Resources.TemplateFromFilePath; } }
        public override Guid ComponentGuid { get { return new Guid("1015698f-df9a-49c1-82da-187dcb672f18"); } }

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
