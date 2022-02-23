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


namespace KarambaIDEA
{
    public class JointEquilibrium : GH_Component
    {
        public JointEquilibrium() : base("Joint Load Equilibrium", "JLE", "Joint Load Equilibrium per Loadcase", "KarambaIDEA", "3. Project utilities")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "P", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("X kN", "X kN", "result of equilibrium analysis in X direction [kN]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Y kN", "Y kN", "result of equilibrium analysis in Y direction [kN]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Z kN", "Z kN", "result of equilibrium analysis in Z direction [kN]", GH_ParamAccess.tree);

            pManager.AddNumberParameter("Mt kNm", "Mt kNm", "result of equilibrium analysis in Mt direction [kNm]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("My kNm", "My kNm", "result of equilibrium analysis in My direction [kNm]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Mz kNm", "Mz kNm", "result of equilibrium analysis in Mz direction [kNm]", GH_ParamAccess.tree);

            pManager.AddBooleanParameter("Equilibrium?", "E?", "True if there is equilibrium", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input variables
            Project project = new Project();

            //Link input
            DA.GetData(0, ref project);

            //output variables
            DataTree<double> xkNs = new DataTree<double>();
            DataTree<double> ykNs = new DataTree<double>();
            DataTree<double> zkNs = new DataTree<double>();

            DataTree<double> MxkNms = new DataTree<double>();
            DataTree<double> MykNms = new DataTree<double>();
            DataTree<double> MzkNms = new DataTree<double>();

            DataTree<bool> eqtree = new DataTree<bool>();

            int a = 0;

            foreach(Joint joint in project.joints)
            {
                GH_Path path = new GH_Path(a);
                foreach (LoadCase lc in project.loadcases)
                {
                    double xkN = new double();
                    double ykN = new double();
                    double zkN = new double();

                    double MxkNm = new double();
                    double MykNm = new double();
                    double MzkNm = new double();

                    int lcId =lc.id;//Grasshopper loadcasses start at zero
                    foreach (AttachedMember member in joint.attachedMembers)
                    {
                        int id = member.element.id;
                        Vector vecN = new Vector();
                        Vector vecVy = new Vector();
                        Vector vecVz = new Vector();
                        Vector vecMt = new Vector();
                        Vector vecMy = new Vector();
                        Vector vecMz = new Vector();

                        if (member.isStartPoint == true)
                        {
                            int sign = 1;

                            double N = sign * lc.loadsPerLines[id].startLoad.N;
                            Vector localX = member.element.localCoordinateSystem.X;
                            vecN = Vector.VecScalMultiply(localX, N);

                            double Vy = sign * lc.loadsPerLines[id].startLoad.Vy;
                            Vector localY = member.element.localCoordinateSystem.Y;
                            vecVy = Vector.VecScalMultiply(localY, Vy);

                            double Vz = sign * lc.loadsPerLines[id].startLoad.Vz;
                            Vector localZ = member.element.localCoordinateSystem.Z;
                            vecVz = Vector.VecScalMultiply(localZ, Vz);

                            double Mt = sign * lc.loadsPerLines[id].startLoad.Mt;
                            Vector localMt = member.element.localCoordinateSystem.X;
                            vecMt = Vector.VecScalMultiply(localMt, Mt);

                            double My = sign * lc.loadsPerLines[id].startLoad.My;
                            Vector localMy = member.element.localCoordinateSystem.Z;
                            vecMy = Vector.VecScalMultiply(localMy, My);

                            double Mz = sign * lc.loadsPerLines[id].startLoad.Mz;
                            Vector localMz = member.element.localCoordinateSystem.Y;
                            vecMz = Vector.VecScalMultiply(localMz, Mz);


                        }
                        else
                        {
                            int sign = -1;

                            double N = sign * lc.loadsPerLines[id].startLoad.N;
                            Vector localX = member.element.localCoordinateSystem.X;
                            vecN = Vector.VecScalMultiply(localX, N);

                            double Vy = sign * lc.loadsPerLines[id].startLoad.Vy;
                            Vector localY = member.element.localCoordinateSystem.Y;
                            vecVy = Vector.VecScalMultiply(localY, Vy);

                            double Vz = sign * lc.loadsPerLines[id].startLoad.Vz;
                            Vector localZ = member.element.localCoordinateSystem.Z;
                            vecVz = Vector.VecScalMultiply(localZ, Vz);

                            double Mt = sign * lc.loadsPerLines[id].startLoad.Mt;
                            Vector localMt = member.element.localCoordinateSystem.X;
                            vecMt = Vector.VecScalMultiply(localMt, Mt);

                            double My = sign * lc.loadsPerLines[id].startLoad.My;
                            Vector localMy = member.element.localCoordinateSystem.Z;
                            vecMy = Vector.VecScalMultiply(localMy, My);

                            double Mz = sign * lc.loadsPerLines[id].startLoad.Mz;
                            Vector localMz = member.element.localCoordinateSystem.Y;
                            vecMz = Vector.VecScalMultiply(localMz, Mz);
                        }
                        xkN = xkN + vecN.X + vecVy.X + vecVz.X;
                        ykN = ykN + vecN.Y + vecVy.Y + vecVz.Y;
                        zkN = zkN + vecN.Z + vecVy.Z + vecVz.Z;

                        MxkNm = MxkNm + vecMt.X + vecMt.X + vecMt.X;
                        MykNm = MykNm + vecMy.Y + vecMy.Y + vecMy.Y;
                        MzkNm = MzkNm + vecMz.Z + vecMz.Z + vecMz.Z;

                    }
                    /*
                    //round off error
                    //If value is smaller than 1e-6, change to zero
                    double tolerance = 1e-6;
                    if (Math.Abs(xkN) < tolerance) { xkN = 0; }
                    if (Math.Abs(ykN) < tolerance) { ykN = 0; }
                    if (Math.Abs(zkN) < tolerance) { zkN = 0; }

                    if (Math.Abs(MxkNm) < tolerance) { xkN = 0; }
                    if (Math.Abs(MykNm) < tolerance) { ykN = 0; }
                    if (Math.Abs(MzkNm) < tolerance) { zkN = 0; }
                    */
                    int dec = 1;
                    xkN = Math.Round(xkN, dec);
                    ykN = Math.Round(ykN, dec);
                    zkN = Math.Round(zkN, dec);
                    MxkNm = Math.Round(MxkNm, dec);
                    MykNm = Math.Round(MykNm, dec);
                    MzkNm = Math.Round(MzkNm, dec);

                    bool equilibrium = false;
                    if(xkN== 0.0 && ykN == 0.0 && zkN == 0.0 && MxkNm == 0.0 && MykNm == 0.0 && MzkNm == 0.0)
                    {
                        equilibrium = true;
                    }

                    xkNs.Add(xkN, path);
                    ykNs.Add(ykN, path);
                    zkNs.Add(zkN, path);

                    MxkNms.Add(MxkNm, path);
                    MykNms.Add(MykNm, path);
                    MzkNms.Add(MzkNm, path);

                    eqtree.Add(equilibrium, path);

                    
                }
                a = a + 1;
            }
            //link output
            DA.SetDataTree(0, xkNs);
            DA.SetDataTree(1, ykNs);
            DA.SetDataTree(2, zkNs);

            DA.SetDataTree(3, MxkNms);
            DA.SetDataTree(4, MykNms);
            DA.SetDataTree(5, MzkNms);

            DA.SetDataTree(6, eqtree);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.JointLoadEquilibrium;

            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("23d3d581-bcbe-4d9a-8262-6a5549a33c01"); }
        }
    }
}

