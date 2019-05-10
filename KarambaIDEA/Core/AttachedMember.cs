// Copyright (c) 2019 Rayaan Ajouz. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;


namespace KarambaIDEA.Core
{
    public class AttachedMember
    {

        public ElementRAZ ElementRAZ;
        public VectorRAZ distanceVector;
        public bool isStartPoint;
        public LineRAZ ideaLine;
        public int ideaOperationID;
        public bool platefailure = true;



    }

    public class BearingMember : AttachedMember
    {
        public Nullable<bool> isSingle;

        public BearingMember()
        {

        }

        public BearingMember(ElementRAZ _elementRAZ, VectorRAZ _distancevector, bool _isStartPoint, LineRAZ _idealine, Nullable<bool> _isSingle = null)
        {
            this.ElementRAZ = _elementRAZ;
            this.distanceVector = _distancevector;
            this.isStartPoint = _isStartPoint;
            this.ideaLine = _idealine;

            this.isSingle = _isSingle;
        }
    }

    public class ConnectingMember : AttachedMember
    {
        public double localEccentricity;

        //public List<int> OperationIds;

        public Weld flangeWeld = new Weld();
        public Weld webWeld = new Weld();
        public double angleWithBear = new double();
        

        public ConnectingMember(ElementRAZ _elementRAZ, VectorRAZ _distancevector, bool _isStartPoint, LineRAZ _idealine, double _localEccentricity)
        {
            this.ElementRAZ = _elementRAZ;
            this.distanceVector = _distancevector;
            this.isStartPoint = _isStartPoint;
            this.ideaLine = _idealine;

            this.localEccentricity = _localEccentricity;




        }
        static public double WebWeldFirstAttachedLength(double a, double phi)
        {
            double answer = 2 * Math.Abs(a / Math.Sin(phi));
            return answer;
        }


        private static void CreateVectorCoordinates(double a, double b, double h, double theta, double phi, double e, out double dplA, out double dblP, out double dblQ, out double dblE, out double dblI)
        {
            dplA = (a + Math.Cos(phi) * b) / Math.Sin(phi);
            dblP = (Math.Cos(theta) * b + Math.Sin(theta) * e - h) / Math.Sin(theta);
            dblQ = (Math.Cos(theta) * b + Math.Sin(theta) * e + h) / Math.Sin(theta);

            dblE = ((Math.Sin(phi) * e - a) * Math.Sin(theta) + Math.Sin(phi) * h) / (Math.Sin(theta) * Math.Cos(phi) - Math.Cos(theta) * Math.Sin(phi));
            dblI = ((Math.Sin(phi) * e - a) * Math.Sin(theta) - Math.Sin(phi) * h) / (Math.Sin(theta) * Math.Cos(phi) - Math.Cos(theta) * Math.Sin(phi));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a">Half Height Vertical</param>
        /// <param name="b">Half Height Horizontal</param>
        /// <param name="h">Half Height Diagonal</param>
        /// <param name="theta">angle of diagonal, range is between 0 and 90 degrees</param>
        /// <param name="phi">angle of vertical, range is between 90 and 180 degrees, default =pi/2</param>
        /// <param name="e">eccentricity, default = 0</param>
        /// <returns></returns>
        static public double WebWeldsHorizontalLength(double a, double b, double h, double theta, double phi = Math.PI / 2, double e = 0.0)
        {
            double dblA, dblP, _dblQ, dblE, dblI;
            CreateVectorCoordinates(a, b, h, theta, phi, e, out dblA, out dblP, out _dblQ, out dblE, out dblI);
            double length;

            if (dblP < dblA)
            {
                if (_dblQ > dblA)
                {
                    length = Math.Sqrt((Math.Pow(Math.Sin(theta) * Math.Cos(phi) * b - Math.Sin(theta) * Math.Sin(phi) * e - Math.Cos(theta) * Math.Sin(phi) * b + Math.Sin(theta) * a - Math.Sin(phi) * h, 2)) / (((Math.Pow(Math.Sin(theta), 2)) * Math.Pow(Math.Sin(phi), 2))));
                }
                else
                {
                    length = 0;
                }

            }
            else
            {
                length = 2 * Math.Abs(h / Math.Sin(theta));
            }

            return length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a">Half Height Vertical</param>
        /// <param name="b">Half Height Horizontal</param>
        /// <param name="h">Half Height Diagonal</param>
        /// <param name="theta">angle of diagonal, range is between 0 and 90 degrees</param>
        /// <param name="phi">angle of vertical, range is between 90 and 180 degrees, default =pi/2</param>
        /// <param name="e">eccentricity, default = 0</param>
        /// <returns></returns>
        static public double WebWeldsVerticalLength(double a, double b, double h, double theta, double phi = Math.PI / 2, double e = 0.0)
        {
            double _dblA, dblP, dblQ, dblE, dblI;
            CreateVectorCoordinates(a, b, h, theta, phi, e, out _dblA, out dblP, out dblQ, out dblE, out dblI);
            double length;
            //if (dblP < _dblA)
            if (dblE < b)
            {
                //if (dblQ > _dblA)
                if (dblI > b)
                {
                    length = Math.Sqrt((-((Math.Pow((Math.Sin(theta) * Math.Cos(phi) * b - Math.Sin(theta) * Math.Sin(phi) * e - Math.Cos(theta) * Math.Sin(phi) * b + Math.Sin(theta) * a + Math.Sin(phi) * h), 2)) / (1)) / ((Math.Pow(Math.Sin(phi), 2)) * (2 * Math.Sin(theta)) * Math.Cos(theta) * Math.Cos(phi) * Math.Sin(phi) + (2 * Math.Pow(Math.Cos(theta), 2)) * (Math.Pow(Math.Cos(phi), 2)) - (Math.Pow(Math.Cos(theta), 2)) - (Math.Pow(Math.Cos(phi), 2)))));
                }
                else
                {
                    length = 0;
                }

            }
            else
            {
                length = 2 * Math.Sqrt(-(Math.Pow(h, 2)) / (((2 * Math.Pow(Math.Cos(phi), 2) - 1) * Math.Pow(Math.Cos(theta), 2) + 2 * Math.Sin(theta) * Math.Cos(theta) * Math.Cos(phi) * Math.Sin(phi) - Math.Pow(Math.Cos(phi), 2))));
            }
            return length;
        }

        static public double WebWeldTotalLength(double a, double b, double h, double theta, double phi = Math.PI / 2, double e = 0.0)
        {
            return WebWeldsHorizontalLength(a, b, h, theta, phi, e) + WebWeldsVerticalLength(a, b, h, theta, phi, e);
        }


        static public double LocalEccentricity(PointRAZ c, PointRAZ a, VectorRAZ dir)
        {
            double numerator = Math.Sqrt(Math.Pow((c.Y - a.Y) * dir.Z - (c.Z - a.Z) * dir.Y, 2) + Math.Pow((c.X - a.X) * dir.Z - (c.Z - a.Z) * dir.X, 2) + Math.Pow((c.X - a.X) * dir.Y - (c.Y - a.Y) * dir.X, 2));
            double denumerator = Math.Sqrt(Math.Pow(dir.X, 2) + Math.Pow(dir.Y, 2) + Math.Pow(dir.Z, 2));

            return numerator / denumerator;
        }




    }

}

