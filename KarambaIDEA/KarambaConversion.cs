/*
using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using Karamba.GHopper.Models;
using Karamba.Models;
using Karamba.Elements;
using Karamba.CrossSections;
using Karamba.GHopper.Utilities;

namespace KarambaIDEA.Grasshopper
{
    //-------
    // In order to make this plug-in compile:
    //-------
    //    - maybe you have to change the references to karamba.gha and karambaCommon.dll
    //    - under karambaIDEA/Properties/BuildEvents: adapt the target path so that the plug in is copied to Rhino6/Plug-ins

    public class KarambaConversion : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public KarambaConversion()
          : base("Karamba3DBeamProps", "K3DBeamProps",
              "Retrieve Karamba3D beam properties for KarambaIDEA.",
              "KarmabaIDEA", "KarambaIDEA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Model(), "Analyzed model", "Model",
                "Model with calculated displacements",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BeamID", "BeamID", "Identifier of each beam", GH_ParamAccess.list);
            pManager.AddTextParameter("CroSecName", "CroSecName", "Cross section name of each beam", GH_ParamAccess.list);
            pManager.AddTextParameter("CroSecShape", "CroSecShape", "Shape of cross section", GH_ParamAccess.list);
            pManager.AddNumberParameter("Height", "Height", "Height of cross section", GH_ParamAccess.list);
            pManager.AddNumberParameter("Width", "Width", "Width of cross section", GH_ParamAccess.list);
            pManager.AddNumberParameter("TFlange", "TFlange", "Thickness of flange", GH_ParamAccess.list);
            pManager.AddNumberParameter("TWeb", "TWeb", "Thickness of web", GH_ParamAccess.list);
            pManager.AddNumberParameter("FRad", "FRad", "Fillet radius", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Model in_gh_model = null;
            DA.GetData<GH_Model>(0, ref in_gh_model);
            if (in_gh_model == null) return;

            var beam_ids = new List<string>();
            var cs_names = new List<string>();
            var cs_shapes = new List<string>();
            var heights = new List<double>();
            var widths = new List<double>();
            var tflanges = new List<double>();
            var twebs = new List<double>();
            var frads = new List<double>();

            Model model = in_gh_model.Value;
            foreach (var elem in model.elems)
            {
                switch (elem)
                {
                    case ModelElementStraightLine beam:
                        beam_ids.Add(elem.id);
                        cs_names.Add(elem.crosec.name);
                        cs_shapes.Add(elem.crosec.shape());
                        switch (elem.crosec)
                        {
                            case CroSec_I crosec_i:
                                heights.Add(crosec_i.getHeight());
                                // use the upper flange thickness
                                widths.Add(crosec_i.uf_width);
                                tflanges.Add(crosec_i.uf_thick);
                                twebs.Add(crosec_i.w_thick);
                                // use the inner fillet radius
                                frads.Add(crosec_i.fillet_r);
                                // this would be the outer fillet radius
                                // frads.Add(crosec.fillet_r1);
                                break;
                            case CroSec_Circle crosec_o:
                                heights.Add(crosec_o.getHeight());
                                // use the upper flange thickness
                                widths.Add(0);
                                tflanges.Add(crosec_o.thick);
                                twebs.Add(0);
                                break;
                        }
                        break;

                    case ModelShell shell:
                        continue;
                    default:
                        throw new ArgumentException(
                            message: "unrecognized type encountered",
                            paramName: nameof(elem));
                }
            }

            // Finally assign the spiral to the output parameter.
            DA.SetDataList(0, ToGH.Values(beam_ids));
            DA.SetDataList(1, ToGH.Values(cs_names));
            DA.SetDataList(2, ToGH.Values(cs_shapes));
            DA.SetDataList(3, heights);
            DA.SetDataList(4, widths);
            DA.SetDataList(5, tflanges);
            DA.SetDataList(6, twebs);
            DA.SetDataList(7, frads);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("BA14B9AA-76A6-4E53-9713-809141980A90"); }
        }
    }
}
*/

