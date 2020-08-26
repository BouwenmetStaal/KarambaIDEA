// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Linq;
using System.Collections.Generic;

using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using KarambaIDEA.Core;
using KarambaIDEA.IDEA;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

namespace KarambaIDEA
{
    public class RetrieveConnectionProperties : GH_Component
    {
        public RetrieveConnectionProperties() : base("Retrieve Connection Properties", "RetConProp", "Retrieve Connection Properties", "KarambaIDEA", "3. Project utilities")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "P", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Message", "M", "", GH_ParamAccess.list);

            pManager.AddNumberParameter("start Sj", "S Sj", "Sj at start of element", GH_ParamAccess.list);
            pManager.AddNumberParameter("end Sj", "E Sj", "Sj at end of element", GH_ParamAccess.list);

            pManager.AddNumberParameter("start Mj,Rd", "S Mj,Rd", "Mj,Rd at start of element", GH_ParamAccess.list);
            pManager.AddNumberParameter("end Mj,Rd", "E Mj,Rd", "Mj,Rd at end of element", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables      
            Project project = new Project();

            //Output variables
            List<string> messages = new List<string>();
            List<double> startSjs = new List<double>();
            List<double> endSjs = new List<double>();
            List<double> startMjrds = new List<double>();
            List<double> endMjrds = new List<double>();

            //Link input
            DA.GetData(0, ref project);
            

            foreach (Element ele in project.elements)
            {
                startSjs.Add(ele.startProperties.Sj);
                startMjrds.Add(ele.startProperties.Mjrd);

                endSjs.Add(ele.endProperties.Sj);
                endMjrds.Add(ele.endProperties.Mjrd);
            }

            //messages = project.MakeTemplateJointMessage();

            //link output
            DA.SetDataList(0, messages);
            DA.SetDataList(1, startSjs);
            DA.SetDataList(2, endSjs);
            DA.SetDataList(3, startMjrds);
            DA.SetDataList(4, endMjrds);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.StiffnessDiagram;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("967bcd5c-889f-4514-be8c-d82b4fb63c37"); }
        }
    }
}
