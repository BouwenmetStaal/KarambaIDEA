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
    public class CreateAndCalculateIDEAfile : GH_Component
    {
        public CreateAndCalculateIDEAfile() : base("Create and Calculate IDEA File", "Create and Calculate IDEA File", "Create and Calculate IDEA file", "KarambaIDEA", "4. IDEA utilities")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("Output folder ", "Output folder", "Save location of IDEA Statica Connection output file. For example: 'C:\\Data'", GH_ParamAccess.item);
            pManager.AddGenericParameter("Template", "Template", "Template", GH_ParamAccess.item);
            //pManager.AddBooleanParameter("RunAllJoints", "RunAllJoints", "If true run all joints, if false run ChooseJoint joint", GH_ParamAccess.item);
            pManager.AddIntegerParameter("ChooseJoint", "ChooseJoint", "Specify the joint that will be calculated in IDEA. Note: starts at zero.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("RunIDEA", "RunIDEA", "Bool for running IDEA Statica Connection", GH_ParamAccess.item);

            // Assign default Workshop Operation.
            Param_GenericObject param0 = (Param_GenericObject)pManager[2];
            param0.PersistentData.Append(new GH_ObjectWrapper(EnumWorkshopOperations.NoOperation));

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Selected Joint", "Selected Joint", "Lines of selected Joint", GH_ParamAccess.list);
            pManager.AddNumberParameter("Analysis", "Analysis", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Plates", "Plates", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bolts", "Bolts", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Welds", "Welds", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Buckling", "Buckling", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Summary", "Summary", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables
            Project project = new Project();
            string outputfolderpath = null;
            EnumWorkshopOperations workshopOperations = EnumWorkshopOperations.NoOperation;
            int createThisJoint = 0;
            bool startIDEA = false;


            //Link input
            DA.GetData(0, ref project);
            DA.GetData(1, ref outputfolderpath);


            DA.GetData(2, ref workshopOperations);

            //DA.GetData(3, ref calculateAllJoints);

            DA.GetData(3, ref createThisJoint);
            DA.GetData(4, ref startIDEA);


            //output variables
            List<Rhino.Geometry.Line> lines = new List<Rhino.Geometry.Line>();
            List<Rhino.Geometry.Line> jointlines = new List<Rhino.Geometry.Line>();
            //output variables
            double analysis = new double();
            double plates = new double();
            double bolts = new double();
            double welds = new double();
            double buckling = new double();
            string summary = string.Empty;

            //Adjust out of bounds index calculateThisJoint
            createThisJoint = createThisJoint % project.joints.Count;

            if (startIDEA == true)
            {
                project.CreateFolder(outputfolderpath);
                Joint joint = project.joints[createThisJoint];
                joint.template = workshopOperations;
                IdeaConnection ideaConnection = new IdeaConnection(joint);

                //Run HiddenCalculation
                joint.JointFilePath = "xx";
                KarambaIDEA.IDEA.HiddenCalculation main = new HiddenCalculation(joint);

                //Retrieve results
                analysis = joint.ResultsSummary.analysis;
                plates = joint.ResultsSummary.plates;
                bolts = joint.ResultsSummary.bolts;
                welds = joint.ResultsSummary.welds;
                buckling = joint.ResultsSummary.buckling;
                summary = joint.ResultsSummary.summary;

            }

            //export lines of joint for visualisation purposes
            foreach (int i in project.joints[createThisJoint].beamIDs)
            {
                Core.Line line = project.elements[i].line;
                Rhino.Geometry.Line rhiline = ImportGrasshopperUtils.CastLineToRhino(line);
                jointlines.Add(rhiline);
            }


            //link output
            DA.SetDataList(0, jointlines);
            DA.SetData(1, analysis);
            DA.SetData(2, plates);
            DA.SetData(3, bolts);
            DA.SetData(4, welds);
            DA.SetData(5, buckling);
            DA.SetData(6, summary);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.KarambaIDEA_logo;

            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("8fbcb8c2-4700-443b-8ab1-3269e2ab0358"); }
        }


    }

}
