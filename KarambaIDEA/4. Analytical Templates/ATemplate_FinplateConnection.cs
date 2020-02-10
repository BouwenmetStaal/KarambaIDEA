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
            pManager.AddTextParameter("Message", "Message", "", GH_ParamAccess.list);
            pManager.AddBrepParameter("Brep", "Brep", "", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables      
            Project project = new Project();
            List<GH_String> brandNamesDirty = new List<GH_String>();
            List<string> brandNames = new List<string>();

            //Output variables
            List<string> messages = new List<string>();
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
                            SetAnaTemplate(joint, breps);
                        }
                    }
                }
            }
            else
            {
                foreach (Joint joint in project.joints)
                {
                    SetAnaTemplate(joint, breps);
                }
            }


            //link output
            DA.SetData(0, project);
            DA.SetDataList(1, messages);
            DA.SetDataList(2, breps);
        }

        private static void SetAnaTemplate(Joint joint, List<Brep> breps)
        {
            joint.template = new Template();
            foreach(ConnectingMember con in joint.attachedMembers.OfType<ConnectingMember>().ToList())
            {
                
                //Step I - retrieve loads
                List<double> vloads = new List<double>();
                foreach(LoadCase lc in joint.project.loadcases)
                {
                    foreach(LoadsPerLine loadsPerLine in lc.loadsPerLines)
                    {
                        if(loadsPerLine.element == con.element)
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
                double vload = vloads.Max()*1000;
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
                foreach (Bolt bolt in bolts)
                {
                    double d0 = bolt.HoleDiameter;

                    double e1 = 1.2 * d0;//Edge distance longitudinal direction
                    double p1 = 2.2 * d0;//Inner distance longitudinal direction
                    double e2 = 1.2 * d0;//Edge distance transversal direction
                    double p2 = 2.2 * d0;//single row, so actually not needed

                    wplate = gap +d0+ 2 * e2;

                    for (int n = 2; n < 4; n++)
                    {
                        if (hmax > e1 * 2 + d0 * n + p1 * (n - 1))//if configuration fits within the beam
                        {
                            //Check Bolts in shear
                            double vRd = n * bolt.ShearResistance();
                            if (vload > vRd)//If Ved>Vrd
                            {
                                goto increaseN;
                            }
                            //Check Plate in bearing
                            double tweb = beam.thicknessWeb;
                            MaterialSteel mat = beam.material;
                            double Vrd1= bolt.BearingRestance(true, true, bolt,tweb, mat, e1, p1, e2, p2);//edge bolt
                            double Vrd2 = bolt.BearingRestance(false, true, bolt, tweb, mat, e1, p1, e2, p2);//inner bolt
                            if (vload > Math.Min(Vrd1, Vrd2))
                            {
                                goto increaseN;
                            }
                            //Check beamWeb in shear
                            if (vload > (tweb * (hmax - n * d0) * (beam.material.Fy / Math.Sqrt(3) ))/ 1.0)
                            {
                                goto increaseN;
                            }
                            //Check Plate in bending         
                            double leverarm = gap + e2 + 0.5 * d0;
                            double Med = leverarm * vload;
                            if (Med > (1 / 6) * tplate * Math.Pow(hmax, 2))
                            {
                                //if this check fails it will be a dead end
                                //TODO: include message
                            }
                            hplate = e1 * 2 + d0 * n + p1 * (n - 1);
                            BoltGrid boltGrid = new BoltGrid(bolt, n, 1);
                            joint.template.boltGrids.Add(boltGrid);
                            //TODO:include message
                            //3x M20 is chosen after x iterions
                            goto finish;
                        }

                    increaseN:;
                    }
                }
                finish:;
                //Step V - Define data for cost analyses
                BearingMember bear = joint.attachedMembers.OfType<BearingMember>().ToList().First();
                double weldsize = Weld.CalWeldSizeFullStrenth90deg(bear.element.crossSection.thicknessFlange, tplate, beam.material, Weld.WeldType.DoubleFillet);
                Weld weld = new Weld("Finplateweld", Weld.WeldType.DoubleFillet, weldsize, hplate);
                joint.template.welds.Add(weld);
                Plate finplate = new Plate("Finplate", hplate, wplate, tplate);
                joint.template.plates.Add(finplate);
                //Step VI - create Brep for visiualisation
                double moveX = (bear.element.crossSection.height) / 2000;
                Core.Point p = new Core.Point();
                Vector vX = new Vector();
                Vector vY = con.element.localCoordinateSystem.Y;
                double sign = 1;

                if (con.isStartPoint == true)
                {
                    p = (con.element.line.start);
                    vX = (con.element.line.Vector);
                }
                else
                {
                    p = (con.element.line.end);
                    vX = (con.element.line.Vector.FlipVector());
                    sign = -1;
                }
                Core.Point pmove = Core.Point.MovePointByVectorandLength(p, vX, moveX);
                //BREP: Finplate
                Point3d pointA = new Point3d( -(hplate) / 2000, 0, 0);
                Point3d pointB = new Point3d( (hplate) / 2000, sign*(wplate) / 1000, (tplate) / 1000);
                BoundingBox bbox = new BoundingBox(pointA, pointB);
                Point3d point = ImportGrasshopperUtils.CastPointToRhino(pmove);
                Vector3d vector = ImportGrasshopperUtils.CastVectorToRhino(vY);
                Plane plane = new Plane(point, vector);
                Box box = new Box(plane, bbox);
                breps.Add(box.ToBrep());

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

                return Properties.Resources.ATempFinPlate;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("5be9ade6-d460-42a1-b0f5-4d5d1df770a6"); }
        }



    }
}
