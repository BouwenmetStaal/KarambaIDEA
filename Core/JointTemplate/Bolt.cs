using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.Core
{
    public class Bolt
    {
        public Project project;
        public string Name;
        public double Diameter;
        public double HoleDiameter;
        public double ShankArea;
        public double CoreArea;
        public BoltSteelGrade BoltSteelGrade;

        

        public Bolt(BoltSteelGrade.Steelgrade bsg, double _diameter, double _holediameter, double _coreArea)
        {
            this.BoltSteelGrade.steelgrade = bsg;
            this.Name = "M" + _diameter.ToString();
            this.Diameter = _diameter;
            this.HoleDiameter = _holediameter;
            this.ShankArea = (Math.PI*Math.Pow(_diameter,2))/4;
            this.CoreArea = _coreArea;

        }

        static public List<Bolt> CreateBoltsList(BoltSteelGrade.Steelgrade bsg)
        {
            List<Bolt> bolts = new List<Bolt>();
            bolts.Add(new Bolt(bsg, 12, 13, 84.3));
            bolts.Add(new Bolt(bsg, 16, 18, 157));
            bolts.Add(new Bolt(bsg, 20, 22, 245));
            bolts.Add(new Bolt(bsg, 24, 26, 353));
            bolts.Add(new Bolt(bsg, 27, 30, 459));
            bolts.Add(new Bolt(bsg, 30, 33, 561));
            bolts.Add(new Bolt(bsg, 36, 39, 817));
            bolts.Add(new Bolt(bsg, 42, 45, 1121));
            bolts.Add(new Bolt(bsg, 48, 51, 1473));
            bolts.Add(new Bolt(bsg, 52, 55, 1758));
            return bolts;
        }

        


        public double ShearResistance()
        {
            double A = this.CoreArea;
            double fub = 640;//TODO: include properties
            double alphaV = 0.5;
            if(this.BoltSteelGrade.steelgrade== BoltSteelGrade.Steelgrade.b8_8|| this.BoltSteelGrade.steelgrade == BoltSteelGrade.Steelgrade.b5_6|| this.BoltSteelGrade.steelgrade == BoltSteelGrade.Steelgrade.b4_6)
            {
                alphaV = 0.6;
            }
            double FvRd = (alphaV*fub*A)/Project.gammaM2;
            return FvRd;
        }
        public double TensionResistance()
        {
            double As = this.ShankArea;
            double k2 = 0.9;
            double fub = 640;//TODO: include properties
            double FtRd = k2 * fub * As / Project.gammaM2;
            return FtRd;
        }

        public double CombinedShearandTension(double FvEd, double FtEd)
        {
            double UC = FvEd / this.ShearResistance() + FtEd / (1.4 * this.TensionResistance());
            return UC;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="EdgeboltOrtho"></param>
        /// <param name="EdgeboltPerp"></param>
        /// <param name="bolt"></param>
        /// <param name="t"></param>
        /// <param name="mat"></param>
        /// <param name="e1"></param>
        /// <param name="p1"></param>
        /// <param name="e2"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public double BearingRestance(bool EdgeboltOrtho, bool EdgeboltPerp, Bolt bolt, double t, MaterialSteel mat, double e1, double p1, double e2, double p2)
        {
            //alpahD: inner or edge bolt force direction
            //k1: inner of edge bolt perpendicular to force direction
            if (EdgeboltPerp == true)
            {
                k1 = Math.Min(2, 8 * (e2 / bolt.HoleDiameter) - 1.7,0.000);//TODO:finish this part
            }
            else
            {

            }
            double k1 = 0.0;
            double alphaB = Math.Min(bolt.BoltSteelGrade.Fub/mat.Fu,1.0);
            double d = bolt.Diameter;
            double FbRd = (k1 * alphaB * d * t) / Project.gammaM2;
            return FbRd;
        }

        
    }
}
