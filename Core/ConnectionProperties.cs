using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.Core
{
    public class ConnectionProperties
    {
        Classification classification;
        public double Sj=double.NaN;
        public double SjR;
        public double SjH;
        public double Mjrd=double.NaN;

        public ConnectionProperties()
        {

        }
    }
    public enum Classification
    {
        Rigid,
        SemiRigid,
        Hinged
    }
}
