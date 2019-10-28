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

        public int id
        {
            get
            {
                return this.project.crossSections.IndexOf(this)+1;//IDEA count from one
            }
        }

        public string name;
        //public string shape;
        public MaterialSteel material;
        public double height;
        public double width;
        public double thicknessFlange;
        public double thicknessWeb;
        public double radius;
        public Project project;
        public Shape shape;


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
            HollowSection,
            CHSsection
        }

        


        //public double CrosssectionProperties(string name)
        //{
        //    _Application excel = new _Excel.Application();
        //    Workbook wb;
        //    Worksheet ws;

        //    wb = excel.Workbooks.Open(@"C:\Users\raz\Google Drive\C#Grasshopper\TotalRayaan06-09\Translate3\CrossSectionValues.xlsx");
        //    ws = wb.Worksheets[1];
        //    for (int i=1; i < 7031; i++)
        //    {
        //        if (ws.Cells[i,4].Value2.Tostring()==name)
        //        {
        //            return ws.Cells[i, 6].Value2.Todouble();

        //        }

        //    }


        //}


    }
}
