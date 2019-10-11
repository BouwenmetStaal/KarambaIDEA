// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
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

namespace KarambaIDEA.Grasshopper
{



    public class JointExporter : GH_Component
    {


#warning: Karmaba... spelfout
        public JointExporter() : base("Joint Exporter", "JE", "Exporting selected joint to IDEA Statica Connection", "KarmabaIDEA", "KarambaIDEA")
        {
            //ensure loading of IDEA dllss
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(Utils.IdeaResolveEventHandler);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Utils.IdeaResolveEventHandler);
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //Single input variables
            pManager.AddTextParameter("Template File", "Template File Location", "File location of template to be used. For example: 'C:\\Data\\template.contemp'", GH_ParamAccess.item);
            pManager.AddTextParameter("Output folder ", "Output folder", "Save location of IDEA Statica Connection output file. For example: 'C:\\Data'", GH_ParamAccess.item);

            //Input needed for creating Joints                 
            pManager.AddTextParameter("Hierarchy", "Hierarchy", "List of hierarchy on with joints are made", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "Points", "Points of connections", GH_ParamAccess.list);

            //Input elements
            pManager.AddLineParameter("Lines", "Lines", "Lines of geometry", GH_ParamAccess.list);
            pManager.AddNumberParameter("LCS rotation", "LCS rotation [Deg]", "Local Coordinate System rotation of element in degrees. Rotation runs from local y to local z-axis", GH_ParamAccess.list);
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
            pManager.AddLineParameter("Joint types", "Joint types", "Types of joint in project", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddGenericParameter("Joints", "Joints", "List of Joint objects of KarambaIdeaCore", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            List<Rhino.Geometry.Line> lines = new List<Rhino.Geometry.Line>();
            List<double> rotationLCS = new List<double>();
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
            string outputfolderpath = null;
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

            List<Rhino.Geometry.Line> jointlines = new List<Rhino.Geometry.Line>();

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
            DA.GetData(1, ref outputfolderpath);


            if (!DA.GetDataList(2, hierarchy)) { return; } ;
            DA.GetDataList(3, centerpoints);

            DA.GetDataList(4, lines);
            if (!DA.GetDataList(5, rotationLCS)) { return; };
            DA.GetDataList(6, groupnamesDirty);
            DA.GetDataList(7, steelgrades);

            DA.GetDataList(8, crossectionsNameDirty);
            DA.GetDataList(9, shapesDirty);
            DA.GetDataList(10, height);
            DA.GetDataList(11, width);
            DA.GetDataList(12, thicknessFlange);
            DA.GetDataList(13, thicknessWeb);
            DA.GetDataList(14, radius);


            DA.GetDataTree(15, out N);
            DA.GetDataTree(16, out Vz);
            DA.GetDataTree(17, out Vy);
            DA.GetDataTree(18, out Mt);
            DA.GetDataTree(19, out My);
            DA.GetDataTree(20, out Mz);
            
            DA.GetData(21, ref calculateThisJoint);
            DA.GetData(22, ref startIDEA);

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
            project.folderpath = outputfolderpath;

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
                Core.Point start = Core.Point.CreateNewOrExisting(project, lines[i].FromX, lines[i].FromY, lines[i].FromZ);
                Core.Point end = Core.Point.CreateNewOrExisting(project, lines[i].ToX, lines[i].ToY, lines[i].ToZ);
                Core.Line line = new Core.Line(i, start, end);

                //Z-VECTOR
                //Vector zvector = new Vector(rotationLCS[i].X, rotationLCS[i].Y, rotationLCS[i].Z);

                int hierarchyId = -1;
                Hierarchy h = project.hierarchylist.FirstOrDefault(a => groupnames[i].StartsWith(a.groupname));
                if (h != null)
                {
                    hierarchyId = h.numberInHierarchy;
                }
                Element element = new Element(project, i, line, crosssection, groupnames[i], hierarchyId, rotationLCS[i]);
            }

            //CREATE LIST OF LOADS
            //Here N,V,M are defined for the startpoint and endpoint of every line in the project.
            List<LoadsPerLine> loadsPerLines = new List<LoadsPerLine>();

            for (int i = 0; i < N.PathCount; i++)
            {
                Load start =    new Load(N[i][0].Value, Vz[i][0].Value, Vy[i][0].Value, Mt[i][0].Value, My[i][0].Value, Mz[i][0].Value);
                Load end =      new Load(N[i][1].Value, Vz[i][1].Value, Vy[i][1].Value, Mt[i][1].Value, My[i][1].Value, Mz[i][1].Value);
                LoadsPerLine w = new LoadsPerLine(start, end);
                loadsPerLines.Add(w);
            }

            //REARRANGE LIST OF LOADS TO SEPERATE LOADCASES
            //the project has x number of loadcases, here the list of loads created is rearranged to separate lists for every loadcase

            int loadcases = N.PathCount / lines.Count;
            int ib = 0;
            for (int a = 0; a < loadcases; a++)
            {

                //loadcase id start from 1 because of IDEA
                int loadCaseNumber = a + 1;
                LoadCase loadcase = new LoadCase(project, loadCaseNumber);
                List<LoadsPerLine> loadsPerline2s = new List<LoadsPerLine>();
                for (int b = 0; b < lines.Count; b++)
                {
                    LoadsPerLine w = new LoadsPerLine(project.elements[b], loadcase, loadsPerLines[ib].startLoad, loadsPerLines[ib].endLoad);
                    ib++;
                    loadsPerline2s.Add(w);
                }


            }


            //Rhino.Point3D to Point
            List<Core.Point> punten = new List<Core.Point>();
            for (int i = 0; i < centerpoints.Count; i++)
            {
                Core.Point Point = Core.Point.CreateNewOrExisting(project, centerpoints[i].X, centerpoints[i].Y, centerpoints[i].Z);
                punten.Add(Point);
            }



            //CREATE LIST OF JOINTS
            double tol = 1e-6;
            project.CreateJoints(tol, eccentricity, punten, project.elements, project.hierarchylist);

            //Adjust out of bounds index calculateThisJoint
            project.calculateThisJoint = calculateThisJoint % project.joints.Count;

            //CALCULATE SAWING CUTS 
            //store them in the element properties
            //Project.CalculateSawingCuts(project, tol);

            //SET ALL THROATS TO MIN-THROAT THICKNESS
            project.SetMinThroats(minThroatThickness);
           
            //SET WELDTYPE
            //project.SetDefaultWeldType();  //Refactored, can be removed. todo after test run


            #warning: what is the point of this if calculaton methods are not implemented?
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
            if (project.analysisMethod == Project.AnalysisMethod.IdeaMethod)
            {
                if (startIDEA == true)
                {
                    //Send DATA to IDEA
                    project.CreateFolder(project.folderpath);
                    if (project.calculateAllJoints)
                    {
                        foreach (Joint joint in project.joints)
                        {
                            CalculateJoint(joint, templatelocation, project.folderpath);
                        }
                    }
                    //Calculate one joint
                    else
                    {
                        Joint joint = project.joints[project.calculateThisJoint];
                        CalculateJoint(joint, templatelocation, project.folderpath);
                    }
                }
            }
            else
            {
                throw new Exception("Calculation methodology currently not implemented");
            }

            //CALCULATE WELDVOLUME
            totalWeldingVolume = project.CalculateTotalWeldVolume();
            

            //Output back to Grasshopper
            //OUTPUT: WELDING, VOLUME PER JOINT
            for (int i = 0; i < project.joints.Count; i++)
            {
                weldVolumePerJoint.Add(project.joints[i].weldVolume);
            }
            //OUTPUT: WELDING, THROATS PER ELEMENT


            foreach (Element ele in project.elements)
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
            for (int i = 0; i < project.elements.Count; i++)
            {
                Element.SawingCut start = project.elements[i].startCut;
                Element.SawingCut end = project.elements[i].endCut;
                if (start == Element.SawingCut.RightAngledCut)
                {
                    TotalRightAngledCuts++;
                }
                if (end == Element.SawingCut.RightAngledCut)
                {
                    TotalRightAngledCuts++;
                }
                if (start == Element.SawingCut.SingleMiterCut)
                {
                    TotalSingleMiterCuts++;
                }
                if (end == Element.SawingCut.SingleMiterCut)
                {
                    TotalSingleMiterCuts++;
                }
                if (start == Element.SawingCut.DoubleMiterCut)
                {
                    TotalDoubleMiterCuts++;
                }
                if (end == Element.SawingCut.DoubleMiterCut)
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

            //Define Brandnames and assemble tree
            project.SetBrandnames(project);
            //select unique brandName
            //GH_Structure<GH_Line> linetree = new GH_Structure<GH_Line>();
            DataTree<Rhino.Geometry.Line> linetree = new DataTree<Rhino.Geometry.Line>();
            
           
            var lstBrand =project.joints.Select(a => a.brandName).Distinct().ToList();
            for(int a = 0; a < lstBrand.Count; a++)
            {
                int d = 0;
                for (int b = 0; b < project.joints.Count; b++)
                {
                    if (lstBrand[a] == project.joints[b].brandName)
                    {
                        for (int c = 0; c < project.joints[b].attachedMembers.Count; c++)
                        {
                            Core.Line oriline = project.joints[b].attachedMembers[c].ideaLine;
                            Core.Point centerpoint = project.joints[b].centralNodeOfJoint;
                            Core.Line line = Core.Line.MoveLineToOrigin(centerpoint, oriline);
                            GH_Path path = new GH_Path(a, d);
                            Point3d start = new Point3d(line.Start.X, line.Start.Y, line.Start.Z);
                            Point3d end = new Point3d(line.End.X, line.End.Y, line.End.Z);
                            Rhino.Geometry.Line rhinoline = new Rhino.Geometry.Line(start, end);
                            linetree.Add(rhinoline, path);
                        }
                        d = d + 1;
                    }
                }
            }

            //joint.attachedMembers.Select(a => a.ideaLine.Start).ToList();

            //Output lines
            DA.SetData(0, project.joints.Count);
            DA.SetDataList(1, jointlines);
            DA.SetDataTree(2, linetree);
            DA.SetData(3, project);
            DA.SetDataList(4, project.joints);

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

        private void CalculateJoint(Joint joint, String templateFilePath, String folderpath)
        {
            // Create connection
            IdeaConnection ideaConnection = new IdeaConnection(joint, templateFilePath, folderpath);

            // Map Connections
            ideaConnection.MapWeldsIdsAndOperationIds();

            // Check connection
            // ideaConnection.CheckConnectionWelds();

            //Save file
            string filePath2 = ideaConnection.filepath + "//" + joint.Name + "joint.ideaCon";
            ideaConnection.SaveIdeaConnectionProjectFile(filePath2);
        }
    }
}
