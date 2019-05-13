// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal, ABT bv. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Collections.Generic;
using System.Linq;


using KarambaIDEA.IDEA;

namespace KarambaIDEA.Core
{

    public class Joint
    {
        public Project project;
        public int id;
        public PointRAZ centralNodeOfJoint;
        public double maxGlobalEccentricity;
        public List<int> beamIDs;
        public bool isWarrenEccentricJoint;
        public VectorRAZ bearingMemberUnitVector;
        public List<AttachedMember> attachedMembers;
        public IdeaConnection ideaConnection;
        public bool IsContinues;
        public double weldVolume;

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
        public Joint(Project _project, int _id, List<int> _beamIDs, List<AttachedMember> _attachedMembers, PointRAZ _centralNodeOfJoint, double _globaleccenticitylength, bool _isWarrenEccentricJoint, VectorRAZ _bearingMemberUnitVector, bool _IsContinues)
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
                        CalculateWeldThroatsFullStrengthLazy();
                        return;
                    }

                case Project.AnalysisMethod.FullStrengthMethod:
                    {
                        CalculateWeldThroatsFullStrength();
                        return;
                    }
                case Project.AnalysisMethod.DirectionalMethod:
                    {
                        CalculateWeldThroatsDirectional();
                        return;
                    }
                case Project.AnalysisMethod.IdeaMethod:
                    {
                        //MainWindow mainWindow = new MainWindow();
                        //mainWindow.Test();
                        //this.ideaConnection = new IdeaConnection(this, @"C:\Data\TEMPLATES\");
                        return;
                    }
            }

        }
        public double CalculateWeldVolume()
        {
            double weldvolume = 0;
            //foreach (ConnectingMember c in this.attachedMembers.OfType<ConnectingMember>())
            List<BearingMember> BMs = this.attachedMembers.OfType<BearingMember>().ToList();
            List<ConnectingMember> CMs = this.attachedMembers.OfType<ConnectingMember>().ToList();
            double phi = new double();
            double eccentricity = new double();
            //Convert eccentricity in meter to milimeter
            //If warren double the eccentricity
            if (this.isWarrenEccentricJoint == true)
            {
                eccentricity = 2 * this.maxGlobalEccentricity * 1000;
            }
            else
            {
                eccentricity = this.maxGlobalEccentricity * 1000;
            }

            for (int i = 0; i < CMs.Count; i++)
            {
                if (i == 0)
                {
                    double a = CMs[i].ElementRAZ.crossSection.height / 2;//half height vertical in mm
                    double phiDirty = VectorRAZ.AngleBetweenVectors(bearingMemberUnitVector, CMs[i].ElementRAZ.line.vector);
                    phi = Math.PI - (Math.Min(phiDirty, (Math.PI - phiDirty)));//range phi is betwween 90 and 180 degrees
                    //VolumeWeb
                    double ttweb = CMs[i].webWeld.size;
                    double weblength = ConnectingMember.WebWeldFirstAttachedLength(a, phi);
                    double throatangle = Math.PI / 2;//throat-angle is 90degrees
                    double weldsurface = Weld.CalcWeldSurface(throatangle, ttweb);
                    double volumeWeb = 2 * weldsurface * weblength;

                    weldvolume += volumeWeb;

                    //VolumeFlanges
                    double ttflange = CMs[i].flangeWeld.size;//throatsize in mm
                    double width = CMs[i].ElementRAZ.crossSection.width;//section with in mm
                    double phiNeg = Math.PI - phi;//other side of plate
                    double weldsurfacePos = Weld.CalcWeldSurface(phi, ttflange);
                    double weldsurfaceNeg = Weld.CalcWeldSurface(phiNeg, ttflange);
                    if (CMs[i].ElementRAZ.crossSection.shape.Equals(CrossSection.Shape.ISection))
                    {
                        //Always flat
                        //4 lines of welds
                        //inner 2 lines of welds are tweb shorter
                        double tweb = CMs[i].ElementRAZ.crossSection.thicknessWeb;
                        weldvolume += weldsurfacePos * (width);         //1
                        weldvolume += weldsurfacePos * (width - tweb);  //2
                        weldvolume += weldsurfaceNeg * (width);         //3
                        weldvolume += weldsurfaceNeg * (width - tweb);  //4

                    }
                    else
                    {
                        //Always flat
                        //2 lines of welds
                        weldvolume += weldsurfacePos * (width);         //1
                        weldvolume += weldsurfaceNeg * (width);         //2
                    }


                }
                if (i == 1)
                {
                    // a     = half height vertical
                    // b     = half height horizontal
                    // h     = half height diagonal
                    // theta = angle of selected connecting member to bearingmember
                    // phi   = anlge of first connecting member to bearingmember
                    // e     = eccntricity
                    double a = CMs[0].ElementRAZ.crossSection.height / 2;
                    double b = BMs[0].ElementRAZ.crossSection.height / 2;
                    double h = CMs[i].ElementRAZ.crossSection.height / 2;
                    double thetaDirty = VectorRAZ.AngleBetweenVectors(this.bearingMemberUnitVector, CMs[i].ideaLine.vector.Unitize());
                    double theta = (Math.Min(thetaDirty, Math.PI - thetaDirty));

                    double hor = ConnectingMember.WebWeldsHorizontalLength(a, b, h, theta, phi, eccentricity);
                    double ver = ConnectingMember.WebWeldsVerticalLength(a, b, h, theta, phi, eccentricity);
                    //VolumeWeb
                    double surface = Weld.CalcWeldSurface(Math.PI / 2, CMs[i].webWeld.size);
                    double volWeb = 2 * (hor + ver) * surface;
                    weldvolume += volWeb;

                    //VolumeFlanges
                    double width = CMs[i].ElementRAZ.crossSection.width;
                    double tweb = CMs[i].ElementRAZ.crossSection.thicknessWeb;
                    if (hor != 0.0 && ver != 0.0)//double miter
                    {
                        double leftAngle = phi - theta;
                        double rightAngle = theta;
                        if (CMs[i].ElementRAZ.crossSection.shape == CrossSection.Shape.HollowSection)
                        {
                            weldvolume += width * Weld.CalcWeldSurface(leftAngle, CMs[i].flangeWeld.size);
                            weldvolume += width * Weld.CalcWeldSurface(rightAngle, CMs[i].flangeWeld.size);
                        }
                        else//Isection
                        {
                            weldvolume += width * Weld.CalcWeldSurface(leftAngle, CMs[i].flangeWeld.size);
                            weldvolume += width * Weld.CalcWeldSurface(rightAngle, CMs[i].flangeWeld.size);
                            weldvolume += (width - tweb) * Weld.CalcWeldSurface(Math.PI - leftAngle, CMs[i].flangeWeld.size);
                            weldvolume += (width - tweb) * Weld.CalcWeldSurface(Math.PI - rightAngle, CMs[i].flangeWeld.size);
                        }
                    }
                    else//single miter
                    {
                        if (CMs[i].ElementRAZ.crossSection.shape == CrossSection.Shape.HollowSection)
                        {
                            weldvolume += width * Weld.CalcWeldSurface(theta, CMs[i].flangeWeld.size);
                            weldvolume += width * Weld.CalcWeldSurface(Math.PI - theta, CMs[i].flangeWeld.size);
                        }
                        else//Isection
                        {
                            weldvolume += width * Weld.CalcWeldSurface(theta, CMs[i].flangeWeld.size);
                            weldvolume += width * Weld.CalcWeldSurface(Math.PI - theta, CMs[i].flangeWeld.size);
                            weldvolume += (width - tweb) * Weld.CalcWeldSurface(theta, CMs[i].flangeWeld.size);
                            weldvolume += (width - tweb) * Weld.CalcWeldSurface(Math.PI - theta, CMs[i].flangeWeld.size);
                        }
                    }

                }
                if (i == 2)
                {
                    // a     = half height vertical
                    // b     = half height horizontal
                    // h     = half height diagonal
                    // theta = angle of selected connecting member to bearingmember
                    // phi   = anlge of first connecting member to bearingmember
                    // e     = eccntricity
                    double a = CMs[0].ElementRAZ.crossSection.height / 2;
                    double b = BMs[0].ElementRAZ.crossSection.height / 2;
                    double h = CMs[i].ElementRAZ.crossSection.height / 2;
                    double thetaDirty = VectorRAZ.AngleBetweenVectors(this.bearingMemberUnitVector, CMs[i].ideaLine.vector.Unitize());
                    double theta = (Math.Min(thetaDirty, Math.PI - thetaDirty));

                    double hor = ConnectingMember.WebWeldsHorizontalLength(a, b, h, theta, phi, eccentricity);
                    double ver = ConnectingMember.WebWeldsVerticalLength(a, b, h, theta, phi, eccentricity);
                    //VolumeWeb
                    double surface = Weld.CalcWeldSurface(Math.PI / 2, CMs[i].webWeld.size);
                    double volWeb = 2 * (hor + ver) * surface;
                    weldvolume += volWeb;

                    //VolumeFlanges
                    double width = CMs[i].ElementRAZ.crossSection.width;
                    double tweb = CMs[i].ElementRAZ.crossSection.thicknessWeb;
                    if (hor != 0.0 && ver != 0.0)//double miter
                    {
                        double leftAngle = phi - theta;
                        double rightAngle = theta;
                        if (CMs[i].ElementRAZ.crossSection.shape == CrossSection.Shape.HollowSection)
                        {
                            weldvolume += width * Weld.CalcWeldSurface(leftAngle, CMs[i].flangeWeld.size);
                            weldvolume += width * Weld.CalcWeldSurface(rightAngle, CMs[i].flangeWeld.size);
                        }
                        else//Isection
                        {
                            weldvolume += width * Weld.CalcWeldSurface(leftAngle, CMs[i].flangeWeld.size);
                            weldvolume += width * Weld.CalcWeldSurface(rightAngle, CMs[i].flangeWeld.size);
                            weldvolume += (width - tweb) * Weld.CalcWeldSurface(Math.PI - leftAngle, CMs[i].flangeWeld.size);
                            weldvolume += (width - tweb) * Weld.CalcWeldSurface(Math.PI - rightAngle, CMs[i].flangeWeld.size);
                        }
                    }
                    else//single miter
                    {
                        if (CMs[i].ElementRAZ.crossSection.shape == CrossSection.Shape.HollowSection)
                        {
                            weldvolume += width * Weld.CalcWeldSurface(theta, CMs[i].flangeWeld.size);
                            weldvolume += width * Weld.CalcWeldSurface(Math.PI - theta, CMs[i].flangeWeld.size);
                        }
                        else//Isection
                        {
                            weldvolume += width * Weld.CalcWeldSurface(theta, CMs[i].flangeWeld.size);
                            weldvolume += width * Weld.CalcWeldSurface(Math.PI - theta, CMs[i].flangeWeld.size);
                            weldvolume += (width - tweb) * Weld.CalcWeldSurface(theta, CMs[i].flangeWeld.size);
                            weldvolume += (width - tweb) * Weld.CalcWeldSurface(Math.PI - theta, CMs[i].flangeWeld.size);
                        }
                    }
                }
                //weldvolume =weldvolume + c.CalculateWeldVolume();

            }
            return weldvolume;
        }
        public void CalculateWeldThroatsFullStrengthLazy()
        {
            List<ConnectingMember> CMs = this.attachedMembers.OfType<ConnectingMember>().ToList();
            for (int i = 0; i < CMs.Count; i++)
            {
                //Calculate Full strength factor with angle of 0 degrees (full shear)
                double factor = Weld.CalcFullStrengthFactor(CMs[i].ElementRAZ.crossSection.material, 0.0);
                double tweb = CMs[i].ElementRAZ.crossSection.thicknessWeb;
                double weldsizeWeb = Math.Ceiling(tweb * factor);
                if (weldsizeWeb > CMs[i].webWeld.size)
                {
                    CMs[i].webWeld.size = weldsizeWeb;
                }
                else
                {

                }
                double tflange = CMs[i].ElementRAZ.crossSection.thicknessFlange;
                double weldsizeFlange = Math.Ceiling(tflange * factor);
                if (weldsizeFlange > CMs[i].flangeWeld.size)
                {
                    CMs[i].flangeWeld.size = weldsizeFlange;
                }
                else
                {

                }

            }
        }
        public void CalculateWeldThroatsFullStrength()
        {
            double angle90 = Math.PI / 2;//throat-angle is 90degrees
            List<BearingMember> BMs = this.attachedMembers.OfType<BearingMember>().ToList();
            List<ConnectingMember> CMs = this.attachedMembers.OfType<ConnectingMember>().ToList();
            double phi = new double();
            double eccentricity = new double();
            //Convert eccentricity in meter to milimeter
            //If warren double the eccentricity
            if (this.isWarrenEccentricJoint == true)
            {
                eccentricity = 2 * this.maxGlobalEccentricity * 1000;
            }
            else
            {
                eccentricity = this.maxGlobalEccentricity * 1000;
            }

            for (int i = 0; i < CMs.Count; i++)
            {
                if (i == 0)
                {
                    double tweb = CMs[i].ElementRAZ.crossSection.thicknessWeb;
                    double tflange = CMs[i].ElementRAZ.crossSection.thicknessFlange;
                    MaterialSteel steelgrade = CMs[i].ElementRAZ.crossSection.material;
                    double phiDirty = VectorRAZ.AngleBetweenVectors(bearingMemberUnitVector, CMs[i].ElementRAZ.line.vector);
                    phi = Math.PI - (Math.Min(phiDirty, (Math.PI - phiDirty)));//range phi is betwween 90 and 180 degrees


                    //ThroatWeb
                    if (CMs[i].ElementRAZ.crossSection.shape.Equals(CrossSection.Shape.ISection))
                    {
                        double factor = 0.5 * Weld.CalcFullStrengthFactor(steelgrade, angle90);
                        double weldsize = Math.Ceiling(factor * tweb);
                        if (weldsize > CMs[i].webWeld.size)
                        {
                            CMs[i].webWeld.size = weldsize;
                        }

                    }
                    else //Hollowsection
                    {
                        double factor = Weld.CalcFullStrengthFactor(steelgrade, angle90);
                        double weldsize = Math.Ceiling(factor * tweb);
                        if (weldsize > CMs[i].webWeld.size)
                        {
                            CMs[i].webWeld.size = weldsize;
                        }

                    }

                    //ThroatFlanges
                    if (CMs[i].ElementRAZ.crossSection.shape.Equals(CrossSection.Shape.ISection))
                    {
                        double factor1 = 0.5 * Weld.CalcFullStrengthFactor(steelgrade, phi);
                        double factor2 = 0.5 * Weld.CalcFullStrengthFactor(steelgrade, Math.PI - phi);
                        double factor = Math.Max(factor1, factor2);
                        double weldsize = Math.Ceiling(factor * tflange);
                        if (weldsize > CMs[i].webWeld.size)
                        {
                            CMs[i].flangeWeld.size = weldsize;
                        }


                    }
                    else //Hollowsection
                    {
                        double factor1 = Weld.CalcFullStrengthFactor(steelgrade, phi);
                        double factor2 = Weld.CalcFullStrengthFactor(steelgrade, Math.PI - phi);
                        double factor = Math.Max(factor1, factor2);
                        double weldsize = Math.Ceiling(factor * tflange);
                        if (weldsize > CMs[i].flangeWeld.size)
                        {
                            CMs[i].flangeWeld.size = weldsize;
                        }

                    }


                }
                if (i == 1)
                {
                    double tweb = CMs[i].ElementRAZ.crossSection.thicknessWeb;
                    double tflange = CMs[i].ElementRAZ.crossSection.thicknessFlange;
                    MaterialSteel steelgrade = CMs[i].ElementRAZ.crossSection.material;
                    // a     = half height vertical
                    // b     = half height horizontal
                    // h     = half height diagonal
                    // theta = angle of selected connecting member to bearingmember
                    // phi   = anlge of first connecting member to bearingmember
                    // e     = eccntricity
                    double a = CMs[0].ElementRAZ.crossSection.height / 2;
                    double b = BMs[0].ElementRAZ.crossSection.height / 2;
                    double h = CMs[i].ElementRAZ.crossSection.height / 2;
                    double thetaDirty = VectorRAZ.AngleBetweenVectors(this.bearingMemberUnitVector, CMs[i].ideaLine.vector.Unitize());
                    double theta = (Math.Min(thetaDirty, Math.PI - thetaDirty));

                    double hor = ConnectingMember.WebWeldsHorizontalLength(a, b, h, theta, phi, eccentricity);
                    double ver = ConnectingMember.WebWeldsVerticalLength(a, b, h, theta, phi, eccentricity);
                    //ThroatWeb
                    if (CMs[i].ElementRAZ.crossSection.shape.Equals(CrossSection.Shape.ISection))
                    {
                        double factor = 0.5 * Weld.CalcFullStrengthFactor(steelgrade, angle90);
                        double weldsize = Math.Ceiling(factor * tweb);
                        if (weldsize > CMs[i].webWeld.size)
                        {
                            CMs[i].webWeld.size = weldsize;
                        }
                        else
                        {

                        }
                    }
                    else //Hollowsection
                    {
                        double factor = Weld.CalcFullStrengthFactor(steelgrade, angle90);
                        double weldsize = Math.Ceiling(factor * tweb);
                        if (weldsize > CMs[i].webWeld.size)
                        {
                            CMs[i].webWeld.size = weldsize;
                        }
                        else
                        {

                        }
                    }


                    //ThroatFlanges                    
                    if (hor != 0.0 && ver != 0.0)//double miter
                    {
                        double leftAngle = phi - theta;

                        if (CMs[i].ElementRAZ.crossSection.shape == CrossSection.Shape.ISection)
                        {
                            double factor1 = 0.5 * Weld.CalcFullStrengthFactor(steelgrade, theta);
                            double factor2 = 0.5 * Weld.CalcFullStrengthFactor(steelgrade, Math.PI - theta);
                            double factor3 = 0.5 * Weld.CalcFullStrengthFactor(steelgrade, leftAngle);
                            double factor4 = 0.5 * Weld.CalcFullStrengthFactor(steelgrade, Math.PI - leftAngle);
                            double factor = Math.Max(Math.Max(factor1, factor2), Math.Max(factor3, factor4));
                            double weldsize = Math.Ceiling(factor * tflange);
                            if (weldsize > CMs[i].flangeWeld.size)
                            {
                                CMs[i].flangeWeld.size = weldsize;
                            }
                            else
                            {

                            }
                        }
                        else//HollowSection
                        {
                            double factor1 = Weld.CalcFullStrengthFactor(steelgrade, theta);
                            double factor2 = Weld.CalcFullStrengthFactor(steelgrade, leftAngle);
                            double factor = Math.Max(factor1, factor2);
                            double weldsize = Math.Ceiling(factor * tflange);
                            if (weldsize > CMs[i].flangeWeld.size)
                            {
                                CMs[i].flangeWeld.size = weldsize;
                            }
                            else
                            {

                            }
                        }
                    }
                    else//single miter
                    {
                        if (CMs[i].ElementRAZ.crossSection.shape == CrossSection.Shape.ISection)
                        {
                            double factor1 = 0.5 * Weld.CalcFullStrengthFactor(steelgrade, theta);
                            double factor2 = 0.5 * Weld.CalcFullStrengthFactor(steelgrade, Math.PI - theta);
                            double factor = Math.Max(factor1, factor2);
                            double weldsize = Math.Ceiling(factor * tflange);
                            if (weldsize > CMs[i].flangeWeld.size)
                            {
                                CMs[i].flangeWeld.size = weldsize;
                            }
                            else
                            {

                            }
                        }
                        else//HollowSection
                        {
                            double factor1 = Weld.CalcFullStrengthFactor(steelgrade, theta);
                            double factor2 = Weld.CalcFullStrengthFactor(steelgrade, Math.PI - theta);
                            double factor = Math.Max(factor1, factor2);
                            double weldsize = Math.Ceiling(factor * tflange);
                            if (weldsize > CMs[i].flangeWeld.size)
                            {
                                CMs[i].flangeWeld.size = weldsize;
                            }
                            else
                            {

                            }
                        }
                    }

                }
                if (i == 2)
                {
                    double tweb = CMs[i].ElementRAZ.crossSection.thicknessWeb;
                    double tflange = CMs[i].ElementRAZ.crossSection.thicknessFlange;
                    MaterialSteel steelgrade = CMs[i].ElementRAZ.crossSection.material;
                    // a     = half height vertical
                    // b     = half height horizontal
                    // h     = half height diagonal
                    // theta = angle of selected connecting member to bearingmember
                    // phi   = anlge of first connecting member to bearingmember
                    // e     = eccntricity
                    double a = CMs[0].ElementRAZ.crossSection.height / 2;
                    double b = BMs[0].ElementRAZ.crossSection.height / 2;
                    double h = CMs[i].ElementRAZ.crossSection.height / 2;
                    double thetaDirty = VectorRAZ.AngleBetweenVectors(this.bearingMemberUnitVector, CMs[i].ideaLine.vector.Unitize());
                    double theta = (Math.Min(thetaDirty, Math.PI - thetaDirty));

                    double hor = ConnectingMember.WebWeldsHorizontalLength(a, b, h, theta, phi, eccentricity);
                    double ver = ConnectingMember.WebWeldsVerticalLength(a, b, h, theta, phi, eccentricity);
                    //ThroatWeb
                    if (CMs[i].ElementRAZ.crossSection.shape.Equals(CrossSection.Shape.ISection))
                    {
                        double factor = 0.5 * Weld.CalcFullStrengthFactor(steelgrade, angle90);
                        double weldsize = Math.Ceiling(factor * tweb);
                        if (weldsize > CMs[i].webWeld.size)
                        {
                            CMs[i].webWeld.size = weldsize;
                        }
                        else
                        {

                        }
                    }
                    else //Hollowsection
                    {
                        double factor = Weld.CalcFullStrengthFactor(steelgrade, angle90);
                        double weldsize = Math.Ceiling(factor * tweb);
                        if (weldsize > CMs[i].webWeld.size)
                        {
                            CMs[i].webWeld.size = weldsize;
                        }
                        else
                        {

                        }
                    }


                    //ThroatFlanges                    
                    if (hor != 0.0 && ver != 0.0)//double miter
                    {
                        double leftAngle = phi - theta;

                        if (CMs[i].ElementRAZ.crossSection.shape == CrossSection.Shape.ISection)
                        {
                            double factor1 = 0.5 * Weld.CalcFullStrengthFactor(steelgrade, theta);
                            double factor2 = 0.5 * Weld.CalcFullStrengthFactor(steelgrade, Math.PI - theta);
                            double factor3 = 0.5 * Weld.CalcFullStrengthFactor(steelgrade, leftAngle);
                            double factor4 = 0.5 * Weld.CalcFullStrengthFactor(steelgrade, Math.PI - leftAngle);
                            double factor = Math.Max(Math.Max(factor1, factor2), Math.Max(factor3, factor4));
                            double weldsize = Math.Ceiling(factor * tflange);
                            if (weldsize > CMs[i].flangeWeld.size)
                            {
                                CMs[i].flangeWeld.size = weldsize;
                            }
                            else
                            {

                            }
                        }
                        else//HollowSection
                        {
                            double factor1 = Weld.CalcFullStrengthFactor(steelgrade, theta);
                            double factor2 = Weld.CalcFullStrengthFactor(steelgrade, leftAngle);
                            double factor = Math.Max(factor1, factor2);
                            double weldsize = Math.Ceiling(factor * tflange);
                            if (weldsize > CMs[i].flangeWeld.size)
                            {
                                CMs[i].flangeWeld.size = weldsize;
                            }
                            else
                            {

                            }
                        }
                    }
                    else//single miter
                    {
                        if (CMs[i].ElementRAZ.crossSection.shape == CrossSection.Shape.ISection)
                        {
                            double factor1 = 0.5 * Weld.CalcFullStrengthFactor(steelgrade, theta);
                            double factor2 = 0.5 * Weld.CalcFullStrengthFactor(steelgrade, Math.PI - theta);
                            double factor = Math.Max(factor1, factor2);
                            double weldsize = Math.Ceiling(factor * tflange);
                            if (weldsize > CMs[i].flangeWeld.size)
                            {
                                CMs[i].flangeWeld.size = weldsize;
                            }
                            else
                            {

                            }
                        }
                        else//HollowSection
                        {
                            double factor1 = Weld.CalcFullStrengthFactor(steelgrade, theta);
                            double factor2 = Weld.CalcFullStrengthFactor(steelgrade, Math.PI - theta);
                            double factor = Math.Max(factor1, factor2);
                            double weldsize = Math.Ceiling(factor * tflange);
                            if (weldsize > CMs[i].flangeWeld.size)
                            {
                                CMs[i].flangeWeld.size = weldsize;
                            }
                            else
                            {

                            }
                        }
                    }
                }
            }
        }
        public void CalculateWeldThroatsDirectional()
        {

            List<BearingMember> BMs = this.attachedMembers.OfType<BearingMember>().ToList();
            List<ConnectingMember> CMs = this.attachedMembers.OfType<ConnectingMember>().ToList();
            double phi = new double();
            double eccentricity = new double();
            //Convert eccentricity in meter to milimeter
            //If warren double the eccentricity
            if (this.isWarrenEccentricJoint == true)
            {
                eccentricity = 2 * this.maxGlobalEccentricity * 1000;
            }
            else
            {
                eccentricity = this.maxGlobalEccentricity * 1000;
            }

            for (int i = 0; i < CMs.Count; i++)
            {
                if (i == 0)
                {
                    double a = CMs[i].ElementRAZ.crossSection.height / 2;//half height vertical in mm
                    double phiDirty = VectorRAZ.AngleBetweenVectors(bearingMemberUnitVector, CMs[i].ElementRAZ.line.vector);
                    phi = Math.PI - (Math.Min(phiDirty, (Math.PI - phiDirty)));//range phi is betwween 90 and 180 degrees
                    double weblength = ConnectingMember.WebWeldFirstAttachedLength(a, phi);

                    double height = CMs[i].ElementRAZ.crossSection.height;
                    double width = CMs[i].ElementRAZ.crossSection.width;
                    double tweb = CMs[i].ElementRAZ.crossSection.thicknessWeb;
                    double tflange = CMs[i].ElementRAZ.crossSection.thicknessFlange;

                    //1.calculate effective width
                    double widthBear = BMs[0].ElementRAZ.crossSection.width;
                    double twebBear = BMs[0].ElementRAZ.crossSection.thicknessWeb;
                    double tflangeBear = BMs[0].ElementRAZ.crossSection.thicknessFlange;
                    double radiusBear = BMs[0].ElementRAZ.crossSection.radius;
                    double fyBear = BMs[0].ElementRAZ.crossSection.material.fy;
                    double fyConn = CMs[i].ElementRAZ.crossSection.material.fy;
                    double k = (tflangeBear / tflange) * (fyConn / fyBear);
                    if (k > 1.0)
                    {
                        k = 1.0;
                    }
                    else
                    {
                        //nothing
                    }
                    double beff = twebBear + twebBear + (7 * k * tflangeBear);

                    double overlap = (widthBear / 2) - (width / 2);
                    if (overlap < 0.0)
                    {
                        overlap = 0.0;
                    }
                    double beff2 = beff - (2 * overlap);


                    //2.calculate total welding length
                    //normal force will be divided over this length
                    double activeLength = new double();
                    if (CMs[i].ElementRAZ.crossSection.shape.Equals(CrossSection.Shape.ISection))
                    {
                        if (width >= beff)
                        {
                            if (BMs[0].ElementRAZ.crossSection.shape == CrossSection.Shape.HollowSection)
                            {
                                activeLength = (weblength - 2 * tflange) * 2 + (beff2) * 2 + (beff2) * 2;
                            }
                            else//bear is Isection
                            {
                                activeLength = (weblength - 2 * tflange) * 2 + (width) * 2 + (width - tweb) * 2;
                            }

                        }
                        else
                        {
                            activeLength = (weblength - 2 * tflange) * 2 + (beff) * 2 + (beff - tweb) * 2;
                        }

                    }
                    else//Hollow section
                    {
                        if (width >= beff)
                        {
                            activeLength = (weblength) * 2 + (width) * 2;
                        }
                        else
                        {

                            //check if webwelds participate, fall in beff range
                            if (BMs[0].ElementRAZ.crossSection.shape == CrossSection.Shape.HollowSection)
                            {
                                activeLength = (weblength) * 2 + (beff2) * 2;
                            }
                            else
                            {
                                activeLength = beff * 2;
                            }
                        }
                    }



                    //3.iterate over loadcases
                    List<double> normalforces = new List<double>();
                    foreach (LoadcaseRAZ l in this.project.loadcases)
                    {
                        if (CMs[i].isStartPoint == true)
                        {
                            double nf = l.loadsPerLineRAZs[CMs[i].ElementRAZ.id].startLoads.N;//kN
                            double nmm = (nf * 1000) / activeLength;
                            normalforces.Add(nmm);
                        }
                        else
                        {
                            double nf = l.loadsPerLineRAZs[CMs[i].ElementRAZ.id].endLoads.N;//kN
                            double nmm = (nf * 1000) / activeLength;
                            normalforces.Add(nmm);
                        }
                    }

                    //4.calculate required throats and pick max
                    MaterialSteel steelgrade = CMs[i].ElementRAZ.crossSection.material;
                    phi = Math.PI - (Math.Min(phiDirty, (Math.PI - phiDirty)));//range phi is betwween 90 and 180 degrees


                    //ThroatWeb (in this case always Single Miter)
                    List<double> weldsizesWeb = new List<double>();
                    foreach (double Nmm in normalforces)
                    {
                        //only one angle
                        double weldsize1 = Weld.CalcDirWebThroat(steelgrade, phi, Nmm);
                        weldsizesWeb.Add(weldsize1);

                    }
                    double weldsizeWebFinal = Math.Ceiling(weldsizesWeb.Max());
                    if (weldsizeWebFinal > CMs[i].webWeld.size)
                    {
                        CMs[i].webWeld.size = weldsizeWebFinal;
                    }
                    else
                    {
                        //nothing
                    }

                    //ThroatFlanges
                    List<double> weldsizesFlange = new List<double>();
                    foreach (double Nmm in normalforces)
                    {
                        //two angles
                        double weldsize1 = Weld.CalcDirFlangeThroat(steelgrade, phi, Nmm);
                        weldsizesFlange.Add(weldsize1);
                        double weldsize2 = Weld.CalcDirFlangeThroat(steelgrade, Math.PI - phi, Nmm);
                        weldsizesFlange.Add(weldsize2);

                    }
                    double weldsizeFlangeFinal = Math.Ceiling(weldsizesFlange.Max());
                    if (weldsizeFlangeFinal > CMs[i].flangeWeld.size)
                    {
                        CMs[i].flangeWeld.size = weldsizeFlangeFinal;
                    }
                    else
                    {
                        //nothing
                    }




                }
                if (i == 1)
                {

                    // a     = half height vertical
                    // b     = half height horizontal
                    // h     = half height diagonal
                    // theta = angle of selected connecting member to bearingmember
                    // phi   = anlge of first connecting member to bearingmember
                    // e     = eccntricity
                    double a = CMs[0].ElementRAZ.crossSection.height / 2;
                    double b = BMs[0].ElementRAZ.crossSection.height / 2;
                    double h = CMs[i].ElementRAZ.crossSection.height / 2;
                    double thetaDirty = VectorRAZ.AngleBetweenVectors(this.bearingMemberUnitVector, CMs[i].ideaLine.vector.Unitize());
                    double theta = (Math.Min(thetaDirty, Math.PI - thetaDirty));

                    double hor = ConnectingMember.WebWeldsHorizontalLength(a, b, h, theta, phi, eccentricity);
                    double ver = ConnectingMember.WebWeldsVerticalLength(a, b, h, theta, phi, eccentricity);

                    double weblength = hor + ver;

                    double height = CMs[i].ElementRAZ.crossSection.height;
                    double width = CMs[i].ElementRAZ.crossSection.width;
                    double tweb = CMs[i].ElementRAZ.crossSection.thicknessWeb;
                    double tflange = CMs[i].ElementRAZ.crossSection.thicknessFlange;

                    //calculate effective width
                    double widthBear = BMs[0].ElementRAZ.crossSection.width;
                    double twebBear = BMs[0].ElementRAZ.crossSection.thicknessWeb;
                    double tflangeBear = BMs[0].ElementRAZ.crossSection.thicknessFlange;
                    double radiusBear = BMs[0].ElementRAZ.crossSection.radius;
                    double fyBear = BMs[0].ElementRAZ.crossSection.material.fy;
                    double fyConn = CMs[i].ElementRAZ.crossSection.material.fy;
                    double k = (tflangeBear / tflange) * (fyConn / fyBear);
                    if (k > 1.0)
                    {
                        k = 1.0;
                    }
                    else
                    {
                        //nothing
                    }
                    double beff = twebBear + twebBear + (7 * k * tflangeBear);

                    double overlap = (widthBear / 2) - (width / 2);
                    if (overlap < 0.0)
                    {
                        overlap = 0.0;
                    }
                    double beff2 = beff - (2 * overlap);


                    //calculate total welding length
                    //normal force will be divided over this length
                    double activeLength = new double();
                    if (CMs[i].ElementRAZ.crossSection.shape.Equals(CrossSection.Shape.ISection))
                    {
                        if (width >= beff)
                        {
                            if (BMs[0].ElementRAZ.crossSection.shape == CrossSection.Shape.HollowSection)
                            {
                                activeLength = (weblength - 2 * tflange) * 2 + (beff2) * 2 + (beff2) * 2;
                            }
                            else//bear is Isection
                            {
                                activeLength = (weblength - 2 * tflange) * 2 + (width) * 2 + (width - tweb) * 2;
                            }

                        }
                        else
                        {
                            activeLength = (weblength - 2 * tflange) * 2 + (beff) * 2 + (beff - tweb) * 2;
                        }

                    }
                    else//Hollow section
                    {
                        if (width >= beff)
                        {
                            activeLength = (weblength) * 2 + (width) * 2;
                        }
                        else
                        {

                            //check if webwelds participate, fall in beff range
                            if (BMs[0].ElementRAZ.crossSection.shape == CrossSection.Shape.HollowSection)
                            {
                                activeLength = (weblength) * 2 + (beff2) * 2;
                            }
                            else
                            {
                                activeLength = beff * 2;
                            }
                        }
                    }



                    //iterate over loadcases
                    List<double> normalforces = new List<double>();
                    foreach (LoadcaseRAZ l in this.project.loadcases)
                    {
                        if (CMs[i].isStartPoint == true)
                        {
                            double nf = l.loadsPerLineRAZs[CMs[i].ElementRAZ.id].startLoads.N;//kN
                            double nmm = (nf * 1000) / activeLength;
                            normalforces.Add(nmm);
                        }
                        else
                        {
                            double nf = l.loadsPerLineRAZs[CMs[i].ElementRAZ.id].endLoads.N;//kN
                            double nmm = (nf * 1000) / activeLength;
                            normalforces.Add(nmm);
                        }
                    }


                    MaterialSteel steelgrade = CMs[i].ElementRAZ.crossSection.material;
                    //double phiDirty = VectorRAZ.AngleBetweenVectors(bearingMemberUnitVector, CMs[i].ElementRAZ.line.vector);



                    //ThroatWeb (in this case always Single Miter)
                    List<double> weldsizesWeb = new List<double>();
                    foreach (double Nmm in normalforces)
                    {
                        if (hor != 0.0 && ver != 0.0)//double miter
                        {
                            //two angles
                            double weldsize1 = Weld.CalcDirWebThroat(steelgrade, phi - theta, Nmm);
                            weldsizesWeb.Add(weldsize1);
                            double weldsize2 = Weld.CalcDirWebThroat(steelgrade, theta, Nmm);
                            weldsizesWeb.Add(weldsize2);
                        }
                        else//single miter
                        {
                            //only one angle
                            double weldsize2 = Weld.CalcDirWebThroat(steelgrade, theta, Nmm);
                            weldsizesWeb.Add(weldsize2);
                        }

                    }
                    double weldsizeWebFinal = Math.Ceiling(weldsizesWeb.Max());
                    if (weldsizeWebFinal > CMs[i].webWeld.size)
                    {
                        CMs[i].webWeld.size = weldsizeWebFinal;
                    }
                    else
                    {
                        //nothing
                    }

                    //ThroatFlanges
                    List<double> weldsizesFlange = new List<double>();
                    foreach (double Nmm in normalforces)
                    {
                        if (hor != 0.0 && ver != 0.0)//double miter
                        {
                            if (CMs[i].ElementRAZ.crossSection.shape == CrossSection.Shape.HollowSection)
                            {
                                //two angles
                                double weldsizeA = Weld.CalcDirFlangeThroat(steelgrade, theta, Nmm);
                                weldsizesFlange.Add(weldsizeA);
                                double weldsizeB = Weld.CalcDirFlangeThroat(steelgrade, phi - theta, Nmm);
                                weldsizesFlange.Add(weldsizeB);
                            }
                            else//Isection
                            {
                                //four angles
                                double weldsizeA = Weld.CalcDirFlangeThroat(steelgrade, theta, Nmm);
                                weldsizesFlange.Add(weldsizeA);
                                double weldsizeB = Weld.CalcDirFlangeThroat(steelgrade, phi - theta, Nmm);
                                weldsizesFlange.Add(weldsizeB);
                                double weldsizeC = Weld.CalcDirFlangeThroat(steelgrade, Math.PI - (phi - theta), Nmm);
                                weldsizesFlange.Add(weldsizeC);
                                double weldsizeD = Weld.CalcDirFlangeThroat(steelgrade, Math.PI - theta, Nmm);
                                weldsizesFlange.Add(weldsizeD);
                            }
                        }
                        else//single miter
                        {
                            double weldsizeA = Weld.CalcDirFlangeThroat(steelgrade, theta, Nmm);
                            weldsizesFlange.Add(weldsizeA);
                            double weldsizeB = Weld.CalcDirFlangeThroat(steelgrade, Math.PI - theta, Nmm);
                            weldsizesFlange.Add(weldsizeB);
                        }



                    }
                    double weldsizeFlangeFinal = Math.Ceiling(weldsizesFlange.Max());
                    if (weldsizeFlangeFinal > CMs[i].flangeWeld.size)
                    {
                        CMs[i].flangeWeld.size = weldsizeFlangeFinal;
                    }
                    else
                    {
                        //nothing
                    }

                }
                if (i == 2)
                {
                    // a     = half height vertical
                    // b     = half height horizontal
                    // h     = half height diagonal
                    // theta = angle of selected connecting member to bearingmember
                    // phi   = anlge of first connecting member to bearingmember
                    // e     = eccntricity
                    double a = CMs[0].ElementRAZ.crossSection.height / 2;
                    double b = BMs[0].ElementRAZ.crossSection.height / 2;
                    double h = CMs[i].ElementRAZ.crossSection.height / 2;
                    double thetaDirty = VectorRAZ.AngleBetweenVectors(this.bearingMemberUnitVector, CMs[i].ideaLine.vector.Unitize());
                    double theta = (Math.Min(thetaDirty, Math.PI - thetaDirty));

                    double hor = ConnectingMember.WebWeldsHorizontalLength(a, b, h, theta, phi, eccentricity);
                    double ver = ConnectingMember.WebWeldsVerticalLength(a, b, h, theta, phi, eccentricity);

                    double weblength = hor + ver;

                    double height = CMs[i].ElementRAZ.crossSection.height;
                    double width = CMs[i].ElementRAZ.crossSection.width;
                    double tweb = CMs[i].ElementRAZ.crossSection.thicknessWeb;
                    double tflange = CMs[i].ElementRAZ.crossSection.thicknessFlange;

                    //calculate effective width
                    double widthBear = BMs[0].ElementRAZ.crossSection.width;
                    double twebBear = BMs[0].ElementRAZ.crossSection.thicknessWeb;
                    double tflangeBear = BMs[0].ElementRAZ.crossSection.thicknessFlange;
                    double radiusBear = BMs[0].ElementRAZ.crossSection.radius;
                    double fyBear = BMs[0].ElementRAZ.crossSection.material.fy;
                    double fyConn = CMs[i].ElementRAZ.crossSection.material.fy;
                    double k = (tflangeBear / tflange) * (fyConn / fyBear);
                    if (k > 1.0)
                    {
                        k = 1.0;
                    }
                    else
                    {
                        //nothing
                    }
                    double beff = twebBear + twebBear + (7 * k * tflangeBear);

                    double overlap = (widthBear / 2) - (width / 2);
                    if (overlap < 0.0)
                    {
                        overlap = 0.0;
                    }
                    double beff2 = beff - (2 * overlap);


                    //calculate total welding length
                    //normal force will be divided over this length
                    double activeLength = new double();
                    if (CMs[i].ElementRAZ.crossSection.shape.Equals(CrossSection.Shape.ISection))
                    {
                        if (width >= beff)
                        {
                            if (BMs[0].ElementRAZ.crossSection.shape == CrossSection.Shape.HollowSection)
                            {
                                activeLength = (weblength - 2 * tflange) * 2 + (beff2) * 2 + (beff2) * 2;
                            }
                            else//bear is Isection
                            {
                                activeLength = (weblength - 2 * tflange) * 2 + (width) * 2 + (width - tweb) * 2;
                            }

                        }
                        else
                        {
                            activeLength = (weblength - 2 * tflange) * 2 + (beff) * 2 + (beff - tweb) * 2;
                        }

                    }
                    else//Hollow section
                    {
                        if (width >= beff)
                        {
                            activeLength = (weblength) * 2 + (width) * 2;
                        }
                        else
                        {

                            //check if webwelds participate, fall in beff range
                            if (BMs[0].ElementRAZ.crossSection.shape == CrossSection.Shape.HollowSection)
                            {
                                activeLength = (weblength) * 2 + (beff2) * 2;
                            }
                            else
                            {
                                activeLength = beff * 2;
                            }
                        }
                    }



                    //iterate over loadcases
                    List<double> normalforces = new List<double>();
                    foreach (LoadcaseRAZ l in this.project.loadcases)
                    {
                        if (CMs[i].isStartPoint == true)
                        {
                            double nf = l.loadsPerLineRAZs[CMs[i].ElementRAZ.id].startLoads.N;//kN
                            double nmm = (nf * 1000) / activeLength;
                            normalforces.Add(nmm);
                        }
                        else
                        {
                            double nf = l.loadsPerLineRAZs[CMs[i].ElementRAZ.id].endLoads.N;//kN
                            double nmm = (nf * 1000) / activeLength;
                            normalforces.Add(nmm);
                        }
                    }


                    MaterialSteel steelgrade = CMs[i].ElementRAZ.crossSection.material;
                    //double phiDirty = VectorRAZ.AngleBetweenVectors(bearingMemberUnitVector, CMs[i].ElementRAZ.line.vector);



                    //ThroatWeb (in this case always Single Miter)
                    List<double> weldsizesWeb = new List<double>();
                    foreach (double Nmm in normalforces)
                    {
                        if (hor != 0.0 && ver != 0.0)//double miter
                        {
                            //two angles
                            double weldsize1 = Weld.CalcDirWebThroat(steelgrade, phi - theta, Nmm);
                            weldsizesWeb.Add(weldsize1);
                            double weldsize2 = Weld.CalcDirWebThroat(steelgrade, theta, Nmm);
                            weldsizesWeb.Add(weldsize2);
                        }
                        else//single miter
                        {
                            //only one angle
                            double weldsize2 = Weld.CalcDirWebThroat(steelgrade, theta, Nmm);
                            weldsizesWeb.Add(weldsize2);
                        }

                    }
                    double weldsizeWebFinal = Math.Ceiling(weldsizesWeb.Max());
                    if (weldsizeWebFinal > CMs[i].webWeld.size)
                    {
                        CMs[i].webWeld.size = weldsizeWebFinal;
                    }
                    else
                    {
                        //nothing
                    }

                    //ThroatFlanges
                    List<double> weldsizesFlange = new List<double>();
                    foreach (double Nmm in normalforces)
                    {
                        if (hor != 0.0 && ver != 0.0)//double miter
                        {
                            if (CMs[i].ElementRAZ.crossSection.shape == CrossSection.Shape.HollowSection)
                            {
                                //two angles
                                double weldsizeA = Weld.CalcDirFlangeThroat(steelgrade, theta, Nmm);
                                weldsizesFlange.Add(weldsizeA);
                                double weldsizeB = Weld.CalcDirFlangeThroat(steelgrade, phi - theta, Nmm);
                                weldsizesFlange.Add(weldsizeB);
                            }
                            else//Isection
                            {
                                //four angles
                                double weldsizeA = Weld.CalcDirFlangeThroat(steelgrade, theta, Nmm);
                                weldsizesFlange.Add(weldsizeA);
                                double weldsizeB = Weld.CalcDirFlangeThroat(steelgrade, phi - theta, Nmm);
                                weldsizesFlange.Add(weldsizeB);
                                double weldsizeC = Weld.CalcDirFlangeThroat(steelgrade, Math.PI - (phi - theta), Nmm);
                                weldsizesFlange.Add(weldsizeC);
                                double weldsizeD = Weld.CalcDirFlangeThroat(steelgrade, Math.PI - theta, Nmm);
                                weldsizesFlange.Add(weldsizeD);
                            }
                        }
                        else//single miter
                        {
                            double weldsizeA = Weld.CalcDirFlangeThroat(steelgrade, theta, Nmm);
                            weldsizesFlange.Add(weldsizeA);
                            double weldsizeB = Weld.CalcDirFlangeThroat(steelgrade, Math.PI - theta, Nmm);
                            weldsizesFlange.Add(weldsizeB);
                        }



                    }
                    double weldsizeFlangeFinal = Math.Ceiling(weldsizesFlange.Max());
                    if (weldsizeFlangeFinal > CMs[i].flangeWeld.size)
                    {
                        CMs[i].flangeWeld.size = weldsizeFlangeFinal;
                    }
                    else
                    {
                        //nothing
                    }

                }
            }

        }

    }
}
