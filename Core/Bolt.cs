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
    }
}
