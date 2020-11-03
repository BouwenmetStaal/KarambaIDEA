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
    public class ATemplate_WeldAllMembers : GH_Component
    {
        public ATemplate_WeldAllMembers() : base("Analytical Template: Welds all members", "AT: WAM", "Analytical Template: weld all members", "KarambaIDEA", "4. Analytical Templates")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("BrandNames", "BrandNames", "BrandNames to apply template to", GH_ParamAccess.list, "");
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("Message", "Message", "", GH_ParamAccess.list);

            pManager.AddTextParameter("Throats Begin of Element", "TB", "ThroatFlange and ThroatWeb at Start of Element", GH_ParamAccess.list);
            pManager.AddTextParameter("Throats End of Element", "TE", "ThroatFlange and ThroatWeb at End of Element", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables      
            Project sourceProject = new Project();
            List<GH_String> brandNamesDirty = new List<GH_String>();
            List<string> brandNames = new List<string>();

            //Output variables
            List<string> messages = new List<string>();
            List<string> throatBegin = new List<string>();
            List<string> throatEnd = new List<string>();

            //Link input
            DA.GetData(0, ref sourceProject);
            DA.GetDataList(1, brandNamesDirty);

            //Clone project
            Project project = null;
            if (Project.copyProject == true) { project = sourceProject.Clone(); }
            else { project = sourceProject; }


            //process
            if (brandNamesDirty.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Value)).Count() > 0)
            {
                List<string> brandNamesDirtyString = brandNamesDirty.Select(x => x.Value.ToString()).ToList();
                brandNames = ImportGrasshopperUtils.DeleteEnterCommandsInGHStrings(brandNamesDirtyString);
            }

            
            if (brandNames.Count != 0)
            {
                foreach (string brandName in brandNames)
                {
                    foreach (Joint joint in project.joints)
                    {
                        if (brandName == joint.brandName)
                        {
                            SetAnaTemplate(joint);
                        }
                    }
                }
            }
            /*
            else
            {
                foreach (Joint joint in project.joints)
                {
                    SetAnaTemplate(joint);
                }
            }
            */

            foreach (Element ele in project.elements)
            {
                throatBegin.Add(ele.BeginThroatsElement());
                throatEnd.Add(ele.EndThroatsElement());
            }


            //link output
            DA.SetData(0, project);
            DA.SetDataList(1, messages);
            DA.SetDataList(2, throatBegin);
            DA.SetDataList(3, throatEnd);
        }

        private static void SetAnaTemplate(Joint joint)
        {
            joint.template = new Template();
            //Ignore highest hierarchy members by only taking connectingmembers
            foreach (ConnectingMember con in joint.attachedMembers.OfType<ConnectingMember>())
            {
                CrossSection cross = con.element.crossSection;
                MaterialSteel mat = cross.material;
                if (cross.shape == CrossSection.Shape.CHSsection)
                {
                    double radius = 0.5 * cross.height;
                    double perimeter = 2 * Math.PI * radius;

                    double weldSizeW = Weld.CalWeldSizeFullStrenth90deg(cross.thicknessWeb, cross.thicknessWeb, mat, Weld.WeldType.Fillet);
                    joint.template.welds.Add(new Weld("WebWeldCHS", Weld.WeldType.Fillet, weldSizeW, perimeter));

                    con.flangeWeld.Size = weldSizeW;
                    con.webWeld.Size = weldSizeW;
                }

                if (cross.shape == CrossSection.Shape.RHSsection)
                {
                    double perimeter = 2 * cross.width + 2 * cross.height;

                    double weldSizeW = Weld.CalWeldSizeFullStrenth90deg(cross.thicknessWeb, cross.thicknessWeb, mat, Weld.WeldType.Fillet);
                    joint.template.welds.Add(new Weld("WebWeldSHS", Weld.WeldType.Fillet, weldSizeW, perimeter));

                    con.flangeWeld.Size = weldSizeW;
                    con.webWeld.Size = weldSizeW;
                }
                if (cross.shape == CrossSection.Shape.ISection)
                {
                    double weldSizeF = Weld.CalWeldSizeFullStrenth90deg(cross.thicknessFlange, cross.thicknessFlange, mat, Weld.WeldType.DoubleFillet);
                    joint.template.welds.Add(new Weld("FlangeWeldTop", Weld.WeldType.DoubleFillet, weldSizeF, cross.width));
                    joint.template.welds.Add(new Weld("FlangeWeldBottom", Weld.WeldType.DoubleFillet, weldSizeF, cross.width));
                    double weldSizeW = Weld.CalWeldSizeFullStrenth90deg(cross.thicknessFlange, cross.thicknessFlange, mat, Weld.WeldType.DoubleFillet);
                    joint.template.welds.Add(new Weld("WebWeld", Weld.WeldType.DoubleFillet, weldSizeW, cross.height - 2 * cross.thicknessFlange));

                    con.flangeWeld.Size = weldSizeF;
                    con.webWeld.Size = weldSizeW;
                }
                else
                {
                    //TODO: include warning, cross-sections not recognized
                }
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.ATempWeldAll_01_01;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("2c935f91-8b68-4ec9-9cef-264cf4e3d6f9"); }
        }
        
        

    }
}
