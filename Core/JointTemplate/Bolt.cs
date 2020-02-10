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
        public BoltSteelGrade BoltSteelGrade = new BoltSteelGrade();

        

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
            double fub = this.BoltSteelGrade.Fub;
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
            double fub = this.BoltSteelGrade.Fub;
            double FtRd = k2 * fub * As / Project.gammaM2;
            return FtRd;
        }

        public double CombinedShearandTension(double FvEd, double FtEd)
        {
            double UC = FvEd / this.ShearResistance() + FtEd / (1.4 * this.TensionResistance());
            return UC;
        }

        /// <summary>
        /// Positioning of holes for bolts NEN-EN-1993 1-8 Table 3.4
        /// </summary>
        /// <param name="EdgeboltOrtho">(e1,p1) Is the bolt an edge bolt in the direction of the force? If False it is an inner bolt.</param>
        /// <param name="EdgeboltPerp">(e2,p2) Is the bolt an edge bolt in the direction perpendicular to the force? If False it is an inner bolt. </param>
        /// <param name="bolt">boltsize</param>
        /// <param name="t">thickness plate</param>
        /// <param name="mat">material of plate</param>
        /// <param name="e1">NEN-EN-1993 1-8, art 3.5 Figure 3.1</param>
        /// <param name="p1">NEN-EN-1993 1-8, art 3.5 Figure 3.1</param>
        /// <param name="e2">NEN-EN-1993 1-8, art 3.5 Figure 3.1</param>
        /// <param name="p2">NEN-EN-1993 1-8, art 3.5 Figure 3.1</param>
        /// <returns></returns>
        public double BearingRestance(bool EdgeboltOrtho, bool EdgeboltPerp, Bolt bolt, double t, MaterialSteel mat, double e1, double p1, double e2, double p2)
        {
            double d0 = bolt.HoleDiameter;
            //alpahD: inner or edge bolt force direction
            double alphaB = Math.Min(bolt.BoltSteelGrade.Fub / mat.Fu, 1.0);
            if (EdgeboltOrtho == true)
            {
                alphaB = Math.Min(alphaB, e1/(3*d0));
            }
            else
            {
                alphaB = Math.Min(alphaB, p1 / (3 * d0) - (1 / 4));
            }
            //k1: inner of edge bolt perpendicular to force direction
            double k1 = 2.5;
            if (EdgeboltPerp == true)
            {
                k1 =Math.Min(Math.Min(2.8 * (e2 / d0) - 1.7, 1.4*(p2/d0)-1.7),2.5);
            }
            else
            {
                k1 = Math.Min(1.4 * (p2 / d0) - 1.7, 2.5);
            }            
            
            double d = bolt.Diameter;
            double FbRd = (k1 * alphaB *mat.Fu*d * t) / Project.gammaM2;
            return FbRd;
        }

        
    }
}
