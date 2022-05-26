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
    public class CreateProject : GH_Component
    {
        public CreateProject() : base("Create Project", "CP", "Exporting selected joint to IDEA Statica Connection", "KarambaIDEA", "1. Project")
        {
            //TODO: below code will trow error if idea dlls are not found.
            /*
            //ensure loading of IDEA dllss
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(Utils.IdeaResolveEventHandler);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Utils.IdeaResolveEventHandler);
            */
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.primary; } }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            //Input needed for creating Joints                 
            pManager.AddTextParameter("Hierarchy", "Hierarchy", "List of hierarchy on with joints are made", GH_ParamAccess.list);
            pManager[0].Optional = true;
            pManager.AddPointParameter("Points", "Points", "Points of connections", GH_ParamAccess.list);
            pManager.AddTextParameter("Jointnames", "Jointnames", "Optional by the user-defined name for every joint. List must be equal to list of Points", GH_ParamAccess.list);
            pManager[2].Optional = true;

            //Input elements
            pManager.AddLineParameter("Lines", "Lines", "Lines of geometry", GH_ParamAccess.list);
            pManager.AddNumberParameter("LCS rotations [Deg]", "LCS rotations", "Local Coordinate System rotation of element in Degrees. Rotation runs from local y to local z-axis", GH_ParamAccess.list);
            pManager[4].Optional = true;
            pManager.AddVectorParameter("Eccentricity vector", "Eccentricity vector", "Eccentricities defined by local x, y and z coordinate", GH_ParamAccess.list);
            pManager[5].Optional = true;
            pManager.AddTextParameter("Groupnames", "Groupnames", "Groupname of element", GH_ParamAccess.list);
            pManager.AddTextParameter("Membernames", "Membernames", "Optional by the user defined names for every member. List must be equal to the number of members", GH_ParamAccess.list);
            pManager[7].Optional = true;

            //Input for creating Cross-section Objects
            pManager.AddTextParameter("Materials", "Materials", "Steel grade of every element", GH_ParamAccess.list);
            pManager.AddTextParameter("Cross-section names", "CroSecNames", "Names of cross-sections", GH_ParamAccess.list);
            pManager.AddTextParameter("Shapes", "Shapes", "Shape of of cross-sections", GH_ParamAccess.list);
            pManager.AddNumberParameter("Height", "Heights", "Heigth of crosssections [mm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Width", "Widths", "Width of crosssections [mm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Thickness Flanges", "TFlanges", "Flange thickness of crosssections [mm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Thickness Webs", "TWebs", "Web thickness of crosssections [mm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Radius", "FRads", "Radius of crosssections [mm]", GH_ParamAccess.list);

            //Input Loadcases, convert these to trees, and remodel them to usable objects with loadcase number as index
            pManager.AddTextParameter("Load-case names", "LoadCaseNames", "Names of load-cases. List must be equal to the number of loadcases present in the internal forces", GH_ParamAccess.list);
            pManager[16].Optional = true;
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
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            string projectnameFromGH = null;
            double eccentricity = 0.0;

            List<GH_String> hierarchyDirty = new List<GH_String>();
            List<string> hierarchy = new List<string>();

            List<Point3d> centerpoints = new List<Point3d>();
            List<string> jointnames = new List<string>();
            List<Rhino.Geometry.Line> lines = new List<Rhino.Geometry.Line>();
            List<double> rotationLCS = new List<double>();
            List<Rhino.Geometry.Vector3d> eccentrictyVecs = new List<Vector3d>();
            List<string> crossectionsNameDirty = new List<string>();
            List<string> crossectionsName = new List<string>();
            List<string> membernames = new List<string>();
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

            List<string> loadCaseNames = new List<string>();
            GH_Structure<GH_Number> N = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> My = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> Vz = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> Vy = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> Mt = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> Mz = new GH_Structure<GH_Number>();


            //Link Input
            #region GrasshopperInput
            DA.GetDataList(0, hierarchyDirty);
            if (hierarchyDirty.Count == 0)
            {
                hierarchyDirty.Add(new GH_String("NoHierarchy"));
            }
            DA.GetDataList(1, centerpoints);
            DA.GetDataList(2, jointnames); //TODO: implement code
            DA.GetDataList(3, lines);
            DA.GetDataList(4, rotationLCS);
            DA.GetDataList(5, eccentrictyVecs); //TODO: implement code
            DA.GetDataList(6, groupnamesDirty);
            DA.GetDataList(7, membernames); //TOD: implement code

            DA.GetDataList(8, steelgrades);
            DA.GetDataList(9, crossectionsNameDirty);
            DA.GetDataList(10, shapesDirty);
            DA.GetDataList(11, height);
            DA.GetDataList(12, width);
            DA.GetDataList(13, thicknessFlange);
            DA.GetDataList(14, thicknessWeb);
            DA.GetDataList(15, radius);

            DA.GetDataList(16, loadCaseNames);

            DA.GetDataTree(17, out N);
            DA.GetDataTree(18, out Vz);
            DA.GetDataTree(19, out Vy);
            DA.GetDataTree(20, out Mt);
            DA.GetDataTree(21, out My);
            DA.GetDataTree(22, out Mz);


            #endregion

            //cast Grassshopper string list to string list
            if (hierarchyDirty.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Value)).Count() > 0)
            {
                hierarchy = hierarchyDirty.Select(x => x.Value.ToString()).ToList();
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
                    shape = CrossSection.Shape.RHSsection;
                }
                else if (shapes[i].StartsWith("()")|| shapes[i].StartsWith("O"))
                {
                    shape = CrossSection.Shape.CHSsection;
                }
                else if (shapes[i].StartsWith("T"))
                {
                    shape = CrossSection.Shape.Tsection;
                }
                else if (shapes[i].StartsWith("V"))
                {
                    shape = CrossSection.Shape.Strip;
                }
                else
                {
                    throw new ArgumentNullException(shapes[i]+" Cross-section not implemented");
                }
                if (crosssection == null)
                {
                    crosssection = new CrossSection(project, crossectionsName[i], shape, material, height[i], width[i], thicknessFlange[i], thicknessWeb[i], radius[i]);
                }
                //LINES
                Core.Point start = Core.Point.CreateNewOrExisting(project, lines[i].FromX, lines[i].FromY, lines[i].FromZ);
                Core.Point end = Core.Point.CreateNewOrExisting(project, lines[i].ToX, lines[i].ToY, lines[i].ToZ);
                Core.Line line = new Core.Line(i, start, end);
                //HIERARCHY
                int hierarchyId = -1;
                Hierarchy h = project.hierarchylist.FirstOrDefault(a => groupnames[i].StartsWith(a.groupname));
                if (h != null)
                {
                    hierarchyId = h.numberInHierarchy;
                }
                //MEMBER-NAME
                string membername = "Member " + i;
                if (membernames.Count != 0)
                {
                    if(membernames.Count != lines.Count)
                    {
                        throw new ArgumentNullException("Membernames not equal to the number of members");
                    }
                    membername = membernames[i];
                }
                //Eccentricity vectors
                Core.Vector eccVec = new Vector(0,0,0);
                if (eccentrictyVecs.Count != 0)
                {
                    if (eccentrictyVecs.Count != lines.Count)
                    {
                        throw new ArgumentNullException("Vectors not equal to the number of members");
                    }
                    eccVec = ImportGrasshopperUtils.CastVectorToCore(eccentrictyVecs[i]);
                }

                double rotation = 0.0;
                if (rotationLCS.Count != 0)
                {
                    rotation = rotationLCS[i];  
                }

                Element element = new Element(project, membername, i, line, crosssection, groupnames[i], hierarchyId, rotation, eccVec);
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
            if (loadCaseNames.Count != 0)
            {
                if (loadcases != loadCaseNames.Count)
                {
                    throw new ArgumentNullException("LoadCaseNames not equal to the number of loadcases");
                }
            }
            
            for (int a = 0; a < loadcases; a++)
            {
                string loadCaseName = "LC index "+a;
                if (loadCaseNames.Count != 0)
                {
                    loadCaseName = loadCaseNames[a];
                }
                LoadCase loadcase = new LoadCase(project, a, loadCaseName);
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
            project.CreateJoints(tol, eccentricity, punten, jointnames, project.elements, project.hierarchylist);

            //DEFINE BRANDNAMES PER JOINT
            project.SetBrandNames(project);

            //Link output
            DA.SetData(0, new GH_KarambaIdeaProject(project));
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.KarambaIDEA_logo; } }

        public override Guid ComponentGuid { get { return new Guid("ca79dc4b-64f2-4627-93b4-066ad7649621"); } }
    }
}
