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
            pManager.AddGenericParameter("Project", "P", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("BrandNames", "BN", "BrandNames to apply template to", GH_ParamAccess.list, "");
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "P", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("Message", "M", "", GH_ParamAccess.tree);

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

            BearingMember bear = joint.attachedMembers.OfType<BearingMember>().ToList().First();
            if(bear.element.crossSection.shape == CrossSection.Shape.SHSSection)
            {
                //Range of Validity for Chord
                CrossSection cc = bear.element.crossSection;
                double b0 = cc.width;
                double h0 = cc.height;
                double t0 = cc.thicknessWeb;
                //Check if CHORD is in tension or compression
                if (b0/t0<=40 && h0 / t0 <= 40)
                {
                    if (IsLoadedInCompression(bear, joint))
                    {
                        int sectionClass = bear.element.crossSection.SectionClass();
                        if (sectionClass > 2)
                        {
                            messages.Add("WARNING Section class of chord insufficient: section class " + sectionClass, path);
                        }
                    }
                }
                else
                {
                    messages.Add("WARNING b0/t0 ratio chord too small ", path);
                }



                foreach (ConnectingMember con in joint.attachedMembers.OfType<ConnectingMember>())
                {
                    //Range of Validity for Brace
                    CrossSection cb = con.element.crossSection;
                    double bi = cb.width;
                    double hi = cb.height;
                    double ti = cb.thicknessWeb;
                    //Check angle
                    double angle = Vector.AngleBetweenVectors(bear.element.Line.Vector, con.element.Line.Vector);
                    double angleDeg = angle * (180 / Math.PI);
                    if (angleDeg < 30)
                    {
                        messages.Add("WARNING angle between chord and brace too small: " + angleDeg +" °", path);
                    }
                    //Aspect ratio
                    if(hi/bi<0.5 || 2.0 < hi / bi)
                    {
                        messages.Add("WARNING aspect ratio hi/bi insufficent", path);
                    }
                    
                    
                    if (bear.element.crossSection.shape == CrossSection.Shape.SHSSection)
                    {
                        //Brace-to-chord ratio RHS Braces
                        if(bi/b0<0.25 && bi / b0 < 0.1 + 0.01 * (b0 / t0))
                        {
                            messages.Add("WARNING brace to chord ratio insufficient", path);
                        }
                        //RHS braces
                        if (bi / ti <= 40 && hi / ti <= 40)
                        {
                            //Check if BRACE is in tension or compression
                            if (IsLoadedInCompression(con, joint))
                            {
                                int sectionClass = con.element.crossSection.SectionClass();
                                if (sectionClass > 2)
                                {
                                    messages.Add("WARNING Section class of brace insufficient: section class " + sectionClass, path);
                                }
                            }
                        }
                        else
                        {
                            messages.Add("WARNING bi/ti ratio brace too small ", path);
                        }
                    }
                    else
                    {
                        //Brace-to-chord ratio CHS Braces
                        //CHS braces
                        throw new ArgumentNullException("Cross-section not implemented");
                    }
                    
                    //General check variables
                    double beta = bi / b0;
                    double C1 = 06-0.5*beta;

                    double N_0_Ed = 0;
                    double Npl_0_Rd = 0;
                    double M_0_Ed = 0;
                    double Mpl_0_Rd = 0;
                    double n = N_0_Ed / Npl_0_Rd + M_0_Ed / Mpl_0_Rd;

                    double Qf = Math.Pow(1 - Math.Abs(n), C1); 
                    double eta = hi/b0;//waar kan ik deze vinden?
                    double fy0 = cc.material.Fy;
                    double fyi = cc.material.Fy;
                    double be = (10 / (b0 / t0)) * (fy0 * t0 / fyi * ti) * bi;//chord compression stress (n<0)
                    if (be > bi)
                    {
                        be = bi;
                    }

                    //Chord face plastification (T, Y and X joints)
                    double Nrd_1 = fy0 * Math.Pow(t0, 2) / Math.Sin(angleDeg)*(2*eta/(1-beta)*Math.Sin(angle)+4/Math.Sqrt(1-beta))*Qf;
                    //Local Brace failure (T, Y and X joints)
                    //Chord Punching shear (T, Y and X joints)
                    //Chord side wall failure (T, Y and X joints)

                    CostDefinition(joint, con);
                }
            }
            else if(bear.element.crossSection.shape == CrossSection.Shape.ISection)
            {

            }
            else
            {
                throw new ArgumentNullException("Cross-section not implemented");
            }

        }

        private static bool IsLoadedInCompression(AttachedMember at, Joint joint)
        {
            foreach (LoadCase lc in joint.project.loadcases)
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

            if (cross.shape == CrossSection.Shape.SHSSection)
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
