// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Collections.Generic;
using System.Linq;


namespace KarambaIDEA.Core
{
    [Serializable]
    public class Element
    {
        internal Project project;
        public int id;
        public int numberInHierarchy;
        private readonly Line line;
        public Line Line
        {
            get
            {
                return line;
            }
        }
        public Line brepLine;
        public CrossSection crossSection;
        public string groupname { get; set; }
        public double rotationLCS;
        public LocalCoordinateSystem localCoordinateSystem = new LocalCoordinateSystem();
        public SawingCuts.SawingCut startCut;
        public SawingCuts.SawingCut endCut;
        public ConnectionProperties startProperties = new ConnectionProperties();
        public ConnectionProperties endProperties = new ConnectionProperties();

        /// <summary>
        /// Construct element for project
        /// </summary>
        /// <param name="_project"></param>
        /// <param name="_id"></param>
        /// <param name="_line">_line cannot be null</param>
        /// <param name="_crossSection"></param>
        /// <param name="_groupname"></param>
        /// <param name="_numberInHierarchy"></param>
        /// <param name="_rotationLCS"></param>
        public Element(Project _project, int _id, Line _line, CrossSection _crossSection, string _groupname, int _numberInHierarchy, double _rotationLCS)

        {

            this.project = _project;
            _project.elements.Add(this);
            this.id = _id;
            this.line = _line?? throw new ArgumentNullException("The argument _line cannot be null");
            this.brepLine = new Line(_line.start,_line.end);
            this.crossSection = _crossSection;
            this.groupname = _groupname;
            this.numberInHierarchy = _numberInHierarchy;
            this.rotationLCS = _rotationLCS;
            this.UpdateLocalCoordinateSystem();

        }

        public void UpdateLocalCoordinateSystem()
        {
            if (this.Line == null)
            {
                throw new ArgumentNullException("The field line cannot be null");
            }
            //Defining LCS for First lineSegment
            double xcor = this.Line.Vector.X;
            double ycor = this.Line.Vector.Y;
            double zcor = this.Line.Vector.Z;

            //Define LCS (local-y in XY plane) and unitize
            Vector vx = new Vector(xcor, ycor, zcor).Unitize();
            Vector vy = new Vector();
            Vector vz = new Vector();
            if (xcor == 0.0 && ycor == 0.0)//If element is vertical (upwward or downward)
            {
                vy = new Vector(0.0, 1.0, 0.0).Unitize(); //(local y axis is in global y-axis direction
                vz = new Vector((-zcor), 0.0, (xcor)).Unitize();
            }
            else
            {
                vy = new Vector(-ycor, xcor, 0.0).Unitize();//vy is the 90 degree rotation of vx in the xy-plane
                vz = new Vector((-zcor * xcor), (-zcor * ycor), ((xcor * xcor) + (ycor * ycor))).Unitize();
            }

            if (this.rotationLCS != 0.0)//if rotation is not zero
            {
                //Rodrigues' rotation formula
                vy = Vector.RotateVector(vx, this.rotationLCS, vy);
                vz = Vector.RotateVector(vx, this.rotationLCS, vz);
            }

            this.localCoordinateSystem.X = vx;
            this.localCoordinateSystem.Y = vy;
            this.localCoordinateSystem.Z = vz;

        }
        public string BeginThroatsElement()
        {
            string info = "";

            List<AttachedMember> attachedmembers = project.joints.SelectMany(a => a.attachedMembers).ToList();
            List<ConnectingMember> conmembers = attachedmembers.OfType<ConnectingMember>().ToList();
            List<ConnectingMember> conmembers_element = conmembers.Where(a => a.element.id == id).ToList();

            if (conmembers_element.Count > 0)
            {
                ConnectingMember connectingStart = conmembers_element.FirstOrDefault(a => a.isStartPoint == true);
                if (connectingStart != null)
                {
                    double f = connectingStart.flangeWeld.Size;
                    double w = connectingStart.webWeld.Size;
                    info = "af" + f + "-aw" + w;
                }
            }
            return info;

        }
        public string EndThroatsElement()
        {
            string info = "";

            List<AttachedMember> attachedmembers = project.joints.SelectMany(a => a.attachedMembers).ToList();
            List<ConnectingMember> conmembers = attachedmembers.OfType<ConnectingMember>().ToList();
            List<ConnectingMember> conmembers_element = conmembers.Where(a => a.element.id == id).ToList();

            if (conmembers_element.Count > 0)
            {
                ConnectingMember connectingStart = conmembers_element.FirstOrDefault(a => a.isStartPoint == false);
                if (connectingStart != null)
                {
                    double f = connectingStart.flangeWeld.Size;
                    double w = connectingStart.webWeld.Size;
                    info = "af" + f + "-aw" + w;
                }
            }
            return info;
        }
        public string BeginPlatesElement()
        {
            string info = "";
            List<AttachedMember> attachedmembers = project.joints.SelectMany(a => a.attachedMembers).ToList();
            List<AttachedMember> members = attachedmembers.Where(a => a.element.id == id).ToList();
            AttachedMember member = members.FirstOrDefault(a => a.isStartPoint == true);
            if (member != null)
            {
                if (member.platefailure == false)
                {
                    info = "FAILURE";
                }
            }
            return info;
        }
        public string EndPlatesElement()
        {
            string info = "";
            List<AttachedMember> attachedmembers = project.joints.SelectMany(a => a.attachedMembers).ToList();
            List<AttachedMember> members = attachedmembers.Where(a => a.element.id == id).ToList();
            AttachedMember member = members.FirstOrDefault(a => a.isStartPoint == false);
            if (member != null)
            {
                if (member.platefailure == false)
                {
                    info = "FAILURE";
                }
            }
            return info;
        }
    }
}
