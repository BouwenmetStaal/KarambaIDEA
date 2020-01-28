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

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables      
            Project project = new Project();
            List<GH_String> brandNamesDirty = new List<GH_String>();
            List<string> brandNames = new List<string>();

            //Output variables
            List<string> messages = new List<string>();

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
                            SetAnaTemplate(joint);
                        }
                    }
                }
            }
            else
            {
                foreach (Joint joint in project.joints)
                {
                    SetAnaTemplate(joint);
                }
            }


            //link output
            DA.SetData(0, project);
            DA.SetDataList(1, messages);
        }

        private static void SetAnaTemplate(Joint joint)
        {
            joint.template = new Template();
            foreach(ConnectingMember con in joint.attachedMembers)
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
                                vloads.Add(Math.Abs(loadsPerLine.startLoad.Vy));
                            }
                            else
                            {
                                vloads.Add(Math.Abs(loadsPerLine.endLoad.Vy));
                            }
                        }
                    }
                }
                //Step II - selecting bolt size
                Bolt bolt = new Bolt(Bolt.BoltType.M20, Bolt.BoltSteelgrade.b8_8);

                
                //Step III - evaluation of different design possibilities
                //Step IV - finplate dimensions
                //Step V - weld dimensions
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

                return Properties.Resources.KarambaIDEA_logo;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("5be9ade6-d460-42a1-b0f5-4d5d1df770a6"); }
        }



    }
}
