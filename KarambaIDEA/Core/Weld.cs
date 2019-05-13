// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal, ABT bv. Please see the LICENSE file	
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
        public double size;
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

        static public double CalcFullStrengthFactor(MaterialSteel materialSteel, double angle)
        {
            //The full strenth factor returned is the factor for single fillet welds
            //In case of double fillet welds take halve of the factor
            //Calculation is made per 1 mm length piece

            double angleHalve = 0.5 * angle;//angle halved
            double beta = materialSteel.beta;
            double M2 = Project.gammaM2;
            double fy = materialSteel.fy;
            double fu = materialSteel.fu;

            double tuss = (2 * Math.Pow(Math.Cos(angleHalve), 2) + 1);

            double numerator = Math.Pow(beta, 2) * Math.Pow(M2, 2) * Math.Pow(fy, 2) * (2 * Math.Pow(Math.Cos(angleHalve), 2) + 1);
            double denominator = Math.Pow(fu, 2);
            double fullStrengthFactor = Math.Sqrt(numerator / denominator);

            return fullStrengthFactor;
        }

        static public double CalcDirFlangeThroat(MaterialSteel materialSteel, double angle, double N)
        {
            //The full strenth factor returned is the factor for single fillet welds
            //In case of double fillet welds take halve of the factor
            //Calculation is made per 1 mm length piece

            double angleHalve = 0.5 * angle;//angle halved
            double beta = materialSteel.beta;
            double M2 = Project.gammaM2;
            double fy = materialSteel.fy;
            double fu = materialSteel.fu;

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
