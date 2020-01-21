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
    public class ATemplate_MomentResistingConnection : GH_Component
    {
        public ATemplate_MomentResistingConnection() : base("Analytical Template: Moment resisting connection", "Analytical Template: Moment resisting connection", "Analytical Template: Moment resisting connection", "KarambaIDEA", "4. Analytical Templates")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("BrandNames", "BrandNames", "BrandNames to apply template to", GH_ParamAccess.list, "");
            pManager.AddNumberParameter("Height haunch [mm]", "Height haunch [mm]", "", GH_ParamAccess.item, 0.0);
            pManager.AddBooleanParameter("Stiffeners?", "Stiffeners?", "", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Unbraced structure?", "Unbraced structure?", "If true joint classification will be assesed accoring to unbraced structures specifications, if false for braced structures.", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "Project", "Project object of KarambaIdeaCore", GH_ParamAccess.item);
            pManager.AddTextParameter("Message", "Message", "", GH_ParamAccess.list);

            pManager.AddNumberParameter("start Sj", "Sj", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("end Sj", "Sj", "", GH_ParamAccess.list);

            pManager.AddNumberParameter("start Mj,Rd", "Mj,Rd", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("start Mj,Rd", "Mj,Rd", "", GH_ParamAccess.list);

            pManager.AddBrepParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);

            pManager.AddTextParameter("Classification", "Classification", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("SjP", "SjP", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("SjR", "SjR", "", GH_ParamAccess.list);
            
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Design rules

            //Input variables      
            Project project = new Project();
            double heightHaunch = new double();
            List<GH_String> brandNamesDirty = new List<GH_String>();
            List<string> brandNames = new List<string>();
            bool stiffeners = new bool();
            bool unbraced = new bool();

            //Output variables
            List<string> messages = new List<string>();
            Brep brep = new Brep();
            List<double> startSjs = new List<double>();
            List<double> endSjs = new List<double>();
            List<double> startMjrds = new List<double>();
            List<double> endMjrds = new List<double>();

            //Link input
            DA.GetData(0, ref project);
            DA.GetDataList(1, brandNamesDirty);
            DA.GetData(2, ref heightHaunch);
            DA.GetData(3, ref stiffeners);
            DA.GetData(4, ref unbraced);


            //process
            if (brandNamesDirty.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Value)).Count() > 0)
            {
                List<string> brandNamesDirtyString = brandNamesDirty.Select(x => x.Value.ToString()).ToList();
                brandNames = ImportGrasshopperUtils.DeleteEnterCommandsInGHStrings(brandNamesDirtyString);
            }

            //TODO: make a message "BrandName 011 is linked to BoltedEndPlateConnection"
            if (brandNames.Count != 0)
            {
                foreach (string brandName in brandNames)
                {
                    foreach (Joint joint in project.joints)
                    {
                        if (brandName == joint.brandName)
                        {
                            if (joint.attachedMembers.OfType<ConnectingMember>().ToList().Count == 1)
                            {
                                SetMomentResitingConnection(joint, heightHaunch, stiffeners, unbraced);
                            }
                            else
                            {
                                //more than one connectingmembers in connection
                                //TODO: include warning
                            }
                        }
                    }
                }
            }

            foreach(Element ele in project.elements)
            {
                startSjs.Add(ele.startProperties.Sj);
                startMjrds.Add(ele.startProperties.Mjrd);

                endSjs.Add(ele.endProperties.Sj);
                endMjrds.Add(ele.endProperties.Mjrd);

                if (ele.startProperties != null)
                {
                    
                }
                if(ele.endProperties != null)
                {
                    
                }
            }

            //messages = project.MakeTemplateJointMessage();

            BoundingBox bbox = new BoundingBox(new Point3d(0, 0, 0), new Point3d(0.1, 0.2, 0.05));
            Plane plane = new Plane(new Point3d(0, 2, 0), new Vector3d(0, 3, 3));
            Box box = new Box(plane, bbox);
            brep = box.ToBrep();


            //link output
            DA.SetData(0, project);
            DA.SetDataList(1, messages);
            DA.SetDataList(2, startSjs);
            DA.SetDataList(3, endSjs);
            DA.SetDataList(4, startMjrds);
            DA.SetDataList(5, endMjrds);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.ATempMomentResistingConnection;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("e8467348-e28f-40e0-991a-fcf88f652aba"); }
        }
        public void SetMomentResitingConnection(Joint joint,double heightHaunch, bool stiffeners, bool unbraced)
        {
            ConnectingMember con = joint.attachedMembers.OfType<ConnectingMember>().ToList().First();
            BearingMember bear = joint.attachedMembers.OfType<BearingMember>().ToList().First();
            int k = 25;
            if (unbraced == true) { k = 8; }
            double kf = 13;
            if (stiffeners == true) { kf = 8.5; }
            int ks = 5;
            double z = heightHaunch + con.element.crossSection.height;
            double E = 210000;
            double Lb = con.element.line.Length*1000;//from m to mm
            double Ib = con.element.crossSection.Iyy();//TODO: Iyy missing include Karamba3D Crosssection dataset in KarambaIDEA
            //calculate SjP
            double SjH = (0.5 * E * Ib) / Lb;
            //calculate SjR
            double SjR = (k * E * Ib) / Lb;
            //Calculate Sj,approx according to Maarten Steenhuis method
            double Sj = (E * Math.Pow(z, 2) * bear.element.crossSection.thicknessFlange) / kf;
            //Calculate Mj,Rd,approx
            double fy = con.element.crossSection.material.Fy;
            double yM0 = 1.0;
            double MjRd = (ks * fy * z * Math.Pow(bear.element.crossSection.thicknessFlange, 2)) / yM0;

            ConnectionProperties conProp = new ConnectionProperties();
            conProp.Sj = Sj * Math.Pow(10, -6);
            conProp.SjH = SjH * Math.Pow(10, -6);
            conProp.SjR = SjR * Math.Pow(10, -6);
            conProp.Mjrd = MjRd*Math.Pow(10,-6);

            if (con.isStartPoint == true)
            {
                con.element.startProperties = conProp;
            }
            else
            {
                con.element.endProperties = conProp;
            }

            

            joint.template = new Template();
            joint.template.workshopOperations = Template.WorkshopOperations.BoltedEndPlateConnection;
            Plate plate = new Plate();
            plate.thickness = heightHaunch;
            joint.template.plates.Add(plate);
        }

    }
}
