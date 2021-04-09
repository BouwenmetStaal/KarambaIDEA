using KarambaIDEA.Core.JointTemplate;
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
        public double p1;
        public double p2;

        public List<Coordinate2D> Coordinates2D;

        public BoltGrid(Bolt _bolttype, double _rows, double _columns, double _e1, double _e2, double _p1, double _p2)
        {
            this.bolttype = _bolttype;
            this.rows = _rows;
            this.columns = _columns;
            this.e1 = _e1;
            this.e2 = _e2;
            this.p1 = _p1;
            this.p2 = _p2;
        }


        public BoltGrid(Bolt _bolttype, List<Coordinate2D> _coordinate2Ds)
        {
            this.bolttype = _bolttype;
            this.Coordinates2D = _coordinate2Ds;
        }
    }
}
