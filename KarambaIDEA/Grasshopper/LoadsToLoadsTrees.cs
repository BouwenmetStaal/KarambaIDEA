// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal, ABT bv. Please see the LICENSE file	
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
    public class LoadsToLoadsTrees: GH_Component
    {
        public LoadsToLoadsTrees() : base("Convert Excel Loads", "CEL", "Convert loads from Excel into dataTree format", "KarmabaIDEA", "KarambaIDEA")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Element ID's", "Element ID's", "Element ID's", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Entry Element ID", "Entry Element ID", "Entry Element ID", GH_ParamAccess.list);
            pManager.AddTextParameter("Entry Loadcase", "Entry Loadcase", "Loadcase", GH_ParamAccess.list);
            
            pManager.AddNumberParameter("Start: N", "Start: N", "Normal force [kN]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Start: Vz", "Start: Vz", "Shear force z-direction[kN]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Start: Vy", "Start: Vy", "Shear force y-direction[kN]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Start: Mt", "Start: Mt", "Torsional force[kNm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Start: My", "Start: My", "Moment force y-direction[kN]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Start: Mz", "Start: Mz", "Moment force z-direction[kN]", GH_ParamAccess.list);

            pManager.AddNumberParameter("End: N", "End: N", "Normal force [kN]", GH_ParamAccess.list);
            pManager.AddNumberParameter("End: Vz", "End: Vz", "Shear force z-direction[kN]", GH_ParamAccess.list);
            pManager.AddNumberParameter("End: Vy", "End: Vy", "Shear force y-direction[kN]", GH_ParamAccess.list);
            pManager.AddNumberParameter("End: Mt", "End: Mt", "Torsional force[kNm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("End: My", "End: My", "Moment force y-direction[kN]", GH_ParamAccess.list);
            pManager.AddNumberParameter("End: Mz", "End: Mz", "Moment force z-direction[kN]", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("#Loadcases found", "#Loadcases found", "Number of loadcases found", GH_ParamAccess.item);
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
            List<int> EleIDs = new List<int>();
            List<int> EntryEleID = new List<int>();
            List<string> EntryLoadcase = new List<string>();

            List<double> Start_N = new List<double>();
            List<double> Start_Vz = new List<double>();
            List<double> Start_Vy = new List<double>();
            List<double> Start_Mt = new List<double>();
            List<double> Start_My = new List<double>();
            List<double> Start_Mz = new List<double>();

            List<double> End_N = new List<double>();
            List<double> End_Vz = new List<double>();
            List<double> End_Vy = new List<double>();
            List<double> End_Mt = new List<double>();
            List<double> End_My = new List<double>();
            List<double> End_Mz = new List<double>();
            
            //Link input
            DA.GetDataList(0, EleIDs);
            DA.GetDataList(1, EntryEleID);
            DA.GetDataList(2, EntryLoadcase);

            DA.GetDataList(3, Start_N);
            DA.GetDataList(4, Start_Vz);
            DA.GetDataList(5, Start_Vy);
            DA.GetDataList(6, Start_Mt);
            DA.GetDataList(7, Start_My);
            DA.GetDataList(8, Start_Mz);

            DA.GetDataList(9, End_N);
            DA.GetDataList(10, End_Vz);
            DA.GetDataList(11, End_Vy);
            DA.GetDataList(12, End_Mt);
            DA.GetDataList(13, End_My);
            DA.GetDataList(14, End_Mz);

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
            foreach(string loadcaseName in EntryLoadcase)
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
                for (int b=0; b < EleIDs.Count; b++)
                {
                    //Loop over datalist of all elements and loadcases
                    for (int c=0; c<EntryEleID.Count;c++)                    
                    {
                        //find dataline of specified element and loadcas
                        if (EntryEleID[c] == EleIDs[b] && EntryLoadcase[c] == UniqueLC[i])
                        {
                            GH_Path path = new GH_Path(i, b);
                            N.Add(Start_N[c], path);
                            N.Add(End_N[c], path);

                            Vz.Add(Start_Vz[c], path);
                            Vz.Add(End_Vz[c], path);

                            Vy.Add(Start_Vy[c], path);
                            Vy.Add(End_Vy[c], path);

                            Mt.Add(Start_Mt[c], path);
                            Mt.Add(End_Mt[c], path);
                          
                            My.Add(Start_My[c], path);
                            My.Add(End_My[c], path);

                            Mz.Add(Start_Mz[c], path);
                            Mz.Add(End_Mz[c], path);
                        }
                    }                    
                }
            }
            
            int LCcount = UniqueLC.Count;
            //link output 
            DA.SetData(0, LCcount);
            DA.SetDataTree(1, N);
            DA.SetDataTree(2, Vz);
            DA.SetDataTree(3, Vy);
            DA.SetDataTree(4, Mt);
            DA.SetDataTree(5, My);
            DA.SetDataTree(6, Mz);        
        }
        /*
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.KarambaIDEAviewer_logo;

            }
        }
        */
        public override Guid ComponentGuid
        {
            get { return new Guid("1a25b1bd-5ab1-4ae1-99e0-d6216887ac14"); }
        }
    }
}
