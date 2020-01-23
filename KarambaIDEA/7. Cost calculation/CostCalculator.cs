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
    public class CostCalculator : GH_Component
    {
        public CostCalculator() : base("CostCalculator", "CostCalculator", "Calculate total costs", "KarambaIDEA", "7. Cost calculation")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Total weight [kg]", "Total weight [kg]", "Total weight", GH_ParamAccess.list, 0.0);
            pManager.AddNumberParameter("Total welding volume [cm3]", "Total welding volume [cm3]", "Total welding volume", GH_ParamAccess.list, 0.0);
            pManager.AddNumberParameter("Total number of elements", "Total number of elements", "Total number of elements", GH_ParamAccess.list, 0.0);
            pManager.AddNumberParameter("Steel: Price per kg", "Steel: Price per kg", "Price of steel per kg, default price is €1,- per kg", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Welding: Price per cm3", "Welding: Price per cm3", "Price of welding per cm3, default price is €2.60 per cm3", GH_ParamAccess.item, 2.6);
            pManager.AddNumberParameter("Transport: Price per element", "Transport: Price per element", "Price of transport per element, default price is €20,-", GH_ParamAccess.item, 20);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Total costs [€]", "Total costs [€]", "Total costs [€]", GH_ParamAccess.item);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            List<double> totalWeights = new List<double>();
            List<double> totalWeldingVolumes = new List<double>();
            List<double> elements = new List<double>();
            double priceSteel = new double();
            double priceWelding = new double();
            double priceTransport = new double();

            //Link input
            DA.GetDataList(0, totalWeights);
            DA.GetDataList(1, totalWeldingVolumes);
            DA.GetDataList(2, elements);

            DA.GetData(3,ref priceSteel);
            DA.GetData(4,ref priceWelding);
            DA.GetData(5, ref priceTransport);

            //output variables
            double totalCosts = new double();
            List<double> weightElements = new List<double>();

            totalCosts =totalCosts+ totalWeights.Sum() * priceSteel;
            totalCosts = totalCosts + totalWeldingVolumes.Sum()*priceWelding;
            totalCosts = totalCosts + elements.Sum() * priceTransport;



            //link output
            DA.SetData(0, totalCosts);
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

