using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KarambaIDEA.Core;
using Newtonsoft.Json;
using IdeaRS.OpenModel;
using IdeaRS.OpenModel.Result;
using IdeaRS.OpenModel.Connection;

namespace KarambaIDEA.IDEA
{

    /*
    class IdeaProductionCost
    {


    }

    public abstract class IdeaItemCost
    {
        public int UniqueId;
        public float UnitCost;
        public float Cost;
        public float TotalWeight;
        public string Name;
        public float Grade;
        public float PlateThickness;

        public IdeaItemCost() { }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class IdeaSteelCost : IdeaItemCost
    {
        public IdeaSteelCost() { }
        public IdeaSteelCost() { }
    }

    public class IdeaWeldCost : IdeaItemCost
    {
        //public int UniqueId { get; set; }
        //public float UnitCost { get; set; }
        //public float Cost { get; set; }
        //public float TotalWeight { get; set; }
        //public float ThroatThickness { get; set; }
        //public float PlateThickness { get; set; }
        public float LegSize { get; set; }
        public int WeldType { get; set; }

        public IdeaWeldCost() { }
        internal IdeaWeldCost(CheckResSummary summaryResult)
        {
            Name = summaryResult.Name;
            CheckValue = summaryResult.CheckValue;
            UnityCheck = summaryResult.CheckValue; //to make it easyier for GH output.
            CheckStatus = summaryResult.CheckStatus;
            UnityCheckMsg = summaryResult.UnityCheckMessage;
            LoadCaseId = summaryResult.LoadCaseId;
        }
    }

    public class IdeaBoltCost : IdeaItemCost
    {

        public IdeaBoltCost(Boltcost boltCost)
        {
            Name = boltResult.Name;
            CheckStatus = boltResult.CheckStatus;
            LoadCaseId = boltResult.LoadCaseId;
            UnityCheck = boltResult.UnityCheck;
        }
    }

    //Production Cost Serialisation - Need Idea Classes 

    public class ProductionCost
    {
        public Steelcost[] SteelCosts { get; set; }
        public Filletweldcost[] FilletWeldCosts { get; set; }
        public Buttweldcost[] ButtWeldCosts { get; set; }
        public Boltcost[] BoltCosts { get; set; }
        public float HoleDrillingCost { get; set; }
        public float TotalEstimatedCost { get; set; }
        public string LogMessage { get; set; }
    }

    public class Steelcost
    {
        public int UniqueId { get; set; }
        public float UnitCost { get; set; }
        public float Cost { get; set; }
        public float TotalWeight { get; set; }
        public string Name { get; set; }
        public float Grade { get; set; }
        public float PlateThickness { get; set; }
    }

    public class Filletweldcost
    {
        public int UniqueId { get; set; }
        public float UnitCost { get; set; }
        public float Cost { get; set; }
        public float TotalWeight { get; set; }
        public object Name { get; set; }
        public float ThroatThickness { get; set; }
        public float PlateThickness { get; set; }
        public float LegSize { get; set; }
        public int WeldType { get; set; }
    }

    public class Buttweldcost
    {
        public int UniqueId { get; set; }
        public float UnitCost { get; set; }
        public float Cost { get; set; }
        public float TotalWeight { get; set; }
        public object Name { get; set; }
        public float ThroatThickness { get; set; }
        public float PlateThickness { get; set; }
        public float LegSize { get; set; }
        public int WeldType { get; set; }
    }

    public class Boltcost
    {
        public int UniqueId { get; set; }
        public float UnitCost { get; set; }
        public float Cost { get; set; }
        public float TotalWeight { get; set; }
        public string Name { get; set; }
        public float Grade { get; set; }
    }
    */
}
