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

namespace KarambaIDEA
{
    public class Template_BoltedEndPlateConnection : GH_Component
    {
        public Template_BoltedEndPlateConnection() : base("Template: Bolted endplate connection", "Template: Bolted endplate connection", "Template: Bolted endplate connection", "KarambaIDEA", "5. IDEA Templates")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("BrandNames", "BrandNames", "BrandNames to apply template to", GH_ParamAccess.list,"");
            pManager.AddNumberParameter("Thickness endplate [mm]", "Thickness endplate [mm]", "", GH_ParamAccess.item, 10.0);
            pManager[1].Optional = true;
            pManager.AddNumberParameter("Edge distance [mm]", "Edge distance [mm]", "", GH_ParamAccess.item, 30);
            pManager.AddTextParameter("Bolttype", "Bolttype", "", GH_ParamAccess.item, "M20");
            pManager.AddTextParameter("Boltsteelgrade", "Boltsteelgrade", "", GH_ParamAccess.item, "8.8");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("Message", "Message", "", GH_ParamAccess.list);
            pManager.AddBrepParameter("Brep", "Brep", "", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables      
            Project sourceProject = new Project();
            double tplate = new double();
            List<GH_String> brandNamesDirty = new List<GH_String>();
            List<string> brandNames = new List<string>();

            string boltsteelgrade = "10.9";
            string bolttypename = "M30";
            double edgedistance = 50;

            //Output variables
            List<string> messages = new List<string>();
            DataTree<Brep> breps = new DataTree<Brep>();

            //Link input
            DA.GetData(0, ref sourceProject);
            DA.GetDataList(1, brandNamesDirty);
            DA.GetData(2, ref tplate);
            DA.GetData("Edge distance [mm]", ref edgedistance);
            DA.GetData("Bolttype", ref bolttypename);
            DA.GetData("Boltsteelgrade", ref boltsteelgrade);

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
                    foreach(Joint joint in project.joints)
                    {
                        if (brandName == joint.brandName)
                        {
                            SetTemplate(tplate, joint, breps, edgedistance,bolttypename, boltsteelgrade);
                        }
                    }
                }
            }
            /*
            else
            {
                foreach (Joint joint in project.joints)
                {
                    SetTemplate(tplate, joint, breps);
                }
            }
            */

            messages = project.MakeTemplateJointMessage();            

            //link output
            DA.SetData(0, project);
            DA.SetDataList(1, messages);
            DA.SetDataTree(2, breps);
        }

        private static void SetTemplate(double tplate, Joint joint, DataTree<Brep> breps, double edgedistance, string bolttypename, string boltsteelgrade)
        {
            joint.template = new Template();
            int jointid = joint.id;
            int b = 0;

            Core.CrossSection beam = joint.attachedMembers.First().element.crossSection;
            Plate plateA = new Plate("endplateA", beam.height, beam.width, tplate);
            Plate plateB = new Plate("endplateB", beam.height, beam.width, tplate);
            
            joint.template.plates.Add(plateA);
            joint.template.plates.Add(plateB);

            //Step I - no loads retrieved
            //Step II - create boltgrid
            //BoltSteelGrade bsg = new BoltSteelGrade.Steelgrade();

            //BoltSteelGrade bsg = BoltSteelGrade.Steelgrade.b8_8;

            List<Bolt> bolts = Bolt.CreateBoltsList(BoltSteelGrade.selectgrade(boltsteelgrade));
            Bolt bolt = bolts.Single(a => bolttypename == a.Name);
            double locXp =  0.5 * beam.width - edgedistance;
            double locY = 0.5 * beam.height - edgedistance;

            List<Coordinate2D> coors = new List<Coordinate2D>();
            coors.Add(new Coordinate2D(locXp, locY));
            coors.Add(new Coordinate2D(locXp, -locY));
            coors.Add(new Coordinate2D(-locXp, locY));
            coors.Add(new Coordinate2D(-locXp, -locY));
            BoltGrid boltGrid = new BoltGrid(bolt, coors);
            joint.template.boltGrids.Add(boltGrid);

            joint.template.workshopOperations = Template.WorkshopOperations.BoltedEndPlateConnection;
            foreach(AttachedMember at in joint.attachedMembers)
            {
                GH_Path path = new GH_Path(jointid, b);
                GH_Path pathPlates = new GH_Path(jointid, b, 0);
                GH_Path pathBolts = new GH_Path(jointid, b, 1);
                b++;

                //Step V - Define data for cost analyses
                double weldsize = Weld.CalWeldSizeFullStrenth90deg(tplate, beam.thicknessFlange, beam.material, Weld.WeldType.DoubleFillet);
                joint.template.welds.Add(new Weld("FlangeweldTop", Weld.WeldType.DoubleFillet, weldsize, beam.width));
                joint.template.welds.Add(new Weld("FlangeweldBot", Weld.WeldType.DoubleFillet, weldsize, beam.width));
                double weldsizeWeb = Weld.CalWeldSizeFullStrenth90deg(tplate, beam.thicknessWeb, beam.material, Weld.WeldType.DoubleFillet);
                joint.template.welds.Add(new Weld("Webweld", Weld.WeldType.DoubleFillet, weldsizeWeb, beam.height));



                //Step VI - BREP beam
                Point3d point = new Point3d();
                Vector vX = new Vector();
                if (at.isStartPoint == true)
                {
                    at.element.brepLine.start = KarambaIDEA.Core.Line.ExtendLine(at.element.Line, -tplate / 1000, true);
                    point = ImportGrasshopperUtils.CastPointToRhino(at.element.Line.start);
                    vX =(at.element.localCoordinateSystem.X);
                }
                else
                {
                    at.element.brepLine.end = KarambaIDEA.Core.Line.ExtendLine(at.element.Line, -tplate / 1000, false);
                    point = ImportGrasshopperUtils.CastPointToRhino(at.element.Line.end);
                    vX = (at.element.localCoordinateSystem.X.FlipVector());
                }

                //Step VII - BREP add plate with holes
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
                    /*
                    Brep tubes = new Brep();

                    double d0 = 24;
                    double topmm = 80;

                    double tol = 0.001;
                    Vector3d locX = plane.XAxis;
                    locX.Unitize();
                    Transform transform2 = Transform.Translation(Vector3d.Multiply(topmm/1000, locX));
                    Plane plane2 = plane;
                    plane2.Transform(transform2);
                    Circle circle = new Circle(plane2, d0 / 2000);//Radius 
                    Surface sur2 = Surface.CreateExtrusion(circle.ToNurbsCurve(), 2*vecX);
                    Brep tube = sur2.ToBrep().CapPlanarHoles(tol);

                    plate = Brep.CreateBooleanDifference(plate, tube, tol).ToList().FirstOrDefault();
                    */
                    //Create Holes
                    BoltGrid bg = joint.template.boltGrids.FirstOrDefault();
                    foreach (Coordinate2D coor in bg.Coordinates2D)
                    {
                        Brep tubes = new Brep();

                        double d0 = bg.bolttype.HeadDiagonalDiameter;
                        double h0 = tplate + bg.bolttype.HeadHeight;
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

                        Circle circle = new Circle(plane3, d0 / 2000);//Radius 

                        Polyline hexagon = Polyline.CreateCircumscribedPolygon(circle, 6);
                        vecX.Unitize();
                        Surface sur2 = Surface.CreateExtrusion(hexagon.ToNurbsCurve(), Vector3d.Multiply(h0 / 1000, vecX));

                        Brep tube = sur2.ToBrep().CapPlanarHoles(tol);
                        breps.Add(tube, pathBolts);
                        //plate = Brep.CreateBooleanDifference(plate, tube, tol).ToList().FirstOrDefault();
                    }



                    //breps.Add(tube);
                    breps.Add(plate, pathPlates);
                }
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
            get { return new Guid("c87e4243-ed21-492f-9d25-a599454de06f"); }
        }


    }
}
