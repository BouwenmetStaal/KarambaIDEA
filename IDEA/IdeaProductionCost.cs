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
    public class IdeaConnectionProductionCost
    {
        //private readonly string _name;
        private readonly double _totalEstimatedCost;
        private readonly double _holeDrillingCost;
        private readonly string _logMessage;

        private readonly IdeaItemSet<IdeaSteelCost> _steelCosts = new IdeaItemSet<IdeaSteelCost>();
        private readonly IdeaItemSet<IdeaWeldCost> _weldCosts = new IdeaItemSet<IdeaWeldCost>();
        private readonly IdeaItemSet<IdeaBoltCost> _boltCosts = new IdeaItemSet<IdeaBoltCost>();
        
        public IdeaConnectionProductionCost(ProductionCost productionCost)
        {
            _totalEstimatedCost = productionCost.TotalEstimatedCost;
            _holeDrillingCost = productionCost.HoleDrillingCost;
            _logMessage = productionCost.LogMessage;

            foreach (Steelcost steelCost in productionCost.SteelCosts)
                _steelCosts.Add(steelCost.UniqueId.ToString(), new IdeaSteelCost(steelCost));
            foreach (Filletweldcost fillCost in productionCost.FilletWeldCosts)
                _weldCosts.Add(fillCost.UniqueId.ToString(), new IdeaWeldCost(fillCost));
            foreach (Buttweldcost buttCost in productionCost.ButtWeldCosts)
                _weldCosts.Add(buttCost.UniqueId.ToString(), new IdeaWeldCost(buttCost));
            foreach (Boltcost boltCost in productionCost.BoltCosts)
                _boltCosts.Add(boltCost.UniqueId.ToString(), new IdeaBoltCost(boltCost));

        }

        public double TotalEstimatedCost { get { return _totalEstimatedCost; } }
        public double HoleDrillingCost { get { return _holeDrillingCost; } }
        public string LogMessage { get { return _logMessage; } }


        public List<IdeaSteelCost> GetSteelCosts(List<string> filterKeys) { return _steelCosts.GetResults(filterKeys); }

        public List<IdeaBoltCost> GetBoltCosts(List<string> filterKeys) { return _boltCosts.GetResults(filterKeys); }

        public List<IdeaWeldCost> GetWeldCosts(List<string> filterKeys) { return _weldCosts.GetResults(filterKeys); }

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
        public IdeaSteelCost(Steelcost steelCost) 
        {
            UniqueId = steelCost.UniqueId;
            UnitCost = steelCost.UnitCost;
            Cost = steelCost.Cost;
            TotalWeight = steelCost.TotalWeight;
            Name = steelCost.Name;
            Grade = steelCost.Grade;
            PlateThickness = steelCost.PlateThickness;
        }
    }

    public class IdeaWeldCost : IdeaItemCost
    {
        public enum WELDTYPE { NotSpecified, Fillet, DoubleFillet, Bevel, Spuare, Plug, LengthAtHaunch, FilletRear, Contact, Intermittent, Butt };

        public double ThroatThickness;
        public double LegSize;
        public WELDTYPE WeldType { get; set; }

        public IdeaWeldCost() { }
        internal IdeaWeldCost(Filletweldcost filletWeld)
        {
            UniqueId = filletWeld.UniqueId;
            UnitCost = filletWeld.UnitCost;
            Cost = filletWeld.Cost;
            TotalWeight = filletWeld.TotalWeight;
            Name = filletWeld.Name;
            PlateThickness = filletWeld.PlateThickness;
            WeldType = WELDTYPE.Fillet;
            WELDTYPE typeWeld = WELDTYPE.Fillet;
            if(Enum.TryParse<WELDTYPE>(filletWeld.WeldType.ToString(), out typeWeld))
                WeldType = typeWeld;

            ThroatThickness = filletWeld.ThroatThickness;
            LegSize = filletWeld.LegSize;
        }

        internal IdeaWeldCost(Buttweldcost buttWeld)
        {
            UniqueId = buttWeld.UniqueId;
            UnitCost = buttWeld.UnitCost;
            Cost = buttWeld.Cost;
            TotalWeight = buttWeld.TotalWeight;
            Name = buttWeld.Name;
            PlateThickness = buttWeld.PlateThickness;
            WeldType = WELDTYPE.Butt;
            ThroatThickness = buttWeld.ThroatThickness;
            LegSize = buttWeld.ThroatThickness;
        }
    }

    public class IdeaBoltCost : IdeaItemCost
    {

        public IdeaBoltCost(Boltcost boltCost)
        {
            UniqueId = boltCost.UniqueId;
            UnitCost = boltCost.UnitCost;
            Cost = boltCost.Cost;
            TotalWeight = boltCost.TotalWeight;
            Name = boltCost.Name;
            Grade = boltCost.Grade;
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
        public string Name { get; set; }
        public float ThroatThickness { get; set; }
        public float PlateThickness { get; set; }
        public float LegSize { get; set; }
        public IdeaRS.OpenModel.Connection.WeldType WeldType { get; set; }
    }

    public class Buttweldcost
    {
        public int UniqueId { get; set; }
        public float UnitCost { get; set; }
        public float Cost { get; set; }
        public float TotalWeight { get; set; }
        public string Name { get; set; }
        public float ThroatThickness { get; set; }
        public float PlateThickness { get; set; }
        public float LegSize { get; set; }
        public IdeaRS.OpenModel.Connection.WeldType WeldType { get; set; }
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
}
