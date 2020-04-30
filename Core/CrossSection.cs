// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System.Linq;
using System;
using System.Collections.Generic;

namespace KarambaIDEA.Core
{
    public class CrossSection
    {
        public Project project;
        public string name;
        public Shape shape;
        public MaterialSteel material;

        public double height;
        public double width;
        public double thicknessFlange;
        public double thicknessWeb;
        public double radius;

        public int Id
        {
            get
            {
                return this.project.crossSections.IndexOf(this) + 1;//IDEA count from one
            }
        }

        public enum Shape
        {
            ISection,
            SHSSection,
            CHSsection,
            Tsection,
            Strip
        }

        public CrossSection()
        {

        }

        public CrossSection(Project _project,string _name, Shape _shape, MaterialSteel _material, double _height, double _width, double _thicknessFlange, double _thicknessWeb, double _radius)
        {
            this.name = _name;
            this.shape = _shape;
            this.material = _material;
            
            this.height = _height;
            this.width = _width;
            this.thicknessFlange = _thicknessFlange;
            this.thicknessWeb = _thicknessWeb;
            this.radius = _radius;

            this.project = _project;
            _project.crossSections.Add(this);


        }
        /// <summary>
        /// Create crosssection only if the cross-section does not exist yet in the project
        /// </summary>
        /// <param name="_project"></param>
        /// <param name="_name"></param>
        /// <param name="_shape"></param>
        /// <param name="_material"></param>
        /// <param name="_height"></param>
        /// <param name="_width"></param>
        /// <param name="_thicknessFlange"></param>
        /// <param name="_thicknessWeb"></param>
        /// <param name="_radius"></param>
        /// <returns></returns>
        public static CrossSection CreateNewOrExisting(Project _project, string _name, Shape _shape, MaterialSteel _material, double _height, double _width, double _thicknessFlange, double _thicknessWeb, double _radius)
        {
            
            double tol = Project.tolerance;
            CrossSection p = _project.crossSections.Where(a => a.name == _name  && a.material == _material).FirstOrDefault();
            if (p == null)
                p = new CrossSection(_project, _name, _shape, _material, _height, _width, _thicknessFlange, _thicknessWeb, _radius);
            return p;
        }

        

        public double Iyy()
        {
            if (this.shape.Equals(Shape.ISection) | this.shape.Equals(Shape.SHSSection))
            {
                double a = Inertia(this.width, this.height);
                double b = Inertia(this.width - this.thicknessWeb, this.height-2*this.thicknessFlange);
                return a - b;                   
            }
            else
            {
                throw new ArgumentNullException("Iyy for this Cross-section not implemented");
            }
        }
        /// <summary>
        /// Calculate area in mm2
        /// </summary>
        /// <returns></returns>
        public double Area()
        {
            if (this.shape.Equals(Shape.ISection))
            {
                double a = this.width * this.height;
                double b = (this.width - 2 * this.thicknessWeb) * (this.height -this.thicknessFlange);
                return a - b;
            }
            if (this.shape.Equals(Shape.SHSSection))
            {
                double a = this.width*this.height;
                double b = (this.width-2*this.thicknessWeb) * (this.height-2*this.thicknessFlange);
                return a - b;
            }
            if (this.shape.Equals(Shape.CHSsection))
            {
                double a = Math.PI*Math.Pow(this.height,2)/4;
                double b = Math.PI * Math.Pow(this.height-2*this.thicknessWeb, 2) / 4;
                return a - b;
            }
            if (this.shape.Equals(Shape.Tsection))
            {
                double a = this.width * this.height;
                double b = (this.width - 2 * this.thicknessWeb) * (this.height - this.thicknessFlange);
                return (a - b)/2;
            }
            if (this.shape.Equals(Shape.Strip))
            {
                double a = this.width * this.height;
                return a;
            }
            else
            {
                throw new ArgumentNullException("Area for this Cross-section not implemented");
            }
        }
        public double Inertia(double b, double h)
        {
            return (1 / 12) * b * Math.Pow(h, 2);
        }
        
    }
}
