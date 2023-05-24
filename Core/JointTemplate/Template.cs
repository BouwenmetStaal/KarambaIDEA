using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.Core
{
    public class Template
    {
        public WorkshopOperations workshopOperations = WorkshopOperations.NoOperation;
        public List<Plate> plates = new List<Plate>();
        public List<Weld> welds = new List<Weld>();
        public List<BoltGrid> boltGrids = new List<BoltGrid>();

        public enum WorkshopOperations
        {
            NoOperation,
            BoltedEndPlateConnection,
            BoltedEndplateOptimizer,
            WeldAllMembers,
            TemplateByFile,
            AddedMember
        }
    }
}
