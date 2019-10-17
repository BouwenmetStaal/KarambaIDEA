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
    public class WeldVolumeGenerator : GH_Component
    {
        public WeldVolumeGenerator() : base("Weld volume generator", "Weld volume generator", "Generate weld volume per joint based on the perimeter of attached cross-sections with a throat thickness determined by the full-strength method", "KarambaIDEA", "3. Project utilities")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Weld volume [cm3]", "Weld volume [cm3]", "Total weld volume of the joint in cm3", GH_ParamAccess.list);
            pManager.AddBrepParameter("Brep weld volume", "Brep weld volume", "Brep weld volume", GH_ParamAccess.tree);

            pManager.AddTextParameter("Throats Begin of Element", "ThroatsBegin", "ThroatFlange and ThroatWeb at Start of Element", GH_ParamAccess.list);
            pManager.AddTextParameter("Throats End of Element", "ThroatsEnd", "ThroatFlange and ThroatWeb at End of Element", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables
            Project project = new Project();
            
            //Link input
            DA.GetData(0, ref project);

            

            //output variables
            List<double> weldVolumes = new List<double>();
            DataTree<Brep> brepVolumes = new DataTree<Brep>();

            List<string> throatBegin = new List<string>();
            List<string> throatEnd = new List<string>();

            foreach (Joint joint in project.joints)
            {
                double weldVolumeJoint = new double();
                //Ignore highest hierarchy members by only taking connectingmembers
                foreach (ConnectingMember con in joint.attachedMembers.OfType<ConnectingMember>())
                {
                    double weldVolume = new double();
                    CrossSection cross = con.element.crossSection;
                    double factor = Weld.CalcFullStrengthFactor(cross, 90);
                    double throatWeb = cross.thicknessWeb * factor;
                    double throatFlange = cross.thicknessFlange * factor;

                    con.webWeld.size = throatWeb;
                    con.flangeWeld.size = throatFlange;

                    if (cross.shape == CrossSection.Shape.CHSsection)
                    {
                        double radius = 0.5*cross.height;
                        double perimeter = 2 * Math.PI * radius;
                        weldVolume = perimeter * Math.Pow(throatWeb, 2);
                    }

                    if (cross.shape == CrossSection.Shape.HollowSection)
                    {
                        double perimeter = 2 * cross.width + 2 * cross.height;
                        weldVolume = perimeter * Math.Pow(throatWeb, 2);
                    }
                    if (cross.shape == CrossSection.Shape.ISection)
                    {
                        double weldVolumeWeb = 2*cross.height * Math.Pow(throatWeb, 2);
                        double weldVolumeFlange = 4*cross.width * Math.Pow(throatFlange, 2);
                        weldVolume = weldVolumeWeb + weldVolumeFlange;
                    }
                    else
                    {
                        //Warning: cross-sections not recognized
                    }

                    weldVolumeJoint = +weldVolume;
                }
                //Calculate full strength weld
                //single fillet vs double fillet weld
                //define perimeter

                weldVolumeJoint = weldVolumeJoint / 1000.0; //conversion from mm3 to cm3

                weldVolumes.Add(weldVolumeJoint);




            }

            foreach (Element ele in project.elements)
            {
                throatBegin.Add(ele.BeginThroatsElement());
                throatEnd.Add(ele.EndThroatsElement());
            }

            //link output
            DA.SetDataList(0, weldVolumes);
            DA.SetDataTree(1, brepVolumes); //visualize weldingvolume through brep

            DA.SetDataList(2, throatBegin);
            DA.SetDataList(3, throatEnd);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.KarambaIDEA_logo;

            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("66b111b3-16d6-46e9-9c99-bfdf4dbeddf4"); }
        }
    }
}
