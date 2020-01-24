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
    public class ATemplate_MomentResistingConnection : GH_Component
    {
        public ATemplate_MomentResistingConnection() : base("Analytical Template: Moment resisting connection", "Analytical Template: Moment resisting connection", "Analytical Template: Moment resisting connection", "KarambaIDEA", "4. Analytical Templates")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("BrandNames", "BrandNames", "BrandNames to apply template to", GH_ParamAccess.list, "");
            pManager.AddNumberParameter("Height haunch [mm]", "Height haunch [mm]", "", GH_ParamAccess.item, 0.0);
            pManager.AddBooleanParameter("Stiffeners?", "Stiffeners?", "", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Unbraced structure?", "Unbraced structure?", "If true joint classification will be assesed accoring to unbraced structures specifications, if false for braced structures.", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("Message", "Message", "", GH_ParamAccess.list);            
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            

            //Input variables      
            Project project = new Project();
            double heightHaunch = new double();
            List<GH_String> brandNamesDirty = new List<GH_String>();
            List<string> brandNames = new List<string>();
            bool stiffeners = new bool();
            bool unbraced = new bool();

            //Output variables
            List<string> messages = new List<string>();
            Brep brep = new Brep();

            //Link input
            DA.GetData(0, ref project);
            DA.GetDataList(1, brandNamesDirty);
            DA.GetData(2, ref heightHaunch);
            DA.GetData(3, ref stiffeners);
            DA.GetData(4, ref unbraced);


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
                            if (joint.attachedMembers.OfType<ConnectingMember>().ToList().Count == 1)
                            {
                                SetMomentResitingConnection(joint, heightHaunch, stiffeners, unbraced);
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
            else
            {
                foreach (Joint joint in project.joints)
                {
                    if (joint.attachedMembers.OfType<ConnectingMember>().ToList().Count == 1)
                    {
                        SetMomentResitingConnection(joint, heightHaunch, stiffeners, unbraced);
                    }
                    else
                    {
                        //more than one connectingmembers in connection
                        //TODO: include warning
                    }
                }
            }
            
            //messages = project.MakeTemplateJointMessage();

            BoundingBox bbox = new BoundingBox(new Point3d(0, 0, 0), new Point3d(0.1, 0.2, 0.05));
            Plane plane = new Plane(new Point3d(0, 2, 0), new Vector3d(0, 3, 3));
            Box box = new Box(plane, bbox);
            brep = box.ToBrep();

            //link output
            DA.SetData(0, project);
            DA.SetDataList(1, messages);
        }
        private static void SetMomentResitingConnection(Joint joint, double heightHaunch, bool stiffeners, bool unbraced)
        {
            ConnectingMember con = joint.attachedMembers.OfType<ConnectingMember>().ToList().First();
            BearingMember bear = joint.attachedMembers.OfType<BearingMember>().ToList().First();

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
                double haunchRatio = 2.0;
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
