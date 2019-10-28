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

    public class Weld
    {
        /// <summary>
        /// Weld Ids, to be set according to Idea. 
        /// </summary>
		public List<int> Ids = new List<int>();
        public WeldType weldType;
        private double _size;
        public double Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = Math.Ceiling(value);
            }
        }
        public double unitycheck;

        public enum WeldType
        {
            NotSpecified,
            Fillet,
            FilletRear,
            DoubleFillet,
            Bevel
        }

        static public double CalcWeldSurface(double angle, double throat)
        {
            double alpha = angle / 2;
            double x = Math.Tan(alpha) * throat;
            double surface = throat * x;

            return surface;
        }

        /// <summary>
        /// In this method the full strength factor for welds is determined. 
        /// This full strength factor can by multiplied with the thickness of the plate to generate the weld throat.
        /// The angle between the connected parts can be taken into account. For right angled connections use an angle of 90 degrees.
        /// The full equation can be found on page 61, equation 6.10 https://repository.tudelft.nl/islandora/object/uuid%3A8e8835b3-171c-471e-8ff4-e9c3e5c8b148
        /// </summary>
        /// <param name="cross">Cross-section of the connected member</param>
        /// <param name="angle">Angle between connected parts in degrees</param>
        /// <returns></returns>
        static public double CalcFullStrengthFactor(CrossSection cross, double angle)
        {
            //The full strenth factor returned is the factor for single fillet welds

            //Calculation is made per 1 mm length piece

            double angleHalve = (Math.PI / 180) * (0.5 * angle);//angle halved and converted to radians
            double beta = cross.material.beta;
            double M2 = Project.gammaM2;
            double fy = cross.material.fy;
            double fu = cross.material.fu;

            //tuss is function for debugging purposes
            double tuss = (2 * Math.Pow(Math.Cos(angleHalve), 2) + 1);

            double numerator = Math.Pow(beta, 2) * Math.Pow(M2, 2) * Math.Pow(fy, 2) * (2 * Math.Pow(Math.Cos(angleHalve), 2) + 1);
            double denominator = Math.Pow(fu, 2);
            double fullStrengthFactor = Math.Sqrt(numerator / denominator);

            if(cross.shape == CrossSection.Shape.ISection)
            {
                //In case of double fillet welds take halve of the factor
                fullStrengthFactor = 0.5 * fullStrengthFactor;
            }
            else
            {
                //Hollow sections have a single weld. Therefore, no reduction.
            }
            

            return fullStrengthFactor;
        }

        static public void CalcFullStrengthWelds(ConnectingMember con)
        {
            CrossSection cross = con.element.crossSection;
            double factor = Weld.CalcFullStrengthFactor(cross, 90);//angle of 90 degrees
            con.webWeld.Size = cross.thicknessWeb * factor;
            con.flangeWeld.Size = cross.thicknessFlange * factor;
        }

        static public double CalcDirFlangeThroat(MaterialSteel materialSteel, double angle, double N)
        {
            //The full strenth factor returned is the factor for single fillet welds
            //In case of double fillet welds take halve of the factor
            //Calculation is made per 1 mm length piece

            double angleHalve = (Math.PI/180)* (0.5 * angle);//angle halved and converted to radians
            double beta = materialSteel.beta;
            double M2 = Project.gammaM2;
            double fy = materialSteel.fy;
            double fu = materialSteel.fu;

            //tuss is function for debugging purposes
            double tuss = (2 * Math.Pow(Math.Cos(angleHalve), 2) + 1);

            double numerator = Math.Pow(beta, 2) * Math.Pow(M2, 2) * Math.Pow(N, 2) * (2 * Math.Pow(Math.Cos(angleHalve), 2) + 1);
            double denominator = Math.Pow(fu, 2);
            double throat = Math.Sqrt(numerator / denominator);

            return throat;
        }

        static public double CalcDirWebThroat(MaterialSteel materialSteel, double angle, double N)
        {
            //The full strenth factor returned is the factor for single fillet welds
            //In case of double fillet welds take halve of the factor
            //Calculation is made per 1 mm length piece

            double angleHalve = angle;//angle NOT halved
            double beta = materialSteel.beta;
            double M2 = Project.gammaM2;
            double fy = materialSteel.fy;
            double fu = materialSteel.fu;

            double tuss = (2 * Math.Pow(Math.Cos(angleHalve), 2) + 1);

            double numerator = Math.Pow(beta, 2) * Math.Pow(M2, 2) * Math.Pow(N, 2) * (Math.Pow(Math.Cos(angleHalve), 2) + 2);
            double denominator = Math.Pow(fu, 2);
            double throat = Math.Sqrt(numerator / denominator);

            return throat;
        }

        
    }
}
