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
using Grasshopper.Kernel.Parameters;

namespace KarambaIDEA.Grasshopper
{



    public class JointExporter : GH_Component
    {
        public JointExporter() : base("Create Project", "CP", "Exporting selected joint to IDEA Statica Connection", "KarambaIDEA", "2. CreateProject")
        {
            //ensure loading of IDEA dllss
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(Utils.IdeaResolveEventHandler);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Utils.IdeaResolveEventHandler);
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            
            //Input needed for creating Joints                 
            pManager.AddTextParameter("Hierarchy", "Hierarchy", "List of hierarchy on with joints are made", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "Points", "Points of connections", GH_ParamAccess.list);

            
            //Input elements
            pManager.AddLineParameter("Lines", "Lines", "Lines of geometry", GH_ParamAccess.list);
            pManager.AddNumberParameter("LCS rotations [Deg]", "LCS rotations", "Local Coordinate System rotation of element in Degrees. Rotation runs from local y to local z-axis", GH_ParamAccess.list);
            pManager.AddTextParameter("Groupnames", "Groupnames", "Groupname of element", GH_ParamAccess.list);
            pManager.AddTextParameter("Materials", "Materials", "Steel grade of every element", GH_ParamAccess.list);

            //Input for creating Cross-section Objects
            pManager.AddTextParameter("Cross-section names", "CroSecNames", "Name of cross-sections", GH_ParamAccess.list);
            pManager.AddTextParameter("Shapes", "Shapes", "Shape of of cross-sections", GH_ParamAccess.list);
            pManager.AddNumberParameter("Height", "Heights", "Heigth of crosssections [mm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Width", "Widths", "Width of crosssections [mm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Thickness Flanges", "TFlanges", "Flange thickness of crosssections [mm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Thickness Webs", "TWebs", "Web thickness of crosssections [mm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Radius", "FRads", "Radius of crosssections [mm]", GH_ParamAccess.list);

            //Input Loadcases, convert these to trees, and remodel them to usable objects with loadcase number as index
            pManager.AddNumberParameter("N", "N", "Normal force [kN]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Vz", "Vz", "Shear force z-direction[kN]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Vy", "Vy", "Shear force y-direction[kN]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Mt", "Mt", "Torsional force[kNm]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("My", "My", "Moment force y-direction[kN]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Mz", "Mz", "Moment force z-direction[kN]", GH_ParamAccess.tree);

            // Assign default Hierarchy.
            Param_String param0 = (Param_String)pManager[0];
            param0.PersistentData.Append(new GH_String(""));


        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddGenericParameter("Joints", "Joints", "List of Joint objects of KarambaIdeaCore", GH_ParamAccess.list);
            

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            string projectnameFromGH = null;
            double eccentricity = 0.0;

            List<GH_String> h_Strings = new List<GH_String>();
            List<string> hierarchy = new List<string>();


            List<Point3d> centerpoints = new List<Point3d>();
            List<Rhino.Geometry.Line> lines = new List<Rhino.Geometry.Line>();
            List<double> rotationLCS = new List<double>();

            List<string> crossectionsNameDirty = new List<string>();
            List<string> crossectionsName = new List<string>();

            List<string> steelgrades = new List<string>();

            List<string> groupnamesDirty = new List<string>();
            List<string> groupnames = new List<string>();

            List<string> shapesDirty = new List<string>();
            List<string> shapes = new List<string>();

            List<double> height = new List<double>();
            List<double> width = new List<double>();
            List<double> thicknessFlange = new List<double>();
            List<double> thicknessWeb = new List<double>();
            List<double> radius = new List<double>();


            GH_Structure<GH_Number> N = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> My = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> Vz = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> Vy = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> Mt = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> Mz = new GH_Structure<GH_Number>();


            //Link Input
            #region GrasshopperInput
            DA.GetDataList(0, h_Strings);
            DA.GetDataList(1, centerpoints);
            DA.GetDataList(2, lines);
            DA.GetDataList(3, rotationLCS);
            DA.GetDataList(4, groupnamesDirty);
            DA.GetDataList(5, steelgrades);

            DA.GetDataList(6, crossectionsNameDirty);
            DA.GetDataList(7, shapesDirty);
            DA.GetDataList(8, height);
            DA.GetDataList(9, width);
            DA.GetDataList(10, thicknessFlange);
            DA.GetDataList(11, thicknessWeb);
            DA.GetDataList(12, radius);

            DA.GetDataTree(13, out N);
            DA.GetDataTree(14, out Vz);
            DA.GetDataTree(15, out Vy);
            DA.GetDataTree(16, out Mt);
            DA.GetDataTree(17, out My);
            DA.GetDataTree(18, out Mz);


            #endregion

            //cast Grassshopper string list to string list
            if (h_Strings.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Value)).Count() > 0)
            {
                hierarchy = h_Strings.Select(x => x.Value.ToString()).ToList();
            }


            //Clean cross-section list from nextline ("\r\n") command produced by Karamba
            crossectionsName = ImportGrasshopperUtils.DeleteEnterCommandsInGHStrings(crossectionsNameDirty);

            //Clean groupnames list from nextline ("\r\n") command produced by Karamba
            groupnames = ImportGrasshopperUtils.DeleteEnterCommandsInGHStrings(groupnamesDirty);

            //Clean shapes list from nextline ("\r\n") command produced by Karamba
            shapes = ImportGrasshopperUtils.DeleteEnterCommandsInGHStrings(shapesDirty);


            //CREATE PROJECT
            Project project = new Project(projectnameFromGH);
           
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

            //Link output
            DA.SetData(0, project);
            DA.SetDataList(1, project.joints);
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
            get { return new Guid("ca79dc4b-64f2-4627-93b4-066ad7649621"); }
        }
        
    }
}
