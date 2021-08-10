// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Linq;
using System.Collections.Generic;

using Rhino.Geometry;
using Rhino;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using KarambaIDEA.Core;
using KarambaIDEA.IDEA;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

namespace KarambaIDEA
{
    public class ATemplate_MomentResistingConnection : GH_Component
    {
        public ATemplate_MomentResistingConnection() : base("Analytical Template: Moment resisting connection", "AT: MRC", "Analytical Template: Moment resisting connection", "KarambaIDEA", "4. Analytical Templates")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("BrandNames", "BrandNames", "BrandNames to apply template to", GH_ParamAccess.list, "");
            pManager.AddNumberParameter("Height haunch [mm]", "Height haunch [mm]", "Heigt of haunch in mm, length of haunch is defined bij a ratio of 1:2", GH_ParamAccess.item, 0.0);
            pManager.AddBooleanParameter("Stiffeners?", "Stiffeners?", "Does the connection include stiffeners?", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Unbraced structure?", "Unbraced structure?", "If true joint classification will be assesed accoring to unbraced structures specifications, if false for braced structures.", GH_ParamAccess.item, false);
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
            Project sourceProject = new Project();
            double heightHaunch = new double();
            List<GH_String> brandNamesDirty = new List<GH_String>();
            List<string> brandNames = new List<string>();
            bool stiffeners = new bool();
            bool unbraced = new bool();

            //Output variables
            List<string> messages = new List<string>();
            List<Brep> breps = new List<Brep>();

            //Link input
            DA.GetData(0, ref sourceProject);
            DA.GetDataList(1, brandNamesDirty);
            DA.GetData(2, ref heightHaunch);
            DA.GetData(3, ref stiffeners);
            DA.GetData(4, ref unbraced);

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
            if (brandNames.Count != 0)
            {
                foreach (string brandName in brandNames)
                {
                    foreach (Joint joint in project.joints)
                    {
                        if (brandName == joint.brandName)
                        {


                            SetMomentResitingConnection(joint, heightHaunch, stiffeners, unbraced, breps);

                            if (joint.attachedMembers.OfType<ConnectingMember>().ToList().Count == 1)
                            {
                                
                            }
                            else
                            {
                                //more than one connectingmembers in connection
                                //TODO: include warning
                            }
                        }
                    }
                }
            }
            /*
            else
            {
                foreach (Joint joint in project.joints)
                {
                    if (joint.attachedMembers.OfType<ConnectingMember>().ToList().Count == 1)
                    {
                        SetMomentResitingConnection(joint, heightHaunch, stiffeners, unbraced, breps);
                    }
                    else
                    {
                        //more than one connectingmembers in connection
                        //TODO: include warning
                    }
                }
            }
            */
            
            //messages = project.MakeTemplateJointMessage();

           

            //link output
            DA.SetData(0, project);
            DA.SetDataList(1, messages);
            DA.SetDataList(2, breps);
        }
        private static void SetMomentResitingConnection(Joint joint, double heightHaunch, bool stiffeners, bool unbraced, List<Brep> breps)
        {
            //ConnectingMember con = joint.attachedMembers.OfType<ConnectingMember>().ToList().First();
            List<BearingMember> bearlist = joint.attachedMembers.OfType<BearingMember>().ToList();
            BearingMember bear = joint.attachedMembers.OfType<BearingMember>().ToList().First();
            double maxBeamHeight = 0.0;

            foreach (ConnectingMember con in joint.attachedMembers.OfType<ConnectingMember>().ToList())
            {
                int k = 25;
                if (unbraced == true) { k = 8; }
                double kf = 13;
                if (stiffeners == true) { kf = 8.5; }
                int ks = 5;
                //Calculate Sj  
                double z = heightHaunch + con.element.crossSection.height;
                double E = Project.EmodulusSteel;
                double Sj = ((E * Math.Pow(z, 2) * bear.element.crossSection.thicknessFlange) / kf) * Math.Pow(10, -6);
                //Calculate Mj,Rd,approx
                double fy = con.element.crossSection.material.Fy;
                double yM0 = 1.0;
                double MjRd = ((ks * fy * z * Math.Pow(bear.element.crossSection.thicknessFlange, 2)) / yM0) * Math.Pow(10, -6);

                ConnectionProperties conProp = new ConnectionProperties(con.element, Sj, MjRd, k);

                if (con.isStartPoint == true)
                {
                    con.element.startProperties = conProp;
                }
                else
                {
                    con.element.endProperties = conProp;
                }

                //DESIGN RULES
                //tstiffener = tbeam,flange
                //tendplate = tcolumn,flange
                //tweb,haunch = tweb,beam
                //tflange,haunch= tflange,beam
                double haunchRatio = 2.0;
                //Generate plates and welds for cost analyses
                CrossSection column = bear.element.crossSection;
                CrossSection beam = con.element.crossSection;
                MaterialSteel mat = column.material;

                double overSize = 50;//oversize needed for top row bolts 
                double tendplate = column.thicknessFlange;

                joint.template = new Template();

                //add plate
                joint.template.plates.Add(new Plate("Endplate", z + overSize, beam.width, column.thicknessFlange));
                //add welds of beam
                double weldsizeB1 = Weld.CalWeldSizeFullStrenth90deg(tendplate, beam.thicknessFlange, mat, Weld.WeldType.DoubleFillet);
                joint.template.welds.Add(new Weld("TopflangeBeam_endplate", Weld.WeldType.DoubleFillet, weldsizeB1, beam.width));
                double weldsizeB2 = Weld.CalWeldSizeFullStrenth90deg(tendplate, beam.thicknessWeb, mat, Weld.WeldType.DoubleFillet);
                joint.template.welds.Add(new Weld("BottomflangeBeam_endplate", Weld.WeldType.DoubleFillet, weldsizeB2, beam.width));
                double weldsizeB3 = Weld.CalWeldSizeFullStrenth90deg(tendplate, beam.thicknessWeb, mat, Weld.WeldType.DoubleFillet);
                joint.template.welds.Add(new Weld("WebBeam_endplate", Weld.WeldType.DoubleFillet, weldsizeB3, beam.height - (2 * beam.thicknessFlange)));

                if (heightHaunch != 0.0)
                {
                    //haunch dimensions

                    double haunchLength = heightHaunch * haunchRatio;
                    double lenHaunchFlange = Math.Sqrt(Math.Pow(heightHaunch, 2) + Math.Pow(haunchRatio * heightHaunch, 2));
                    //add plates of haunch
                    joint.template.plates.Add(new Plate("HaunchWeb", haunchLength, column.width, beam.thicknessWeb, true));
                    joint.template.plates.Add(new Plate("HaunchFlange", lenHaunchFlange, column.width, beam.thicknessFlange));
                    //add welds of haunch
                    double weldsize1 = Weld.CalWeldSizeFullStrenth90deg(beam.thicknessFlange, beam.thicknessWeb, mat, Weld.WeldType.DoubleFillet);
                    joint.template.welds.Add(new Weld("Haunchweb_Bottomflange", Weld.WeldType.DoubleFillet, weldsize1, haunchLength));
                    double weldsize5 = Weld.CalWeldSizeFullStrenth90deg(column.thicknessFlange, beam.thicknessWeb, mat, Weld.WeldType.DoubleFillet);
                    joint.template.welds.Add(new Weld("Haunchweb_ColumnFlange", Weld.WeldType.DoubleFillet, weldsize5, heightHaunch));
                    double weldsize2 = Weld.CalWeldSizeFullStrenth90deg(beam.thicknessFlange, beam.thicknessWeb, mat, Weld.WeldType.DoubleFillet);
                    joint.template.welds.Add(new Weld("Haunchweb_Haunchflange", Weld.WeldType.DoubleFillet, weldsize2, lenHaunchFlange));
                    double weldsize3 = Weld.CalWeldSizeFullStrenth90deg(beam.thicknessFlange, column.thicknessFlange, mat, Weld.WeldType.Fillet);
                    joint.template.welds.Add(new Weld("Haunchflange_Beamflange", Weld.WeldType.Fillet, weldsize3, beam.width));
                    double weldsize4 = Weld.CalWeldSizeFullStrenth90deg(beam.thicknessFlange, beam.thicknessFlange, mat, Weld.WeldType.Fillet);
                    joint.template.welds.Add(new Weld("Haunchflange_Endplate", Weld.WeldType.Fillet, weldsize4, beam.width));
                }

                //Add stiffeners if true
                if (stiffeners == true)
                {
                    double len = bear.element.crossSection.height - (2 * bear.element.crossSection.thicknessFlange);
                    double wid = (bear.element.crossSection.width - bear.element.crossSection.thicknessWeb) / 2;
                    double tstiffener = con.element.crossSection.thicknessFlange;
                    for (int i = 1; i <= 4; i++)
                    {
                        //add plates 
                        joint.template.plates.Add(new Plate("Stiffener" + i, len, wid, tstiffener));
                        //add welds
                        double weldsizeS1 = Weld.CalWeldSizeFullStrenth90deg(tstiffener, column.thicknessWeb, mat, Weld.WeldType.DoubleFillet);
                        joint.template.welds.Add(new Weld("Stiffener_Beamweb", Weld.WeldType.DoubleFillet, weldsizeS1, column.height - (2 * column.thicknessFlange)));
                        for (int b = 1; b <= 2; b++)
                        {
                            double weldsizeS2 = Weld.CalWeldSizeFullStrenth90deg(beam.thicknessFlange, beam.thicknessFlange, mat, Weld.WeldType.DoubleFillet);
                            joint.template.welds.Add(new Weld("Stiffener_Beamflange", Weld.WeldType.DoubleFillet, weldsizeS2, (column.width - column.thicknessWeb) / 2));
                        }
                    }
                }

                //Brep
                double moveX = (bear.element.crossSection.height) / 2000;
                Core.Point p = new Core.Point();
                Vector vX = new Vector();
                if (con.isStartPoint == true)
                {
                    p = (con.element.Line.start);
                    vX = (con.element.Line.Vector);
                }
                else
                {
                    
                    p = (con.element.Line.end);
                    vX = (con.element.Line.Vector.FlipVector());
                }
                Core.Point pmove = Core.Point.MovePointByVectorandLength(p, vX, moveX);
                //BREP: endplate
                Point3d pointA = new Point3d(-(beam.width) / 2000, -(beam.height) / 2000 - (heightHaunch / 1000), 0);
                Point3d pointB = new Point3d((beam.width) / 2000, (beam.height) / 2000, (column.thicknessFlange) / 1000);
                BoundingBox bbox = new BoundingBox(pointA, pointB);
                Point3d point = ImportGrasshopperUtils.CastPointToRhino(pmove);
                Vector3d vector = ImportGrasshopperUtils.CastVectorToRhino(vX);
                Plane plane = new Plane(point, vector);
                Box box = new Box(plane, bbox);
                breps.Add(box.ToBrep());
                //BREP: haunchflange
                Vector vZ = con.element.localCoordinateSystem.Z.FlipVector();
                Core.Point p1 = Core.Point.MovePointByVectorandLength(pmove, vZ, beam.height / 2000);
                Core.Point p2 = Core.Point.MovePointByVectorandLength(pmove, vZ, (beam.height / 2000) + (heightHaunch / 1000));
                Core.Point p3 = Core.Point.MovePointByVectorandLength(p1, vX, (heightHaunch * haunchRatio) / 1000);
                Vector vy = con.element.localCoordinateSystem.Y.Unitize();
                if (heightHaunch != 0.0)
                {
                    //create perimeter
                    Point3d hp1 = ImportGrasshopperUtils.CastPointToRhino(Core.Point.MovePointByVectorandLength(p2, vy, -beam.width / 2000));
                    Point3d hp2 = ImportGrasshopperUtils.CastPointToRhino(Core.Point.MovePointByVectorandLength(p3, vy, -beam.width / 2000));
                    Point3d hp3 = ImportGrasshopperUtils.CastPointToRhino(Core.Point.MovePointByVectorandLength(p3, vy, +beam.width / 2000));
                    Point3d hp4 = ImportGrasshopperUtils.CastPointToRhino(Core.Point.MovePointByVectorandLength(p2, vy, +beam.width / 2000));
                    List<Point3d> hwp = new List<Point3d>() { hp1, hp2, hp3, hp4, hp1 };
                    Polyline poly = new Polyline(hwp);

                    //Get plane normal
                    Rhino.Geometry.LineCurve line = new Rhino.Geometry.LineCurve(ImportGrasshopperUtils.CastPointToRhino(p2), ImportGrasshopperUtils.CastPointToRhino(p3));
                    Vector vYmove = Vector.VecScalMultiply(con.element.localCoordinateSystem.Y.FlipVector().Unitize(), beam.width / 2000);
                    Transform transform2 = Transform.Translation(ImportGrasshopperUtils.CastVectorToRhino(vYmove));
                    line.Transform(transform2);
                    Vector vYwidth = Vector.VecScalMultiply(con.element.localCoordinateSystem.Y.Unitize(), beam.width / 1000);
                    Surface sur2 = Surface.CreateExtrusion(line, ImportGrasshopperUtils.CastVectorToRhino(vYwidth));

                    Vector3d normal = sur2.NormalAt(0, 0);
                    normal.Unitize();
                    Vector3d vyMove = Vector3d.Multiply(-beam.thicknessFlange / 2000, normal);   
                    Vector3d vyExtrude = Vector3d.Multiply(beam.thicknessFlange / 1000, normal);
                    Transform transform = Transform.Translation(vyMove);
                    poly.Transform(transform);
                    Surface sur = Surface.CreateExtrusion(poly.ToNurbsCurve(), vyExtrude);
                    breps.Add(sur.ToBrep().CapPlanarHoles(Project.tolerance));
                }
                //BREP:haunchweb
                if (heightHaunch != 0.0)
                {
                    
                    Point3d hp1= ImportGrasshopperUtils.CastPointToRhino(p1);
                    Point3d hp2 = ImportGrasshopperUtils.CastPointToRhino(p2);
                    Point3d hp3 = ImportGrasshopperUtils.CastPointToRhino(p3);
                    List<Point3d> hwp = new List<Point3d>() { hp1, hp2, hp3, hp1 };
                    Polyline poly = new Polyline(hwp);
                    
                    Vector vyMove = Vector.VecScalMultiply(vy.FlipVector(), beam.thicknessWeb / 2000);
                    Vector vyExtrude = Vector.VecScalMultiply(vy, beam.thicknessWeb / 1000);
                    Transform transform = Transform.Translation(ImportGrasshopperUtils.CastVectorToRhino(vyMove));
                    poly.Transform(transform);
                    Surface sur = Surface.CreateExtrusion(poly.ToNurbsCurve(), ImportGrasshopperUtils.CastVectorToRhino(vyExtrude));
                    breps.Add(sur.ToBrep().CapPlanarHoles(Project.tolerance));
                    /*

                    NurbsSurface plate = NurbsSurface.CreateFromCorners(hwp[0], hwp[1], hwp[2]);
                    Brep plateB = plate.ToBrep();
                    breps.Add(plateB);
                    */
                }
                //BREP:Stiffeners
                if (stiffeners == true)
                {
                    //dependend on direction of column
                    Point3d pointAs = new Point3d(-(column.width) / 2000, -(column.height) / 2000, -(beam.thicknessFlange) / 2000);
                    Point3d pointBs = new Point3d((column.width) / 2000, (column.height) / 2000, (beam.thicknessFlange) / 2000);
                    BoundingBox bboxstif = new BoundingBox(pointAs, pointBs);
                    Vector vXb = bear.element.localCoordinateSystem.X;
                    Vector vYb = bear.element.localCoordinateSystem.Y;
                    Vector vZb = bear.element.localCoordinateSystem.Z;

                    Core.Point topP = Core.Point.MovePointByVectorandLength(p, vXb, (beam.height / 2000));
                    Plane planeTop = new Plane(ImportGrasshopperUtils.CastPointToRhino(topP), ImportGrasshopperUtils.CastVectorToRhino(vYb), ImportGrasshopperUtils.CastVectorToRhino(vZb));
                    Box topStiff = new Box(planeTop, bboxstif);
                    breps.Add(topStiff.ToBrep());

                    Core.Point botP = Core.Point.MovePointByVectorandLength(p, vXb, (-beam.height / 2000) - (heightHaunch / 1000));
                    Plane planeBot = new Plane(ImportGrasshopperUtils.CastPointToRhino(botP), ImportGrasshopperUtils.CastVectorToRhino(vYb), ImportGrasshopperUtils.CastVectorToRhino(vZb));
                    Box botStiff = new Box(planeBot, bboxstif);
                    breps.Add(botStiff.ToBrep());
                }

                //Step VII - modify beam brepline
                KarambaIDEA.Core.Line.ModifyBeamBrepLine(bear, con, tendplate);
                maxBeamHeight = Math.Max(maxBeamHeight, beam.height);

            }
            


            //Step VIII - modify column brepline
            KarambaIDEA.Core.Line.ModifyColumnBrepLine(bearlist, maxBeamHeight);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.ATempMomentResistingConnection;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("e8467348-e28f-40e0-991a-fcf88f652aba"); }
        }
        

    }
}
