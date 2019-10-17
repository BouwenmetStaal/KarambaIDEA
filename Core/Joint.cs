// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Collections.Generic;
using System.Linq;

namespace KarambaIDEA.Core
{

    public class Joint
    {
        public Project project;
        public int id;
        public Point centralNodeOfJoint;
        public double maxGlobalEccentricity;
        public List<int> beamIDs;
        public bool isWarrenEccentricJoint;
        public Vector bearingMemberUnitVector;
        public List<AttachedMember> attachedMembers;
        // public IdeaConnection ideaConnection;
        public bool IsContinues;
        public double weldVolume;
        public string brandName;
        public EnumWorkshopOperations workshopOperation;

        public string Name
        {
            get
            {
                return "C" + this.id+"-brandname"+this.brandName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_project"></param>
        /// <param name="_id">starts from 1</param>
        /// <param name="_beamIDs"></param>
        /// <param name="_attachedMembers"></param>
        /// <param name="_centralNodeOfJoint"></param>
        /// <param name="_globaleccenticitylength">eccentricity in meters</param>
        /// <param name="_isWarrenEccentricJoint"></param>
        /// <param name="_bearingMemberUnitVector"></param>
        /// <param name="_IsContinues">defines wether joint is continues or ended</param>
        public Joint(Project _project, int _id, List<int> _beamIDs, List<AttachedMember> _attachedMembers, Point _centralNodeOfJoint, double _globaleccenticitylength, Vector _bearingMemberUnitVector, bool _IsContinues)
        {
            this.project = _project;
            this.id = _id;
            this.attachedMembers = _attachedMembers;
            this.beamIDs = _beamIDs;
            this.centralNodeOfJoint = _centralNodeOfJoint;
            this.maxGlobalEccentricity = _globaleccenticitylength;
            this.bearingMemberUnitVector = _bearingMemberUnitVector;
            this.IsContinues = _IsContinues;
        }

        
        public double GenerateWeldVolume()
        {
            return 0.0;
        }

        
        /// <summary>
        /// Fillet welds are assigned to Hollow sections, double fillet welds are assigned to Isections
        /// </summary>
        public void SetDefaultWeldType()
        {
            foreach (ConnectingMember CM in this.attachedMembers.OfType<ConnectingMember>())
            {
                if (CM.element.crossSection.shape == CrossSection.Shape.HollowSection)
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
