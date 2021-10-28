// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KarambaIDEA.Core
{

    public class Joint
    {
        public Project project;
        private string jointFilePath;
        public int id;
        public string name; 

        public List<AttachedMember> attachedMembers;
        public List<int> beamIDs;
        public Point centralNodeOfJoint;
        public bool IsContinuous;
        public string brandName;

        
        public Template template=null;
        public string ideaTemplateLocation=null;

        
        public ResultsSummary ResultsSummary;

        public string Name
        {
            get
            {
                //return "C" + this.id+"-brandname"+this.brandName;
                return this.name;
            }
        }

        public string JointFilePath
        {
            get
            {
                return jointFilePath;
            }
            set
            {
                if(this.project!= null)
                {
                    if (this.project.projectFolderPath != null)
                    {
                        string fileName = this.Name + ".ideaCon";
                        jointFilePath = Path.Combine(this.project.projectFolderPath, this.Name, fileName);
                    }
                }
                else
                {
                    jointFilePath = value;
                }
                
            }
        }

        public Joint()
        {

        }

        public Joint(Project _project, int _id, string _name, List<int> _beamIDs, List<AttachedMember> _attachedMembers, Point _centralNodeOfJoint, bool _IsContinues)
        {
            this.project = _project;
            this.id = _id;
            this.name = _name;
            this.attachedMembers = _attachedMembers;
            this.beamIDs = _beamIDs;
            this.centralNodeOfJoint = _centralNodeOfJoint;
            this.IsContinuous = _IsContinues;
        }
        /// <summary>
        /// Fillet welds are assigned to Hollow sections, double fillet welds are assigned to Isections
        /// </summary>
        public void SetDefaultWeldType()
        {
            foreach (ConnectingMember CM in this.attachedMembers.OfType<ConnectingMember>())
            {
                if (CM.element.crossSection.shape == CrossSection.Shape.RHSsection)
                {
                    CM.flangeWeld.weldType = Weld.WeldType.Fillet;
                    CM.webWeld.weldType = Weld.WeldType.Fillet;
                }
                else
                {
                    CM.flangeWeld.weldType = Weld.WeldType.DoubleFillet;
                    CM.webWeld.weldType = Weld.WeldType.DoubleFillet;
                }
            }
        }
    }
}
