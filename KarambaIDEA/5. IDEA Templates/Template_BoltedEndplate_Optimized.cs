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

namespace KarambaIDEA._5._IDEA_Templates
{
    public class Template_BoltedEndplate_Optimized : GH_Component
    {
        public Template_BoltedEndplate_Optimized() : base("Template: Bolted endplate connection", "Template: Bolted endplate connection", "Template: Bolted endplate connection", "KarambaIDEA", "5. IDEA Templates")
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
            pManager.AddBrepParameter("Brep", "Brep", "", GH_ParamAccess.list);
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
            List<Brep> breps = new List<Brep>();

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
            /*
            else
            {
                foreach (Joint joint in project.joints)
                {
                    SetTemplate(tplate, joint, breps);
                }
            }
            */

            //messages = project.MakeTemplateJointMessage();

            //link output
            DA.SetData(0, project);
            DA.SetDataTree(1, messages);
            DA.SetDataList(2, breps);
        }

        private static void SetTemplate(double tplate, Joint joint, List<Brep> breps, DataTree<string> messages)
        {
            joint.template = new Template();
            int a = joint.id;
            int b = 0;

            Core.CrossSection beam = joint.attachedMembers.First().element.crossSection;
            Plate plateA = new Plate("endplateA", beam.height, beam.width, tplate);
            Plate plateB = new Plate("endplateB", beam.height, beam.width, tplate);

            joint.template.plates.Add(plateA);
            joint.template.plates.Add(plateB);

            joint.template.workshopOperations = Template.WorkshopOperations.BoltedEndPlateConnection;
            foreach (AttachedMember at in joint.attachedMembers)
            {
                GH_Path path = new GH_Path(a, b);
                b++;
                //Step I - retrieve loads
                List<double> vloads = new List<double>();
                foreach (LoadCase lc in joint.project.loadcases)
                {
                    foreach (LoadsPerLine loadsPerLine in lc.loadsPerLines)
                    {
                        if (loadsPerLine.element == at.element)
                        {
                            if (at.isStartPoint == true)
                            {
                                vloads.Add(Math.Abs(loadsPerLine.startLoad.N));
                            }
                            else
                            {
                                vloads.Add(Math.Abs(loadsPerLine.endLoad.N));
                            }
                        }
                    }
                }
                double vload = vloads.Max() * 1000;
                //Step II - Create bolts list
                List<Bolt> bolts = Bolt.CreateBoltsList(BoltSteelGrade.Steelgrade.b8_8);
                //Step III - determine maximum finplate height
                double hmax = beam.height - 2 * (beam.thicknessFlange + beam.radius);//Straight portion of the web
                double gap = 20;//TODO: update
                //double tplate = Math.Ceiling(beam.thicknessWeb / 5) * 5;
                double wplate = new double();
                double hplate = new double();


                //Step IV - evaluation of different bolt configurations
                int it = 0;
                foreach (Bolt bolt in bolts)
                {
                    double d0 = bolt.HoleDiameter;

                    double e1 = 1.2 * d0;//Edge distance longitudinal direction
                    double p1 = 2.2 * d0;//Inner distance longitudinal direction
                    double e2 = 1.2 * d0;//Edge distance transversal direction
                    double p2 = 2.2 * d0;//single row, so actually not needed

                    wplate = d0 + 2 * e2;

                    for (int n = 2; n < 5; n++)
                    {
                    retry:;
                        double hmin = e1 * 2 + d0 * n + p1 * (n - 1);

                        if (hmax > hmin)//if configuration fits within the beam
                        {
                            hplate = hmin;
                            it = it + 1;
                            string info = n + "x " + bolt.Name + " H=" + hplate + " mm";

                            //Check Bolts in shear
                            double vRdShearBolts = n * bolt.ShearResistance();
                            if (vload > vRdShearBolts)//If Ved>Vrd
                            {
                                messages.Add(info + ": Bolts fail in shear", path);
                                goto increaseN;
                            }
                            //Check Plate in bearing
                            double tweb = beam.thicknessWeb;
                            MaterialSteel mat = beam.material;

                            double Nrd1 = bolt.TensionResistance();
                            


                            double Vrd1 = bolt.BearingRestance(true, true, bolt, tweb, mat, e1, p1, e2, p2);//edge bolt
                            if (vload > Vrd1)
                            {
                                messages.Add(info + ": Beamweb fails in bearing (edge bolt)", path);
                                e1 = e1 * 1.5;//
                                //goto retry;
                                goto increaseN;
                            }
                            double Vrd2 = bolt.BearingRestance(false, true, bolt, tweb, mat, e1, p1, e2, p2);//inner bolt
                            if (vload > Vrd2)
                            {
                                messages.Add(info + ": Beamweb fails in bearing (inner bolt)", path);
                                p1 = p1 * 1.5;
                                goto increaseN;
                            }
                            //Check beamWeb in shear
                            double vRdShearWeb = (tweb * (hmin - n * d0) * (beam.material.Fy / Math.Sqrt(3))) / 1.0;
                            if (vload > vRdShearWeb)
                            {
                                messages.Add(info + ": Beamweb fails in shear", path);
                                goto increaseN;
                            }
                            //Check Plate in bending         
                            double leverarm = gap + e2 + 0.5 * d0;
                            double Med = leverarm * vload;
                            double MRd = (tplate * Math.Pow(hmin, 2) * mat.Fy) / 6;
                            if (Med > MRd)
                            {
                                messages.Add(info + ": Finplate fails in bending", path);
                                //if this check fails it will be a dead end
                                //TODO: redesign height plate
                                //double hbending = Math.Sqrt(Med / ((1 / 6) * b * mat.Fy));

                                if (hplate + 100 < hmax)
                                {
                                    e1 = e1 + 50;
                                    goto retry;
                                }
                                else
                                {
                                    //messages.Add(info + ": Finplate fails in bending", path);
                                }


                            }

                            BoltGrid boltGrid = new BoltGrid(bolt, n, 1, e1, e2, p1, p2);
                            joint.template.boltGrids.Add(boltGrid);
                            //TODO:include message
                            //3x M20 is chosen after x iterions
                            messages.Add(info + " is chosen after " + it + " iterations.", path);
                            goto finish;
                        }
                        else
                        {
                            //no else commands
                        }
                    increaseN:;
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

                
                BearingMember bear = joint.attachedMembers.OfType<BearingMember>().ToList().First();
                double weldsize = Weld.CalWeldSizeFullStrenth90deg(bear.element.crossSection.thicknessFlange, tplate, beam.material, Weld.WeldType.DoubleFillet);
                Weld weld = new Weld("Finplateweld", Weld.WeldType.DoubleFillet, weldsize, hplate);
                joint.template.welds.Add(weld);
                Plate finplate = new Plate("Finplate", hplate, wplate + gap, tplate);
                joint.template.plates.Add(finplate);

                weldsize = Weld.CalWeldSizeFullStrenth90deg(tplate, beam.thicknessFlange, beam.material, Weld.WeldType.DoubleFillet);
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

                    Plane plane = new Plane(point, vecZ, vecY);

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
                    Brep tubes = new Brep();

                    double d0 = 24;
                    double topmm = 80;

                    double tol = 0.001;
                    Vector3d locX = plane.XAxis;
                    locX.Unitize();
                    Transform transform2 = Transform.Translation(Vector3d.Multiply(topmm / 1000, locX));
                    Plane plane2 = plane;
                    plane2.Transform(transform2);




                    Circle circle = new Circle(plane2, d0 / 2000);//Radius 
                    Surface sur2 = Surface.CreateExtrusion(circle.ToNurbsCurve(), vecX);
                    Brep tube = sur2.ToBrep().CapPlanarHoles(tol);

                    //List<Brep> breps8 = Brep.CreateBooleanDifference(plate, tube, tol).ToList();
                    //plate = breps8.FirstOrDefault();

                    breps.Add(tube);
                    breps.Add(plate);
                }
            }
            nosolution:;

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
