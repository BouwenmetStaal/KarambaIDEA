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
        public double e1;
        public double e2;

        public BoltGrid(Bolt _bolttype, double _rows, double _columns, double _e1, double _e2)
        {
            this.bolttype = _bolttype;
            this.rows = _rows;
            this.columns = _columns;
            this.e1 = _e1;
            this.e2 = _e2;
        }
    }
    
}
