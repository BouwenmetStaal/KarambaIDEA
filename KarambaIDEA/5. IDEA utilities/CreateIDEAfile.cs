﻿// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using KarambaIDEA.Core;
using KarambaIDEA.IDEA;
using Grasshopper.Kernel.Types;

namespace KarambaIDEA.Grasshopper
{

    public class CreateIDEAModelBIM : GH_Component
    {
        public CreateIDEAModelBIM() : base("Export to ModelBIM", "ModelBIM", "Export a Project to a ModelBIM file XML file which can be imported directly in IDEA Checkbot Application", "KarambaIDEA", "1. Project")
        {

        }

        public override GH_Exposure Exposure { get { return GH_Exposure.primary; } }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("Output folder ", "Output folder", "Save location of ModelBIM XML output file. For example: 'C:\\Data'", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run IDEA", "Run IDEA", "Bool for Enabling the Creation of the ModelBIM file.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Filepath", "P", "Filepath of ModelBIM file", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            GH_KarambaIdeaProject ghProject = null;

            //Project project = new Project();
            string outputfolderpath = "";
            bool startIDEA = false;

            //Link input
            //Update the project to a goo input.
            if (DA.GetData(0, ref ghProject))
            {
                if (ghProject.Value == null)
                {
                    base.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Project is null");
                    return;
                }
                else
                {
                    Project project = ghProject.Value;

                    DA.GetData(1, ref outputfolderpath);

                    DA.GetData(2, ref startIDEA);


                    string path = "";

                    //Test input folderpath
                    //TODO if no directory provided wrtie to the GH file directory.
                    if (Directory.Exists(outputfolderpath))
                    {
                        string name = "modelBIMtest";
                        path = Path.Combine(outputfolderpath, string.Format("{0}.xml",name));
                    }
                    else
                        base.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Directory does not exist");

                    if (startIDEA)
                    {
                        try
                        {
                            project.ExportToMobelBIM(path);
                            DA.SetData(0, path);
                        }
                        catch (Exception e)
                        {
                            base.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                        }
                    }
                }
            }
        }
        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.ExportIOM; } }
        public override Guid ComponentGuid { get { return new Guid("4DEC0CE9-5B9B-4833-BBC4-0F7CE0C7B6A4"); } }
    }


    public class CreateIDEAfile : GH_Component
    {
        private bool _userFeedback = false;

        public CreateIDEAfile() : base("Create IDEA Connection", "CreateIDEACon", "Create IDEA Connection file from the KarambaIDEA Project", "KarambaIDEA", "6. IDEA Connection") { }

        public override GH_Exposure Exposure { get { return GH_Exposure.primary; } }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Joint", "J", "Specify the joint that will be calculated in IDEA.", GH_ParamAccess.item);
            pManager.AddTextParameter("Output folder ", "F", "Save location of IDEA Statica Connection output files. For example: 'C:\\Data'", GH_ParamAccess.item, "C:\\Data");
            pManager.AddBooleanParameter("Run IDEA", "Run", "Bool for running IDEA Statica Connection", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Connection", "C", "List of Created IDEA StatiCa Conneciton files from project Joints", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool startIDEA = false;

            GH_KarambaIdeaJoint ghJoint = null;
            DA.GetData<GH_KarambaIdeaJoint>(0, ref ghJoint);
            KarambaIdeaJoint joint = ghJoint.Value;

            string outputfolderpath = null;
            DA.GetData(1, ref outputfolderpath);

            DA.GetData<bool>(2, ref startIDEA);

            if (startIDEA == true)
            {
                LoadingForm form = new LoadingForm("IDEA StatiCa: creating Joint");
                form.Show();

                IdeaConnection_2 ideaConnection = new IdeaConnection_2(outputfolderpath, joint, _userFeedback);

                DA.SetData(0, new KarambaIDEA.Grasshopper.GH_IdeaConnection(ideaConnection));

                form.Close();
            }
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.CreateConnection; } }
        public override Guid ComponentGuid { get { return new Guid("D2CA03EA-424C-4278-81E8-605F6E4D10C3"); } }
    }


    public class CreateIDEAfileSS_OBSOLETE : GH_Component
    {
        public CreateIDEAfileSS_OBSOLETE() : base("Create IDEA File", "Create IDEA File", "Create IDEA file", "KarambaIDEA", "6. IDEA Connection")
        {

        }

        public override GH_Exposure Exposure { get { return GH_Exposure.hidden; } }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("Output folder ", "Output folder", "Save location of IDEA Statica Connection output file. For example: 'C:\\Data'", GH_ParamAccess.item);
            pManager.AddBooleanParameter("UserFeedback", "User Feedback", "Feedback of calculation", GH_ParamAccess.item);
            pManager.AddBooleanParameter("CreateAllJoints", "CreateAllJoints", "If true create all joints, if false create only the selected joint", GH_ParamAccess.item);
            pManager.AddIntegerParameter("ChooseJoint", "ChooseJoint", "Specify the joint that will be calculated in IDEA. Note: starts at zero.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("RunIDEA", "RunIDEA", "Bool for running IDEA Statica Connection", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Selected Joint", "Selected Joint", "Lines of selected Joint", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            Project project = new Project();
            string outputfolderpath = null;
            bool userFeedback = false;
            bool createAllJoints = false;
            List<int> createThisJoint = new List<int>();
            bool startIDEA = false;

            //Link input
            DA.GetData(0, ref project);
            DA.GetData(1, ref outputfolderpath);
            DA.GetData(2, ref userFeedback);
            DA.GetData(3, ref createAllJoints);
            DA.GetDataList(4, createThisJoint);
            DA.GetData(5, ref startIDEA);

            //output variables
            DataTree<Rhino.Geometry.Line> jointlines = new DataTree<Rhino.Geometry.Line>();

            //Adjust out of bounds index calculateThisJoint
            List<int> jointIndexes = new List<int>();
            foreach (int i in createThisJoint)
            {
                jointIndexes.Add(i % project.joints.Count);
            }
            jointIndexes = jointIndexes.Distinct().ToList();

            //this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Warning Message");
            //this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error Message");

            if (startIDEA == true)
            {
                LoadingForm form = new LoadingForm("Running IDEA StatiCa");
                //form.Location = form.Parent.Location;
                form.Show();

                project.CreateFolder(outputfolderpath);
                if (createAllJoints == true)
                {
                    foreach(Joint joint in project.joints)
                    {
                        IdeaConnection ideaConnection = new IdeaConnection(joint, userFeedback);
                    }
                }
                else
                {
                    foreach (int i in jointIndexes)
                    {
                        Joint joint = project.joints[i];
                        IdeaConnection ideaConnection = new IdeaConnection(joint, userFeedback);
                    }
                }
                form.Close();
            }

            //export lines of joint for visualisation purposes
            foreach (int index in jointIndexes)
            {
                GH_Path path = new GH_Path(index);
                foreach (int i in project.joints[index].beamIDs)
                {
                    Core.Line line = project.elements[i].Line;
                    Rhino.Geometry.Line rhiline = ImportGrasshopperUtils.CastLineToRhino(line);
                    jointlines.Add(rhiline, path);
                }
            }

            //link output
            DA.SetDataTree(0, jointlines);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.IDEAlogo;

            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("52308472-3ab8-4f23-89d0-d3746ed013f6"); }
        }
    }
}