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
        public WeldVolumeGenerator() : base("Weld volume generator", "Weld volume generator", "Generate weld volume per joint based on the perimeter of attached cross-sections with a throat thickness determined by the full-strength method", "KarambaIDEA", "7. Cost calculation")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Weld volume [cm3]", "Weld volume [cm3]", "Retrieve welding volume per joint in project", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            Project project = new Project();
            
            //Link input
            DA.GetData(0, ref project);

            //output variables
            DataTree<double> weldVolumes = new DataTree<double>();
            List<string> throatBegin = new List<string>();
            List<string> throatEnd = new List<string>();

            int a = 0;
            foreach (Joint joint in project.joints)
            {
                GH_Path path = new GH_Path(a);
                if (joint.template != null)
                {
                    if (joint.template.welds.Count != 0)
                    {
                        foreach (Weld weld in joint.template.welds)
                        {
                            double weldVolume = weld.volume * Math.Pow(10, -3); //conversion from mm3 to cm3
                            weldVolumes.Add(weldVolume, path);
                        }
                    }
                    else
                    {
                        weldVolumes.Add(0.0, path);
                    }
                }
                else
                {
                    weldVolumes.Add(0.0, path);
                }
                a = a + 1;
            }
            
            //link output
            DA.SetDataTree(0, weldVolumes);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.WeldingVolume;

            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("66b111b3-16d6-46e9-9c99-bfdf4dbeddf4"); }
        }
    }
}
