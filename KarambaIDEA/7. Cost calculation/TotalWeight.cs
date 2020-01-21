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
    public class TotalWeight : GH_Component
    {
        public TotalWeight() : base("Total Weight", "Total Weight", "Generate total weight of elements and plates", "KarambaIDEA", "7. Cost calculation")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Weight elements [kg]", "Weight elements [kg]", "Total weight per element", GH_ParamAccess.list);
            pManager.AddNumberParameter("Weight plates [kg]", "Weight plates [kg]", "Total weight of plates per joint", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            Project project = new Project();

            //Link input
            DA.GetData(0, ref project);

            //output variables
            List<double> weightElements = new List<double>();
            DataTree<double> weightPlates = new DataTree<double>();

            double massSteel = 7850;

            foreach(Element ele in project.elements)
            {
                double area = ele.crossSection.Area();
                double len = ele.line.Length;
                weightElements.Add(area * len * massSteel);
            }
            int a = 0;
            
            foreach (Joint joint in project.joints)
            {
                GH_Path path = new GH_Path(a);
                foreach(Plate plate in joint.template.plates)
                {
                    weightPlates.Add(10, path);
                }
                a = a + 1;
            }

            //link output
            DA.SetDataList(0, weightElements);
            DA.SetDataTree(1, weightPlates);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.Weight;

            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("70edbb4c-869f-4796-811b-d3c809fbe92b"); }
        }
    }
}
