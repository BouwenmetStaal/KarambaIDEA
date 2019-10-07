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
    public class Point
    {
        public string name;
        public double X;
        public double Y;
        public double Z;
        public Project project;
        
        public int id
        {
            get
            {
                return this.project.points.IndexOf(this)+1;//IDEA count from one
            }
        }

        public Point()
        {

        }

        public Point(Project _project, double _X, double _Y, double _Z)
        {
            this.X = _X;
            this.Y = _Y;
            this.Z = _Z;
            this.project = _project;
            this.project.points.Add(this);
        }

       

        /// <summary>
        /// Creates a point at specified coordinates or finds an existing point within given tolerances
        /// </summary>
        /// <param name="_project">The project the point belongs to</param>
        /// <param name="_x">The X coordinate of the point </param>
        /// <param name="_y"> The Y coordinate of the point</param>
        /// <param name="_z">The Z coordinate of the point</param>
        /// <param name="tol">Tolerance within an existing point will be returned</param>
        /// <returns></returns>
        public static Point CreateNewOrExisting(Project _project, double _x, double _y, double _z)
        {
            double tol = Project.tolerance;
            Point p = _project.points.Where(a => Math.Abs(a.X - _x) <= tol && Math.Abs(a.Y - _y) <= tol && Math.Abs(a.Z - _z) <= tol).FirstOrDefault();
            if (p == null)
                p = new Point(_project, _x, _y, _z);
            return p;
        }

        public Point(string _name, double _X, double _Y, double _Z)
        {
            this.name = _name;
            this.X = _X;
            this.Y = _Y;
            this.Z = _Z;
        }

        public Point(double _X, double _Y, double _Z)
        {
            this.X = _X;
            this.Y = _Y;
            this.Z = _Z;
        }



        static public bool ArePointsEqual(double tol, Point a, Point b)
        {
            if (Math.Abs(a.X - b.X) < tol && Math.Abs(a.Y - b.Y) < tol && Math.Abs(a.Z - b.Z) < tol)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 0
        /// </summary>
        /// <param name="centerpoint">Centerpoint of Joint</param>
        /// <param name="point">Point that will be moved</param>
        /// <returns></returns>
        public static Point MovePointToOrigin(Point centerpoint, Point point)
        {
            double x = point.X - centerpoint.X;
            double y = point.Y - centerpoint.Y;
            double z = point.Z - centerpoint.Z;
            Point newpoint = new Point(x, y, z);
            return newpoint;
        }
    }
}
