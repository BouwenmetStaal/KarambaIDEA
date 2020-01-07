// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.Core
{
    public class MaterialSteel
    {
        public Project project;
        public SteelGrade steelGrade = SteelGrade.S235;

        public string Name
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
        public int Id
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
        public double Fu
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
        public double Fy
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
        public double Fu40
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
        public double Fy40
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
        public double Beta
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

        public enum SteelGrade
        {
            S235,
            S275,
            S355,
            S435
        }
    }
}
