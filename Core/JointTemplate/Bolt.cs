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
        public BoltType boltType;
        public BoltSteelgrade boltSteelgrade = BoltSteelgrade.b8_8;

        public Bolt(BoltType _boltType, BoltSteelgrade _boltSteelgrade)
        {
            this.boltType = _boltType;
            this.boltSteelgrade = _boltSteelgrade;
        }

        public Bolt(string name, double _holediameter, double _shankArea, double coreArea)
        {

        }

        public double HoleDiameter
        {
            get
            {
                switch (this.boltType)
                {
                    case BoltType.M12:
                        {
                            return 13;
                        }
                    case BoltType.M16:
                        {
                            return 18;
                        }

                    case BoltType.M20:
                        {
                            return 22;
                        }

                    default:
                        {
                            return double.NaN;
                        }
                }
            }
        }

        public double ShankArea
        {
            get
            {
                switch (this.boltType)
                {
                    case BoltType.M12:
                        {
                            return 113.1;
                        }
                    case BoltType.M16:
                        {
                            return 201.1;
                        }

                    case BoltType.M20:
                        {
                            return 314.2;
                        }

                    default:
                        {
                            return double.NaN;
                        }
                }
            }
        }

        public double CoreArea
        {
            get
            {
                switch (this.boltType)
                {
                    case BoltType.M12:
                        {
                            return 84.3;
                        }
                    case BoltType.M16:
                        {
                            return 157;
                        }

                    case BoltType.M20:
                        {
                            return 245.0;
                        }

                    default:
                        {
                            return double.NaN;
                        }
                }
            }
        }

        public enum BoltType
        {
            M12,
            M16,
            M20,
            M22,
            M24,
            M27,
            M30,
            M36,
            M39,
            M42,
            M48,
            M52
        }

        public enum BoltSteelgrade
        {
            b4_6,
            b4_8,
            b5_6,
            b6_8,
            b8_8,
            b10_9
        }


        public double ShearResistance()
        {
            double A = this.CoreArea;
            double fub = 640;
            double alphaV = 0.5;
            if(this.boltSteelgrade==BoltSteelgrade.b8_8|| this.boltSteelgrade == BoltSteelgrade.b5_6|| this.boltSteelgrade == BoltSteelgrade.b4_6)
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
            double fub = 640;
            double FtRd = k2 * fub * As / Project.gammaM2;
            return FtRd;
        }
        public double CombinedShearandTension(double FvEd, double FtEd)
        {
            return FvEd / this.ShearResistance() + FtEd / (1.4 * this.TensionResistance());
        }
    }
}
