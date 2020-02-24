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
    public class ATemplate_FinplateConnection : GH_Component
    {
        public ATemplate_FinplateConnection() : base("ATemplate_FinplateConnection", "ATemplate_FinplateConnection", "ATemplate_FinplateConnection", "KarambaIDEA", "4. Analytical Templates")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("BrandNames", "BrandNames", "BrandNames to apply template to", GH_ParamAccess.list, "");
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
            Project project = new Project();
            List<GH_String> brandNamesDirty = new List<GH_String>();
            List<string> brandNames = new List<string>();

            //Output variables
            DataTree<string> messages = new DataTree<string>();
            List<Brep> breps = new List<Brep>();

            //Link input
            DA.GetData(0, ref project);
            DA.GetDataList(1, brandNamesDirty);


            //process
            if (brandNamesDirty.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Value)).Count() > 0)
            {
                List<string> brandNamesDirtyString = brandNamesDirty.Select(x => x.Value.ToString()).ToList();
                brandNames = ImportGrasshopperUtils.DeleteEnterCommandsInGHStrings(brandNamesDirtyString);
            }


            if (brandNames.Count != 0)
            {
                foreach (string brandName in brandNames)
                {
                    foreach (Joint joint in project.joints)
                    {
                        if (brandName == joint.brandName)
                        {
                            SetAnaTemplate(joint, breps, messages);
                        }
                    }
                }
            }
            /*
            else
            {
                foreach (Joint joint in project.joints)
                {
                    SetAnaTemplate(joint, breps, messages);
                }
            }
            */


            //link output
            DA.SetData(0, project);
            DA.SetDataTree(1, messages);
            DA.SetDataList(2, breps);
        }

        private static void SetAnaTemplate(Joint joint, List<Brep> breps, DataTree<string> messages)
        {
            joint.template = new Template();
            int a = joint.id;
            int b = 0;
            joint.template.plates.Clear();
            joint.template.welds.Clear();
            joint.template.boltGrids.Clear();

            List<BearingMember> bearlist = joint.attachedMembers.OfType<BearingMember>().ToList();
            BearingMember column = bearlist.First();
            double maxBeamHeight = 0.0;

            foreach (ConnectingMember con in joint.attachedMembers.OfType<ConnectingMember>().ToList())
            {
                GH_Path path = new GH_Path(a, b);
                b++;
                //Step I - retrieve loads
                List<double> vloads = new List<double>();
                foreach (LoadCase lc in joint.project.loadcases)
                {
                    foreach (LoadsPerLine loadsPerLine in lc.loadsPerLines)
                    {
                        if (loadsPerLine.element == con.element)
                        {
                            if (con.isStartPoint == true)
                            {
                                vloads.Add(Math.Abs(loadsPerLine.startLoad.Vz));
                            }
                            else
                            {
                                vloads.Add(Math.Abs(loadsPerLine.endLoad.Vz));
                            }
                        }
                    }
                }
                double vload = vloads.Max() * 1000;
                //Step II - Create bolts list
                List<Bolt> bolts = Bolt.CreateBoltsList(BoltSteelGrade.Steelgrade.b8_8);
                //Step III - determine maximum finplate height
                CrossSection beam = con.element.crossSection;
                double hmax = beam.height - 2 * (beam.thicknessFlange + beam.radius);//Straight portion of the web
                double gap = 20;//TODO: update
                double tplate = Math.Ceiling(beam.thicknessWeb / 5) * 5;
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
                joint.template.plates.Clear();
                joint.template.welds.Clear();
                joint.template.boltGrids.Clear();

            finish:;
                //Step V - Define data for cost analyses
                BearingMember bear = joint.attachedMembers.OfType<BearingMember>().ToList().First();
                double weldsize = Weld.CalWeldSizeFullStrenth90deg(bear.element.crossSection.thicknessFlange, tplate, beam.material, Weld.WeldType.DoubleFillet);
                Weld weld = new Weld("Finplateweld", Weld.WeldType.DoubleFillet, weldsize, hplate);
                joint.template.welds.Add(weld);
                Plate finplate = new Plate("Finplate", hplate, wplate+gap, tplate);
                joint.template.plates.Add(finplate);



                //Step VI - create Brep for visiualisation
                double moveX = (bear.element.crossSection.height) / 2000 +(gap/1000)+ (wplate / 2000);
                Core.Point p = new Core.Point();
                Vector vX = new Vector();
                Vector vY = new Vector();

                if (con.isStartPoint == true)
                {
                    p = (con.element.Line.start);
                    vX = (con.element.Line.Vector);
                    vY = con.element.localCoordinateSystem.Y;
                }
                else
                {
                    p = (con.element.Line.end);
                    vX = (con.element.Line.Vector.FlipVector());
                    vY = con.element.localCoordinateSystem.Y.FlipVector();
                }
                Core.Point pmove = Core.Point.MovePointByVectorandLength(p, vX, moveX);
                //BREP: Finplate
                if (joint.template.boltGrids.Count != 0)
                {
                    //Create plate
                    double tol = 0.001;
                    Point3d pointA = new Point3d((-hplate) / 2000, (-wplate) / 2000-(gap/1000), 0);
                    Point3d pointB = new Point3d((hplate) / 2000, (wplate) / 2000, (tplate) / 1000);
                    BoundingBox bbox = new BoundingBox(pointA, pointB);
                    Point3d point = ImportGrasshopperUtils.CastPointToRhino(pmove);
                    Vector3d vector = ImportGrasshopperUtils.CastVectorToRhino(vY);
                    Vector3d vecZ = ImportGrasshopperUtils.CastVectorToRhino(con.element.localCoordinateSystem.Z);
                    Vector3d vecX = ImportGrasshopperUtils.CastVectorToRhino(vX);
                    //Plane plane = new Plane(point, vector);
                    Plane plane = new Plane(point, vecZ,vecX);
                    Box box = new Box(plane, bbox);
                    Brep plate = box.ToBrep();

                    //Create Holes
                    Brep tubes = new Brep();
                    double rows = joint.template.boltGrids.FirstOrDefault().rows;
                    double p1 = joint.template.boltGrids.FirstOrDefault().p1;
                    for (int ba = 0; ba < rows; ba++)
                    {
                        double d0 = joint.template.boltGrids.FirstOrDefault().bolttype.HoleDiameter;
                        double topmm = (0.5 * (rows - 1) * (p1 + d0) / 1000) - (((p1 + d0) * ba) / 1000);
                        Vector3d locX = plane.XAxis;
                        locX.Unitize();
                        Transform transform = Transform.Translation(Vector3d.Multiply(topmm, locX));
                        Plane plane2 = plane;
                        plane2.Transform(transform);
                        Circle circle = new Circle(plane2, d0 / 2000);//Radius 
                        Surface sur = Surface.CreateExtrusion(circle.ToNurbsCurve(), vector);
                        Brep tube = sur.ToBrep().CapPlanarHoles(tol);
                        plate = Brep.CreateBooleanDifference(plate, tube, tol).ToList().First();

                    }
                    //Add plate with holes
                    breps.Add(plate);

                }

                //Step VII - modify beam brepline
                KarambaIDEA.Core.Line.ModifyBeamBrepLine(column, con, gap);
                maxBeamHeight = Math.Max(maxBeamHeight, beam.height);
            }

            //Step VIII - modify column brepline
            KarambaIDEA.Core.Line.ModifyColumnBrepLine(bearlist, maxBeamHeight);

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

                return Properties.Resources.ATempFinPlate;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("5be9ade6-d460-42a1-b0f5-4d5d1df770a6"); }
        }



    }
}
