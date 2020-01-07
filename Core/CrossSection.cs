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

        public enum Shape
        {
            ISection,
            SHSSection,
            CHSsection
        }
    }
}
