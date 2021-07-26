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
        public CreateAndCalculateIDEAfile() : base("Create and Calculate IDEA File", "Create and Calculate IDEA File", "Create and Calculate IDEA file", "KarambaIDEA", "6. IDEA utilities")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("Output folder ", "Output folder", "Save location of IDEA Statica Connection output file. For example: 'C:\\Data'", GH_ParamAccess.item);
            pManager.AddBooleanParameter("UserFeedback", "User Feedback", "Feedback of calculation", GH_ParamAccess.item);
            pManager.AddBooleanParameter("RunAllJoints", "RunAllJoints", "If true run all joints, if false run ChooseJoint joint", GH_ParamAccess.item);
            pManager.AddIntegerParameter("ChooseJoint", "ChooseJoint", "Specify the joint that will be calculated in IDEA. Note: starts at zero.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("RunIDEA", "RunIDEA", "Bool for running IDEA Statica Connection", GH_ParamAccess.item);

            // Assign default Workshop Operation.
            //Param_GenericObject param0 = (Param_GenericObject)pManager[2];
            //param0.PersistentData.Append(new GH_ObjectWrapper(Template.WorkshopOperations.NoOperation));

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Selected Joint", "Selected Joint", "Lines of selected Joint", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Analysis", "Analysis", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Plates", "Plates", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Bolts", "Bolts", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Welds", "Welds", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Buckling", "Buckling", "", GH_ParamAccess.tree);
            pManager.AddTextParameter("Summary", "Summary", "", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables
            Project project = new Project();
            string outputfolderpath = null;
            bool userFeedback = false;
            bool createAllJoints = false;
            List<int> createAndCalculateThisJoint = new List<int>();
            bool startIDEA = false;


            //Link input
            DA.GetData(0, ref project);
            DA.GetData(1, ref outputfolderpath);
            DA.GetData(2, ref userFeedback);
            DA.GetData(3, ref createAllJoints);
            DA.GetDataList(4, createAndCalculateThisJoint);
            DA.GetData(5, ref startIDEA);


            //output variables
            DataTree<Rhino.Geometry.Line> jointlines = new DataTree<Rhino.Geometry.Line>();

            //output variables
            DataTree<double?> analysis = new DataTree<double?>();
            DataTree<double?> plates = new DataTree<double?>();
            DataTree<double?> bolts = new DataTree<double?>();
            DataTree<double?> welds = new DataTree<double?>();
            DataTree<double?> buckling = new DataTree<double?>();
            DataTree<string> summary = new DataTree<string>();

            //Adjust out of bounds index calculateThisJoint
            List<int> jointIndexes = new List<int>();
            foreach (int i in createAndCalculateThisJoint)
            {
                jointIndexes.Add(i % project.joints.Count);
            }
            jointIndexes = jointIndexes.Distinct().ToList();


            if (startIDEA == true)
            {
                project.CreateFolder(outputfolderpath);
                if (createAllJoints == true)
                {
                    foreach (Joint joint in project.joints)
                    {
                        IdeaConnection ideaConnection = new IdeaConnection(joint, userFeedback);

                        //Run HiddenCalculation
                        joint.JointFilePath = "xx";
                        HiddenCalculationV20.Calculate(joint, userFeedback);

                        //Retrieve results
                        GH_Path path = new GH_Path(joint.id-1);//TODO: check if joint id can be zero in IDEA
                        analysis.Add(joint.ResultsSummary.analysis, path);
                        plates.Add(joint.ResultsSummary.plates, path);
                        bolts.Add(joint.ResultsSummary.bolts, path);
                        welds.Add(joint.ResultsSummary.welds, path);
                        buckling.Add(joint.ResultsSummary.buckling, path);
                        summary.Add(joint.ResultsSummary.summary, path);
                    }
                }
                else
                {
                    foreach (int index in jointIndexes)
                    {
                        Joint joint = project.joints[index];
                        IdeaConnection ideaConnection = new IdeaConnection(joint, userFeedback);

                        //Run HiddenCalculation
                        joint.JointFilePath = "xx";
                        HiddenCalculationV20.Calculate(joint, userFeedback);

                        //Retrieve results
                        GH_Path path = new GH_Path(index);
                        analysis.Add(joint.ResultsSummary.analysis, path);
                        plates.Add(joint.ResultsSummary.plates, path);
                        bolts.Add(joint.ResultsSummary.bolts, path);
                        welds.Add(joint.ResultsSummary.welds, path);
                        buckling.Add(joint.ResultsSummary.buckling, path);
                        summary.Add(joint.ResultsSummary.summary, path);
                    }
                }
            }

            //export lines of joint for visualisation purposes
            foreach (int index in jointIndexes)
            {
                GH_Path path = new GH_Path(index);
                foreach (int i in project.joints[index].beamIDs)
                {
                    Core.Line line = project.elements[i].Line;
                    Rhino.Geometry.Line rhiline = ImportGrasshopperUtils.CastLineToRhino(line);
                    jointlines.Add(rhiline,path);
                }
            }
            


            //link output
            DA.SetDataTree(0, jointlines);
            DA.SetDataTree(1, analysis);
            DA.SetDataTree(2, plates);
            DA.SetDataTree(3, bolts);
            DA.SetDataTree(4, welds);
            DA.SetDataTree(5, buckling);
            DA.SetDataTree(6, summary);
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
            get { return new Guid("8fbcb8c2-4700-443b-8ab1-3269e2ab0358"); }
        }


    }

}
