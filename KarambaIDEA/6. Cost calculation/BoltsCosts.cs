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


namespace KarambaIDEA._7._Cost_calculation
{
    public class BoltsCosts : GH_Component
    {
        public BoltsCosts() : base("Bolt costs", "Bolt costs", "Retrieve bolt costs per joint for cost analysis", "KarambaIDEA", "6. Cost calculation")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddNumberParameter("Boltprices", "Boltprices", "Boltprices as list", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Bolt costs", "Bolt costs", "Retrieve bolt costs per joint in project", GH_ParamAccess.tree);
            pManager.AddTextParameter("Message", "M", "", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            Project project = new Project();
            List<double> prices = new List<double>();

            //Link input
            DA.GetData(0, ref project);
            DA.GetDataList(1, prices);

            //output variables
            DataTree<double> boltCosts = new DataTree<double>();
            List<string> messages = new List<string>();


            List<Bolt> bolts = Bolt.CreateBoltsList(BoltSteelGrade.Steelgrade.b8_8);
            for (int i = 0; i < bolts.Count; i++)
            {
                string message = bolts[i].Name + " is priced at € ";
                if (prices.Count != 0)
                {
                    if (i > prices.Count - 1)
                    {
                        message += "0.0";
                        bolts[i].price = 0.0;
                    }
                    else
                    {
                        message += prices[i].ToString();
                        bolts[i].price = prices[i];
                    }
                    
                }

                messages.Add(message);
            }


            int b = 0;
            foreach (Joint joint in project.joints)
            {
                GH_Path path = new GH_Path(b);
                if (joint.template != null)
                {
                    if (joint.template.boltGrids.Count != 0)
                    {
                        foreach (BoltGrid boltgrid in joint.template.boltGrids)
                        {
                            if(boltgrid.Coordinates2D != null)
                            {
                                foreach (Core.JointTemplate.Coordinate2D cor in boltgrid.Coordinates2D)
                                {
                                    double boltCost = bolts.Single(a => boltgrid.bolttype.Name == a.Name).price;
                                    boltCosts.Add(boltCost, path);
                                }
                                
                            }
                            if(boltgrid.rows != 0 && boltgrid.columns != 0)
                            {
                                double boltCost = boltgrid.rows * boltgrid.columns * bolts.Single(a => boltgrid.bolttype.Name == a.Name).price;
                                boltCosts.Add(boltCost, path);
                            }
                            else
                            {
                                boltCosts.Add(0.0, path);
                            }
                        }
                        
                    }
                    else
                    {
                        boltCosts.Add(0.0, path);
                    }
                }
                else
                {
                    boltCosts.Add(0.0, path);
                }
                b = b + 1;
            }

            //link output
            DA.SetDataTree(0, boltCosts);
            DA.SetDataList(1, messages);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.bolts_01_01;

            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("1f9f5636-a3e7-41c4-99a8-daccc758ae17"); }
        }
    }
}
