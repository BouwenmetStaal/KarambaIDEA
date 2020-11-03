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

namespace KarambaIDEA._4._Analytical_Templates
{
    public class ATemplate_PlanarTrussJoint : GH_Component
    {
        public ATemplate_PlanarTrussJoint() : base("Analytical Template: Planar Truss Joint", "AT: PTJ", "Analytical Template: Planar Truss Joint", "KarambaIDEA", "4. Analytical Templates")
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
            pManager.AddTextParameter("Message", "Message", "", GH_ParamAccess.tree);

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
            DataTree<string> messages = new DataTree<string>();
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
                            SetAnaTemplate(joint, messages);
                        }
                    }
                }
            }

            foreach (Element ele in project.elements)
            {
                throatBegin.Add(ele.BeginThroatsElement());
                throatEnd.Add(ele.EndThroatsElement());
            }


            //link output
            DA.SetData(0, project);
            DA.SetDataTree(1, messages);
            DA.SetDataList(2, throatBegin);
            DA.SetDataList(3, throatEnd);
        }

        private static void SetAnaTemplate(Joint joint, DataTree<string> messages)
        {
            GH_Path path = new GH_Path(joint.id-1);
            joint.template = new Template();

            BearingMember chord = joint.attachedMembers.OfType<BearingMember>().ToList().First();
            if(chord.element.crossSection.shape == CrossSection.Shape.RHSsection)
            {
                //Range of Validity for RHS Chord NEN-EN 1993-1-8 table 7.8
                CrossSection cc = chord.element.crossSection;
                double b0 = cc.width;
                double h0 = cc.height;
                double t0 = cc.thicknessWeb;
                //Check if CHORD is in tension or compression NEN-EN 1993-1-8 table 7.8
                if (b0/t0<=35 && h0 / t0 <= 35)
                {
                    //Chord should always be at class 1 or 2 NEN-EN 1993-1-8 table 7.8
                    int sectionClass = chord.element.crossSection.SectionClass();
                    if (sectionClass > 2)
                    {
                        messages.Add("WARNING Section class of CHORD insufficient NEN-EN 1993-1-8 art 7.1.2 (2): section class " + sectionClass, path);
                    }
                }
                else
                {
                    messages.Add("WARNING b0/t0 ratio chord too small NEN-EN 1993-1-8 table 7.8", path);
                }
                double gamma = b0 / 2 * t0;//NEN-EN 1993-1-8 art 1.5 (6)
                //Max stress (in the connected face), now max stress is retrieved
                double n = (chord.Maxstress() / chord.element.crossSection.material.Fy) / Project.gammaM5;//NEN-EN 1993-1-8 art 1.5 (5)
                string mes = string.Format("n = {0:0.00} \u03B3 = {1:0.0} b0/t0 = {2:0.0}", n, gamma,b0/t0);
                messages.Add(mes, path);

                foreach (ConnectingMember brace in joint.attachedMembers.OfType<ConnectingMember>())
                {
                    //Range of Validity for CHS or RHS Braces NEN-EN 1993-1-8 table 7.8
                    CrossSection cb = brace.element.crossSection;
                    double bi = cb.width;
                    double hi = cb.height;
                    double ti = cb.thicknessWeb;
                    //Check angle NEN-EN 1993-1-8 art 7.1.2 (3)
                    double angle = Vector.AngleBetweenVectors(chord.element.Line.Vector, brace.element.Line.Vector);
                    double angleDeg = angle * (180 / Math.PI);
                    if (angleDeg < 30)
                    {
                        messages.Add("WARNING angle between CHORD and BRACE too small NEN-EN 1993-1-8 art 7.1.2 (3): " + angleDeg +" °", path);
                    }
                    //Aspect ratio NEN-EN 1993-1-8 table 7.8
                    if (hi/bi<0.5 || 2.0 < hi / bi)
                    {
                        messages.Add("WARNING aspect ratio hi/bi insufficent NEN-EN 1993-1-8 table 7.8", path);
                    }
                    
                    
                    if (brace.element.crossSection.shape == CrossSection.Shape.RHSsection)
                    {
                        //Brace-to-chord ratio RHS Braces NEN-EN 1993-1-8 Table 7.8
                        if (bi/b0<0.25)
                        {
                            messages.Add("WARNING BRACE to CHORD ratio insufficient, NEN-EN 1993-1-8 table 7.8", path);
                        }
                        //RHS braces NEN-EN 1993-1-8 Table 7.8
                        if (bi / ti <= 35 && hi / ti <= 35)//
                        {
                            //Check if BRACE is in tension or compression
                            if (IsLoadedInCompression(brace))
                            {
                                int sectionClass = brace.element.crossSection.SectionClass();
                                //Check if BRACE cross section class is 1 or 2, NEN-EN 1993-1-8 art 7.1.2 (2)
                                if (sectionClass > 2)
                                {
                                    messages.Add("WARNING Section class of BRACE insufficient NEN-EN 1993-1-8 art 7.1.2 (2): section class " + sectionClass, path); //NEN-EN 1993-1-8 art 7.1.2 (2)
                                }
                            }
                        }
                        //CHS NEN-EN 1993-1-8 Table 7.8
                        else
                        {
                            messages.Add("WARNING bi/ti ratio BRACE too small, NEN-EN 1993-1-8 table 7.8 ", path);
                        }
                    }
                    else if (brace.element.crossSection.shape == CrossSection.Shape.CHSsection)
                    {

                    }
                    else
                    {
                        //Brace-to-chord ratio CHS Braces
                        //CHS braces
                        throw new ArgumentNullException("Cross-section not implemented");
                    }
                    
                    //General check variables
                    double beta = bi / b0;
                    double fy0 = cc.material.Fy;
                    double fyi = cc.material.Fy;
                    double be = (10 / (b0 / t0)) * (fy0 * t0 / fyi * ti) * bi;//chord compression stress (n<0)
                    if (be > bi)
                    {
                        be = bi;
                    }
                    double gammaM5 = 1.0;
                    double k_n = 1.3 - 0.4 * n / beta;
                    if (k_n>1.0)
                    {
                        k_n = 1.0;
                    }
                    //T,Y and X joints NEN-EN 1993-1-8 Table 7.11
                    if (beta <= 0.85)
                    {

                    }
                    double Nird = ((k_n * fy0 * Math.Pow(t0, 2)) / ((1 - beta) * Math.Sin(angle)) * (2 * beta / Math.Sin(angle) + 4 * Math.Sqrt(1 - beta))) / gammaM5;
                    Nird = Nird / 1000;
                    
                    //K and N joints with gap NEN-EN 1993-1-8 Table 7.11
                    if (beta <= 1.0)
                    {

                    }
                    double b1 = bi;//TODO
                    double b2 = bi;//TODO
                    
                    double Nird2 = (((8.9 * Math.Pow(gamma, 0.5) * k_n * fy0 * Math.Pow(t0, 2)) / Math.Sin(angle))*((b1+b2)/2*b0))/gammaM5;
                    Nird2 = Nird2 / 1000;

                    double Nmax = brace.MaxAxialLoad();
                    double UC = Nmax / Math.Min(Nird, Nird2);
                    string message = string.Format("Nmax = {0:0.0} kN Nird = {1:0.0} kN Nird = {2:0.0} kN UC = {3:0.00} \u03B2 = {4:0.00}",  Nmax, Nird, Nird2, UC, beta);
                    messages.Add(message, path);
                    //K and N joints with overlap NEN-EN 1993-1-8 Table 7.11




                    CostDefinition(joint, brace);
                }
            }
            else if(chord.element.crossSection.shape == CrossSection.Shape.ISection)
            {
                //Range of Validity for CHS Chord NEN-EN 1993-1-8 table 7.1
            }
            else
            {
                throw new ArgumentNullException("Cross-section not implemented");
            }

        }

        private static bool IsLoadedInCompression(AttachedMember at)
        {
            foreach (LoadCase lc in at.element.project.loadcases)
            {
                foreach (LoadsPerLine loadsPerLine in lc.loadsPerLines)
                {
                    if (loadsPerLine.element == at.element)
                    {
                        if (at.isStartPoint == true)
                        {
                            if (loadsPerLine.startLoad.N < 0)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (loadsPerLine.endLoad.N < 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        



        private static void CostDefinition(Joint joint, ConnectingMember con)
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

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.TrussJoint_01;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("36f90f28-3f13-486a-a001-726a74384966"); }
        }



    }
}
