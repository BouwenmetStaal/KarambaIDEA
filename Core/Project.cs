﻿// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
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
        public double minthroat;
        public string projectFolderPath;

        public List<Point> points = new List<Point>();
        public List<Element> elements = new List<Element>();
        public List<LoadCase> loadcases = new List<LoadCase>();
        public List<Joint> joints = new List<Joint>();
        public List<Hierarchy> hierarchylist = new List<Hierarchy>();
        public List<CrossSection> crossSections = new List<CrossSection>();
        public List<MaterialSteel> materials = new List<MaterialSteel>();


        public AnalysisMethod analysisMethod = AnalysisMethod.FullStrengthMethod;

        public static double tolerance = 1e-6;
        public static double gammaM2 = 1.25;
        public static double EmodulusSteel = 210000;//N/mm2
        public static double massSteel = 7850;//kg/m3

        public Project()
        {

        }

        public Project(string _projectName)
        {
            this.projectName = _projectName;
            
        }
       
        public Project(string projectname, List<Hierarchy> hierarchylist, List<Element> _elements, AnalysisMethod _analysisMethod = AnalysisMethod.FullStrengthMethod)
        {
            this.elements = _elements;
            this.analysisMethod = _analysisMethod;
        }
        /// <summary>
        /// All data is inventised and converted to seperate data needed to calculate individual joints 
        /// </summary>
        /// <param name="tol">tolerance</param>
        /// <param name="eccentricity">eccentricity if specified</param>
        /// <param name="points">points in project</param>
        /// <param name="elements">elements of project</param>
        /// <param name="hierarchy">hierarchy if specified</param>
        public void CreateJoints(double tol, double eccentricity, List<Point> points, List<Element> elements, List<Hierarchy> hierarchy)
        {
            double tolbox = tol + eccentricity;
            List<Joint> joints = new List<Joint>();

            //iterate over all the points that represent centerpoints of the joint
            for (int i = 0; i < points.Count; i++)
            {
                Point centerpoint = points[i];
                List<Vector> eccentricityVectors = new List<Vector>();
                List<int> elementIDs = new List<int>();
                List<Line> linesatcenter = new List<Line>();

                List<AttachedMember> attachedMemTemp = new List<AttachedMember>();
                List<AttachedMember> attachedMembers = new List<AttachedMember>();

                //1. DEFINE JOINTS
                //iterate over all lines in project
                foreach (Element element in elements)
                {

                    //STARTPoints
                    //If fromPoints or startPoints of line fall in the tolerancebox than add lines.
                    if (Point.ArePointsEqual(tolbox, centerpoint, element.line.start) && element.line.Vector.Length > tolbox)
                    {
                        Line line = element.line;
                        Vector distancevector = new Vector(0.0, 0.0, 0.0);
                        double localEccnetricty = 0.0;



                        ConnectingMember connectingMember = new ConnectingMember(element, distancevector, true, line, localEccnetricty);

                        elementIDs.Add(elements.IndexOf(element));
                        attachedMemTemp.Add(connectingMember);
                    }
                    //ENDPoints
                    //If toPoints or endPoints of line fall in the tolerancebox than add lines.
                    if (Point.ArePointsEqual(tolbox, centerpoint, element.line.end) && element.line.Vector.Length > tolbox)
                    {

                        Vector distancevector = new Vector(0.0, 0.0, 0.0);
                        double localEccnetricty = 0.0;
                        //IDEAline
                        Line idealine = Line.FlipLine(element.line);//in this case of endpoint line needs to be flipped


                        ConnectingMember connectingMember = new ConnectingMember(element, distancevector, false, idealine, localEccnetricty);

                        elementIDs.Add(elements.IndexOf(element));
                        attachedMemTemp.Add(connectingMember);
                    }
                }


                //2. ORDER ATTACHEDMEMBERS ACCORDING TO HIERARCHY

                bool IsContinues = true;
                //Redistribute attachedMemTemp over BearingMember and ConnectingMember
                //iterate over hierarchy rank to make sure list is created in a orded way
                if (!hierarchy.Any())//if there is no hierarchy
                {
                    //no hierarchy, first member found is an ended bearing member
                    IsContinues = false;
                    //First member is bearing
                    AttachedMember w = attachedMemTemp.First();
                    BearingMember bearing = new BearingMember(w.element, w.distanceVector, w.isStartPoint, w.ideaLine);
                    attachedMembers.Add(bearing);
                    //Rest of members are connecting members
                    for (int b = 1; b < attachedMemTemp.Count; b++)
                    {
                        attachedMembers.Add(attachedMemTemp[b]);
                    }
                }
                else//if there is a hierarchy
                {
                    //hierarchy determined, list will be build based on hierarchy
                    //If only one hierarchy entry defined
                    /*
                    if (hierarchy.Count == 1)
                    {
                        IsContinues = false;
                        //First member is bearing
                        AttachedMember w = attachedMemTemp.First();
                        BearingMember bearing = new BearingMember(w.element, w.distanceVector, w.isStartPoint, w.ideaLine);
                        attachedMembers.Add(bearing);
                        //Rest of members are connecting members
                        for (int b = 1; b < attachedMemTemp.Count; b++)
                        {
                            attachedMembers.Add(attachedMemTemp[b]);
                        }
                    }
                    */
                    if (attachedMemTemp.Count == 2)//if joint has two attached members. First is bearing, Second is attached.
                    {
                        //TODO: christalyze method
                        IsContinues = false;
                        //First member is bearing
                        AttachedMember w = attachedMemTemp.FirstOrDefault();
                        BearingMember bearing = new BearingMember(w.element, w.distanceVector, w.isStartPoint, w.ideaLine);
                        attachedMembers.Add(bearing);
                        //Rest of members are connecting members
                        for (int b = 1; b < attachedMemTemp.Count; b++)
                        {
                            attachedMembers.Add(attachedMemTemp[b]);
                        }
                    }
                    else//there is a hierarchy, thus set bearingmembers according to hierarchy
                    {
                        //TODO:include warning if not all available hierarchies are defined in hierarchylist
                        for (int rank = 0; rank < 1 + hierarchy.Max(a => a.numberInHierarchy); rank++)
                        {
                            //iterate over attachedMembers of every joint
                            //List<AttachedMember> templist = new List<AttachedMember>();

                            for (int ibb = 0; ibb < attachedMemTemp.Count; ibb++)
                            {
                                AttachedMember w = attachedMemTemp[ibb];
                                //if hierarchy if the highest occuring
                                if (w.element.numberInHierarchy == rank && rank == attachedMemTemp.Min(a => a.element.numberInHierarchy))
                                {
                                    BearingMember bearing = new BearingMember(w.element, w.distanceVector, w.isStartPoint, w.ideaLine);
                                    attachedMembers.Add(bearing);
                                }
                                else if (w.element.numberInHierarchy == rank && rank != attachedMemTemp.Min(a => a.element.numberInHierarchy))
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
                Vector bearingMemberUnitVector = new Vector(1.0, 0.0, 0.0);
                Joint joint = new Joint(this, i + 1, elementIDs, attachedMembers, centerpoint, maxGlobalEccentricity, bearingMemberUnitVector, IsContinues);
                this.joints.Add(joint);
            }

        }


        /// <summary>
        /// Brandnames of joints are defined to recognize similiar joints. 
        /// The Brandname is defined based on the number of attached elements and their hierarchy. 
        /// The integers of all occuring hierarchys are chronologically ordered, creating a string that defines the brandname
        /// </summary>
        /// <param name="project"></param>
        public void SetBrandNames(Project project)
        {
            foreach (Joint joint in project.joints)
            {
                List<int> intlist = new List<int>();
                foreach (AttachedMember atta in joint.attachedMembers)
                {
                    int number = atta.element.numberInHierarchy;
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

        

        /// <summary>
        /// Create Folder to save IDEA files on location specified, or on default location
        /// </summary>
        /// <param name="userpath">folder specified by user</param>
        public void CreateFolder(string userpath)
        {
            //create folder
            String timeStamp = DateTime.Now.ToString("yyyy MM dd HHmmss");
            string pathlocation = "";
            if (string.IsNullOrEmpty(userpath) || string.IsNullOrWhiteSpace(userpath))
            {
                pathlocation = @"C:\Data";
            }
            else
            {
                pathlocation = userpath;
            }            
                        
            this.projectFolderPath = Path.Combine(pathlocation, timeStamp);
            if (!Directory.Exists(this.projectFolderPath))
            {
                if (Uri.IsWellFormedUriString(projectFolderPath, UriKind.Absolute))
                {
                    Directory.CreateDirectory(this.projectFolderPath);
                }
                else
                {
                    //System.Windows.MessageBox.Show("Provided path is invalid, provided path:" + userpath, "Path invalid");
                    //TODO include workerthread or progress bar
                }
            }
        }

        /// <summary>
        /// Make a list with messages
        /// These messages will tell the which templates are applied to the joint brand names available in the project.
        /// </summary>
        /// <returns></returns>
        public List<string> MakeTemplateJointMessage()
        {
            List<string> messages = new List<string>();
            List<string> uniqueBrandNames = this.joints.Select(a => a.brandName).Distinct().ToList();
            foreach (string brandName in uniqueBrandNames)
            {
                foreach (Joint joint in this.joints.Where(a => a.brandName == brandName))
                {
                    if (brandName == joint.brandName)
                    {
                        messages.Add("BrandName '" + brandName + "' is linked to " + joint.template.workshopOperations.ToString());
                        goto here;
                    }
                }
            here:;
            }
            return messages;
        }

        

        /// <summary>
        /// Set a specific minimum throat thickness to all welds in project
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
                        CM.flangeWeld.Size = minThroatThickness;
                        CM.webWeld.Size = minThroatThickness;
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

        public void SaveToXML(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Create))
            {

                var XML = new XmlSerializer(typeof(Project));
                XML.Serialize(stream, this);
                stream.Close();

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
    }






}
