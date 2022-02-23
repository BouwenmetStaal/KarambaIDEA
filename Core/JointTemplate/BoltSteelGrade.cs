using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.Core
{
    public class BoltSteelGrade
    {
        public Steelgrade steelgrade = Steelgrade.b8_8;
        public double Fub
        {
            get
            {
                switch (this.steelgrade)
                {
                    case Steelgrade.b4_6:
                        {
                            return 400;
                        }
                    case Steelgrade.b4_8:
                        {
                            return 400;
                        }
                    case Steelgrade.b5_6:
                        {
                            return 500;
                        }
                    case Steelgrade.b6_8:
                        {
                            return 600;
                        }
                    case Steelgrade.b8_8:
                        {
                            return 800;
                        }
                    case Steelgrade.b10_9:
                        {
                            return 1000;
                        }
                    default:
                        {
                            return -1;
                        }
                }
            }
        }
        public double Fyb
        {
            get
            {
                switch (this.steelgrade)
                {
                    case Steelgrade.b4_6:
                        {
                            return 240;
                        }
                    case Steelgrade.b4_8:
                        {
                            return 320;
                        }
                    case Steelgrade.b5_6:
                        {
                            return 300;
                        }
                    case Steelgrade.b6_8:
                        {
                            return 480;
                        }
                    case Steelgrade.b8_8:
                        {
                            return 640;
                        }
                    case Steelgrade.b10_9:
                        {
                            return 900;
                        }
                    default:
                        {
                            return -1;
                        }
                }
            }
        }

        public string name
        {
            get
            {
                switch (this.steelgrade)
                {
                    case Steelgrade.b4_6:
                        {
                            return "4.6";
                        }
                    case Steelgrade.b4_8:
                        {
                            return "4.8";
                        }
                    case Steelgrade.b5_6:
                        {
                            return "5.6";
                        }
                    case Steelgrade.b6_8:
                        {
                            return "6.8";
                        }
                    case Steelgrade.b8_8:
                        {
                            return "8.8";
                        }
                    case Steelgrade.b10_9:
                        {
                            return "10.9";
                        }
                    default:
                        {
                            return "8.8";
                        }
                }
            }
        }
        public BoltSteelGrade()
        {

        }
        public enum Steelgrade
        {
            b4_6,
            b4_8,
            b5_6,
            b6_8,
            b8_8,
            b10_9
        }

        public static BoltSteelGrade.Steelgrade selectgrade (string name)
        {
            if (name == "10.9")
            {
                return Steelgrade.b10_9;
            }
            else
            {
                return Steelgrade.b8_8;
            }
            
        }
    }
}
