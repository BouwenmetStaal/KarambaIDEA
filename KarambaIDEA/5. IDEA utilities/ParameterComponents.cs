using System;
using System.Linq;
using System.Collections.Generic;

using Rhino.Geometry;
using KarambaIDEA.IDEA.Parameters;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using KarambaIDEA.Core;
using KarambaIDEA.IDEA;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using System.IO;

namespace KarambaIDEA.Grasshopper
{
    public class SetParameter : GH_Component
    {
        public SetParameter() : base("Set Param Value", "SetParam", "Set the Value of a Parameter", "KarambaIDEA", "6. IDEA Connection") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Parameter", "P", "Parameter", GH_ParamAccess.item);
            pManager.AddGenericParameter("Value", "V", "Value to Set to Parameter", GH_ParamAccess.item);
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.quinary | GH_Exposure.obscure; } }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Parameter", "P", "Updated Parmater", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_IdeaParameter ghParam = null;

            if (DA.GetData<GH_IdeaParameter>(0, ref ghParam))
            {
                IGH_Goo value = null;
                DA.GetData<IGH_Goo>(1, ref value);

                string textvalue = value.ToString();

                IIdeaParameter param = ghParam.Value;

                if (param is IdeaParameterInt intparam)
                {
                    int id;
                    GH_Convert.ToInt32(value, out id, GH_Conversion.Both);
                    IdeaParameterInt clone = new IdeaParameterInt(intparam.Clone() as parameter);
                    clone.SetValue(id);
                    DA.SetData(0, new GH_IdeaParameter(clone));
                    return;
                }
                else if (param is IdeaParameterFloat floatparam)
                {
                    double number;
                    GH_Convert.ToDouble(value, out number, GH_Conversion.Both);
                    IdeaParameterFloat clone = new IdeaParameterFloat(floatparam.Clone() as parameter);
                    clone.SetValue(number);
                    DA.SetData(0, new GH_IdeaParameter(clone));
                    return;
                }
                else
                {
                    IdeaParameterString clone = new IdeaParameterString(param.Clone() as parameter);
                    clone.SetValue(textvalue);
                    DA.SetData(0, new GH_IdeaParameter(clone));
                    return;
                }
            }
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.SetParameterValue; } }
        public override Guid ComponentGuid { get { return new Guid("65D37365-755F-4998-99CD-FDA73C8E2788"); } }
    }
}
