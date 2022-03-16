using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KarambaIDEA.Core;
using IdeaRS.OpenModel;
using IdeaRS.OpenModel.Result;
using IdeaRS.OpenModel.Connection;

namespace KarambaIDEA.IDEA
{

    public class IdeaItemResultSet<T> : Dictionary<string, T>
    {
        public IdeaItemResultSet() { }

    }

    public abstract class IdeaItemResult
    {
        public bool CheckStatus;
        public int LoadCaseId;
        public double UnityCheck;

        public IdeaItemResult() { }
    }


    public class IdeaResultSummary : IdeaItemResult
    {
        public double CheckValue;
        public string UnityCheckMsg;

        public IdeaResultSummary() { }
        internal IdeaResultSummary(CheckResSummary summaryResult)
        {
            CheckValue = summaryResult.CheckValue;
            CheckStatus = summaryResult.CheckStatus;
            UnityCheckMsg = summaryResult.UnityCheckMessage;
            LoadCaseId = summaryResult.LoadCaseId;
        }
    }

    public class IdeaBoltResult : IdeaItemResult
    {
        public IdeaBoltResult(CheckResBolt boltResult)
        {
            CheckStatus = boltResult.CheckStatus;
            LoadCaseId = boltResult.LoadCaseId;
            UnityCheck = boltResult.UnityCheck;
        }
    }

    public class IdeaPlateResult : IdeaItemResult
    {
        public double MaxStrain;
        public double MaxStress;
        public List<int> Items;

        public IdeaPlateResult(CheckResPlate plateResult)
        {
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
            CheckStatus = weldResult.CheckStatus;
            LoadCaseId = weldResult.LoadCaseId;
            Id = weldResult.Id;
            Items = weldResult.Items;
        }
    }


    public class IdeaConnectionResult
    {
        public IdeaItemResultSet<IdeaResultSummary> SummaryResults = new IdeaItemResultSet<IdeaResultSummary>();
        public IdeaItemResultSet<IdeaPlateResult> PlateResults = new IdeaItemResultSet<IdeaPlateResult>();
        public IdeaItemResultSet<IdeaBoltResult> BoltResults = new IdeaItemResultSet<IdeaBoltResult>();
        public IdeaItemResultSet<IdeaWeldResult> WeldResults = new IdeaItemResultSet<IdeaWeldResult>();
        //etc..

        public IdeaConnectionResult(ConnectionResultsData connectionResult)
        {
            ConnectionCheckRes checkResult = connectionResult.ConnectionCheckRes[0];

            foreach (CheckResSummary summaryResult in checkResult.CheckResSummary)
                SummaryResults.Add(summaryResult.Name, new IdeaResultSummary(summaryResult));
            foreach (CheckResPlate plateResult in checkResult.CheckResPlate)
                PlateResults.Add(plateResult.Name, new IdeaPlateResult(plateResult));
            foreach (CheckResBolt boltResult in checkResult.CheckResBolt)
                BoltResults.Add(boltResult.Name, new IdeaBoltResult(boltResult));
            foreach (CheckResWeld weldResult in checkResult.CheckResWeld)
                WeldResults.Add(weldResult.Name, new IdeaWeldResult(weldResult));
            
            //etc...
        }
    }
}
