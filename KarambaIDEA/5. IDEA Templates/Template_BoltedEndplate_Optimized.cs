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
using KarambaIDEA.Core.JointTemplate;

namespace KarambaIDEA._5._IDEA_Templates
{
    public class Template_BoltedEndplate_Optimized : GH_Component
    {
        public Template_BoltedEndplate_Optimized() : base("Template: Bolted endplate optimizer", "Template: Bolted endplate optimizer", "Template: Bolted endplate optimizer", "KarambaIDEA", "5. IDEA Templates")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("BrandNames", "BrandNames", "BrandNames to apply template to", GH_ParamAccess.list, "");
            pManager.AddNumberParameter("Thickness endplate [mm]", "Thickness endplate [mm]", "", GH_ParamAccess.item, 10.0);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("Message", "Message", "", GH_ParamAccess.tree);
            pManager.AddBrepParameter("Brep", "Brep", "", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables      
            Project sourceProject = new Project();
            double tplate = new double();
            List<GH_String> brandNamesDirty = new List<GH_String>();
            List<string> brandNames = new List<string>();

            //Output variables
            DataTree<string> messages = new DataTree<string>();
            DataTree<Brep> breps = new DataTree<Brep>();

            //Link input
            DA.GetData(0, ref sourceProject);
            DA.GetDataList(1, brandNamesDirty);
            DA.GetData(2, ref tplate);

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

            //TODO: make a message "BrandName 011 is linked to BoltedEndPlateConnection"
            if (brandNames.Count != 0)
            {
                foreach (string brandName in brandNames)
                {
                    foreach (Joint joint in project.joints)
                    {
                        if (brandName == joint.brandName)
                        {
                            SetTemplate(tplate, joint, breps, messages);
                        }
                    }
                }
            }
            //link output
            DA.SetData(0, project);
            DA.SetDataTree(1, messages);
            DA.SetDataTree(2, breps);
        }

        private static void SetTemplate(double tplate, Joint joint, DataTree<Brep> breps, DataTree<string> messages)
        {
            joint.template = new Template();
            int a = joint.id;
            int b = 0;
            joint.template.boltGrids.Clear();
            Core.CrossSection beam = joint.attachedMembers.First().element.crossSection;

            joint.template.workshopOperations = Template.WorkshopOperations.BoltedEndPlateConnection;
            foreach (AttachedMember at in joint.attachedMembers)
            {
                GH_Path path = new GH_Path(a, b);
                GH_Path pathPlates = new GH_Path(a, b,0);
                GH_Path pathBolts = new GH_Path(a, b,1);
                b++;
                //Step I - retrieve loads
                List<double> nloads = new List<double>();
                foreach (LoadCase lc in joint.project.loadcases)
                {
                    foreach (LoadsPerLine loadsPerLine in lc.loadsPerLines)
                    {
                        if (loadsPerLine.element == at.element)
                        {
                            if (at.isStartPoint == true)
                            {
                                nloads.Add(Math.Abs(loadsPerLine.startLoad.N));
                            }
                            else
                            {
                                nloads.Add(Math.Abs(loadsPerLine.endLoad.N));
                            }
                        }
                    }
                }
                double nload = nloads.Max() * 1000;
                //Step II - Create bolts list
                List<Bolt> bolts = Bolt.CreateBoltsList(BoltSteelGrade.Steelgrade.b8_8);
                
                //Step IV - evaluation of different bolt configurations
                int it = 0;
                foreach (Bolt bolt in bolts)
                {
                    //Step III - determine maximum inner height and inner width of I section
                    double hymax = beam.height - 2 * (beam.thicknessFlange);
                    double hxmax = (beam.width - beam.thicknessWeb) / 2;


                    double d0 = bolt.HoleDiameter;

                    double e1 = 1.2 * d0;//Edge distance longitudinal direction (local Y)
                    double p1 = 2.2 * d0;//Inner distance longitudinal direction
                    double e2 = 1.2 * d0;//Edge distance transversal direction (local X)
                    double p2 = 2.2 * d0;//single row, so actually not needed

                    hymax -= (2 * e1);
                    hxmax -= (2 * e2);

                    if (bolt.Name == "M16")
                    {
                        int adfadfa = 0;
                    }

                    for (int nx = 1; nx < 3; nx++)
                    {
                        double hXspaceleft = hxmax - (d0 * nx) - (p2 * (nx - 1));
                        if (hXspaceleft >= 0)
                        {
                            for (int ny = 2; ny < 4; ny++)
                            {
                                string info = bolt.Name + " " + nx + "x" + ny + "";
                                double hYspaceleft = hymax - (d0 * ny) - (p1 * (ny - 1));
                                if (hYspaceleft >= 0)//if configuration fits within the beam
                                {
                                    it++;

                                    //Check Bolts in tension
                                    double pryingForcesFactor = 1;
                                    double nRdTensionBolts = ny*nx *2* bolt.TensionResistance() * pryingForcesFactor;
                                    if (nload > nRdTensionBolts)//If Ved>Vrd
                                    {
                                        double uc = Math.Round(nload / nRdTensionBolts,2);
                                        messages.Add(info + ": Bolts fail in tension [uc="+uc+"]", path);
                                        goto increaseN;
                                    }
                                    messages.Add(info + " is chosen after " + it + " iterations.", path);
                                    //Assemble bolt grid
                                    List<Coordinate2D> coors = new List<Coordinate2D>();
                                    double locX = (beam.width/2)-(0.5*d0+e2);
                                    for(int col = 0; col < nx;col++)
                                    {
                                        double locY = (hymax-d0)/2;
                                        for (int row = 0; row < ny; row++)
                                        {
                                            coors.Add(new Coordinate2D(locX, locY));///or
                                            coors.Add(new Coordinate2D(-locX, locY));//mirrored bolt
                                            locY = locY - ((hymax - d0) / (ny - 1));
                                        }
                                        locX = locX - (d0 + e2);
                                    }
                                    BoltGrid boltGrid = new BoltGrid(bolt, coors);
                                    joint.template.boltGrids.Add(boltGrid);
                                    goto finish;
                                }
                                else
                                {
                                    messages.Add(info + ": too little space in Y ["+hYspaceleft+" mm]", path);

                                    //extend plate
                                    double hExtraPlate =2*(d0 + 2 * e1);

                                }
                            increaseN:;
                            }
                        }
                        else
                        {
                            messages.Add(bolt.Name + " " + nx + "x?: too little space in X [" + hXspaceleft + " mm]", path);
                        }
                        
                    }
                }
                messages.Add("No solution found", path);
                goto nosolution;
            /*
            joint.template.plates.Clear();
            joint.template.welds.Clear();
            joint.template.boltGrids.Clear();
            */

            finish:;
                //Step V - Define data for cost analyses
                Plate plateA = new Plate("endplateA", beam.height, beam.width, tplate);
                Plate plateB = new Plate("endplateB", beam.height, beam.width, tplate);

                joint.template.plates.Add(plateA);
                joint.template.plates.Add(plateB);

                double weldsize = Weld.CalWeldSizeFullStrenth90deg(tplate, beam.thicknessFlange, beam.material, Weld.WeldType.DoubleFillet);
                joint.template.welds.Add(new Weld("FlangeweldTop", Weld.WeldType.DoubleFillet, weldsize, beam.width));
                joint.template.welds.Add(new Weld("FlangeweldBot", Weld.WeldType.DoubleFillet, weldsize, beam.width));
                double weldsizeWeb = Weld.CalWeldSizeFullStrenth90deg(tplate, beam.thicknessWeb, beam.material, Weld.WeldType.DoubleFillet);
                joint.template.welds.Add(new Weld("Webweld", Weld.WeldType.DoubleFillet, weldsizeWeb, beam.height));
                

                //Step VII - create Brep for visiualisation
                //BREP beam
                Point3d point = new Point3d();
                Vector vX = new Vector();
                if (at.isStartPoint == true)
                {
                    at.element.brepLine.start = KarambaIDEA.Core.Line.ExtendLine(at.element.Line, -tplate / 1000, true);
                    point = ImportGrasshopperUtils.CastPointToRhino(at.element.Line.start);
                    vX = (at.element.localCoordinateSystem.X);
                }
                else
                {
                    at.element.brepLine.end = KarambaIDEA.Core.Line.ExtendLine(at.element.Line, -tplate / 1000, false);
                    point = ImportGrasshopperUtils.CastPointToRhino(at.element.Line.end);
                    vX = (at.element.localCoordinateSystem.X.FlipVector());
                }

                //BREP add plate
                if (tplate != 0.0)
                {
                    //Create plate
                    CrossSection c = at.element.crossSection;

                    Vector3d vecZ = ImportGrasshopperUtils.CastVectorToRhino(at.element.localCoordinateSystem.Z);
                    Vector3d vecY = ImportGrasshopperUtils.CastVectorToRhino(at.element.localCoordinateSystem.Y);
                    Vector3d vecX = ImportGrasshopperUtils.CastVectorToRhino(Vector.VecScalMultiply(vX.Unitize(), (tplate / 1000)));

                    Plane plane = new Plane(point, vecY, vecZ);

                    Point3d P1 = new Point3d(-c.width / 2000, c.height / 2000, 0);
                    Point3d P6 = new Point3d(-c.width / 2000, -c.height / 2000, 0);
                    Point3d P7 = new Point3d(c.width / 2000, -c.height / 2000, 0);
                    Point3d P12 = new Point3d(c.width / 2000, c.height / 2000, 0);
                    IEnumerable<Point3d> points = new Point3d[] { P1, P6, P7, P12, P1 };
                    Polyline poly = new Polyline(points);
                    NurbsCurve nurbsCurve = poly.ToNurbsCurve();

                    Transform transform = Transform.PlaneToPlane(Plane.WorldXY, plane);
                    nurbsCurve.Transform(transform);
                    Surface sur = Surface.CreateExtrusion(nurbsCurve, vecX);
                    Brep plate = sur.ToBrep().CapPlanarHoles(Project.tolerance);

                    //Create Holes
                    BoltGrid bg = joint.template.boltGrids.FirstOrDefault();
                    foreach(Coordinate2D coor in bg.Coordinates2D)
                    {
                        Brep tubes = new Brep();

                        double d0 = bg.bolttype.HoleDiameter;
                        double corX = coor.locX / 1000;
                        double corY = coor.locY / 1000;
                        double tol = 0.001;

                        Vector3d planeVecX = plane.XAxis;
                        planeVecX.Unitize();
                        Transform transform2 = Transform.Translation(Vector3d.Multiply(corX, planeVecX));
                        Plane plane2 = plane;
                        plane2.Transform(transform2);

                        Vector3d planeVecY = plane.YAxis;
                        planeVecY.Unitize();
                        Transform transform3 = Transform.Translation(Vector3d.Multiply(corY, planeVecY));
                        Plane plane3 = plane2;
                        plane3.Transform(transform3);

                        Circle circle = new Circle(plane3, d0/2000);//Radius 

                        Polyline hexagon = Polyline.CreateCircumscribedPolygon(circle, 6);
                        Surface sur2 = Surface.CreateExtrusion(hexagon.ToNurbsCurve(), 2 * vecX);
                        
                        Brep tube = sur2.ToBrep().CapPlanarHoles(tol);
                        breps.Add(tube, pathBolts);
                        //plate = Brep.CreateBooleanDifference(plate, tube, tol).ToList().FirstOrDefault();
                    }
                    breps.Add(plate, pathPlates);
                }
                nosolution:;
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.TempBoltedEndplateConnection;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("5a10321e-750c-4c31-ae29-21b2d09e9750"); }
        }


    }
}
