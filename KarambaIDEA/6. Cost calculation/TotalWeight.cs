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


namespace KarambaIDEA
{
    public class TotalWeight : GH_Component
    {
        public TotalWeight() : base("Total Weight", "Total Weight", "Generate total weight of elements and plates", "KarambaIDEA", "6. Cost calculation")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Weight elements [kg]", "Weight elements [kg]", "Total weight per element", GH_ParamAccess.list);
            pManager.AddNumberParameter("Weight plates [kg]", "Weight plates [kg]", "Total weight of plates per joint", GH_ParamAccess.tree);
            pManager.AddBrepParameter("Brep", "Brep", "", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            Project project = new Project();

            //Link input
            DA.GetData(0, ref project);

            //output variables
            List<double> weightElements = new List<double>();
            DataTree<double> weightPlates = new DataTree<double>();
            weightPlates.Clear();
            List<Brep> breps = new List<Brep>();

            double massSteel = Project.massSteel;

            foreach(Element ele in project.elements)
            {
                double area = ele.crossSection.Area()*Math.Pow(10,-6);//convert mm2 to m2
                double len = ele.brepLine.Length;
                weightElements.Add(area * len * massSteel);

                //TODO: add BREP
                //TODO: add calculation on BREP linelength
                //TODO: test rotation LCS
                //CREATE PLANE
                Vector vY = ele.localCoordinateSystem.Y;
                Vector vZ = ele.localCoordinateSystem.Z;
                Vector vX = ele.localCoordinateSystem.X.Unitize();
                vX = Vector.VecScalMultiply(vX, len);
                Vector3d vecY = ImportGrasshopperUtils.CastVectorToRhino(vY);
                Vector3d vecZ = ImportGrasshopperUtils.CastVectorToRhino(vZ);
                Vector3d vecX = ImportGrasshopperUtils.CastVectorToRhino(vX);

                KarambaIDEA.Core.Point startPoint = ele.brepLine.start;
                CrossSection c = ele.crossSection;
                Point3d point = ImportGrasshopperUtils.CastPointToRhino(startPoint);
                Plane plane = new Plane(point, vecY, vecZ);
                NurbsCurve nurbsCurve = new Polyline().ToNurbsCurve();
                if (c.shape == CrossSection.Shape.ISection)
                {
                    Point3d P1 = new Point3d(-c.width / 2000, c.height / 2000, 0);

                    Point3d P2 = new Point3d(-c.width / 2000, c.height / 2000-c.thicknessFlange/1000, 0);
                    Point3d P3 = new Point3d(-c.thicknessWeb / 2000, c.height / 2000 - c.thicknessFlange / 1000, 0);
                    Point3d P4 = new Point3d(-c.thicknessWeb / 2000, -c.height / 2000 + c.thicknessFlange / 1000, 0);
                    Point3d P5 = new Point3d(-c.width / 2000, -c.height / 2000 + c.thicknessFlange / 1000, 0);

                    Point3d P6 = new Point3d(-c.width / 2000, -c.height / 2000, 0);
                    Point3d P7 = new Point3d(c.width / 2000, -c.height / 2000, 0);

                    Point3d P8 = new Point3d(c.width / 2000, -c.height / 2000 + c.thicknessFlange / 1000, 0);
                    Point3d P9 = new Point3d(c.thicknessWeb / 2000, -c.height / 2000 + c.thicknessFlange / 1000, 0);
                    Point3d P10 = new Point3d(c.thicknessWeb / 2000, c.height / 2000 - c.thicknessFlange / 1000, 0);
                    Point3d P11= new Point3d(c.width / 2000, c.height / 2000 - c.thicknessFlange / 1000, 0);

                    Point3d P12 = new Point3d(c.width / 2000, c.height / 2000, 0);
                    IEnumerable<Point3d> points = new Point3d[] { P1,P2,P3,P4,P5, P6, P7,P8,P9,P10,P11, P12, P1 };
                    Polyline poly = new Polyline(points);
                    nurbsCurve = poly.ToNurbsCurve();
                }

                if (c.shape == CrossSection.Shape.RHSsection)
                {
                    Point3d P1 = new Point3d(-c.width / 2000, c.height / 2000,0);
                    Point3d P6 = new Point3d(-c.width / 2000, -c.height / 2000,0);
                    Point3d P7 = new Point3d(c.width / 2000, -c.height / 2000,0);
                    Point3d P12 = new Point3d(c.width / 2000, c.height / 2000,0);
                    IEnumerable<Point3d> points = new Point3d[] { P1, P6,P7,P12,P1 };
                    Polyline poly = new Polyline(points);
                    nurbsCurve = poly.ToNurbsCurve();
                }

                if (c.shape == CrossSection.Shape.CHSsection)
                {
                    Circle circle = new Circle(plane, c.height / 2000);
                    nurbsCurve = circle.ToNurbsCurve();
                }

                if (c.shape == CrossSection.Shape.Tsection)
                {
                    Point3d P1 = new Point3d(-c.width / 2000, c.height / 2000, 0);

                    Point3d P2 = new Point3d(-c.width / 2000, c.height / 2000 - c.thicknessFlange / 1000, 0);
                    Point3d P3 = new Point3d(-c.thicknessWeb / 2000, c.height / 2000 - c.thicknessFlange / 1000, 0);

                    Point3d P4 = new Point3d(-c.thicknessWeb / 2000, 0, 0);                    
                    Point3d P9 = new Point3d(c.thicknessWeb / 2000, 0, 0);

                    Point3d P10 = new Point3d(c.thicknessWeb / 2000, c.height / 2000 - c.thicknessFlange / 1000, 0);
                    Point3d P11 = new Point3d(c.width / 2000, c.height / 2000 - c.thicknessFlange / 1000, 0);

                    Point3d P12 = new Point3d(c.width / 2000, c.height / 2000, 0);
                    IEnumerable<Point3d> points = new Point3d[] { P1, P2, P3, P4, P9, P10, P11, P12, P1 };
                    Polyline poly = new Polyline(points);
                    nurbsCurve = poly.ToNurbsCurve();
                    //TODO: move centerline T-section to gravity center
                }

                if (c.shape == CrossSection.Shape.Strip)
                {
                    Point3d P1 = new Point3d(-c.width / 2000, c.height / 2000, 0);
                    Point3d P6 = new Point3d(-c.width / 2000, -c.height / 2000, 0);
                    Point3d P7 = new Point3d(c.width / 2000, -c.height / 2000, 0);
                    Point3d P12 = new Point3d(c.width / 2000, c.height / 2000, 0);
                    IEnumerable<Point3d> points = new Point3d[] { P1, P6, P7, P12, P1 };
                    Polyline poly = new Polyline(points);
                    nurbsCurve = poly.ToNurbsCurve();
                }

                Transform transform = Transform.PlaneToPlane(Plane.WorldXY, plane);
                nurbsCurve.Transform(transform);
                Surface sur = Surface.CreateExtrusion(nurbsCurve, vecX);
                breps.Add(sur.ToBrep().CapPlanarHoles(Project.tolerance));

                //CREATE BOUNDING BOX

                Interval interA = new Interval(-c.width / 2000, c.width / 2000);
                Interval interB = new Interval(-c.height / 2000, c.height / 2000);
                Rhino.Geometry.PlaneSurface planeSurface = new PlaneSurface(plane, interA, interB);
                //EXTRUDE SURFACE
                //breps.Add(planeSurface.ToBrep());
                
                //BOUNDING BOX
                Point3d pointAs = new Point3d(-(c.width) / 2000, -(c.height) / 2000, -(c.thicknessFlange) / 2000);
                Point3d pointBs = new Point3d((c.width) / 2000, (c.height) / 2000, (c.thicknessFlange) / 2000);
                BoundingBox bboxstif = new BoundingBox(pointAs, pointBs);
                Box botStiff = new Box(plane, bboxstif);
                //breps.Add(botStiff.ToBrep());



            }
            int a = 0;
            
            foreach (Joint joint in project.joints)
            {
                GH_Path path = new GH_Path(a);
                if (joint.template != null)
                {
                    if (joint.template.plates.Count!=0)
                    {
                        foreach (Plate plate in joint.template.plates)
                        {
                            double volM2 = plate.volume * Math.Pow(10, -9);//convert mm3 to m3
                            double mass = volM2 * massSteel;
                            weightPlates.Add(mass, path);
                        }
                    }
                    else
                    {
                        weightPlates.Add(0.0, path);
                    }
                }
                else
                {
                    weightPlates.Add(0.0, path);
                }
                a = a + 1;
            }

            //link output
            DA.SetDataList(0, weightElements);
            DA.SetDataTree(1, weightPlates);
            DA.SetDataList(2, breps);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.Weight_trans_01;

            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("70edbb4c-869f-4796-811b-d3c809fbe92b"); }
        }
    }
}
