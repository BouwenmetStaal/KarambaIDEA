using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.Core
{
    public class Hierarchy
    {
        public int numberInHierarchy;
        public string groupname;

        public Hierarchy()
        {

        }

        public Hierarchy(int _numberInHierarchy, string _groupname)
        {
            this.numberInHierarchy = _numberInHierarchy;
            this.groupname = _groupname;
        }
    }
}
