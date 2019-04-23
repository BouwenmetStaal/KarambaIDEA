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


        public static Project ReadProjectFromXML(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open))
            {

                var XML = new XmlSerializer(typeof(Project));
                return (Project)XML.Deserialize(stream);



            }

        }

        public void CreateFolder()
        {
            //create folder
            String timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            this.filepath = Path.Combine(@"C:\Data", timeStamp);
            if (!Directory.Exists(this.filepath))
            {
                Directory.CreateDirectory(this.filepath);
            }
        }

        public void CreateJoints(double tol, double eccentricity, List<PointRAZ> punten, List<ElementRAZ> elementRAZs, List<Hierarchy> hierarchy)
        {
            double tolbox = tol + eccentricity;
            List<Joint> joints = new List<Joint>();
            //iterate over all the points that represent centerpoints of the joint
            for (int i = 0; i < punten.Count; i++)
            {
                PointRAZ centerpoint = punten[i];
                List<VectorRAZ> eccentricityVectors = new List<VectorRAZ>();
                List<int> elementIDs = new List<int>();
                List<LineRAZ> linesatcenter = new List<LineRAZ>();

                List<AttachedMember> attachedMemTemp = new List<AttachedMember>();
                List<AttachedMember> attachedMembers = new List<AttachedMember>();

                //iterate over all lines in project
                foreach (ElementRAZ element in elementRAZs)
                {
                    //DETECT ECCENTRIC WARREN TRUSS JOINTS
                    //if the number of lines attached to the centerpoint is equal to two it is a eccentric warren truss joint
                    if (PointRAZ.ArePointsEqual(tol, centerpoint, element.line.End) == true || PointRAZ.ArePointsEqual(tol, centerpoint, element.line.Start) == true)
                    {
                        linesatcenter.Add(element.line);
                    }
                    //ENDPoints
                    //If toPoints or endPoints of line fall in the tolerancebox than add lines.
                    if (PointRAZ.ArePointsEqual(tolbox, centerpoint, element.line.End) && element.line.vector.length > tolbox)
                    {
                        //flag_exists prevents that duplicates are added to the list
                        bool flag_exists = false;
                        foreach (int id in elementIDs)
                        {
                            if (elementRAZs.IndexOf(element) == id)
                            {
                                flag_exists = true;
                            }
                        }
                        if (!flag_exists)
                        {
                            VectorRAZ distancevector = new VectorRAZ(centerpoint.X - element.line.End.X, centerpoint.Y - element.line.End.Y, centerpoint.Z - element.line.End.Z);
                            LineRAZ transposedline = LineRAZ.TranslateLineWithVector(this, element.line, distancevector);//translate line with vector
                            LineRAZ idealine = LineRAZ.FlipLine(transposedline);//in this case of endpoint line needs to be flipped
                            int signlocalEccentricity = LineRAZ.ShouldEccentricityBeAssumedPOSOrNEG(tol, centerpoint, idealine);
                            double localEccnetricty = signlocalEccentricity * ConnectingMember.LocalEccentricity(centerpoint, element.line.End, element.line.vector);

                            ConnectingMember connectingMember = new ConnectingMember(element, distancevector, false, idealine, localEccnetricty);

                            elementIDs.Add(elementRAZs.IndexOf(element));
                            attachedMemTemp.Add(connectingMember);
                        }
                    }
                    //STARTPoints
                    //If fromPoints or startPoints of line fall in the tolerancebox than add lines.
                    if (PointRAZ.ArePointsEqual(tolbox, centerpoint, element.line.Start) && element.line.vector.length > tolbox)
                    {
                        //flag_exists prevents that duplicates are added to the list
                        bool flag_exists = false;
                        foreach (int id in elementIDs)
                        {
                            if (elementRAZs.IndexOf(element) == id)
                            {
                                flag_exists = true;
                            }
                        }
                        if (!flag_exists)
                        {
                            VectorRAZ distancevector = new VectorRAZ(centerpoint.X - element.line.Start.X, centerpoint.Y - element.line.Start.Y, centerpoint.Z - element.line.Start.Z);
                            LineRAZ transposedline = LineRAZ.TranslateLineWithVector(this, element.line, distancevector);//No flip needed
                            int signlocalEccentricity = LineRAZ.ShouldEccentricityBeAssumedPOSOrNEG(tol, centerpoint, transposedline);
                            double localEccnetricty = signlocalEccentricity * ConnectingMember.LocalEccentricity(centerpoint, element.line.Start, element.line.vector);


                            ConnectingMember connectingMember = new ConnectingMember(element, distancevector, true, transposedline, localEccnetricty);

                            elementIDs.Add(elementRAZs.IndexOf(element));
                            attachedMemTemp.Add(connectingMember);
                        }
                    }
                }

                //Check if joint is WarrenEccentrictyJoint
                bool WEJ = false;
                if (linesatcenter.Count == 2 && VectorRAZ.AreVectorsEqual(tol, linesatcenter[0].vector, linesatcenter[1].vector) == true)
                {
                    WEJ = true;
                }

                //bearingMemberVector
                //iterate over hierarchy rank
                VectorRAZ bearingMemberUnitVector = new VectorRAZ();
                for (int rank = 0; rank < 1 + this.hierarchylist.Max(a => a.numberInHierarchy); rank++)
                {
                    //iterate over attachedMembers of every joint
                    for (int ibb = 0; ibb < attachedMemTemp.Count; ibb++)
                    {
                        //find highest member in hierarchy, create bearingMemberUnitVector
                        if (attachedMemTemp[ibb].ElementRAZ.numberInHierarchy == rank)
                        {

                            bearingMemberUnitVector = attachedMemTemp[ibb].ideaLine.vector.Unitize();
                            goto End;
                        }

                    }

                }
                End:

                //Redistribute attachedMemTemp over BearingMember and ConnectingMember
                //iterate over hierarchy rank to make sure list is created in a orded way
                for (int rank = 0; rank < 1 + hierarchy.Max(a => a.numberInHierarchy); rank++)
                {

                    //iterate over attachedMembers of every joint
                    List<AttachedMember> templist = new List<AttachedMember>();

                    for (int ibb = 0; ibb < attachedMemTemp.Count; ibb++)
                    {
                        AttachedMember w = attachedMemTemp[ibb];
                        if (w.ElementRAZ.numberInHierarchy == rank && rank == attachedMemTemp.Min(a => a.ElementRAZ.numberInHierarchy))
                        {
                            BearingMember bearing = new BearingMember(w.ElementRAZ, w.distanceVector, w.isStartPoint, w.ideaLine);
                            attachedMembers.Add(bearing);
                        }
                        if (w.ElementRAZ.numberInHierarchy == rank && rank != attachedMemTemp.Min(a => a.ElementRAZ.numberInHierarchy))
                        {
                            //temp
                            templist.Add(attachedMemTemp[ibb]);
                        }
                    }
                    //Hierarchy in same rank will be determined by the angle, member with biggest angle receives priority.
                    if (templist.Count > 1)
                    {
                        foreach (AttachedMember member in templist)
                        {
                            attachedMembers.Add(member);
                        }

                       
                    }
                    if (templist.Count == 1)
                    {
                        attachedMembers.Add(templist[0]);
                    }

                }
                //If there is more than one Bearing Member, IsContinues joint
                List<BearingMember> BM = attachedMembers.OfType<BearingMember>().ToList();
                bool IsContinues = true;
                if (BM.Count == 1)
                {
                    IsContinues = false;
                }

                //All attachedMembers Found
                double maxGlobalEccentricity = attachedMembers.Max(a => a.distanceVector.length);

                //REMOVE JOINT WHICH ARE NO JOINTS BUT CONTINUES BEAM, HAPPENS IN WARREN TRUSSES
                if (attachedMemTemp.Count == 2 && VectorRAZ.AreVectorsEqual(tol, attachedMemTemp[0].ElementRAZ.line.vector, attachedMemTemp[1].ElementRAZ.line.vector) == true)
                {
                    //This is not a joint
                }
                else
                {
                    //CREATE JOINT ADD TO PROJECT
                    //Joint id starts from one, because IDEA counts from one
                    Joint w = new Joint(this, i + 1, elementIDs, attachedMembers, centerpoint, maxGlobalEccentricity, WEJ, bearingMemberUnitVector, IsContinues);
                    this.joints.Add(w);

                    //Add ideaOperationID;
                    //note: since IDEA has bug in importing sequence of members, this workaround is needed
                    List<BearingMember> bearlist = w.attachedMembers.OfType<BearingMember>().ToList();
                    foreach (BearingMember BearM in bearlist)
                    {
                        BearM.ideaOperationID = 1;
                    }
                    List<ConnectingMember> conlist = w.attachedMembers.OfType<ConnectingMember>().ToList();
                    VectorRAZ bear = w.bearingMemberUnitVector;
                    if (bear.X < 0.0 || bear.Z < 0.0)
                    {
                        bear = VectorRAZ.FlipVector(bear);
                    }
                    foreach (ConnectingMember con in conlist)
                    {
                        VectorRAZ convec = con.ideaLine.vector.Unitize();
                        con.angleWithBear = VectorRAZ.AngleBetweenVectors(bear, convec);
                    }
                    List<ConnectingMember> newlist = conlist.OrderByDescending(a => a.angleWithBear).ToList();
                    for (int ic = 0; ic < newlist.Count(); ic++)
                    {

                        newlist[ic].ideaOperationID = 2 + ic;
                    }

                }



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

        public void CalculateWeldsProject()
        {
            if (this.analysisMethod == AnalysisMethod.IdeaMethod)
            {
                if (startIDEA == true)
                {
                    CreateFolder();
                    if (calculateAllJoints == true)
                    {
                        foreach (Joint j in this.joints)
                        {
                            MainWindow mainWindow = new MainWindow();
                            mainWindow.Test(j);
                        }
                    }
                    else
                    {
                        Joint j = this.joints[calculateThisJoint];
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Test(j);
                    }


                    
                    

                    //MainWindow mainWindow = new MainWindow();
                    //mainWindow.Test(this.joints[7]);
                }
            }
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
