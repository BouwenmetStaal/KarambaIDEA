// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Collections.Generic;

using Rhino.Geometry;
using System.Linq;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Reflection;


using KarambaIDEA.Core;
using KarambaIDEA.IDEA;
using Grasshopper.Kernel.Parameters;

namespace KarambaIDEA._0._Utilities
{



    public class CroSecClass : GH_Component
    {
        public CroSecClass() : base("CroSecClass", "CroSecClass", "Analyse Cross-section Class based on NEN-EN 1993-1-1 Table 5.2 and 5.3", "KarambaIDEA", "0. Utilities")
        {
            
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //Input elements
            pManager.AddTextParameter("Materials", "Materials", "Steel grade of every element", GH_ParamAccess.list);

            //Input for creating Cross-section Objects
            pManager.AddTextParameter("Cross-section names", "CroSecNames", "Name of cross-sections", GH_ParamAccess.list);
            pManager.AddTextParameter("Shapes", "Shapes", "Shape of of cross-sections", GH_ParamAccess.list);
            pManager.AddNumberParameter("Height", "Heights", "Heigth of crosssections [mm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Width", "Widths", "Width of crosssections [mm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Thickness Flanges", "TFlanges", "Flange thickness of crosssections [mm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Thickness Webs", "TWebs", "Web thickness of crosssections [mm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Radius", "FRads", "Radius of crosssections [mm]", GH_ParamAccess.list);

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("CroSecClass", "CroSecClass", "CroSecClass of Crosssection", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            List<string> crossectionsNameDirty = new List<string>();
            List<string> crossectionsName = new List<string>();

            List<string> steelgrades = new List<string>();

            List<string> groupnamesDirty = new List<string>();
            List<string> groupnames = new List<string>();

            List<string> shapesDirty = new List<string>();
            List<string> shapes = new List<string>();

            List<double> height = new List<double>();
            List<double> width = new List<double>();
            List<double> thicknessFlange = new List<double>();
            List<double> thicknessWeb = new List<double>();
            List<double> radius = new List<double>();


            


            //Link Input
            #region GrasshopperInput
           
           
            DA.GetDataList(0, steelgrades);
            DA.GetDataList(1, crossectionsNameDirty);
            DA.GetDataList(2, shapesDirty);
            DA.GetDataList(3, height);
            DA.GetDataList(4, width);
            DA.GetDataList(5, thicknessFlange);
            DA.GetDataList(6, thicknessWeb);
            DA.GetDataList(7, radius);

            #endregion

          


            //Clean cross-section list from nextline ("\r\n") command produced by Karamba
            crossectionsName = ImportGrasshopperUtils.DeleteEnterCommandsInGHStrings(crossectionsNameDirty);


            //Clean shapes list from nextline ("\r\n") command produced by Karamba
            shapes = ImportGrasshopperUtils.DeleteEnterCommandsInGHStrings(shapesDirty);


            //CREATE PROJECT
            Project projectCross = new Project("");

            List<int> crosSecClasses = new List<int>();

            //CREATE LIST OF ELEMENT OBJECTS
            for (int i = 0; i < crossectionsName.Count; i++)
            {
                MaterialSteel.SteelGrade steelGrade = (MaterialSteel.SteelGrade)Enum.Parse(typeof(MaterialSteel.SteelGrade), steelgrades[i]);
                MaterialSteel material = projectCross.materials.FirstOrDefault(a => a.steelGrade == steelGrade);
                if (material == null)
                {
                    material = new MaterialSteel(projectCross, steelGrade);
                }
                //CROSS SECTIONS
                CrossSection crosssection = projectCross.crossSections.FirstOrDefault(a => a.name == crossectionsName[i] && a.material == material);
                CrossSection.Shape shape = new CrossSection.Shape();

                if (shapes[i].StartsWith("I"))
                {
                    shape = CrossSection.Shape.ISection;
                }
                else if (shapes[i].StartsWith("[]"))
                {
                    shape = CrossSection.Shape.RHSsection;
                }
                else if (shapes[i].StartsWith("()") || shapes[i].StartsWith("O"))
                {
                    shape = CrossSection.Shape.CHSsection;
                }
                else if (shapes[i].StartsWith("T"))
                {
                    shape = CrossSection.Shape.Tsection;
                }
                else if (shapes[i].StartsWith("V"))
                {
                    shape = CrossSection.Shape.Strip;
                }
                else
                {
                    throw new ArgumentNullException(shapes[i] + " Cross-section not implemented");
                }
                if (crosssection == null)
                {
                    crosssection = new CrossSection(projectCross, crossectionsName[i], shape, material, height[i], width[i], thicknessFlange[i], thicknessWeb[i], radius[i]);
                }

                crosSecClasses.Add(crosssection.SectionClass());
            }
           

            //Link output
            DA.SetDataList(0, crosSecClasses);
        }


        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.CroSecClass_01_01;

            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("552311d5-12e3-4e4a-8f98-62254c97d267"); }
        }

    }
}
