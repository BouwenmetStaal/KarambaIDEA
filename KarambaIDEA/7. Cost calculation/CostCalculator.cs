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
        public CostCalculator() : base("CostCalculator", "CostCalculator", "Calculate total costs", "KarambaIDEA", "7. Cost calculation")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Weight elements [kg]", "Weight elements [kg]", "Weight elements", GH_ParamAccess.list, 0.0);
            pManager.AddNumberParameter("Weight plates [kg]", "Weight plates [kg]", "Weight plates", GH_ParamAccess.tree, 0.0);
            pManager.AddNumberParameter("Welding volume [cm3]", "Welding volume [cm3]", "Welding volume", GH_ParamAccess.tree, 0.0);
            pManager.AddNumberParameter("Number of elements", "Number of elements", "Number of elements", GH_ParamAccess.list, 0.0);
            pManager.AddNumberParameter("Steel: Price per kg", "Steel: Price per kg", "Price of steel per kg, default price is €1,- per kg", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Welding: Price per cm3", "Welding: Price per cm3", "Price of welding per cm3, default price is €2.60 per cm3", GH_ParamAccess.item, 2.6);
            pManager.AddNumberParameter("Transport: Price per element", "Transport: Price per element", "Price of transport per element, default price is €20,-", GH_ParamAccess.item, 20);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Costs of beam material [€]", "Costs of beam material [€]", "Costs of beam material [€]", GH_ParamAccess.item);
            pManager.AddTextParameter("Costs per Joint [€]", "Costs per Joint [€]", "Costs per Joint[€]", GH_ParamAccess.list);
            pManager.AddTextParameter("Transport cost [€]", "Transport cost [€]", "Transport cost[€]", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            List<double> elementWeights = new List<double>();
            GH_Structure<GH_Number> totalPlateWeights = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> totalWeldingVolumes = new GH_Structure<GH_Number>();
            List<double> elements = new List<double>();

            double priceSteel = new double();
            double priceWelding = new double();
            double priceTransport = new double();

            //Link input
            DA.GetDataList(0, elementWeights);
            DA.GetDataTree(1, out totalPlateWeights);
            DA.GetDataTree(2, out totalWeldingVolumes);
            DA.GetDataList(3, elements);

            DA.GetData(4,ref priceSteel);
            DA.GetData(5,ref priceWelding);
            DA.GetData(6, ref priceTransport);

            //output variables
            double materialCosts = new double();
            List<string> jointCosts = new List<string>();
            double transportcosts = new double();

            for (int a=0; a < totalWeldingVolumes.Branches.Count; a++)
            {
                double weld = new double();
                foreach (GH_Number number in totalWeldingVolumes[a])
                {
                    weld = weld + number.Value;
                }
                weld = weld * priceWelding;

                double plate = new double();
                foreach (GH_Number number in totalPlateWeights[a])
                {
                    plate = plate + number.Value;
                }
                plate = plate * priceSteel;

                double price = Math.Ceiling(weld+plate);
                string result = string.Empty;
                if (price != 0)
                {
                    result ="€ "+price.ToString()+",-";
                }

                jointCosts.Add(result);
            }
            
            materialCosts =materialCosts+ elementWeights.Sum() * priceSteel;
            transportcosts = transportcosts + elements.Sum() * priceTransport;



            //link output
            DA.SetData(0, materialCosts);
            DA.SetDataList(1, jointCosts);
            DA.SetData(2, transportcosts);
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

