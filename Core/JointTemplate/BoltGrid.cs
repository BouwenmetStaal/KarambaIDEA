using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.Core
{
    public class BoltGrid
    {
        public Project project;
        public double rows;
        public double columns;
        public Bolt bolttype;

        public BoltGrid(Bolt _bolttype, double _rows, double _columns)
        {
            this.bolttype = _bolttype;
            this.rows = _rows;
            this.columns=_columns:
        }
    }
    
}
