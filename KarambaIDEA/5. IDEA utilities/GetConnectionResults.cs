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
    public class ConnectionResults : GH_Component
    {
        public ConnectionResults() : base("Connection Results", "ConResults", "Retrieve Connection Results from a calculated connection", "KarambaIDEA", "5. IDEA Connection") {}

        public override GH_Exposure Exposure { get { return GH_Exposure.tertiary; } }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Connection", "C", "Idea Connection which has results avaliable", GH_ParamAccess.item);
            pManager.AddTextParameter("Summary Keys", "S", "Optional Summary Key to search and filter summary results", GH_ParamAccess.list);
            pManager.AddTextParameter("Plate Keys", "P", "Optional Plate Keys to search and filter plate results", GH_ParamAccess.list);
            pManager.AddTextParameter("Weld Keys", "W", "Optional Weld Keys to search and filter weld results", GH_ParamAccess.list);
            pManager.AddTextParameter("Bolt Keys", "B", "Optional Bolt Keys to search and filter bolt results", GH_ParamAccess.list);
            pManager.AddTextParameter("Anchor Keys", "A", "Optional Anchor Keys to search and filter anchor results", GH_ParamAccess.list);
            pManager.AddTextParameter("Conc Block Keys", "CB", "Optional Conc Block keys to search and filter anchor results", GH_ParamAccess.list);

            pManager[1].Optional = pManager[2].Optional = pManager[3].Optional = pManager[4].Optional = pManager[5].Optional = pManager[6].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of Connection", GH_ParamAccess.item);
            pManager.AddGenericParameter("Summary Results", "S", "List of Summary Results", GH_ParamAccess.list);
            pManager.AddGenericParameter("Plate Results", "P", "List of Plate Results", GH_ParamAccess.list);
            pManager.AddGenericParameter("Weld Results", "W", "List of Weld Results", GH_ParamAccess.list);
            pManager.AddGenericParameter("Bolt Results", "B", "List of Bolt Results", GH_ParamAccess.list);
            pManager.AddGenericParameter("Anchor Results", "A", "List of Anchor Results", GH_ParamAccess.list);
            pManager.AddGenericParameter("Concrete Block Results", "C", "List of Concrete Block Results", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_IdeaConnection connection = null;


            if (DA.GetData<GH_IdeaConnection>(0, ref connection))
            {
                if (connection.Value.Results != null)
                {
                    IdeaConnectionResult result = connection.Value.Results;

                    List<string> summarykeys = new List<string>();
                    DA.GetDataList(1, summarykeys);

                    List<string> platekeys = new List<string>();
                    DA.GetDataList(2, platekeys);

                    List<string> weldkeys = new List<string>();
                    DA.GetDataList(3, weldkeys);

                    List<string> boltkeys = new List<string>();
                    DA.GetDataList(4, boltkeys);

                    List<string> anchorkeys = new List<string>();
                    DA.GetDataList(5, anchorkeys);
                    
                    List<string> concblockkeys = new List<string>();
                    DA.GetDataList(6, concblockkeys);

                    string name = result.Name;
                    List<GH_IdeaItemResult> summaryList = result.GetSummaryResults(summarykeys).ConvertAll(x => new GH_IdeaItemResult(x));
                    List<GH_IdeaItemResult> plateList = result.GetPlateResults(platekeys).ConvertAll(x => new GH_IdeaItemResult(x));
                    List<GH_IdeaItemResult> weldList = result.GetWeldResults(weldkeys).ConvertAll(x => new GH_IdeaItemResult(x));
                    List<GH_IdeaItemResult> boltList = result.GetBoltResults(boltkeys).ConvertAll(x => new GH_IdeaItemResult(x));
                    List<GH_IdeaItemResult> anchorList = result.GetAnchorResults(anchorkeys).ConvertAll(x => new GH_IdeaItemResult(x));
                    List<GH_IdeaItemResult> concblockList = result.GetConcreteBlockResults(concblockkeys).ConvertAll(x => new GH_IdeaItemResult(x));

                    DA.SetData(0, name);
                    DA.SetDataList(1, summaryList);
                    DA.SetDataList(2, plateList);
                    DA.SetDataList(3, weldList);
                    DA.SetDataList(4, boltList);
                    DA.SetDataList(5, anchorList);
                    DA.SetDataList(6, concblockList);
                }
            }
            else
                base.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Connection does not have any results available. Calculate Connection using Calculate Component.");
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.IDEAlogo; } }
        public override Guid ComponentGuid { get { return new Guid("3AD39EF3-4C78-4A4E-9FE2-C1C7C76C7A97"); } }
    }


    public class DeconstructItemResult : GH_Component
    {
        public DeconstructItemResult() : base("Deconstruct Item Result", "DecItemResult", "Deconstruct a Connection Item Result", "KarambaIDEA", "5. IDEA Connection") { }

        public override GH_Exposure Exposure { get { return GH_Exposure.tertiary; } }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Item Result", "R", "Idea Connection Item result to deconstruct", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of Connection", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Check Status", "S", "Check Status of Item Result (true/false)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Unity Check", "U", "Unity Check of Result. This is the Check Value for Summary Result Items", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pl Max Stress", "Pl S", "Plate Stress for plate result items", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pl Max Strain", "Pl e", "Plate Strain for plate result items", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Items", "I", "List of Items associated with result. Used for Plate and Weld Results", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Item Id", "Id", "Critical Item Id. Used for Weld Results only", GH_ParamAccess.item);
            pManager.AddTextParameter("Check Msg", "I", "Further check message information", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_IdeaItemResult itemResult = null;

            DA.GetData<GH_IdeaItemResult>(0, ref itemResult);

            if (itemResult == null)
            {
                base.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Itemresults does not have any results available.");
            }
            else
            {
                if (itemResult.Value != null)
                {
                    IdeaItemResult result = itemResult.Value;

                    DA.SetData(0, result.Name);
                    DA.SetData(1, result.CheckStatus);
                    DA.SetData(2, result.UnityCheck);

                    if (result is IdeaPlateResult plResult)
                    {
                        DA.SetData(3, plResult.MaxStress);
                        DA.SetData(4, plResult.MaxStrain);
                        DA.SetDataList(5, plResult.Items);
                    }
                    else if (result is IdeaWeldResult weldResult)
                    {
                        DA.SetDataList(5, weldResult.Items);
                        DA.SetData(6, weldResult.Id);
                    }
                    else if (result is IdeaSummaryResult summaryResult)
                    {
                        DA.SetData(7, summaryResult.UnityCheckMsg);
                    }
                }
            }
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.Deconstruct_ItemResult; } }
        public override Guid ComponentGuid { get { return new Guid("02BE617E-1BA5-436F-A59E-F3C56E1FDB6F"); } }
    }

}
