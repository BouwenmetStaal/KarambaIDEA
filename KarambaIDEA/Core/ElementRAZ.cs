// Copyright (c) 2019 Rayaan Ajouz. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System.Collections.Generic;
using System.Linq;


namespace KarambaIDEA.Core
{



    public class ElementRAZ
    {
        public Project project;
        public int id;
        public int numberInHierarchy;
        public LineRAZ line;
        public CrossSection crossSection;
        public string groupname;
        public SawingCut startCut;
        public SawingCut endCut;



        public ElementRAZ(Project _project, int _id, LineRAZ _line, CrossSection _crossSection, string _groupname, int _numberInHierarchy)

        {
            this.project = _project;
            _project.elementRAZs.Add(this);
            this.id = _id;
            this.line = _line;
            this.crossSection = _crossSection;
            this.groupname = _groupname;
            this.numberInHierarchy = _numberInHierarchy;

        }

        public ElementRAZ()
        {

        }
        public enum SawingCut
        {
            NoCut = 0,
            RightAngledCut = 1,
            SingleMiterCut = 2,
            DoubleMiterCut = 3
        }

        public string BeginThroatsElement()
        {
            string info = "";

            List<AttachedMember> attachedmembers = project.joints.SelectMany(a => a.attachedMembers).ToList();
            List<ConnectingMember> conmembers = attachedmembers.OfType<ConnectingMember>().ToList();
            List<ConnectingMember> conmembers_element = conmembers.Where(a => a.ElementRAZ.id == id).ToList();

            if (conmembers_element.Count > 0)
            {
                ConnectingMember connectingStart = conmembers_element.FirstOrDefault(a => a.isStartPoint == true);
                if (connectingStart != null)
                {
                    double f = connectingStart.flangeWeld.size;
                    double w = connectingStart.webWeld.size;
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
            List<ConnectingMember> conmembers_element = conmembers.Where(a => a.ElementRAZ.id == id).ToList();

            if (conmembers_element.Count > 0)
            {
                ConnectingMember connectingStart = conmembers_element.FirstOrDefault(a => a.isStartPoint == false);
                if (connectingStart != null)
                {
                    double f = connectingStart.flangeWeld.size;
                    double w = connectingStart.webWeld.size;
                    info = "af" + f + "-aw" + w;
                }
            }
            return info;
        }

        public string BeginPlatesElement()
        {
            string info = "";
            List<AttachedMember> attachedmembers = project.joints.SelectMany(a => a.attachedMembers).ToList();
            List<AttachedMember> members = attachedmembers.Where(a => a.ElementRAZ.id == id).ToList();
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
            List<AttachedMember> members = attachedmembers.Where(a => a.ElementRAZ.id == id).ToList();
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
