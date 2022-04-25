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
using Grasshopper.Kernel.Types;


namespace KarambaIDEA
{
    public class CostCalculator : GH_Component
    {
        public CostCalculator() : base("CostCalculator", "CostCalculator", "Calculate total costs", "KarambaIDEA", "6. Cost calculation")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Weight elements [kg]", "Weight elements [kg]", "Weight elements per beam elemeent", GH_ParamAccess.list, -1);
            pManager.AddNumberParameter("Weight plates [kg]", "Weight plates [kg]", "Weight plates per plate per joint", GH_ParamAccess.tree, -1);
            pManager.AddNumberParameter("Welding volume [cm3]", "Welding volume [cm3]", "Welding volume per weld per joint", GH_ParamAccess.tree, -1);
            pManager.AddNumberParameter("Bolt costs", "Bolt costs", "Bolt costs per bolt per joint", GH_ParamAccess.tree, -1);
            pManager.AddNumberParameter("Number of elements", "Number of elements", "Number of elements", GH_ParamAccess.list, -1);
            pManager.AddNumberParameter("Steel: Price per kg", "Steel: Price per kg", "Price of steel per kg, default price is €1,- per kg", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Welding: Price per cm3", "Welding: Price per cm3", "Price of welding per cm3, default price is €2.60 per cm3", GH_ParamAccess.item, 2.6);
            pManager.AddNumberParameter("Transport: Price per element", "Transport: Price per element", "Price of transport per element, default price is €20,-", GH_ParamAccess.item, 20);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Costs of beam material [€]", "Costs of beam material [€]", "Costs of beam material [€]", GH_ParamAccess.item);
            pManager.AddTextParameter("Costs per Joint [€]", "Costs per Joint [€]", "Costs per Joint[€]", GH_ParamAccess.list);
            pManager.AddTextParameter("Costs per Joint Detailed [€]", "Costs per Joint Detailed [€]", "Costs per Joint on element level. First costs of Welds, then Plates, then bolts", GH_ParamAccess.list);
            pManager.AddTextParameter("Transport cost [€]", "Transport cost [€]", "Transport cost[€]", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            List<double> elementWeights = new List<double>();
            GH_Structure<GH_Number> totalPlateWeights = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> totalWeldingVolumes = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> boltscosts = new GH_Structure<GH_Number>();
            List<double> elements = new List<double>();

            double priceSteel = new double();
            double priceWelding = new double();
            double priceTransport = new double();

            //Link input
            DA.GetDataList("Weight elements [kg]", elementWeights);
            DA.GetDataTree("Weight plates [kg]", out totalPlateWeights);
            DA.GetDataTree("Welding volume [cm3]", out totalWeldingVolumes);
            DA.GetDataTree("Bolt costs", out boltscosts);
            DA.GetDataList("Number of elements", elements);

            DA.GetData("Steel: Price per kg", ref priceSteel);
            DA.GetData("Welding: Price per cm3", ref priceWelding);
            DA.GetData("Transport: Price per element", ref priceTransport);

            //output variables
            double materialCosts = new double();
            List<string> jointCostsString = new List<string>();
            DataTree<double> jointCostsDouble = new DataTree<double>();
            double transportcosts = new double();

            for (int a = 0; a < totalWeldingVolumes.Branches.Count; a++)
            {
                GH_Path path = new GH_Path(a);
                double weldcosts = new double();
                if (totalWeldingVolumes[0].FirstOrDefault().Value == 0 && totalWeldingVolumes.Branches.Count == 1 && totalWeldingVolumes[0].Count == 1)
                {
                    //no data specified
                }
                else
                {
                    foreach (GH_Number number in totalWeldingVolumes[a])
                    {
                        double weldcost = number.Value * priceWelding;
                        jointCostsDouble.Add(weldcost, path);
                        weldcosts = weldcosts + weldcost;
                    }
                }



                double platecosts = new double();
                if (totalPlateWeights[0].FirstOrDefault().Value == 0 && totalPlateWeights.Branches.Count == 1 && totalPlateWeights[0].Count == 1)
                {
                    //no data specified
                }
                else
                {
                    foreach (GH_Number number in totalPlateWeights[a])
                    {
                        double platecost = number.Value * priceSteel;
                        jointCostsDouble.Add(number.Value, path);
                        platecosts = platecosts + platecost;
                    }
                }
                
                

                double boltcosts = new double();
                if (boltscosts[0].FirstOrDefault().Value == 0 && boltscosts.Branches.Count == 1 && boltscosts[0].Count == 1)
                {
                    //no data specified
                }
                else
                {
                    foreach (GH_Number number in boltscosts[a])
                    {
                        boltcosts = boltcosts + number.Value;//number value is already a price
                        jointCostsDouble.Add(number.Value, path);
                    }
                }
                
                //bolt = bolt * priceSteel;

                double price = Math.Ceiling(weldcosts+platecosts+boltcosts);
                string result = string.Empty;
                if (price != 0)
                {
                    result ="€ "+price.ToString()+",-";
                }

                jointCostsString.Add(result);
            }
            
            materialCosts = materialCosts+ elementWeights.Sum() * priceSteel;
            transportcosts = transportcosts + elements.Sum() * priceTransport;



            //link output
            DA.SetData(0, materialCosts);
            DA.SetDataList(1, jointCostsString);
            DA.SetDataTree(2, jointCostsDouble);
            DA.SetData(3, transportcosts);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.Costs;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("4f34b8c2-aa8a-4533-beb3-0b44192d234e"); }
        }
    }
}

