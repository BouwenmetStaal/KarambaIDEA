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

        public enum WorkshopOperations
        {
            NoOperation,
            BoltedEndPlateConnection,
            WeldAllMembers
        }
    }
}
