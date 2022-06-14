
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
using KarambaIDEA.IDEA.Parameters;
using KarambaIDEA.Grasshopper;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using System.Runtime.Remoting;
using System.ComponentModel;

namespace KarambaIDEA
{
    public class CalculateIDEA : GH_Component
    {
        private bool UserFeeback = false; 

        public CalculateIDEA() : base("Calculate Connection", "Calculate IDEA Connection", "Calculate IDEA Connection. This is a connection file that has been saved and referenced", "KarambaIDEA", "5. IDEA Connection") {}

        public override GH_Exposure Exposure { get { return GH_Exposure.tertiary; } }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Connection", "C", "Connections which have been created on disk and referenced", GH_ParamAccess.item);
            pManager.AddGenericParameter("Modifications", "M", "List of Modifications to apply to the conneciton file prior to Calculation", GH_ParamAccess.list);
            pManager.AddGenericParameter("Code Setup", "S", "Code Setup Settings as JSON string", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run IDEA", "R", "Run the Connection", GH_ParamAccess.item);

            pManager[1].Optional = pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Connection", "C", "Calculated Connections with Modification Applied", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool startIDEA = false;
            DA.GetData(3, ref startIDEA);

            if (startIDEA)
            {
                GH_IdeaConnection ghCon = null;
                DA.GetData<GH_IdeaConnection>(0, ref ghCon);

                List<GH_IdeaModification> mods = new List<GH_IdeaModification>();

                DA.GetDataList<GH_IdeaModification>(1, mods);

                IdeaConnection_2 conCopy = new IdeaConnection_2(ghCon.Value);

                GH_IdeaCodeSetup setUp = null;
                IdeaCodeSetup codeSetUp = null;
                if (DA.GetData<GH_IdeaCodeSetup>(2, ref setUp))
                {
                    codeSetUp = setUp.Value;
                }

                conCopy.CalculateConnection(mods.Select(x => x.Value).ToList(), codeSetUp, UserFeeback);

                DA.SetData(0, new GH_IdeaConnection(conCopy));
            }
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.Calculate; } }
        public override Guid ComponentGuid {  get { return new Guid("8036C064-4B56-4AA5-91E8-D54E72605566"); }  }
    }

    public class CalcCodeSetup : GH_Component
    {
        public CalcCodeSetup() : base("Connection Code Setup", "ConCodeSetup", "JSON string representing the defaults for conneciton set-up", "KarambaIDEA", "5. IDEA Connection") { }

        public override GH_Exposure Exposure { get { return GH_Exposure.tertiary; } }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Code Setup", "S", "Code Setup that has been loaded from an existing connection. If non Provided the default settings will be loaded", GH_ParamAccess.item);
            pManager.AddGenericParameter("Key", "K", "Key to Change in Set-up. Key relates to first level only.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Value", "V", "Value to Change in Set-up. All values are text", GH_ParamAccess.list);

            pManager[0].Optional = pManager[1].Optional = pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Code Setup", "S", "Calculation Code Set-up to Apply to Connection Project", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_IdeaCodeSetup ghSetup = null;

            Dictionary<string, dynamic> clone = new Dictionary<string, dynamic>();

            if (DA.GetData<GH_IdeaCodeSetup>(0, ref ghSetup))
            {
                if (ghSetup.Value != null)
                {
                    clone = new Dictionary<string, dynamic>(ghSetup.Value.CodeSetUpDictionary);
                }
            }
            else
                clone = IdeaCodeSetup.CreateDefault().CodeSetUpDictionary;

            List<string> keys = new List<string>();
            List<IGH_Goo> values = new List<IGH_Goo>();

            if (DA.GetDataList<string>(1, keys))
            {
                if (DA.GetDataList<IGH_Goo>(2, values))
                {
                    if (keys.Count == values.Count)
                    {
                        for(int i = 0; i < keys.Count; i++)
                        {
                            string stringGoo = values[i].ToString();
                            dynamic val = clone[keys[i]];

                            if (val is double)
                            {
                                double d;
                                if (GH_Convert.ToDouble(values[i], out d, GH_Conversion.Both))
                                {
                                    clone[keys[i]] = d;
                                }
                                else
                                    base.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not convert to double");

                            }
                            else if (val is int)
                            {
                                int inte;
                                if (GH_Convert.ToInt32(values[i], out inte, GH_Conversion.Both))
                                {
                                    clone[keys[i]] = inte;
                                }
                                else
                                    base.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not convert to Integer");
                            }
                            else if (val is bool)
                            {
                                bool b;
                                if (GH_Convert.ToBoolean(values[i], out b, GH_Conversion.Both))
                                {
                                    clone[keys[i]] = b;
                                }
                                else
                                    base.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not convert to Integer");
                            }
                            else
                            {
                                clone[keys[i]] = stringGoo;
                            }
                        }
                    }
                    else
                    {
                        base.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Keys and Values do not match");
                        return;
                    }
                }
            }

            IdeaCodeSetup setUp = new IdeaCodeSetup(clone);

            DA.SetData(0, new GH_IdeaCodeSetup(setUp));
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.CodeSetup; } }
        public override Guid ComponentGuid { get { return new Guid("5CC6E7B0-9EB2-413D-9775-C43BD805F08B"); } }
    }

    public class RefConnectionByFilePath : GH_Component
    {
        public RefConnectionByFilePath() : base("Reference Connection", "RefCon", "Reference an existing connection which has been created manually by a filepath", "KarambaIDEA", "5. IDEA Connection") { }

        public override GH_Exposure Exposure { get { return GH_Exposure.secondary; } }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "Filepath of existing connection file to import (.ideaCon)", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Name/s of the Connection in the Connection Project File to Reference. If No Name is provided the First connection will be Used. If multiple is provided a seperate connection reference will be created for each item.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Load Info", "L", "XXX Not Yet Implemented XXX. Attempt to Load as much information as possible from the Connection file including Parameters, Project Items, Costs. Note: This requires an avaliable IdeaStatiCa License", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Connection", "C", "Referenced connection which can be put into Calculate Component", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = "";
            List<string> items = new List<string>();
            bool load = false;

            DA.GetData<string>(0, ref path);

            if (!path.EndsWith(".ideaCon"))
            {
                base.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Incorrect filepath extension.");
                return;
            }

            List<GH_IdeaConnection> ghCons = new List<GH_IdeaConnection>();
            DA.GetData<bool>(2, ref load);

            if (DA.GetDataList<string>(1, items))
            {
                foreach (var itemName in items)
                    ghCons.Add(new GH_IdeaConnection(new IdeaConnection_2(path, -1, load, itemName)));
            }
            else
            {
                ghCons.Add(new GH_IdeaConnection(new IdeaConnection_2(path, -1, load)));
            }

            DA.SetDataList(0, ghCons);
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.ReferenceConnection; } }
        public override Guid ComponentGuid { get { return new Guid("CAF6BB4D-4DB5-4856-BECE-BF744F9E9ED4"); } }
    }

    public class DeconstructConnection : GH_Component
    {
        public DeconstructConnection() : base("Deconstruct Connection", "ConDeconstruct", "Decosntruct Information avaliable from an IDEA Connection Project", "KarambaIDEA", "5. IDEA Connection") { }

        public override GH_Exposure Exposure { get { return GH_Exposure.secondary; } }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Connection", "C", "Idea Connection", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of Connection in IDEA Connection Project", GH_ParamAccess.item);
            pManager.AddGenericParameter("Load Effects", "L", "XX NOT IMPLEMENTED. Load Effects on Connection.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Parameters", "P", "Parameters Avaliable in Connection", GH_ParamAccess.list);
            pManager.AddGenericParameter("Code Setup", "S", "Code Setup in Connection Project", GH_ParamAccess.list);
            pManager.AddTextParameter("Filepath", "P", "Filepath of the IDEA Connection Project", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_IdeaConnection ghConnection = null;

            if (DA.GetData<GH_IdeaConnection>(0, ref ghConnection))
            {
                IdeaConnection_2 connection = ghConnection.Value;

                if (connection != null)
                {
                    DA.SetData(0, connection.ConnectionNameRef);
                    //DA.SetDataList(1, summaryList);
                    List<GH_IdeaParameter> ghParams = connection.Parameters.Select(x => new GH_IdeaParameter(x)).ToList();
                    DA.SetDataList(2, ghParams);
                    
                    if(connection.CodeSetUp != null)
                    {
                        DA.SetData(3, new GH_IdeaCodeSetup(connection.CodeSetUp));
                    }
                    
                    DA.SetData(4, connection.FilePath);
                }
            }
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.DeconstructJoint; } }
        public override Guid ComponentGuid { get { return new Guid("689C58C5-0AD0-4EDE-93C4-FE6A60503AEC"); } }
    }

}
