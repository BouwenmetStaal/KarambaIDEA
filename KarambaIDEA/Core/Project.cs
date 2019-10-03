// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;




using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Text;



using System.Xml;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace KarambaIDEA.Core
{
    public class Project
    {
        public string projectName = null;
        public string author;
        public string filepath;
        public string templatePath;
        public double minthroat;
        public bool startIDEA;
        public bool calculateAllJoints;
        public int calculateThisJoint;

        public List<PointRAZ> pointRAZs = new List<PointRAZ>();
        public List<ElementRAZ> elementRAZs = new List<ElementRAZ>();
        public List<LoadcaseRAZ> loadcases = new List<LoadcaseRAZ>();
        public List<Joint> joints = new List<Joint>();
        public List<Hierarchy> hierarchylist = new List<Hierarchy>();
        public List<CrossSection> crossSections = new List<CrossSection>();
        public List<MaterialSteel> materials = new List<MaterialSteel>();


        public AnalysisMethod analysisMethod = AnalysisMethod.FullStrengthMethod;

        public static double tolerance = 1e-6;
        public static double gammaM2 = 1.25;

        public Project()
        {

        }

        public Project(string _projectName)
        {
            this.projectName = _projectName;
            
        }

        public Project(string projectname, List<Hierarchy> hierarchylist, List<ElementRAZ> _elementRAZs, AnalysisMethod _analysisMethod = AnalysisMethod.FullStrengthMethod)
        {
            this.elementRAZs = _elementRAZs;
            this.analysisMethod = _analysisMethod;
        }


        public void SaveToXML(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Create))
            {

                var XML = new XmlSerializer(typeof(Project));
                XML.Serialize(stream, this);
                stream.Close();

            }
        }

        public void SetBrandnames(Project project)
        {
            foreach (Joint joint in project.joints)
            {
                List<int> intlist = new List<int>();
                foreach (AttachedMember atta in joint.attachedMembers)
                {
                    int number = atta.ElementRAZ.numberInHierarchy;
                    intlist.Add(number);
                }
                intlist= intlist.OrderBy(x => x).ToList();
                string str = "";
                foreach (var s in intlist)
                {
                    str = str + s.ToString();
                }

                joint.brandName = str;
            }
        }

        public static Project ReadProjectFromXML(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open))
            {
                var XML = new XmlSerializer(typeof(Project));
                return (Project)XML.Deserialize(stream);
            }
        }

        /// <summary>
        /// Create Folder to save IDEA files on location spedified, or on default location
        /// </summary>
        /// <param name="userpath">folder specified by user</param>
        public void CreateFolder(string userpath)
        {
            //create folder
            String timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string pathlocation = "";
            if (string.IsNullOrEmpty(userpath) || string.IsNullOrWhiteSpace(userpath))
            {
                pathlocation = @"C:\Data";
            }
            else
            {
                
                pathlocation = userpath;
               
            }            
                        
            this.filepath = Path.Combine(pathlocation, timeStamp);
            if (!Directory.Exists(this.filepath))
            {
                if (Uri.IsWellFormedUriString(filepath, UriKind.Absolute))
                {
                    Directory.CreateDirectory(this.filepath);
                }
                else
                {
                    //System.Windows.MessageBox.Show("Provided path is invalid, provided path:" + userpath, "Path invalid");
                    //TODO include workerthread or progress bar
                }

            }
        }

        /// <summary>
        /// All data is inventised and converted to seperate data needed to calculate individual joints 
        /// </summary>
        /// <param name="tol">tolerance</param>
        /// <param name="eccentricity">eccentricity if specified</param>
        /// <param name="points">points in project</param>
        /// <param name="elementRAZs">elements of project</param>
        /// <param name="hierarchy">hierarchy if specified</param>
        public void CreateJoints(double tol, double eccentricity, List<PointRAZ> points, List<ElementRAZ> elementRAZs, List<Hierarchy> hierarchy)
        {
            double tolbox = tol + eccentricity;
            List<Joint> joints = new List<Joint>();

            //iterate over all the points that represent centerpoints of the joint
            for (int i = 0; i < points.Count; i++)
            {
                PointRAZ centerpoint = points[i];
                List<VectorRAZ> eccentricityVectors = new List<VectorRAZ>();
                List<int> elementIDs = new List<int>();
                List<LineRAZ> linesatcenter = new List<LineRAZ>();

                List<AttachedMember> attachedMemTemp = new List<AttachedMember>();
                List<AttachedMember> attachedMembers = new List<AttachedMember>();

                //1. DEFINE JOINTS
                //iterate over all lines in project
                foreach (ElementRAZ element in elementRAZs)
                {
                                      
                    //STARTPoints
                    //If fromPoints or startPoints of line fall in the tolerancebox than add lines.
                    if (PointRAZ.ArePointsEqual(tolbox, centerpoint, element.line.Start) && element.line.vector.length > tolbox)
                    {
                        LineRAZ line = element.line;
                        VectorRAZ distancevector = new VectorRAZ(0.0, 0.0, 0.0);
                        double localEccnetricty = 0.0;
                        
                        

                        ConnectingMember connectingMember = new ConnectingMember(element, distancevector, true, line, localEccnetricty);

                        elementIDs.Add(elementRAZs.IndexOf(element));
                        attachedMemTemp.Add(connectingMember);
                    }
                    //ENDPoints
                    //If toPoints or endPoints of line fall in the tolerancebox than add lines.
                    if (PointRAZ.ArePointsEqual(tolbox, centerpoint, element.line.End) && element.line.vector.length > tolbox)
                    {
                        
                        VectorRAZ distancevector = new VectorRAZ(0.0, 0.0, 0.0);
                        double localEccnetricty = 0.0;
                        //IDEAline
                        LineRAZ idealine = LineRAZ.FlipLine(element.line);//in this case of endpoint line needs to be flipped
                        

                        ConnectingMember connectingMember = new ConnectingMember(element, distancevector, false, idealine, localEccnetricty);

                        elementIDs.Add(elementRAZs.IndexOf(element));
                        attachedMemTemp.Add(connectingMember);
                    }
                }


                //2. ORDER ATTACHEDMEMBERS ACCORDING TO HIERARCHY

                bool IsContinues = true;
                //Redistribute attachedMemTemp over BearingMember and ConnectingMember
                //iterate over hierarchy rank to make sure list is created in a orded way
                if (!hierarchy.Any())
                {
                    //no hierarchy, first member found is an ended bearing member
                    IsContinues = false;
                    //First member is bearing
                    AttachedMember w = attachedMemTemp.First();
                    BearingMember bearing = new BearingMember(w.ElementRAZ, w.distanceVector, w.isStartPoint, w.ideaLine);
                    attachedMembers.Add(bearing);
                    //Rest of members are connecting members
                    for (int b = 1; b < attachedMemTemp.Count; b++)
                    {
                        attachedMembers.Add(attachedMemTemp[b]);
                    }
                }
                else
                {
                    //hierarchy determined, list will be build based on hierarchy
                    //If only one hierarchy entry defined
                    if (hierarchy.Count == 1)
                    {
                        IsContinues = false;
                        //First member is bearing
                        AttachedMember w = attachedMemTemp.First();
                        BearingMember bearing = new BearingMember(w.ElementRAZ, w.distanceVector, w.isStartPoint, w.ideaLine);
                        attachedMembers.Add(bearing);
                        //Rest of members are connecting members
                        for (int b = 1; b < attachedMemTemp.Count; b++)
                        {
                            attachedMembers.Add(attachedMemTemp[b]);
                        }
                    }
                    else
                    {
                        for (int rank = 0; rank < 1 + hierarchy.Max(a => a.numberInHierarchy); rank++)
                        {

                            //iterate over attachedMembers of every joint
                            //List<AttachedMember> templist = new List<AttachedMember>();

                            for (int ibb = 0; ibb < attachedMemTemp.Count; ibb++)
                            {
                                AttachedMember w = attachedMemTemp[ibb];
                                //if hierarchy if the highest occuring
                                if (w.ElementRAZ.numberInHierarchy == rank && rank == attachedMemTemp.Min(a => a.ElementRAZ.numberInHierarchy))
                                {
                                    BearingMember bearing = new BearingMember(w.ElementRAZ, w.distanceVector, w.isStartPoint, w.ideaLine);
                                    attachedMembers.Add(bearing);
                                }
                                if (w.ElementRAZ.numberInHierarchy == rank && rank != attachedMemTemp.Min(a => a.ElementRAZ.numberInHierarchy))
                                {
                                    attachedMembers.Add(w);
                                    //temp
                                    //templist.Add(attachedMemTemp[ibb]);
                                }
                            }
                        }
                    }

                    
                    //If there is more than one Bearing Member, IsContinues joint
                    List<BearingMember> BM = attachedMembers.OfType<BearingMember>().ToList();
                    
                    if (BM.Count == 1)
                    {
                        IsContinues = false;
                    }

                }
                
                //3. ADD JOINTS TO PROJECT
                //CREATE JOINT ADD TO PROJECT
                //Joint id starts from one, because IDEA counts from one
                double maxGlobalEccentricity = 0.0;
                bool WEJ = false;
                VectorRAZ bearingMemberUnitVector = new VectorRAZ(1.0, 0.0, 0.0);
                Joint joint = new Joint(this, i + 1, elementIDs, attachedMembers, centerpoint, maxGlobalEccentricity, WEJ, bearingMemberUnitVector, IsContinues);
                this.joints.Add(joint);
            }

        }

        public static void CalculateSawingCuts(Project project, double tol)
        {
            foreach (Joint j in project.joints)
            {
                double eccentricity = new double();
                //Convert eccentricity in meter to milimeter
                //If warren double the eccentricity
                if (j.isWarrenEccentricJoint == true)
                {
                    eccentricity = 2 * j.maxGlobalEccentricity * 1000;
                }
                else
                {
                    eccentricity = j.maxGlobalEccentricity * 1000;
                }

                //phi is the angle between the bearing member and the highest ranked connecting member
                double phi = new double();
                List<AttachedMember> connectingMembers = new List<AttachedMember>();
                AttachedMember bear = new AttachedMember();
                foreach (var con in j.attachedMembers)
                {
                    BearingMember bearing = new BearingMember();

                    if (con is BearingMember)
                    {
                        bear = con;

                    }
                    if (con is ConnectingMember)
                    {
                        connectingMembers.Add(con);
                    }

                }
                BearingMember BM = j.attachedMembers.OfType<BearingMember>().First();
                if (j.IsContinues == false)
                {
                    if (BM.isStartPoint == true)
                    {
                        BM.ElementRAZ.startCut = ElementRAZ.SawingCut.RightAngledCut;
                    }
                    else
                    {
                        BM.ElementRAZ.endCut = ElementRAZ.SawingCut.RightAngledCut;
                    }
                }

                for (int i = 0; i < connectingMembers.Count; i++)
                {




                    if (i == 0)
                    {

                        double phiDirty = VectorRAZ.AngleBetweenVectors(j.bearingMemberUnitVector, connectingMembers[i].ElementRAZ.line.vector);
                        //phi should be an angle between 90 and 180 degree

                        phi = Math.PI - (Math.Min(phiDirty, (Math.PI - phiDirty)));

                        //Check if the first connecting member is a RightAngledCut or SingleMiterCut
                        //This is checked by whether theta is equal to 90 degrees (1.57 rad)
                        //phi==0 is needed for the midspan T-joint where the value of phi is lost because of some mysterious reason
                        if (Math.Abs(0.5 * Math.PI - phi) < tol || phi == 0 || phi == Math.PI)
                        {
                            //RightAngledCut
                            if (connectingMembers[i].isStartPoint == true)
                            {
                                connectingMembers[i].ElementRAZ.startCut = ElementRAZ.SawingCut.RightAngledCut;
                            }
                            else
                            {
                                connectingMembers[i].ElementRAZ.endCut = ElementRAZ.SawingCut.RightAngledCut;
                            }
                        }
                        else
                        {
                            //SingleMiterCut
                            if (connectingMembers[i].isStartPoint == true)
                            {
                                connectingMembers[i].ElementRAZ.startCut = ElementRAZ.SawingCut.SingleMiterCut;
                            }
                            else
                            {
                                connectingMembers[i].ElementRAZ.endCut = ElementRAZ.SawingCut.SingleMiterCut;
                            }
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

                        double a = connectingMembers[0].ElementRAZ.crossSection.height / 2;
                        double b = bear.ElementRAZ.crossSection.height / 2;
                        double h = connectingMembers[1].ElementRAZ.crossSection.height / 2;
                        double thetaDirty = VectorRAZ.AngleBetweenVectors(j.bearingMemberUnitVector, connectingMembers[1].ElementRAZ.line.vector);
                        //theta should be an angle between 0 and 90 degree
                        double theta = (Math.Min(thetaDirty, Math.PI - thetaDirty));

                        double hor = ConnectingMember.WebWeldsHorizontalLength(a, b, h, theta, phi, eccentricity);
                        double ver = ConnectingMember.WebWeldsVerticalLength(a, b, h, theta, phi, eccentricity);

                        if (hor != 0.0 && ver != 0.0)
                        {
                            //DoubleMiterCut
                            if (connectingMembers[i].isStartPoint == true)
                            {
                                connectingMembers[i].ElementRAZ.startCut = ElementRAZ.SawingCut.DoubleMiterCut;
                            }
                            else
                            {
                                connectingMembers[i].ElementRAZ.endCut = ElementRAZ.SawingCut.DoubleMiterCut;
                            }
                        }
                        else
                        {
                            //SingleMiterCut
                            if (connectingMembers[i].isStartPoint == true)
                            {
                                connectingMembers[i].ElementRAZ.startCut = ElementRAZ.SawingCut.SingleMiterCut;
                            }
                            else
                            {
                                connectingMembers[i].ElementRAZ.endCut = ElementRAZ.SawingCut.SingleMiterCut;
                            }
                        }


                    }
                    if (i == 2)
                    {
                        double a = connectingMembers[0].ElementRAZ.crossSection.height / 2;
                        double b = bear.ElementRAZ.crossSection.height / 2;
                        double h = connectingMembers[2].ElementRAZ.crossSection.height / 2;
                        double thetaDirty = VectorRAZ.AngleBetweenVectors(j.bearingMemberUnitVector, connectingMembers[2].ElementRAZ.line.vector);
                        //theta should be an angle between 0 and 90 degree
                        double theta = (Math.Min(thetaDirty, Math.PI - thetaDirty));

                        double hor = ConnectingMember.WebWeldsHorizontalLength(a, b, h, theta, phi, eccentricity);
                        double ver = ConnectingMember.WebWeldsVerticalLength(a, b, h, theta, phi, eccentricity);

                        if (hor != 0.0 && ver != 0.0)
                        {
                            //DoubleMiterCut
                            if (connectingMembers[i].isStartPoint == true)
                            {
                                connectingMembers[i].ElementRAZ.startCut = ElementRAZ.SawingCut.DoubleMiterCut;
                            }
                            else
                            {
                                connectingMembers[i].ElementRAZ.endCut = ElementRAZ.SawingCut.DoubleMiterCut;
                            }
                        }
                        else
                        {
                            //SingleMiterCut
                            if (connectingMembers[i].isStartPoint == true)
                            {
                                connectingMembers[i].ElementRAZ.startCut = ElementRAZ.SawingCut.SingleMiterCut;
                            }
                            else
                            {
                                connectingMembers[i].ElementRAZ.endCut = ElementRAZ.SawingCut.SingleMiterCut;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Caluclate welds of all joints in project according to the specified method
        /// </summary>
        public void CalculateWeldsProject(string userpath)
        {
            //calculate welds by using IDEA statica
            if (this.analysisMethod == AnalysisMethod.IdeaMethod)
            {
                if (startIDEA == true)
                {
                    //Calculate all joints
                    CreateFolder(userpath);
                    if (calculateAllJoints == true)
                    {
                        foreach (Joint j in this.joints)
                        {
                            KarambaIDEA.IDEA.IdeaConnection idea = new IDEA.IdeaConnection(j);
                        }
                    }
                    //Calculate one joint
                    else
                    {
                        
                        Joint j = this.joints[calculateThisJoint];
                        KarambaIDEA.IDEA.IdeaConnection idea = new IDEA.IdeaConnection(j);
                        //MainWindow mainWindow = new MainWindow();
                        //mainWindow.Test(j);
                    }
                }
            }
            //calculate welds by using other methods
            else
            {
                foreach (Joint j in this.joints)
                {
                    j.CalculateWelds();
                }
            }
        }

        public double CalculateWeldVolume()
        {
            double weldvolume = 0;
            foreach (Joint j in this.joints)
            {
                double weldVolumeJoint = j.CalculateWeldVolume();
                j.weldVolume = weldVolumeJoint;//setWeldVolume
                weldvolume = weldvolume + weldVolumeJoint;
            }
            return weldvolume;
        }

        /// <summary>
        /// Set a specific minimum throat thickness to all welds
        /// </summary>
        /// <param name="minThroatThickness">minimum throat thickness of welds</param>
        public void SetMinThroats(double minThroatThickness)
        {
            this.minthroat = minThroatThickness;
            foreach (Joint j in this.joints)
            {
                foreach (ConnectingMember CM in j.attachedMembers.OfType<ConnectingMember>())
                {
                    if (CM is ConnectingMember)
                    {
                        CM.flangeWeld.size = minThroatThickness;
                        CM.webWeld.size = minThroatThickness;
                    }
                }
            }
        }

        /// <summary>
        /// Fillet welds are assigned to Hollow sections, double fillet welds are assigned to Isections
        /// </summary>
        public void SetWeldType()
        {
            foreach (Joint j in this.joints)
            {
                foreach (ConnectingMember CM in j.attachedMembers.OfType<ConnectingMember>())
                {
                    if (CM.ElementRAZ.crossSection.shape == CrossSection.Shape.HollowSection)
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

        /// <summary>
        /// Different analysis methods to calculate welding volume
        /// </summary>
        public enum AnalysisMethod
        {
            MinSetWelds,
            FullStrengthLazy,
            FullStrengthMethod,
            DirectionalMethod,
            IdeaMethod
        }

    }






}
