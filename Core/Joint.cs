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

        public string Name
        {
            get
            {
                return "C" + this.id;
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
        public Joint(Project _project, int _id, List<int> _beamIDs, List<AttachedMember> _attachedMembers, Point _centralNodeOfJoint, double _globaleccenticitylength, bool _isWarrenEccentricJoint, Vector _bearingMemberUnitVector, bool _IsContinues)
        {
            this.project = _project;
            this.id = _id;
            this.attachedMembers = _attachedMembers;
            this.beamIDs = _beamIDs;
            this.centralNodeOfJoint = _centralNodeOfJoint;
            this.maxGlobalEccentricity = _globaleccenticitylength;
            this.isWarrenEccentricJoint = _isWarrenEccentricJoint;
            this.bearingMemberUnitVector = _bearingMemberUnitVector;
            this.IsContinues = _IsContinues;
        }



#warning This method gives expectations but ultimately does not implement functionality. Where is the functionality? Alternatively remove method
        public void CalculateWelds()
        {
            switch (this.project.analysisMethod)
            {
                case Project.AnalysisMethod.MinSetWelds:
                    {
                        //Volume are calculated with min weldsize
                        return;
                    }

                case Project.AnalysisMethod.FullStrengthLazy:
                    {
                        
                        return;
                    }

                case Project.AnalysisMethod.FullStrengthMethod:
                    {
                        
                        return;
                    }
                case Project.AnalysisMethod.DirectionalMethod:
                    {
                        
                        return;
                    }
                case Project.AnalysisMethod.IdeaMethod:
                    {
                        return;
                    }
            }

        }
        public double CalculateWeldVolume()
        {
            return 0.0;
        }
        

    }
}
