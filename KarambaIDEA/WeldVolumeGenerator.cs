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
            pManager.AddGenericParameter("Joints", "Joints", "List of Joint objects of KarambaIdeaCore", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Weld volume [cm3]", "Weld volume", "Total weld volume of the joint in cm3", GH_ParamAccess.list);
            pManager.AddBrepParameter("Brep weld volume", "Brep weld volume", "Brep weld volume", GH_ParamAccess.tree);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables
            List<Joint> joints = new List<Joint>();


            //Link input
            DA.GetDataList(0, joints);

            //output variables
            List<double> weldVolumes = new List<double>();
            DataTree<Brep> brepVolumes = new DataTree<Brep>();

            foreach(Joint joint in joints)
            {
                double weldVolumeJoint = new double();
                //Ignore highest hierarchy members by only taking connectingmembers
                foreach (ConnectingMember con in joint.attachedMembers.OfType<ConnectingMember>())
                {
                    double weldVolume = new double();
                    CrossSection cross = con.element.crossSection;
                    double factorWeb = Weld.CalcFullStrengthFactor(cross, 90);
                    double throatWeb = cross.thicknessWeb * factorWeb;

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
                        double factorFlange = Weld.CalcFullStrengthFactor(cross, 90);
                        double throatFlange = cross.thicknessFlange * factorFlange;

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

            

            //link output
            DA.SetDataList(0, weldVolumes);
            DA.SetDataTree(1, brepVolumes); //visualize weldingvolume through brep
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
