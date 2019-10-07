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
    public class Vector
    {
        public double X;
        public double Y;
        public double Z;

        public double length
        {
            get
            {
                return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
            }
        }


        public Vector()
        {

        }

        public Vector(Point startpoint, Point endpoint)
        {
            this.X = endpoint.X - startpoint.X;
            this.Y = endpoint.Y - startpoint.Y;
            this.Z = endpoint.Z - startpoint.Z;
        }

        public Vector(double _X, double _Y, double _Z)
        {
            this.X = _X;
            this.Y = _Y;
            this.Z = _Z;
        }

        //public unitVector()

        public Vector Unitize()
        {
            double le = this.length;
            this.X = this.X / le;
            this.Y = this.Y / le;
            this.Z = this.Z / le;
            return this;
        }
        /// <summary>
        /// In this method two vectors are compared with each other. If the unitvector is equal or the inverse is equal, the method will return true.
        /// </summary>
        /// <param name="tol"></param>
        /// <param name="vectorA"></param>
        /// <param name="vectorB"></param>
        /// <returns></returns>
        static public bool AreVectorsEqual(double tol, Vector vectorA, Vector vectorB)
        {

            Vector a = vectorA.Unitize();
            Vector b = vectorB.Unitize();
            //same direction
            if (Math.Abs(a.X - b.X) < tol && Math.Abs(a.Y - b.Y) < tol && Math.Abs(a.Z - b.Z) < tol)
            {
                return true;
            }
            //opposite direction
            if (Math.Abs(a.X + b.X) < tol && Math.Abs(a.Y + b.Y) < tol && Math.Abs(a.Z + b.Z) < tol)
            {
                return true;
            }

            else
            {
                return false;
            }



        }

        static public double AngleBetweenVectors(Vector eerste, Vector tweede)
        {
            double ans = ((eerste.X * tweede.X) + (eerste.Y * tweede.Y) + (eerste.Z * tweede.Z)) / (eerste.length * tweede.length);
            //angle will be in most cases the smallest angle between the vectors.
            double angle = Math.Acos(ans);
            //reflexangle will be in most cases the largest angle between the vectors.
            double reflexAngle = (Math.PI * 2) - angle;
            //we only need the smallest angle between the vectors
            return Math.Min(angle, reflexAngle);

        }

        static public Vector FlipVector(Vector vector)
        {
            Vector invers = new Vector(-vector.X, -vector.Y, -vector.Z);
            return invers;
        }

        static public Vector CrossProduct(Vector a, Vector b)
        {
            double x = a.Y * b.Z - a.Z * b.Y;
            double y = a.Z * b.X - a.X * b.Z;
            double z = a.X * b.Y - a.Y * b.X;
            Vector vector = new Vector(x, y, z);
            return vector;
        }

        static public double DotProduct(Vector a, Vector b)
        {
            double scalar = a.X*b.X+a.Y*b.Y+a.Z*b.Z;
            return scalar;
        }

        static public Vector VecScalMultiply(Vector vec, double scalar)
        {
            Vector vector = new Vector(vec.X * scalar, vec.Y * scalar, vec.Z * scalar);
            return vector;
        }

        /// <summary>
        /// This method rotates a the vector v arount the vector-axis n by degree degrees.
        /// </summary>
        /// <param name="n">axis to rotate around</param>
        /// <param name="degree">rotation in degrees</param>
        /// <param name="v">vector to rotate</param>
        /// <returns></returns>
        static public Vector RotateVector(Vector n, double degree, Vector v)
        {
            double rad = System.Math.PI * (degree / 180.0);
            //n should be converted to a unit-vector
            n = n.Unitize();

            //Rodrigues' rotation formula
            Vector p1 = VecScalMultiply(v, System.Math.Cos(rad));
            Vector p2 = VecScalMultiply(VecScalMultiply(n, DotProduct(v, n)), (1.0 - System.Math.Cos(rad)));
            Vector p3 = VecScalMultiply(CrossProduct(n, v), System.Math.Sin(rad));

            Vector vector = new Vector(p1.X + p2.X + p3.X, p1.Y + p2.Y + p3.Y, p1.Z + p2.Z + p3.Z);
            
            return vector;
          
        }
    }
}
