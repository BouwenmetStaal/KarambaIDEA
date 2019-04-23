using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.Core
{
    public class VectorRAZ
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


        public VectorRAZ()
        {

        }

        public VectorRAZ(PointRAZ startpoint, PointRAZ endpoint)
        {
            this.X = endpoint.X - startpoint.X;
            this.Y = endpoint.Y - startpoint.Y;
            this.Z = endpoint.Z - startpoint.Z;
        }

        public VectorRAZ(double _X, double _Y, double _Z)
        {
            this.X = _X;
            this.Y = _Y;
            this.Z = _Z;
        }

        //public unitVector()

        public VectorRAZ Unitize()
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
        static public bool AreVectorsEqual(double tol, VectorRAZ vectorA, VectorRAZ vectorB)
        {

            VectorRAZ a = vectorA.Unitize();
            VectorRAZ b = vectorB.Unitize();
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

        static public double AngleBetweenVectors(VectorRAZ eerste, VectorRAZ tweede)
        {
            double ans = ((eerste.X * tweede.X) + (eerste.Y * tweede.Y) + (eerste.Z * tweede.Z)) / (eerste.length * tweede.length);
            //angle will be in most cases the smallest angle between the vectors.
            double angle = Math.Acos(ans);
            //reflexangle will be in most cases the largest angle between the vectors.
            double reflexAngle = (Math.PI * 2) - angle;
            //we only need the smallest angle between the vectors
            return Math.Min(angle, reflexAngle);

        }

        static public VectorRAZ FlipVector(VectorRAZ vector)
        {
            VectorRAZ invers = new VectorRAZ(-vector.X, -vector.Y, -vector.Z);
            return invers;
        }
    }
}
