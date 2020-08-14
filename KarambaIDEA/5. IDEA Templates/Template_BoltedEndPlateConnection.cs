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
            double tplate = new double();
            List<GH_String> brandNamesDirty = new List<GH_String>();
            List<string> brandNames = new List<string>();

            //Output variables
            List<string> messages = new List<string>();
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
                    foreach(Joint joint in project.joints)
                    {
                        if (brandName == joint.brandName)
                        {
                            SetTemplate(tplate, joint, breps);
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
            DA.SetDataList(2, breps);
        }

        private static void SetTemplate(double tplate, Joint joint, List<Brep> breps)
        {
            joint.template = new Template();
            

            Core.CrossSection beam = joint.attachedMembers.First().element.crossSection;
            Plate plateA = new Plate("endplateA", beam.height, beam.width, tplate);
            Plate plateB = new Plate("endplateB", beam.height, beam.width, tplate);
            
            joint.template.plates.Add(plateA);
            joint.template.plates.Add(plateB);

            joint.template.workshopOperations = Template.WorkshopOperations.BoltedEndPlateConnection;
            foreach(AttachedMember at in joint.attachedMembers)
            {
                double weldsize = Weld.CalWeldSizeFullStrenth90deg(tplate, beam.thicknessFlange, beam.material, Weld.WeldType.DoubleFillet);
                joint.template.welds.Add(new Weld("FlangeweldTop", Weld.WeldType.DoubleFillet, weldsize, beam.width));
                joint.template.welds.Add(new Weld("FlangeweldBot", Weld.WeldType.DoubleFillet, weldsize, beam.width));
                double weldsizeWeb = Weld.CalWeldSizeFullStrenth90deg(tplate, beam.thicknessWeb, beam.material, Weld.WeldType.DoubleFillet);
                joint.template.welds.Add(new Weld("Webweld", Weld.WeldType.DoubleFillet, weldsizeWeb, beam.height));



                //BREP beam
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

                    //breps.Add(tube);
                    breps.Add(plate);
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
