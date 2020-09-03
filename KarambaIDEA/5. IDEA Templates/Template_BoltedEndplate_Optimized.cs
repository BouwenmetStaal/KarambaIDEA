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
using System.Runtime.InteropServices;

namespace KarambaIDEA._5._IDEA_Templates
{
    public class Template_BoltedEndplate_Optimized : GH_Component
    {
        public Template_BoltedEndplate_Optimized() : base("Template: Bolted endplate optimizer", "T: BEO", "Template: Bolted endplate optimizer", "KarambaIDEA", "5. IDEA Templates")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "P", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("BrandNames", "BN", "BrandNames to apply template to", GH_ParamAccess.list, "");
            pManager.AddNumberParameter("Thickness endplate [mm]", "T [mm]", "", GH_ParamAccess.item, 10.0);
            pManager.AddNumberParameter("factor Prying forces", "PF", "", GH_ParamAccess.item, 0.5);
            pManager.AddBooleanParameter("Stiffeners?", "S?", "Does the connection include stiffeners?", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "P", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("Message", "M", "", GH_ParamAccess.tree);
            pManager.AddBrepParameter("Brep", "B", "", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables      
            Project sourceProject = new Project();
            double tplate = new double();
            List<GH_String> brandNamesDirty = new List<GH_String>();
            List<string> brandNames = new List<string>();
            double pryingForcesFactor = new double();
            bool stiffener = new bool();

            //Output variables
            DataTree<string> messages = new DataTree<string>();
            DataTree<Brep> breps = new DataTree<Brep>();

            //Link input
            DA.GetData(0, ref sourceProject);
            DA.GetDataList(1, brandNamesDirty);
            DA.GetData(2, ref tplate);
            DA.GetData(3, ref pryingForcesFactor);
            DA.GetData(4, ref stiffener);

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
                            SetTemplate(tplate, pryingForcesFactor, stiffener, joint, breps, messages);
                        }
                    }
                }
            }
            //link output
            DA.SetData(0, project);
            DA.SetDataTree(1, messages);
            DA.SetDataTree(2, breps);
        }

        private static void SetTemplate(double tplate, double pryingForcesFactor, bool stiffener, Joint joint, DataTree<Brep> breps, DataTree<string> messages)
        {
            joint.template = new Template();
            int a = joint.id;
            int b = 0;
            joint.template.boltGrids.Clear();
            Core.CrossSection beam = joint.attachedMembers.First().element.crossSection;
            
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
                double hEndplate = beam.height;
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

                    for (int nx = 1; nx < 3; nx++)
                    {
                        double hXspaceleft = hxmax - (d0 * nx) - (p2 * (nx - 1));
                        if (hXspaceleft >= 0)
                        {
                            int ny_inner = 1;
                            hEndplate = beam.height;
                            for (int ny = 2; ny < 4; ny++)
                            {
                                string info = bolt.Name + " " + nx + "x" + ny + "";
                                double hYspaceleft = hymax - (d0 * ny) - (p1 * (ny - 1));
                                if (hYspaceleft >= 0)//if configuration fits within the beam
                                {
                                    //hEndplate = beam.height;
                                    ny_inner++;
                                    it++;
                                    //Check Bolts in tension
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
                                            coors.Add(new Coordinate2D(locX, locY));//original
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
                                    //messages.Add(info + ": too little space in Y ["+hYspaceleft+" mm]", path);
                                    int ny_outer = ny - ny_inner;
                                    //extend plate
                                    double hExtraPlate =2*(d0 + 2 * e1);
                                    hEndplate = hEndplate + hExtraPlate;

                                    //Check Bolts in tension
                                    double nRdTensionBolts = (ny_inner + ny_outer*2) * nx * 2 * bolt.TensionResistance() * pryingForcesFactor;
                                    if (nload > nRdTensionBolts)//If Ved>Vrd
                                    {
                                        double uc = Math.Round(nload / nRdTensionBolts, 2);
                                        messages.Add(info + ": Bolts fail in tension [uc=" + uc + "]", path);
                                        goto increaseN;
                                    }
                                    messages.Add(info + " is chosen after " + it + " iterations.", path);
                                    //Assemble bolt grid
                                    List<Coordinate2D> coors = new List<Coordinate2D>();
                                    double locX = (beam.width / 2) - (0.5 * d0 + e2);
                                    for (int col = 0; col < nx; col++)
                                    {
                                        double locY_inn = (hymax - d0) / 2;
                                        for (int row = 0; row < ny_inner; row++)
                                        {
                                            if(ny_inner == 1)
                                            {
                                                locY_inn = 0;
                                            }
                                            
                                            coors.Add(new Coordinate2D(locX, locY_inn));//original
                                            coors.Add(new Coordinate2D(-locX, locY_inn));//mirrored bolt
                                            locY_inn = locY_inn - ((hymax - d0) / (ny_inner - 1));
                                        }
                                        double locY_out = ((hEndplate) / 2)-(0.5 * d0 + e2);
                                        for (int row = 0; row < ny_outer; row++)
                                        {
                                            coors.Add(new Coordinate2D(locX, locY_out));//original
                                            coors.Add(new Coordinate2D(-locX, locY_out));//mirrored bolt
                                            coors.Add(new Coordinate2D(locX, -locY_out));//original
                                            coors.Add(new Coordinate2D(-locX, -locY_out));//mirrored bolt
                                            locY_out = locY_out - (d0+e2);
                                        }
                                        locX = locX - (d0 + e2);
                                    }
                                    BoltGrid boltGrid = new BoltGrid(bolt, coors);
                                    joint.template.boltGrids.Add(boltGrid);
                                    goto finish;

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
                Plate plateA = new Plate("endplate", hEndplate, beam.width, tplate);

                joint.template.plates.Add(plateA);

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

                    Point3d P1 = new Point3d(-c.width / 2000, hEndplate / 2000, 0);
                    Point3d P6 = new Point3d(-c.width / 2000, -hEndplate / 2000, 0);
                    Point3d P7 = new Point3d(c.width / 2000, -hEndplate / 2000, 0);
                    Point3d P12 = new Point3d(c.width / 2000, hEndplate / 2000, 0);
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

                        double d0 = bg.bolttype.HeadDiagonalDiameter;
                        double h0 = tplate+bg.bolttype.HeadHeight;
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
                        vecX.Unitize();
                        Surface sur2 = Surface.CreateExtrusion(hexagon.ToNurbsCurve(), Vector3d.Multiply(h0/1000, vecX));
                        
                        Brep tube = sur2.ToBrep().CapPlanarHoles(tol);
                        breps.Add(tube, pathBolts);
                        //plate = Brep.CreateBooleanDifference(plate, tube, tol).ToList().FirstOrDefault();
                    }
                    breps.Add(plate, pathPlates);
                }
                //BREP add stiffeners
                if (stiffener == true && hEndplate > beam.height)
                {
                    double hstif = (hEndplate - beam.height)/ 2;
                    double lstif = hstif * 2;
                    CrossSection c = at.element.crossSection;
                    double tstiff = c.thicknessWeb;


                    Vector3d vecZ = ImportGrasshopperUtils.CastVectorToRhino(Vector.VecScalMultiply(at.element.localCoordinateSystem.Z.Unitize(), (beam.height/2000)));
                    Vector3d vecY = ImportGrasshopperUtils.CastVectorToRhino(at.element.localCoordinateSystem.Y.Unitize());
                    Vector3d vecX = ImportGrasshopperUtils.CastVectorToRhino(vX.Unitize());


                    //Add cost data
                    joint.template.plates.Add(new Plate("widenerUp", hstif, lstif, tstiff, true));
                    joint.template.plates.Add(new Plate("widenerDown", hstif, lstif, tstiff, true));

                    double weldsize2 = Weld.CalWeldSizeFullStrenth90deg(tstiff, tplate, beam.material, Weld.WeldType.DoubleFillet);
                    joint.template.welds.Add(new Weld("WidEndUp", Weld.WeldType.DoubleFillet, weldsize, hstif));
                    joint.template.welds.Add(new Weld("WidEndDown", Weld.WeldType.DoubleFillet, weldsize, hstif));
                    double weldsizeWeb2 = Weld.CalWeldSizeFullStrenth90deg(tstiff, beam.thicknessFlange, beam.material, Weld.WeldType.DoubleFillet);
                    joint.template.welds.Add(new Weld("WidBeamUp", Weld.WeldType.DoubleFillet, weldsizeWeb, lstif));
                    joint.template.welds.Add(new Weld("WidBeamDown", Weld.WeldType.DoubleFillet, weldsizeWeb, lstif));

                    //upper widener
                    point.Transform(Transform.Translation(vecZ));
                    Plane plane = new Plane(point, vecX, vecZ);

                    Point3d P1 = new Point3d(0, 0, 0);
                    Point3d P2 = new Point3d(0, hstif / 1000, 0);
                    Point3d P3 = new Point3d(lstif/1000,0, 0);
                    IEnumerable<Point3d> points = new Point3d[] { P1, P2,P3, P1 };
                    Polyline poly = new Polyline(points);
                    NurbsCurve nurbsCurve = poly.ToNurbsCurve();

                    Transform transform = Transform.PlaneToPlane(Plane.WorldXY, plane);
                    nurbsCurve.Transform(transform);
                    nurbsCurve.Transform(Transform.Translation(Vector3d.Multiply(-tstiff / 2000, vecY)));
                    nurbsCurve.Transform(Transform.Translation(Vector3d.Multiply(tplate/1000, vecX)));
                    Surface sur = Surface.CreateExtrusion(nurbsCurve, Vector3d.Multiply(tstiff/1000, vecY));
                    Brep plate = sur.ToBrep().CapPlanarHoles(Project.tolerance);
                    breps.Add(plate, pathPlates);

                    //lower widener
                    point.Transform(Transform.Translation(-2 * vecZ));//anders (-2*vecZ)
                    Plane plane2 = new Plane(point, vecX, -vecZ);

                    Point3d P12 = new Point3d(0, 0, 0);
                    Point3d P22 = new Point3d(0, hstif / 1000, 0);
                    Point3d P32 = new Point3d(lstif / 1000, 0, 0);
                    IEnumerable<Point3d> points2 = new Point3d[] { P12, P22, P32, P12 };
                    Polyline poly2 = new Polyline(points2);
                    NurbsCurve nurbsCurve2 = poly2.ToNurbsCurve();

                    Transform transform2 = Transform.PlaneToPlane(Plane.WorldXY, plane2);
                    nurbsCurve2.Transform(transform2);
                    nurbsCurve2.Transform(Transform.Translation(Vector3d.Multiply(-tstiff / 2000, vecY)));
                    nurbsCurve2.Transform(Transform.Translation(Vector3d.Multiply(tplate / 1000, vecX)));
                    Surface sur2 = Surface.CreateExtrusion(nurbsCurve2, Vector3d.Multiply(tstiff/1000, vecY));
                    Brep plate2 = sur2.ToBrep().CapPlanarHoles(Project.tolerance);
                    breps.Add(plate2, pathPlates);
                }
                nosolution:;
            }
            joint.template.workshopOperations = Template.WorkshopOperations.BoltedEndplateOptimizer;
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.TempBoltedEndplateOptimizer2_01;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("5a10321e-750c-4c31-ae29-21b2d09e9750"); }
        }


    }
}
