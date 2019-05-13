// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal, ABT bv. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Collections.Generic;

using Rhino.Geometry;
using System.Linq;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;


using KarambaIDEA.Core;


namespace KarambaIDEA
{


    public class JointExporter : GH_Component
    {
        public JointExporter() : base("Joint Exporter", "JE", "Exporting selected joint to IDEA Statica Connection", "KarmabaIDEA", "KarambaIDEA")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //Single input variables
            //pManager.AddTextParameter("Projectname", "PN", "Name of project or truss variant", GH_ParamAccess.item);
            //pManager.AddIntegerParameter("MinThroat", "minA", "Minimal throat thickness [mm]", GH_ParamAccess.item);
            //pManager.AddNumberParameter("MaxEccentricity", "e_max", "Maximum eccentricity present in project [m]", GH_ParamAccess.item);
            //pManager.AddIntegerParameter("AnalysisMethod", "AnalyzeMeth", "Type of analysis", GH_ParamAccess.item);
            pManager.AddTextParameter("Template File", "Template File Location", "File location of template to be used. For example: 'C:\\Data\\template.contemp'", GH_ParamAccess.item);
            pManager.AddTextParameter("Output Filepath", "Output Filepath", "Save location of IDEA Statica Connection output file. For example: 'C:\\Data'", GH_ParamAccess.item);

            //Input needed for creating Joints                 
            pManager.AddTextParameter("Hierarchy", "Hierarchy", "List of hierarchy on with joints are made", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "Points", "Points of connections", GH_ParamAccess.list);

            //Input ElementsRAZ
            pManager.AddLineParameter("Lines", "Lines", "Lines of geometry", GH_ParamAccess.list);
            pManager.AddTextParameter("Groupnames", "Groupnames", "Groupname of element", GH_ParamAccess.list);
            pManager.AddTextParameter("Material", "Material", "Steel grade of every element", GH_ParamAccess.list);

            //Input for creating Cross-section Objects
            pManager.AddTextParameter("Cross-section", "CroSecName", "Name of cross-section", GH_ParamAccess.list);
            pManager.AddTextParameter("Shape", "Shape", "Shape of of member", GH_ParamAccess.list);
            pManager.AddNumberParameter("Height", "height", "Heigth of crosssection [mm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Width", "width", "Width of crosssection [mm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("ThicknessFlange", "t_f", "thickness of flange crosssection [mm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("ThicknessWeb", "t_w", "thickness of web crosssection [mm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Radius", "radius", "radius of crosssection [mm]", GH_ParamAccess.list);

            //Input Loadcases, convert these to trees, and remodel them to usable objects with loadcase number as index
            pManager.AddNumberParameter("N", "N", "Normal force [kN]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Vz", "Vz", "Shear force z-direction[kN]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Vy", "Vy", "Shear force y-direction[kN]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Mt", "Mt", "Torsional force[kNm]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("My", "My", "Moment force y-direction[kN]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Mz", "Mz", "Moment force z-direction[kN]", GH_ParamAccess.tree);


            
            //pManager.AddBooleanParameter("RunAllJoints", "RunAllJoints", "If true run all joints, if false run ChooseJoint joint", GH_ParamAccess.item);
            pManager.AddIntegerParameter("ChooseJoint", "ChooseJoint", "Specify the joint that will be calculated in IDEA. Note: starts at zero.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("RunIDEA", "RunIDEA", "Bool for running IDEA Statica Connection", GH_ParamAccess.item);


        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("#Joints found", "#Joints found", "Number of Joints found", GH_ParamAccess.item);
            pManager.AddLineParameter("Selected Joint", "Selected Joint", "Lines of selected Joint", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            List<Line> lines = new List<Line>();
            List<string> crossectionsNameDirty = new List<string>();
            List<string> crossectionsName = new List<string>();
            List<string> groupnamesDirty = new List<string>();
            List<string> groupnames = new List<string>();
            List<string> steelgrades = new List<string>();

            GH_Structure<GH_Number> N = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> My = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> Vz = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> Vy = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> Mt = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> Mz = new GH_Structure<GH_Number>();


            int minThroatThickness = new int();
            List<string> hierarchy = new List<string>();
            int analysisMethod = new int();
            analysisMethod = 4;

            double eccentricity = double.NaN;
            eccentricity = 0.0;

            List<Point3d> centerpoints = new List<Point3d>();
            string projectnameFromGH = null;
            string templatelocation = null;
            string IDEAfilepath = null;
            List<string> shapesDirty = new List<string>();
            List<string> shapes = new List<string>();

            List<double> height = new List<double>();
            List<double> width = new List<double>();
            List<double> thicknessFlange = new List<double>();
            List<double> thicknessWeb = new List<double>();
            List<double> radius = new List<double>();

            bool startIDEA = false;
            bool calculateAllJoints = false;
            int calculateThisJoint = 0;

            //Output

            List<Line> jointlines = new List<Line>();

            List<int> numberOfSawingCuts = new List<int>();
            int TotalRightAngledCuts = new int();
            int TotalSingleMiterCuts = new int();
            int TotalDoubleMiterCuts = new int();
            double totalWeldingVolume = new double();
            List<double> weldVolumePerJoint = new List<double>();
            List<string> plateYieldingJoint = new List<string>();

            List<string> throatBegin = new List<string>();
            List<string> throatEnd = new List<string>();

            List<string> plateBegin = new List<string>();
            List<string> plateEnd = new List<string>();

            List<string> CSSnames = new List<string>();

            // grasshopperinput
            #region GrasshopperInput
            DA.GetData(0, ref templatelocation);
            DA.GetData(1, ref IDEAfilepath);


            if (!DA.GetDataList(2, hierarchy)) { return; } ;
            DA.GetDataList(3, centerpoints);

            DA.GetDataList(4, lines);
            DA.GetDataList(5, groupnamesDirty);
            DA.GetDataList(6, steelgrades);

            DA.GetDataList(7, crossectionsNameDirty);
            DA.GetDataList(8, shapesDirty);
            DA.GetDataList(9, height);
            DA.GetDataList(10, width);
            DA.GetDataList(11, thicknessFlange);
            DA.GetDataList(12, thicknessWeb);
            DA.GetDataList(13, radius);


            DA.GetDataTree(14, out N);
            DA.GetDataTree(15, out Vz);
            DA.GetDataTree(16, out Vy);
            DA.GetDataTree(17, out Mt);
            DA.GetDataTree(18, out My);
            DA.GetDataTree(19, out Mz);

            
            
            DA.GetData(20, ref calculateThisJoint);
            DA.GetData(21, ref startIDEA);

            #endregion

            //Clean cross-section list from nextline ("\r\n") command produced by Karamba
            crossectionsName = ImportGrasshopperUtils.DeleteEnterCommandsInGHStrings(crossectionsNameDirty);

            //Clean groupnames list from nextline ("\r\n") command produced by Karamba
            groupnames = ImportGrasshopperUtils.DeleteEnterCommandsInGHStrings(groupnamesDirty);

            //Clean groupnames list from nextline ("\r\n") command produced by Karamba
            shapes = ImportGrasshopperUtils.DeleteEnterCommandsInGHStrings(shapesDirty);


            //CREATE PROJECT
            Project project = new Project(projectnameFromGH);

            //Load paths
            project.templatePath = templatelocation;
            project.filepath = IDEAfilepath;

            //START IDEA BOOL
            project.startIDEA = startIDEA;
            project.calculateAllJoints = calculateAllJoints;
            project.calculateThisJoint = calculateThisJoint;
            

            //CREATE HIERARCHY
            for (int i = 0; i < hierarchy.Count; i++)
            {
                Hierarchy w = new Hierarchy(i, hierarchy[i]);
                project.hierarchylist.Add(w);
            }

            //CREATE LIST OF ELEMENT OBJECTS
            for (int i = 0; i < crossectionsName.Count; i++)
            {
                MaterialSteel.SteelGrade steelGrade = (MaterialSteel.SteelGrade)Enum.Parse(typeof(MaterialSteel.SteelGrade), steelgrades[i]);
                MaterialSteel material = project.materials.FirstOrDefault(a => a.steelGrade == steelGrade);
                if (material == null)
                {
                    material = new MaterialSteel(project, steelGrade);
                }
                //CROSS SECTIONS
                CrossSection crosssection = project.crossSections.FirstOrDefault(a => a.name == crossectionsName[i] && a.material == material);
                CrossSection.Shape shape = new CrossSection.Shape();
                
                if (shapes[i].StartsWith("I"))
                {
                    shape = CrossSection.Shape.ISection;
                }
                else if (shapes[i].StartsWith("[]"))
                {
                    shape = CrossSection.Shape.HollowSection;
                }
                else
                {
                    shape = CrossSection.Shape.CHSsection;
                }
                if (crosssection == null)
                {
                    crosssection = new CrossSection(project, crossectionsName[i], shape, material, height[i], width[i], thicknessFlange[i], thicknessWeb[i], radius[i]);
                }

                //LINES
                PointRAZ start = PointRAZ.CreateNewOrExisting(project, lines[i].FromX, lines[i].FromY, lines[i].FromZ);
                PointRAZ end = PointRAZ.CreateNewOrExisting(project, lines[i].ToX, lines[i].ToY, lines[i].ToZ);
                LineRAZ line = new LineRAZ(i, start, end);

                int hierarchyId = -1;
                Hierarchy h = project.hierarchylist.FirstOrDefault(a => groupnames[i].StartsWith(a.groupname));
                if (h != null)
                {
                    hierarchyId = h.numberInHierarchy;
                }
                ElementRAZ element = new ElementRAZ(project, i, line, crosssection, groupnames[i], hierarchyId);
            }

            //CREATE LIST OF LOADS
            //Here N,V,M are defined for the startpoint and endpoint of every line in the project.
            List<LoadsPerLineRAZ> loadsPerLineRAZs = new List<LoadsPerLineRAZ>();

            for (int i = 0; i < N.PathCount; i++)
            {
                LoadsRAZ start =    new LoadsRAZ(N[i][0].Value, Vz[i][0].Value, Vy[i][0].Value, Mt[i][0].Value, My[i][0].Value, Mz[i][0].Value);
                LoadsRAZ end =      new LoadsRAZ(N[i][1].Value, Vz[i][1].Value, Vy[i][1].Value, Mt[i][1].Value, My[i][1].Value, Mz[i][1].Value);
                LoadsPerLineRAZ w = new LoadsPerLineRAZ(start, end);
                loadsPerLineRAZs.Add(w);
            }

            //REARRANGE LIST OF LOADS TO SEPERATE LOADCASES
            //the project has x number of loadcases, here the list of loads created is rearranged to separate lists for every loadcase

            int loadcases = N.PathCount / lines.Count;
            int ib = 0;
            for (int a = 0; a < loadcases; a++)
            {

                //LoadcasesRAZ loadcasesRAZ = new LoadcasesRAZ();
                //loadcase id start from 1 because of IDEA
                int loadCaseNumber = a + 1;
                LoadcaseRAZ loadcase = new LoadcaseRAZ(project, loadCaseNumber);
                List<LoadsPerLineRAZ> loadsPerline2s = new List<LoadsPerLineRAZ>();
                for (int b = 0; b < lines.Count; b++)
                {
                    //loadcasesRAZ w = 
                    LoadsPerLineRAZ w = new LoadsPerLineRAZ(project.elementRAZs[b], loadcase, loadsPerLineRAZs[ib].startLoads, loadsPerLineRAZs[ib].endLoads);
                    ib++;
                    loadsPerline2s.Add(w);
                }


            }


            //Rhino.Point3D to PointRAZ
            List<PointRAZ> punten = new List<PointRAZ>();
            for (int i = 0; i < centerpoints.Count; i++)
            {
                PointRAZ pointRAZ = PointRAZ.CreateNewOrExisting(project, centerpoints[i].X, centerpoints[i].Y, centerpoints[i].Z);
                punten.Add(pointRAZ);
            }



            //CREATE LIST OF JOINTS
            double tol = 1e-6;
            project.CreateJoints(tol, eccentricity, punten, project.elementRAZs, project.hierarchylist);

            //Adjust out of bounds index calculateThisJoint
            project.calculateThisJoint = calculateThisJoint % project.joints.Count;

            //CALCULATE SAWING CUTS 
            //store them in the element properties
            //Project.CalculateSawingCuts(project, tol);

            //SET ALL THROATS TO MIN-THROAT THICKNESS
            project.SetMinThroats(minThroatThickness);
            //SET WELDTYPE
            project.SetWeldType();



            //DEFINE ANALYSES METHODS
            if (analysisMethod == 0)
            {
                project.analysisMethod = Project.AnalysisMethod.MinSetWelds;
            }
            if (analysisMethod == 1)
            {
                project.analysisMethod = Project.AnalysisMethod.FullStrengthLazy;
            }
            if (analysisMethod == 2)
            {
                project.analysisMethod = Project.AnalysisMethod.FullStrengthMethod;
            }
            if (analysisMethod == 3)
            {
                project.analysisMethod = Project.AnalysisMethod.DirectionalMethod;
            }
            if (analysisMethod == 4)
            {
                project.analysisMethod = Project.AnalysisMethod.IdeaMethod;
            }



            //CALCULATE THROATS ACCORDING TO ANALYSIS METHOD
            //Send DATA to IDEA
            project.CalculateWeldsProject(project.filepath);

            //CALCULATE WELDVOLUME
            totalWeldingVolume = project.CalculateWeldVolume();
            

            //Output back to Grasshopper
            //OUTPUT: WELDING, VOLUME PER JOINT
            for (int i = 0; i < project.joints.Count; i++)
            {
                weldVolumePerJoint.Add(project.joints[i].weldVolume);
            }
            //OUTPUT: WELDING, THROATS PER ELEMENT


            foreach (ElementRAZ ele in project.elementRAZs)
            {
                throatBegin.Add(ele.BeginThroatsElement());
                throatEnd.Add(ele.EndThroatsElement());

                plateBegin.Add(ele.BeginPlatesElement());
                plateEnd.Add(ele.EndPlatesElement());

                if (ele.line.vector.length > tol + eccentricity)
                {
                    CSSnames.Add(ele.crossSection.name);
                }
                else
                {
                    CSSnames.Add("");
                }
            }

            

            //OUTPUT:SAWING 
            for (int i = 0; i < project.elementRAZs.Count; i++)
            {
                ElementRAZ.SawingCut start = project.elementRAZs[i].startCut;
                ElementRAZ.SawingCut end = project.elementRAZs[i].endCut;
                if (start == ElementRAZ.SawingCut.RightAngledCut)
                {
                    TotalRightAngledCuts++;
                }
                if (end == ElementRAZ.SawingCut.RightAngledCut)
                {
                    TotalRightAngledCuts++;
                }
                if (start == ElementRAZ.SawingCut.SingleMiterCut)
                {
                    TotalSingleMiterCuts++;
                }
                if (end == ElementRAZ.SawingCut.SingleMiterCut)
                {
                    TotalSingleMiterCuts++;
                }
                if (start == ElementRAZ.SawingCut.DoubleMiterCut)
                {
                    TotalDoubleMiterCuts++;
                }
                if (end == ElementRAZ.SawingCut.DoubleMiterCut)
                {
                    TotalDoubleMiterCuts++;
                }
                int tot = (int)start + (int)end;
                //If more than element has more than one cut, substract one cut
                //In the sawing line the endcut of one member serves as the start cut of the other
                int cutsPerElement;
                if (tot > 1)
                {
                    cutsPerElement = tot - 1;
                }
                else
                {
                    cutsPerElement = tot;
                }
                numberOfSawingCuts.Add(cutsPerElement);
            }
            

            //export lines of joint for visualisation purposes
            foreach (int i in project.joints[project.calculateThisJoint].beamIDs)
            {
                jointlines.Add(lines[i]);
            }

            //Output lines
            DA.SetData(0, project.joints.Count);
            DA.SetDataList(1, jointlines);
            
      
            


        }

        

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;

                return Properties.Resources.KarambaIDEA_logo;
                
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("ca79dc4b-64f2-4627-93b4-066ad7649621"); }
        }
    }
}
