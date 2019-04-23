using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.Core
{
    public class MaterialSteel
    {
        public string name
        {
            get
            {
                switch (this.steelGrade)
                {
                    case SteelGrade.S235:
                        {
                            return "S235";
                        }
                    case SteelGrade.S275:
                        {
                            return "S275";
                        }

                    case SteelGrade.S355:
                        {
                            return "S355";
                        }

                    default:
                        {
                            return "NotFound";
                        }
                }
            }
        }
        public int id
        {
            get
            {
                switch (this.steelGrade)
                {
                    case SteelGrade.S235:
                        {
                            return 1;
                        }
                    case SteelGrade.S275:
                        {
                            return 2;
                        }

                    case SteelGrade.S355:
                        {
                            return 3;
                        }

                    default:
                        {
                            return -1;
                        }
                }
            }
        }
        public SteelGrade steelGrade = SteelGrade.S235;

        public double fu
        {
            get
            {
                switch (this.steelGrade)
                {
                    case SteelGrade.S235:
                        {
                            return 360.0;
                        }
                    case SteelGrade.S275:
                        {
                            return 430.0;
                        }

                    case SteelGrade.S355:
                        {
                            return 510.0;
                        }

                    default:
                        {
                            return 0.0;
                        }
                }
            }
        }
        public double fy
        {
            get
            {
                switch (this.steelGrade)
                {
                    case SteelGrade.S235:
                        {
                            return 235.0;
                        }
                    case SteelGrade.S275:
                        {
                            return 275.0;
                        }

                    case SteelGrade.S355:
                        {
                            return 355.0;
                        }

                    default:
                        {
                            return 0.0;
                        }
                }
            }
        }

        public double fu40
        {
            get
            {
                switch (this.steelGrade)
                {
                    case SteelGrade.S235:
                        {
                            return 360.0;
                        }
                    case SteelGrade.S275:
                        {
                            return 410.0;
                        }

                    case SteelGrade.S355:
                        {
                            return 470.0;
                        }

                    default:
                        {
                            return 0.0;
                        }
                }
            }
        }
        public double fy40
        {
            get
            {
                switch (this.steelGrade)
                {
                    case SteelGrade.S235:
                        {
                            return 215.0;
                        }
                    case SteelGrade.S275:
                        {
                            return 255.0;
                        }

                    case SteelGrade.S355:
                        {
                            return 335.0;
                        }

                    default:
                        {
                            return 0.0;
                        }
                }
            }
        }
        public double beta
        {
            get
            {
                switch (this.steelGrade)
                {
                    case SteelGrade.S235:
                        {
                            return 0.8;
                        }
                    case SteelGrade.S275:
                        {
                            return 0.85;
                        }

                    case SteelGrade.S355:
                        {
                            return 0.9;
                        }
                    default:
                        {
                            return 0.0;
                        }
                }
            }
        }

    

        public MaterialSteel(Project _project,SteelGrade _steelGrade)
        {
            this.project = _project;
            _project.materials.Add(this);
            this.steelGrade = _steelGrade;
            
        }

        public Project project;

        public enum SteelGrade
        {
            S235,
            S275,
            S355,
            S435

        }
    }
}
