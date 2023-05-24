using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using KarambaIDEA.Core;
using KarambaIDEA.IDEA;

namespace KarambaIDEA.Grasshopper
{
    public class ConnectionProductionCost : GH_Component
    {
        public ConnectionProductionCost() : base("Connection Costs", "ConCosts", "Retrieve Connection Productions costs from a connection", "KarambaIDEA", "6. IDEA Connection") { }

        public override GH_Exposure Exposure { get { return GH_Exposure.tertiary; } }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Connection", "C", "Idea Connection which has production cost avaliable", GH_ParamAccess.item);
            //pManager.AddTextParameter("Summary Keys", "S", "Optional Summary Key to search and filter summary results", GH_ParamAccess.list);
            //pManager.AddTextParameter("Plate Keys", "P", "Optional Plate Keys to search and filter plate results", GH_ParamAccess.list);
            //pManager.AddTextParameter("Weld Keys", "W", "Optional Weld Keys to search and filter weld results", GH_ParamAccess.list);
            //pManager.AddTextParameter("Bolt Keys", "B", "Optional Bolt Keys to search and filter bolt results", GH_ParamAccess.list);
            //pManager.AddTextParameter("Anchor Keys", "A", "Optional Anchor Keys to search and filter anchor results", GH_ParamAccess.list);
            //pManager.AddTextParameter("Conc Block Keys", "CB", "Optional Conc Block keys to search and filter anchor results", GH_ParamAccess.list);

            //pManager[1].Optional = pManager[2].Optional = pManager[3].Optional = pManager[4].Optional = pManager[5].Optional = pManager[6].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of Connection", GH_ParamAccess.item);
            pManager.AddNumberParameter("Total Est. Cost", "T", "Total Estimated Cost of the Connection", GH_ParamAccess.item);
            pManager.AddGenericParameter("Steel Costs", "S", "List of Steel Cost Items", GH_ParamAccess.list);
            pManager.AddGenericParameter("Weld Costs", "W", "List of Weld Cost Items", GH_ParamAccess.list);
            pManager.AddGenericParameter("Bolt Costs", "B", "List of Bolt Cost Items", GH_ParamAccess.list);
            pManager.AddNumberParameter("Hole Drilling Cost", "H", "Estimated Hole Drilling Cost", GH_ParamAccess.item);
            pManager.AddTextParameter("Messages", "M", "Production Cost Messages", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_IdeaConnection connection = null;

            if (DA.GetData<GH_IdeaConnection>(0, ref connection))
            {
                if (connection.Value.ProductionCosts != null)
                {
                    IdeaConnectionProductionCost cost = connection.Value.ProductionCosts;

                    //List<string> summarykeys = new List<string>();
                    //DA.GetDataList(1, summarykeys);

                    //List<string> platekeys = new List<string>();
                    //DA.GetDataList(2, platekeys);

                    //List<string> weldkeys = new List<string>();
                    //DA.GetDataList(3, weldkeys);

                    string name = connection.Value.ConnectionNameRef;

                    List<GH_IdeaItemCost> steelCosts = cost.GetSteelCosts(new List<string>()).ConvertAll(x => new GH_IdeaItemCost(x));
                    List<GH_IdeaItemCost> weldCosts = cost.GetWeldCosts(new List<string>()).ConvertAll(x => new GH_IdeaItemCost(x));
                    List<GH_IdeaItemCost> boltCosts = cost.GetBoltCosts(new List<string>()).ConvertAll(x => new GH_IdeaItemCost(x));

                    DA.SetData(0, name);
                    DA.SetData(1, cost.TotalEstimatedCost);
                    DA.SetDataList(2, steelCosts);
                    DA.SetDataList(3, weldCosts);
                    DA.SetDataList(4, boltCosts);
                    DA.SetData(5, cost.HoleDrillingCost);
                    DA.SetData(6, cost.LogMessage);
                }
            }
            else
                base.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Connection Production Cost is not Loaded.");
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.ConnectionCost; } }
        public override Guid ComponentGuid { get { return new Guid("B17A7402-F481-4E75-8886-BBA5C1DFF9A4"); } }
    }

    public class DeconstructCostItem : GH_Component
    {
        public DeconstructCostItem() : base("Deconstruct Cost Item", "DecCostItem", "Deconstruct a Connection Cost Item", "KarambaIDEA", "6. IDEA Connection") { }

        public override GH_Exposure Exposure { get { return GH_Exposure.tertiary | GH_Exposure.obscure; } }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Cost Item", "C", "Idea Connection Cost Item to deconstruct", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of Cost Item", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Id", "Id", "Unique Id of Cost Item", GH_ParamAccess.item);
            pManager.AddNumberParameter("Unit Cost", "UC", "Unit Cost of Cost Item", GH_ParamAccess.item);
            pManager.AddNumberParameter("Cost", "C", "Total Cost of Cost Item", GH_ParamAccess.item);
            pManager.AddNumberParameter("Total Weight", "W", "Total Weight of Cost Item", GH_ParamAccess.item);
            pManager.AddNumberParameter("Grade", "G", "Grade of Cost Item", GH_ParamAccess.item);
            pManager.AddNumberParameter("Plate Thk", "T", "Plate Thickness of Cost Item", GH_ParamAccess.item);
            pManager.AddTextParameter("Weld Type", "W", "Weld Type of Cost Item (Applicable for Weld Cost Items)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Weld Leg Thk", "Tt", "Weld Throat Thickness of Cost Item (Applicable for Weld Cost Items)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Weld Throat Thk", "Tt", "Weld Throat Thickness of Cost Item (Applicable for Weld Cost Items)", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_IdeaItemCost itemCost = null;

            if (DA.GetData<GH_IdeaItemCost>(0, ref itemCost))
            {
                if (itemCost.Value != null)
                {
                    IdeaItemCost cost = itemCost.Value;

                    DA.SetData(0, cost.Name);
                    DA.SetData(1, cost.UniqueId);
                    DA.SetData(2, cost.UnitCost);
                    DA.SetData(3, cost.Cost);
                    DA.SetData(4, cost.TotalWeight);
                    DA.SetData(5, cost.Grade);
                    DA.SetData(6, cost.PlateThickness);

                    if (cost is IdeaWeldCost weldCost)
                    {
                        DA.SetData(7, weldCost.WeldType.ToString());
                        DA.SetData(8, weldCost.LegSize);
                        DA.SetData(9, weldCost.ThroatThickness);
                    }
                }
                else
                {
                    base.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No cost Items available.");
                }
            }
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.DeconstructCostItem; } }
        public override Guid ComponentGuid { get { return new Guid("4073470B-76E0-4720-BB30-125750061924"); } }
    }
}
