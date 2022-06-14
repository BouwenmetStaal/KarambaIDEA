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

    public class IdeaItemSet<T> : Dictionary<string, T>
    {
        public IdeaItemSet() { }

        public List<T> GetResults(List<string> keys)
        {
            List<T> results = new List<T>();

            if (keys == null || keys.Count == 0)
                foreach (var result in this)
                    results.Add(result.Value);
            else
                foreach (var key in keys)
                    if (this.TryGetValue(key, out T result))
                        results.Add(result);

            return results;
        }

        public void AddWithNameCheck(string Name, T result, int repeat)
        {
            string name = Name + (repeat == 0 ? "" : "(" + repeat.ToString() + ")");
            if (this.ContainsKey(name))
            {
                repeat++;
                this.AddWithNameCheck(name, result, repeat);
            }
            else
            {
                this.Add(name, result);
            }
        }
    }

    public abstract class IdeaItemResult
    {
        public string Name;
        public bool CheckStatus;
        public int LoadCaseId;
        public double UnityCheck;

        public IdeaItemResult() { }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class IdeaSummaryResult : IdeaItemResult
    {
        public double CheckValue;
        public string UnityCheckMsg;

        public IdeaSummaryResult() { }
        internal IdeaSummaryResult(CheckResSummary summaryResult)
        {
            Name = summaryResult.Name;
            CheckValue = summaryResult.CheckValue;
            UnityCheck = summaryResult.CheckValue; //to make it easyier for GH output.
            CheckStatus = summaryResult.CheckStatus;
            UnityCheckMsg = summaryResult.UnityCheckMessage;
            LoadCaseId = summaryResult.LoadCaseId;
        }
    }

    public class IdeaBoltResult : IdeaItemResult
    {
        public IdeaBoltResult(CheckResBolt boltResult)
        {
            Name = boltResult.Name;
            CheckStatus = boltResult.CheckStatus;
            LoadCaseId = boltResult.LoadCaseId;
            UnityCheck = boltResult.UnityCheck;
        }
    }

    public class IdeaAnchorResult : IdeaItemResult
    {
        public IdeaAnchorResult(CheckResAnchor anchorResult)
        {
            Name = anchorResult.Name;
            CheckStatus = anchorResult.CheckStatus;
            LoadCaseId = anchorResult.LoadCaseId;
            UnityCheck = anchorResult.UnityCheck;
        }
    }

    public class IdeaPlateResult : IdeaItemResult
    {
        public double MaxStrain;
        public double MaxStress;
        public List<int> Items;

        public IdeaPlateResult(CheckResPlate plateResult)
        {
            Name = plateResult.Name;
            CheckStatus = plateResult.CheckStatus;
            LoadCaseId = plateResult.LoadCaseId;
            MaxStrain = plateResult.MaxStrain;
            MaxStress = plateResult.MaxStress;
            Items = plateResult.Items;
        }
    }

    public class IdeaWeldResult : IdeaItemResult
    {
        public int Id;
        public List<int> Items;
        
        public IdeaWeldResult(CheckResWeld weldResult)
        {
            Name = weldResult.Name;
            CheckStatus = weldResult.CheckStatus;
            UnityCheck = weldResult.UnityCheck;
            LoadCaseId = weldResult.LoadCaseId;
            Id = weldResult.Id;
            Items = weldResult.Items;
        }
    }

    public class IdeaConcreteBlockResult : IdeaItemResult
    {
        public IdeaConcreteBlockResult(CheckResConcreteBlock concreteBlockResult)
        {
            Name = concreteBlockResult.Name;
            CheckStatus = concreteBlockResult.CheckStatus;
            LoadCaseId = concreteBlockResult.LoadCaseId;
            UnityCheck = concreteBlockResult.UnityCheck;
        }
    }

    public class IdeaConnectionResult
    {
        private readonly string _name;
        private readonly IdeaItemSet<IdeaSummaryResult> _summaryResults = new IdeaItemSet<IdeaSummaryResult>();
        private readonly IdeaItemSet<IdeaPlateResult> _plateResults = new IdeaItemSet<IdeaPlateResult>();
        private readonly IdeaItemSet<IdeaBoltResult> _boltResults = new IdeaItemSet<IdeaBoltResult>();
        private readonly IdeaItemSet<IdeaWeldResult> _weldResults = new IdeaItemSet<IdeaWeldResult>();
        private readonly IdeaItemSet<IdeaAnchorResult> _anchorResults = new IdeaItemSet<IdeaAnchorResult>();
        private readonly IdeaItemSet<IdeaConcreteBlockResult> _concreteBlockResults = new IdeaItemSet<IdeaConcreteBlockResult>();

        public IdeaConnectionResult(ConnectionResultsData connectionResult)
        {
            ConnectionCheckRes checkResult = connectionResult.ConnectionCheckRes[0];

            _name = checkResult.Name;
            
            foreach (CheckResSummary summaryResult in checkResult.CheckResSummary)
                _summaryResults.Add(summaryResult.Name, new IdeaSummaryResult(summaryResult));
            foreach (CheckResPlate plateResult in checkResult.CheckResPlate)
                _plateResults.AddWithNameCheck(plateResult.Name, new IdeaPlateResult(plateResult), 0);
            foreach (CheckResBolt boltResult in checkResult.CheckResBolt)
                _boltResults.AddWithNameCheck(boltResult.Name, new IdeaBoltResult(boltResult), 0);
            foreach (CheckResWeld weldResult in checkResult.CheckResWeld)
                _weldResults.AddWithNameCheck(weldResult.Name, new IdeaWeldResult(weldResult), 0);
            foreach (CheckResAnchor anchorResult in checkResult.CheckResAnchor)
                _anchorResults.AddWithNameCheck(anchorResult.Name, new IdeaAnchorResult(anchorResult), 0);
            foreach (CheckResConcreteBlock concreteBlockResult in checkResult.CheckResConcreteBlock)
                _concreteBlockResults.AddWithNameCheck(concreteBlockResult.Name, new IdeaConcreteBlockResult(concreteBlockResult),0);

            //To Do: Add Calculation Messages.
        }

        public string Name { get { return _name; } }

        public List<IdeaSummaryResult> GetSummaryResults(List<string> filterKeys) { return _summaryResults.GetResults(filterKeys); }
        
        public List<IdeaPlateResult> GetPlateResults(List<string> filterKeys) { return _plateResults.GetResults(filterKeys); }

        public List<IdeaBoltResult> GetBoltResults(List<string> filterKeys) { return _boltResults.GetResults(filterKeys); }
        
        public List<IdeaWeldResult> GetWeldResults(List<string> filterKeys) { return _weldResults.GetResults(filterKeys); }

        public List<IdeaAnchorResult> GetAnchorResults(List<string> filterKeys) { return _anchorResults.GetResults(filterKeys); }

        public List<IdeaConcreteBlockResult> GetConcreteBlockResults(List<string> filterKeys) { return _concreteBlockResults.GetResults(filterKeys); }
    }
}
