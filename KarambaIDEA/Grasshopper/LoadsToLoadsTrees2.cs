// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Collections.Generic;

using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;

namespace KarambaIDEA.Grasshopper
{
    public class LoadsToLoadsTrees2 : GH_Component
    {
        public LoadsToLoadsTrees2() : base("Convert Excel Loads", "CEL", "Convert loads from Excel into dataTree format", "KarmabaIDEA", "KarambaIDEA")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Element Name", "Element Name", "Element Name", GH_ParamAccess.list);
            pManager.AddNumberParameter("Element Length", "Element Length", "Length of element in meters", GH_ParamAccess.list);

            pManager.AddTextParameter("Entry Element Name", "Entry Element Name", "Entry Element Name", GH_ParamAccess.list);
            pManager.AddTextParameter("Entry Loadcase", "Entry Loadcase", "Loadcase", GH_ParamAccess.list);
            pManager.AddNumberParameter("Entry Length", "Entry Length", "Length of element entry in meters", GH_ParamAccess.list);

            pManager.AddNumberParameter("Entry: N", "Entry: N", "Normal force [kN]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Entry: Vz", "Entry: Vz", "Shear force z-direction[kN]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Entry: Vy", "Entry: Vy", "Shear force y-direction[kN]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Entry: Mt", "Entry: Mt", "Torsional force[kNm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Entry: My", "Entry: My", "Moment force y-direction[kN]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Entry: Mz", "Entry: Mz", "Moment force z-direction[kN]", GH_ParamAccess.list);            
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("#Loadcases found", "#Loadcases found", "Number of loadcases found", GH_ParamAccess.item);
            pManager.AddTextParameter("LoadcaseNames", "LoadcaseNames", "Names of loadcases", GH_ParamAccess.list);
            pManager.AddNumberParameter("N", "N", "Normal force [kN]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Vz", "Vz", "Shear force z-direction[kN]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Vy", "Vy", "Shear force y-direction[kN]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Mt", "Mt", "Torsional force[kNm]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("My", "My", "Moment force y-direction[kN]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Mz", "Mz", "Moment force z-direction[kN]", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Input variables
            List<string> EleIDs = new List<string>();
            List<double> elementlength = new List<double>();

            List<string> EntryEleID = new List<string>();
            List<string> EntryLoadcase = new List<string>();
            List<double> Entrylength = new List<double>();

            List<double> Entry_N = new List<double>();
            List<double> Entry_Vz = new List<double>();
            List<double> Entry_Vy = new List<double>();
            List<double> Entry_Mt = new List<double>();
            List<double> Entry_My = new List<double>();
            List<double> Entry_Mz = new List<double>();
            

            //Link input
            DA.GetDataList(0, EleIDs);
            DA.GetDataList(1, elementlength);
            DA.GetDataList(2, EntryEleID);
            DA.GetDataList(3, EntryLoadcase);
            DA.GetDataList(4, Entrylength);

            DA.GetDataList(5, Entry_N);
            DA.GetDataList(6, Entry_Vz);
            DA.GetDataList(7, Entry_Vy);
            DA.GetDataList(8, Entry_Mt);
            DA.GetDataList(9, Entry_My);
            DA.GetDataList(10, Entry_Mz);

            
            //output variables            
            DataTree<double> N = new DataTree<double>();
            DataTree<double> Vz = new DataTree<double>();
            DataTree<double> Vy = new DataTree<double>();
            DataTree<double> Mt = new DataTree<double>();
            DataTree<double> My = new DataTree<double>();
            DataTree<double> Mz = new DataTree<double>();

            //6 Data trees are created in which the order is according the element ID's order
            //Each branch within the data tree is a different loadcase

            //First find unique loadcases
            List<string> UniqueLC = new List<string>();
            foreach (string loadcaseName in EntryLoadcase)
            {
                if (!UniqueLC.Contains(loadcaseName))
                {
                    UniqueLC.Add(loadcaseName);
                }
            }
            //Loop over unique loadcases
            for (int i = 0; i < UniqueLC.Count; i++)
            {
                //Loop over unique elements
                for (int b = 0; b < EleIDs.Count; b++)
                {
                    //Loop over datalist of all elements and loadcases
                    for (int c = 0; c < EntryEleID.Count; c++)
                    {
                        //find dataline of specified element and loadcas
                        if (EntryEleID[c] == EleIDs[b] && EntryLoadcase[c] == UniqueLC[i] && Entrylength[c]==0.0)
                        {
                            GH_Path path = new GH_Path(i, b);
                            N.Add(Entry_N[c], path);
                            Vz.Add(Entry_Vz[c], path);
                            Vy.Add(Entry_Vy[c], path);
                            Mt.Add(Entry_Mt[c], path);
                            My.Add(Entry_My[c], path);
                            Mz.Add(Entry_Mz[c], path);
                        }
                        if (EntryEleID[c] == EleIDs[b] && EntryLoadcase[c] == UniqueLC[i] &&  elementlength[b].ToString().StartsWith(Entrylength[c].ToString()))
                        {
                            GH_Path path = new GH_Path(i, b);
                            N.Add(Entry_N[c], path);
                            Vz.Add(Entry_Vz[c], path);
                            Vy.Add(Entry_Vy[c], path);
                            Mt.Add(Entry_Mt[c], path);
                            My.Add(Entry_My[c], path);
                            Mz.Add(Entry_Mz[c], path);
                        }
                    }
                }
            }

            int LCcount = UniqueLC.Count;
            //link output 
            DA.SetData(0, LCcount);
            DA.SetDataList(1, UniqueLC);
            DA.SetDataTree(2, N);
            DA.SetDataTree(3, Vz);
            DA.SetDataTree(4, Vy);
            DA.SetDataTree(5, Mt);
            DA.SetDataTree(6, My);
            DA.SetDataTree(7, Mz);
        }
        
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.KarambaIDEA_logo_excel;

            }
        }
        
        public override Guid ComponentGuid
        {
            get { return new Guid("d4d103e1-d6b1-4117-8aa6-2f1ce8f5310d"); }
        }
    }
}
