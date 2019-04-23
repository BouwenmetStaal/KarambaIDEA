////using CI.GiCL2D;
//using IdeaRS.MPRL;
//using IdeaRS.MprlManager;
//using IdeaRS.OpenModel;
//using IdeaRS.OpenModel.Connection;
//using IdeaRS.OpenModel.CrossSection;
//using IdeaRS.OpenModel.Geometry3D;
//using IdeaRS.OpenModel.Loading;
//using IdeaRS.OpenModel.Material;
//using IdeaRS.OpenModel.Message;
//using IdeaRS.OpenModel.Model;
//using IdeaRS.OpenModel.Result;
//using ETABSv17;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Diagnostics;
//using System.Linq;
//using CG = CI.Geometry3D;
//using WM = System.Windows.Media.Media3D;
//using CI;
////using CI.Geometry3D;


//namespace ETABSv17ToIDEAOpenModelBase
//{
//    public partial class ETABSv17ToIOM
//    {
//        public class MemberHelp
//        {
//            public int Number;
//            public int IDEA_Id;
//            public string Name;
//            public double Length;
//        }

//        private IMprlManager mprl;
//        public BackgroundWorker Worker { get; set; }

//        public Dictionary<string, Point3D> myPoint3D = new Dictionary<string, Point3D>();
//        public Dictionary<string, LoadCase> myLoadCases = new Dictionary<string, LoadCase>();
//        private IIOMSettings structuralSettings;
//        public IDictionary<string, IList<Guid>> NonConformity { get; set; }

//        public IDictionary<int, MemberHelp> myDictMemberHelp = new Dictionary<int, MemberHelp>();

//        private IDictionary<string, Material> dictMaterial = new Dictionary<string, Material>();
//        private IDictionary<string, CrossSectionParameter> dictCrossSectionParameter = new Dictionary<string, CrossSectionParameter>();

//        private IDictionary<long, string> dictNotRecognizedCSS = new Dictionary<long, string>();
//        global::log4net.ILog logger;
//        public ETABSv17ToIOM(global::log4net.ILog logger)
//        {
//            this.logger = logger;
//        }

//        public void CreateIDEAOpenModel(IIOMSettings structuralSettings, ref ETABSv17.cSapModel SapModel, out OpenModel openStructModel, out OpenMessages openMessages)
//        {
//            SapModel.GetPresentUnits_2(ref forceUnits, ref lengthUnits, ref temperatureUnits);
//            //ut = SapModel.GetPresentUnits();
//            this.structuralSettings = structuralSettings;
//            openStructModel = new OpenModel();
//            openMessages = new OpenMessages();
//            openStructModel.OriginSettings = new OriginSettings() { CrossSectionConversionTable = IdeaRS.OpenModel.CrossSectionConversionTable.SAP2000 };
//            myPoint3D.Clear();
//            dictNotRecognizedCSS.Clear();
//            myDictMemberHelp.Clear();
//            dictCrossSectionParameter.Clear();
//            if (mprl == null)
//            {
//                mprl = new MprlManager();
//            }
//        }


//        /// <summary>
//        /// Harvesting the active SAP and creates dictionardy holds FramesGUID and Labels
//        /// </summary>
//        /// <param name="Model">Active SAP Model</param>
//        /// <param name="myFrameList"> List of Labels of SAP Frames </param>
//        public void GetSAPFrameList(ref cSapModel SapModel, OpenModel openStructModel, OpenMessages openMessages) // <GUID, Label> 
//        {
//            string[] IDs = null;
//            int NumbOfFrames = 0;
//            long ret = SapModel.FrameObj.GetNameList(ref NumbOfFrames, ref IDs);
//            if (IDs != null)
//            {
//                List<string> myFrameList = IDs.ToList();
//                double step = 40.0 / (double)myFrameList.Count;
//                for (int i = 0; i < myFrameList.Count; i++)
//                {
//                    string matProp = "A992Fy50"; // default value
//                    string secName = "W12X14"; // default value
//                    string secCatalog = "AISC14"; // default value
//                    string Just = "MiddleCenter"; // default value
//                    double Rot = 0; // default value

//                    int intstep = (int)step;
//                    Worker.ReportProgress(10 + intstep, "Beam " + myFrameList[i]);
//                    GetFrm(ref SapModel, openStructModel, openMessages, myFrameList[i], i, ref matProp, ref secName, ref Just, ref Rot, ref secCatalog, null, false);

//                }
//            }
//        }

//        enum eUC
//        {
//            force,
//            length,
//            temp
//        }

//        eForce forceUnits = eForce.N;
//        eLength lengthUnits = eLength.m;
//        eTemperature temperatureUnits = eTemperature.C;

//        private double UC(eUC type)
//        {
//            switch (type)
//            {
//                case eUC.force:
//                    switch (forceUnits)
//                    {
//                        case eForce.N:
//                            return 1.0;
//                        case eForce.NotApplicable:
//                            return 1.0;
//                        case eForce.kN:
//                            return 1000.0;
//                        case eForce.kgf:
//                            return 1.0 / 0.102;
//                        case eForce.kip:
//                            return 4448;
//                        case eForce.lb:
//                            return 4.448;
//                        case eForce.tonf:
//                            return 1000.0 * 9.806;
//                        default:
//                            break;
//                    }
//                    break;
//                case eUC.length:
//                    switch (lengthUnits)
//                    {
//                        case eLength.NotApplicable:
//                            return 1.0;
//                        case eLength.cm:
//                            return 1 / 100.0;
//                        case eLength.ft:
//                            return 1.0 / 3.281;
//                        case eLength.inch:
//                            return 1.0 / 39.37;
//                        case eLength.m:
//                            return 1.0;
//                        case eLength.micron:
//                            return 1.0 / 1000000.0;
//                        case eLength.mm:
//                            return 1.0 / 1000.0;
//                        default:
//                            break;
//                    }
//                    break;
//                case eUC.temp:
//                    switch (temperatureUnits)
//                    {
//                        case eTemperature.C:
//                            break;
//                        case eTemperature.F:
//                            break;
//                        case eTemperature.NotApplicable:
//                            break;
//                        default:
//                            break;
//                    }
//                    break;
//                default:
//                    break;
//            }
//            //switch (ut)
//            //{
//            //	case eUnits.N_cm_C:
//            //		switch (type)
//            //		{
//            //			case eUC.length:
//            //				return 1 / 100.0;
//            //		}
//            //		break;
//            //	case eUnits.N_m_C:
//            //		break;
//            //	case eUnits.N_mm_C:
//            //		switch (type)
//            //		{
//            //			case eUC.length:
//            //				return 1 / 1000.0;
//            //		}
//            //		break;
//            //	case eUnits.Ton_cm_C:
//            //		switch (type)
//            //		{
//            //			case eUC.force:
//            //				return 1000.0 *9.81;
//            //			case eUC.length:
//            //				return 1 / 100.0;
//            //		}
//            //		break;
//            //	case eUnits.Ton_m_C:
//            //		switch (type)
//            //		{
//            //			case eUC.force:
//            //				return 1000.0;
//            //		}
//            //		break;
//            //	case eUnits.Ton_mm_C:
//            //		switch (type)
//            //		{
//            //			case eUC.force:
//            //				return 1000.0;
//            //			case eUC.length:
//            //				return 1 / 1000.0;
//            //		}
//            //		break;
//            //	case eUnits.kN_cm_C:
//            //		switch (type)
//            //		{
//            //			case eUC.force:
//            //				return 1000.0;
//            //			case eUC.length:
//            //				return 1 / 100.0;
//            //		}
//            //		break;
//            //	case eUnits.kN_m_C:
//            //		switch (type)
//            //		{
//            //			case eUC.force:
//            //				return 1000.0;
//            //		}
//            //		break;
//            //	case eUnits.kN_mm_C:
//            //		switch (type)
//            //		{
//            //			case eUC.force:
//            //				return 1000.0;
//            //			case eUC.length:
//            //				return 1 / 1000.0;
//            //		}
//            //		break;
//            //	case eUnits.kgf_cm_C:
//            //		switch (type)
//            //		{
//            //			case eUC.force:
//            //				return 1.0 / 0.102;
//            //			case eUC.length:
//            //				return 1 / 100.0;
//            //		}
//            //		break;
//            //	case eUnits.kgf_m_C:
//            //		switch (type)
//            //		{
//            //			case eUC.force:
//            //				return 1.0 / 0.102;
//            //		}
//            //		break;
//            //	case eUnits.kgf_mm_C:
//            //		switch (type)
//            //		{
//            //			case eUC.force:
//            //				return 1.0 / 0.102;
//            //			case eUC.length:
//            //				return 1 / 1000.0;
//            //		}
//            //		break;
//            //	case eUnits.kip_ft_F:
//            //		switch (type)
//            //		{
//            //			case eUC.force:
//            //				return 1.0 / 2.248e10 - 4;
//            //			case eUC.length:
//            //				return 1.0 / 3.281;
//            //		}
//            //		break;
//            //	case eUnits.kip_in_F:
//            //		switch (type)
//            //		{
//            //			case eUC.force:
//            //				return 1.0 / 2.248e10 - 4;
//            //			case eUC.length:
//            //				return 1.0 / 39.37;
//            //		}
//            //		break;
//            //	case eUnits.lb_ft_F:
//            //		switch (type)
//            //		{
//            //			case eUC.force:
//            //				return 2.025;
//            //			case eUC.length:
//            //				return 1.0 / 3.281;
//            //		}
//            //		break;
//            //	case eUnits.lb_in_F:
//            //		switch (type)
//            //		{
//            //			case eUC.force:
//            //				return 2.025;
//            //			case eUC.length:
//            //				return 1.0 / 39.37;
//            //		}
//            //		break;
//            //	default:
//            //		break;
//            //}
//            return 1;
//        }
//        // private eUnits ut;

//        public void AddBeamsToOpenModel(ref ETABSv17.cSapModel SapModel, ref ETABSv17.cPluginCallback ISapPlugin, OpenModel openStructModel, OpenMessages openMessages, List<string> selBeams)
//        {

//            for (int i = 0; i < selBeams.Count; i++)
//            {
//                string matProp = "A992Fy50"; // default value
//                string secName = "W12X14"; // default value
//                string secCatalog = "AISC14"; // default value
//                string Just = "MiddleCenter"; // default value
//                double Rot = 0; // default value

//                GetFrm(ref SapModel, openStructModel, openMessages, selBeams[i], i, ref matProp, ref secName, ref Just, ref Rot, ref secCatalog, null, true);


//                //SectionProp secProp = new SectionProp(secName, matProp, secCatalog);
//                //Frame d_frm = new Frame(s, e, secProp, Just, Rot);
//                //d_frm.Label = FrmIds[i];
//            }
//        }


//        public ConnectionPoint AddConnectionToOpenModel(ref ETABSv17.cSapModel SapModel, ref ETABSv17.cPluginCallback ISapPlugin, OpenModel openStructModel, OpenMessages openMessages, List<string> selNodes, List<string> selBeams)
//        {

//            //int NumberItems = 0;
//            //int[] ObjectTypes = null;
//            //string[] ObjectNames = null;
//            //SapModel.SelectObj.GetSelected(ref NumberItems, ref ObjectTypes, ref ObjectNames);
//            ConnectionPoint connection = new ConnectionPoint();

//            Double myStartX = 0;
//            Double myStartY = 0;
//            Double myStartZ = 0;
//            //ut = SapModel.GetPresentUnits();
//            SapModel.GetPresentUnits_2(ref forceUnits, ref lengthUnits, ref temperatureUnits);

//            SapModel.SetPresentUnits_2(forceUnits, lengthUnits, temperatureUnits);

//            long ret = SapModel.PointObj.GetCoordCartesian(selNodes[0], ref myStartX, ref myStartY, ref myStartZ);
//            var point = new Point3D() { X = myStartX * UC(eUC.length), Y = myStartY * UC(eUC.length), Z = myStartZ * UC(eUC.length) };
//            point.Name = selNodes[0];
//            point.Id = openStructModel.Point3D.Count + 1;
//            openStructModel.Point3D.Add(point);
//            myPoint3D.Add(selNodes[0], point);

//            connection.Node = new ReferenceElement(point);
//            connection.Id = point.Id;
//            connection.Name = "Con " + point.Name;

//            //GetSAPFrameList(ref SapModel);

//            for (int i = 0; i < selBeams.Count; i++)
//            {
//                string matProp = "A992Fy50"; // default value
//                string secName = "W12X14"; // default value
//                string secCatalog = "AISC14"; // default value
//                string Just = "MiddleCenter"; // default value
//                double Rot = 0; // default value

//                GetFrm(ref SapModel, openStructModel, openMessages, selBeams[i], i, ref matProp, ref secName, ref Just, ref Rot, ref secCatalog, connection, true);


//                //SectionProp secProp = new SectionProp(secName, matProp, secCatalog);
//                //Frame d_frm = new Frame(s, e, secProp, Just, Rot);
//                //d_frm.Label = FrmIds[i];
//            }
//            openStructModel.AddObject(connection);
//            return connection;
//        }



//        public void GetFrm(ref cSapModel SapModel, OpenModel openStructModel, OpenMessages openMessages, string frmId, int inx, ref string MatProp, ref string SecName, ref string Just, ref double Rot, ref string SecCatalog, ConnectionPoint connection, bool addCssName) //Length Scale Factor
//        {

//            long ret = 0;
//            // Get Geometry
//            // SAP Frame start and end point
//            string StartPoint = string.Empty;
//            string EndPoint = string.Empty;

//            // getting start and end point
//            ret = SapModel.FrameObj.GetPoints(frmId, ref StartPoint, ref EndPoint);

//            Point3D ptB = AddNodeToOpenModel(StartPoint, ref SapModel, openStructModel);
//            Point3D ptE = AddNodeToOpenModel(EndPoint, ref SapModel, openStructModel);
//            //getting coordinates of starting point
//            //getting coordinates of ending point

//            // Section
//            string SAuto = string.Empty;
//            ret = SapModel.FrameObj.GetSection(frmId, ref SecName, ref SAuto);
//            CrossSectionParameter cssO;

//            // MatProp
//            ETABSv17ToIOM_CSS(ref SapModel, ref openStructModel, openMessages, SecName, out cssO);
//            //{
//            //	double Area = 0;
//            //	double As2 = 0;
//            //	double As3 = 0;
//            //	double Torsion = 0;
//            //	double I22 = 0;
//            //	double I33 = 0;
//            //	double I23 = 0;
//            //	double S22 = 0;
//            //	double S33 = 0;
//            //	double Z22 = 0;
//            //	double Z33 = 0;
//            //	double R22 = 0;
//            //	double R33 = 0;
//            //	double EccV2 = 0;
//            //	double EccV3 = 0;
//            //  SapModel.PropFrame.GetSectProps_1(SecName, ref Area, ref As2, ref As3, ref Torsion, ref I22, ref I33, ref I23, ref S22, ref S33, ref Z22, ref Z33, ref R22, ref R33, ref EccV2, ref EccV3);
//            //}

//            PolyLine3D polyLine3D = new PolyLine3D();
//            polyLine3D.Id = inx + 1;
//            LineSegment3D ls = new LineSegment3D();
//            ls.Id = inx + 1;


//            double Ang = 0;
//            bool Advanced = false;
//            ret = SapModel.FrameObj.GetLocalAxes(frmId, ref Ang, ref Advanced);

//            //if (Advanced)
//            //{
//            //	bool Active = true;
//            //	int Plane2 = 0;
//            //	int PlVectOpt = 0;
//            //	string PlCSys = string.Empty;
//            //	int[] PlDir = null;
//            //	string[] PlPt = null;
//            //	double[] PlVect = null;
//            //	ret = SapModel.FrameObj.GetLocalAxesAdvanced(frmId, ref Active, ref Plane2, ref PlVectOpt, ref PlCSys, ref PlDir, ref PlPt, ref PlVect);

//            //	double X = 0;
//            //	double Y = 0;
//            //	double Z = 0;
//            //	double RZ = 0;
//            //	double RY = 0;
//            //	double RX = 0;
//            //	SapModel.CoordSys.GetCoordSys(PlCSys, ref X, ref Y, ref Z, ref RZ, ref RY, ref RX);


//            //}

//            double[] matrix = new double[9];
//            ret = SapModel.FrameObj.GetTransformationMatrix(frmId, ref matrix, true);

//            //double[][] matrix44 = new double[4][];
//            //for (int i = 0; i < 4; i++)
//            //{
//            //  matrix44[i] = new double[4];
//            //}
//            //matrix44[0][0] = matrix[0];
//            //matrix44[0][1] = matrix[2];
//            //matrix44[0][2] = -matrix[1];

//            //matrix44[1][0] = matrix[3];
//            //matrix44[1][1] = matrix[5];
//            //matrix44[1][2] = -matrix[4];

//            //matrix44[2][0] = matrix[6];
//            //matrix44[2][1] = matrix[8];
//            //matrix44[2][2] = -matrix[7];

//            //matrix44[0][3] = 0;
//            //matrix44[1][3] = 0;
//            //matrix44[2][3] = 0;

//            //matrix44[3][0] = ptB.X;
//            //matrix44[3][1] = ptB.Y;
//            //matrix44[3][2] = ptB.Z;
//            //matrix44[3][3] = 1.0;


//            CI.Geometry3D.Matrix44 matrix44 = new CG.Matrix44();
//            matrix44.AxisX
//            CI.Geometry3D.Matrix44 matIDEA = new CI.Geometry3D.Matrix44(new WM.Point3D(ptB.X, ptB.Y, ptB.Z),
//                new CI.Geometry3D.Vector3D(matrix[0], matrix[3], matrix[6]),
//                new CI.Geometry3D.Vector3D(matrix[1], matrix[4], matrix[7]),
//                new CI.Geometry3D.Vector3D(matrix[2], matrix[5], matrix[8]));
//            //CG.IMatrix44 matIDEAa = new CG.Matrix44(matIDEA);
//            //CG.IMatrix44 matIDEAb = new CG.Matrix44(matIDEA);
//            matIDEA.Rotate()
//            matIDEA.Rotate(-Math.PI / 2, matIDEA.AxisX);

//            //matIDEAa.Rotate(Math.PI / 2, new CI.Geometry3D.Vector3D(1, 0, 0));
//            //matIDEAb.Rotate(-Math.PI / 2, new CI.Geometry3D.Vector3D(1, 0, 0));

//            var LocalCoordinateSystem = new CoordSystemByVector();
//            LocalCoordinateSystem.VecX = new Vector3D() { X = matIDEA.AxisX.DirectionX, Y = matIDEA.AxisX.DirectionY, Z = matIDEA.AxisX.DirectionZ };
//            LocalCoordinateSystem.VecY = new Vector3D() { X = matIDEA.AxisY.DirectionX, Y = matIDEA.AxisY.DirectionY, Z = matIDEA.AxisY.DirectionZ };
//            LocalCoordinateSystem.VecZ = new Vector3D() { X = matIDEA.AxisZ.DirectionX, Y = matIDEA.AxisZ.DirectionY, Z = matIDEA.AxisZ.DirectionZ };

//            ls.LocalCoordinateSystem = LocalCoordinateSystem;
//            //CG.IMatrix44 matIDEA_Gl = new CG.Matrix44();
//            //matIDEA_Gl.SetToIdentity();

//            // Rot = matIDEA.GetRotation(matIDEA_Gl, CG.Axis.XAxis);
//            //double Rot1x = matIDEA_Gl.GetRotation(matIDEA, CG.Axis.XAxis);
//            //double Rot1y = matIDEA_Gl.GetRotation(matIDEA, CG.Axis.YAxis);
//            //double Rot1z = matIDEA_Gl.GetRotation(matIDEA, CG.Axis.ZAxis);
//            //System.Windows.Media.Media3D.Matrix3D mat = CI.SceneDrw3D.MatrixHelper.MatrixFrom44(matrix44);

//            //int NumberNames = 0;
//            //string[] MyName = null;
//            //SapModel.CoordSys.GetNameList(ref NumberNames, ref MyName);
//            //double[] glmatrix = new double[9];
//            //SapModel.CoordSys.GetTransformationMatrix(MyName[0], ref glmatrix);
//            //int CardinalPoint = 0;
//            //bool Mirror2 = false;
//            //bool StiffTransform = false;

//            //double[] offset1 = new double[2];
//            //double[] offset2 = new double[2];
//            //string CSys = string.Empty;

//            //ret = SapModel.FrameObj.GetInsertionPoint(frmId, ref CardinalPoint, ref Mirror2, ref StiffTransform, ref offset1, ref offset2, ref CSys);

//            /*
//			 * CardinalPoint:
//					1 = bottom left
//					2 = bottom center
//					3 = bottom right
//					4 = middle left
//					5 = middle center
//					6 = middle right
//					7 = top left
//					8 = top center
//					9 = top right
//					10 = centroid
//					11 = shear center
//			 */

//            ls.StartPoint = new ReferenceElement(ptB);
//            ls.EndPoint = new ReferenceElement(ptE);
//            polyLine3D.Segments.Add(new ReferenceElement(ls));

//            openStructModel.PolyLine3D.Add(polyLine3D);
//            openStructModel.LineSegment3D.Add(ls);

//            Element1D el = new Element1D();
//            el.Id = inx + 1;
//            el.Name = "E" + (inx + 1).ToString();
//            el.Segment = new ReferenceElement(ls);
//            if (frmId == "2")
//            {
//                //el.EccentricityBeginZ = 0.5;
//                //        el.EccentricityEndZ = 0.5;
//            }
//            //el.RotationRx = Rot1x - Math.PI / 2; //  / 180.0 * Math.PI/* + Math.PI / 2*/;

//            //long idCssBeg = 0;
//            //long idCssEnd = 0;
//            el.CrossSectionBegin = new ReferenceElement(cssO);
//            el.CrossSectionEnd = new ReferenceElement(cssO);
//            string cssName = cssO.Name;
//            openStructModel.Element1D.Add(el);

//            Member1D mb = new Member1D();
//            mb.Id = (inx + 1);
//            String nameBar = frmId;
//            //if (barPtr.Name.Length > 0)
//            //{
//            //  nameBar = barPtr.Name;
//            //}
//            if (addCssName)
//            {
//                mb.Name = nameBar + " " + cssName;
//            }
//            else
//            {
//                mb.Name = nameBar;
//            }
//            mb.Elements1D.Add(new ReferenceElement(el));
//            openStructModel.Member1D.Add(mb);

//            ConnectedMember conMb = new ConnectedMember();
//            conMb.Id = mb.Id;
//            conMb.MemberId = new ReferenceElement(mb);

//            if (connection != null)
//            {
//                connection.ConnectedMembers.Add(conMb);
//                MemberHelp mbh = new MemberHelp()
//                {
//                    Name = frmId,
//                    IDEA_Id = mb.Id,
//                    Number = connection.ConnectedMembers.Count - 1,
//                };
//                myDictMemberHelp.Add(connection.ConnectedMembers.Count - 1, mbh);
//            }
//            else
//            {
//                MemberHelp mbh = new MemberHelp()
//                {
//                    Name = frmId,
//                    IDEA_Id = mb.Id,
//                    Number = openStructModel.Member1D.Count - 1,
//                };
//                myDictMemberHelp.Add(openStructModel.Member1D.Count - 1, mbh);
//            }
//            //// Justification
//            //Just = JustificationMapper.SapToDynamoFrm(ref Model, frmId);

//            //// Rotation
//            //bool ifadvanced = false;
//            //ret = Model.FrameObj.GetLocalAxes(frmId, ref Rot, ref ifadvanced);

//        }

//        private void AddCssNonconformity(Guid giudNonConf, string item)
//        {
//            if (NonConformity != null)
//            {
//                IList<Guid> guidList = null;
//                if (NonConformity.TryGetValue(item, out guidList))
//                {
//                    if (guidList == null)
//                    {
//                        guidList = new List<Guid>();
//                    }

//                    guidList.Add(giudNonConf);
//                }
//                else
//                {
//                    guidList = new List<Guid>();
//                    guidList.Add(giudNonConf);
//                    NonConformity.Add(item, guidList);
//                }
//            }
//        }

//        private Point3D AddNodeToOpenModel(string idNode, ref ETABSv17.cSapModel SapModel, OpenModel openStructModel)
//        {
//            // update start point
//            Point3D point;
//            myPoint3D.TryGetValue(idNode, out point);
//            if (!openStructModel.Point3D.Contains(point))
//            {
//                Double myStartX = 0;
//                Double myStartY = 0;
//                Double myStartZ = 0;
//                long ret = SapModel.PointObj.GetCoordCartesian(idNode, ref myStartX, ref myStartY, ref myStartZ);
//                point = new Point3D() { X = myStartX * UC(eUC.length), Y = myStartY * UC(eUC.length), Z = myStartZ * UC(eUC.length) };
//                point.Name = idNode;
//                point.Id = openStructModel.Point3D.Count + 1;
//                openStructModel.Point3D.Add(point);
//                myPoint3D.Add(idNode, point);
//            }
//            return point;
//        }

//        public void GetCombination(ref cSapModel Model, OpenModel openStructModel)
//        {
//            //Debug.Assert(false, "Napojení SAP2000");
//            string[] CombiNames = null;

//            int NumberNames = 0;
//            int ret = Model.RespCombo.GetNameList(ref NumberNames, ref CombiNames);
//            int iCombiItem = 1;
//            if (CombiNames == null)
//            {
//                return;
//            }

//            foreach (string cname in CombiNames)
//            {
//                AddOneCombination(ref Model, cname, openStructModel, ref iCombiItem);
//            }
//        }

//        private void AddOneCombination(ref cSapModel Model, string cname, OpenModel openStructModel, ref int iCombiItem)
//        {
//            int NumberItems = 0;
//            //int Count = 0;
//            //Model.RespCombo.CountCase(cname, ref Count);
//            eCNameType[] CNameType = null; // new eCNameType[Count];
//            string[] CName = null;
//            double[] SF = null;
//            Model.RespCombo.GetCaseList(cname, ref NumberItems, ref CNameType, ref CName, ref SF);
//            CombiInputEC combi = new CombiInputEC();
//            combi.Id = openStructModel.GetMaxId(combi) + 1;
//            combi.Name = cname;
//            combi.TypeCombiEC = TypeOfCombiEC.ULS;
//            combi.Items = new List<CombiItem>();
//            for (int i = 0; i < NumberItems; i++)
//            {
//                if (CNameType[i] == eCNameType.LoadCase)
//                {
//                    LoadCase lc = openStructModel.LoadCase.FirstOrDefault(c => c.Name == CName[i]);
//                    if (lc != null)
//                    {
//                        CombiItem it = new CombiItem();
//                        it.Id = iCombiItem;
//                        iCombiItem++;
//                        it.LoadCase = new ReferenceElement(lc);
//                        it.Coeff = SF[i];
//                        combi.Items.Add(it);
//                        //openStructModel.AddObject(it);
//                    }
//                    else
//                    {
//                        // něco je špatně
//                        //Debug.Assert(false, "Nemám stav v kombinaci");
//                    }
//                }
//                if (CNameType[i] == eCNameType.LoadCombo)
//                {
//                    CombiInput combo = openStructModel.CombiInput.FirstOrDefault(c => c.Name == CName[i]);
//                    if (combo != null)
//                    {
//                        CombiItem it = new CombiItem();
//                        it.Id = iCombiItem;
//                        iCombiItem++;
//                        it.Combination = new ReferenceElement(combo);
//                        it.Coeff = SF[i];
//                        combi.Items.Add(it);
//                        //openStructModel.AddObject(it);
//                    }
//                    else
//                    {
//                        AddOneCombination(ref Model, CName[i], openStructModel, ref iCombiItem);
//                        combo = openStructModel.CombiInput.FirstOrDefault(c => c.Name == CName[i]);
//                        if (combo != null)
//                        {
//                            CombiItem it = new CombiItem();
//                            it.Id = iCombiItem;
//                            iCombiItem++;
//                            it.Combination = new ReferenceElement(combo);
//                            it.Coeff = SF[i];
//                            combi.Items.Add(it);
//                            //openStructModel.AddObject(it);
//                        }
//                    }
//                }
//            }
//            openStructModel.AddObject(combi);
//        }


//        public void GetLoadCases(ref cSapModel Model, OpenModel openStructModel)
//        {
//            string[] LoadCaseNames = null;
//            double[] LoadCaseMultipliers = null;
//            string[] LoadCaseTypes = null;

//            int NumberNames = 0;
//            int ret = Model.LoadCases.GetNameList(ref NumberNames, ref LoadCaseNames);

//            LoadCaseMultipliers = new double[NumberNames];
//            LoadCaseTypes = new string[NumberNames];

//            IDictionary<string, LoadGroupEC> lgDict = new Dictionary<string, LoadGroupEC>();
//            int NumberLc = 0;
//            string[] CaseName = null;
//            int[] Status = null;
//            Model.Analyze.GetCaseStatus(ref NumberLc, ref CaseName, ref Status);
//            foreach (string lcname in LoadCaseNames)
//            {
//                //Parameters that we need to get
//                //dummy eLoadCaseType
//                int pos = Array.IndexOf(LoadCaseNames, lcname);
//                eLoadCaseType cType = eLoadCaseType.LinearStatic;
//                int subType = 0;
//                eLoadPatternType DesignType = eLoadPatternType.Dead;
//                int DesignTypeOption = 0;
//                int Auto = 0;
//                ret = Model.LoadCases.GetTypeOAPI_1(lcname, ref cType, ref subType, ref DesignType, ref DesignTypeOption, ref Auto);
//                if (cType == ETABSv17.eLoadCaseType.LinearStatic && Status[pos] != 4)
//                {
//                    continue;
//                }

//                int NumberResults = 0;
//                string[] Obj = null;
//                double[] ObjSta = null;
//                string[] Elm = null;
//                double[] ElmSta = null;
//                string[] LoadCase = null;
//                string[] StepType = null;
//                double[] StepNum = null;
//                double[] P = null;
//                double[] V2 = null;
//                double[] V3 = null;
//                double[] T = null;
//                double[] M2 = null;
//                double[] M3 = null;
//                eItemTypeElm ItemTypeElm = eItemTypeElm.ObjectElm;

//                var retLC = Model.Results.Setup.DeselectAllCasesAndCombosForOutput();
//                retLC = Model.Results.Setup.SetCaseSelectedForOutput(lcname);
//                {
//                    Model.Results.FrameForce(myDictMemberHelp.FirstOrDefault().Value.Name, ItemTypeElm, ref NumberResults, ref Obj, ref ObjSta, ref Elm, ref ElmSta, ref LoadCase, ref StepType, ref StepNum, ref P, ref V2, ref V3, ref T, ref M2, ref M3);
//                    if (NumberResults < 1)
//                    {
//                        continue;
//                    }
//                }


//                //get the load case type
//                if (cType != eLoadCaseType.Modal)
//                {
//                    var lc = AddLoadCase(openStructModel, lgDict, lcname, cType, DesignType);
//                    myLoadCases.Add(lcname, lc);
//                }
//                LoadCaseTypes[pos] = cType.ToString();
//            }
//        }

//        private LoadCase AddLoadCase(OpenModel openStructModel, IDictionary<string, LoadGroupEC> lgDict, string lcname, eLoadCaseType type, eLoadPatternType DesignType)
//        {
//            String caseName = lcname;

//            LoadCase loadCase = new LoadCase();
//            loadCase.Name = lcname;
//            loadCase.Id = openStructModel.LoadCase.Count + 1;

//            loadCase.LoadType = (DesignType == eLoadPatternType.Dead || DesignType == eLoadPatternType.SuperDead) ? LoadCaseType.Permanent : LoadCaseType.Variable;
//            switch (loadCase.LoadType)
//            {
//                case LoadCaseType.Permanent:
//                    loadCase.Type = LoadCaseSubType.PermanentSelfweight;
//                    break;
//                case LoadCaseType.Variable:
//                    loadCase.Type = LoadCaseSubType.VariableStatic;
//                    break;
//                case LoadCaseType.Accidental:
//                    loadCase.Type = LoadCaseSubType.VariableStatic;
//                    break;
//                case LoadCaseType.Nonlinear:
//                    break;
//                default:
//                    break;
//            }


//            // load group - natvrdo
//            /*		LoadGroup(pOutStream, 1, _T("Permanent"), 0, _T("Standard"), 0, _T("Permanent"));
//							LoadGroup(pOutStream, 2, _T("Variable"), 0, _T("Standard"), 1, _T("Variable"));
//							LoadGroup(pOutStream, 3, _T("Wind"), 1, _T("Exclusive"), 1, _T("Variable"));
//							LoadGroup(pOutStream, 4, _T("Snow"), 0, _T("Standard"), 1, _T("Variable"));
//							LoadGroup(pOutStream, 5, _T("Temperature"), 0, _T("Standard"), 1, _T("Variable"));
//							LoadGroup(pOutStream, 6, _T("Accidental"), 0, _T("Standard"), 2, _T("Accidental"));
//							LoadGroup(pOutStream, 7, _T("Seismic"), 0, _T("Standard"), 3, _T("Seismic"));*/

//            int lgId = 0;
//            String lgName = string.Empty;
//            switch (DesignType)
//            {
//                case eLoadPatternType.ActiveEarthPressure:
//                    break;
//                case eLoadPatternType.Bouyancy:
//                    break;
//                case eLoadPatternType.Braking:
//                    break;
//                case eLoadPatternType.Centrifugal:
//                    break;
//                case eLoadPatternType.Construction:
//                    break;
//                case eLoadPatternType.Creep:
//                    break;
//                case eLoadPatternType.Dead:
//                    lgId = 1; lgName = "Permanent";
//                    break;
//                case eLoadPatternType.DeadManufacture:
//                    break;
//                case eLoadPatternType.DeadWater:
//                    break;
//                case eLoadPatternType.DeadWearing:
//                    break;
//                case eLoadPatternType.DownDrag:
//                    break;
//                case eLoadPatternType.EarthHydrostatic:
//                    break;
//                case eLoadPatternType.EarthSurcharge:
//                    break;
//                case eLoadPatternType.EuroLm1Char:
//                    break;
//                case eLoadPatternType.EuroLm1Freq:
//                    break;
//                case eLoadPatternType.EuroLm2:
//                    break;
//                case eLoadPatternType.EuroLm3:
//                    break;
//                case eLoadPatternType.EuroLm4:
//                    break;
//                case eLoadPatternType.Friction:
//                    break;
//                case eLoadPatternType.HorizontalEarthPressure:
//                    break;
//                case eLoadPatternType.Hyperstatic:
//                    break;
//                case eLoadPatternType.Ice:
//                    break;
//                case eLoadPatternType.Impact:
//                    break;
//                case eLoadPatternType.Live:
//                    lgId = 5; lgName = "Live";
//                    break;
//                case eLoadPatternType.LiveLoadSurcharge:
//                    break;
//                case eLoadPatternType.LockedInForces:
//                    break;
//                case eLoadPatternType.Move:
//                    lgId = 4; lgName = "Move";
//                    break;
//                case eLoadPatternType.Notional:
//                    break;
//                case eLoadPatternType.Other:
//                    break;
//                case eLoadPatternType.PassiveEarthPressure:
//                    break;
//                case eLoadPatternType.PatternLive:
//                    break;
//                case eLoadPatternType.PedestrianLL:
//                    break;
//                case eLoadPatternType.PedestrianLLReduced:
//                    break;
//                case eLoadPatternType.Prestress:
//                    break;
//                case eLoadPatternType.Quake:
//                    break;
//                case eLoadPatternType.ReduceLive:
//                    break;
//                case eLoadPatternType.Rooflive:
//                    break;
//                case eLoadPatternType.Settlement:
//                    break;
//                case eLoadPatternType.Shrinkage:
//                    break;
//                case eLoadPatternType.Snow:
//                    lgId = 3; lgName = "Snow";
//                    break;
//                case eLoadPatternType.SnowHighAltitude:
//                    break;
//                case eLoadPatternType.StreamFlow:
//                    break;
//                case eLoadPatternType.Temperature:
//                    break;
//                case eLoadPatternType.TemperatureGradient:
//                    break;
//                case eLoadPatternType.VehicleCollision:
//                    break;
//                case eLoadPatternType.VerticalEarthPressure:
//                    break;
//                case eLoadPatternType.VesselCollision:
//                    break;
//                case eLoadPatternType.WaterloadPressure:
//                    break;
//                case eLoadPatternType.Wave:
//                    break;
//                case eLoadPatternType.Wind:
//                    lgId = 2; lgName = "Wind";
//                    break;
//                case eLoadPatternType.WindOnLiveLoad:
//                    break;
//                default:
//                    break;
//            }
//            LoadGroupEC loadGroup = null;
//            if (!lgDict.ContainsKey(lgName))
//            {
//                loadGroup = new LoadGroupEC();
//                loadGroup.Id = lgId;
//                loadGroup.Name = lgName;
//                loadGroup.GammaQ = 1.5;
//                loadGroup.Psi0 = 0.7;
//                loadGroup.Psi1 = 0.5;
//                loadGroup.Psi2 = 0.3;
//                loadGroup.GammaGInf = 1.0;
//                loadGroup.GammaGSup = 1.35;
//                loadGroup.Dzeta = 0.85;
//                openStructModel.AddObject(loadGroup);
//                lgDict.Add(lgName, loadGroup);
//            }
//            else
//            {
//                lgDict.TryGetValue(lgName, out loadGroup);
//            }
//            switch (loadCase.Type)
//            {
//                case LoadCaseSubType.PermanentSelfweight:
//                    loadGroup.GroupType = LoadGroupType.Permanent;
//                    break;
//                case LoadCaseSubType.PermanentStandard:
//                    loadGroup.GroupType = LoadGroupType.Permanent;
//                    break;
//                case LoadCaseSubType.PermanentPrestress:
//                    loadGroup.GroupType = LoadGroupType.Permanent;
//                    break;
//                case LoadCaseSubType.PermanentRheologic:
//                    loadGroup.GroupType = LoadGroupType.Permanent;
//                    break;
//                case LoadCaseSubType.PermanentPrimaryEffect:
//                    loadGroup.GroupType = LoadGroupType.Permanent;
//                    break;
//                case LoadCaseSubType.VariableStatic:
//                    loadGroup.GroupType = LoadGroupType.Variable;
//                    break;
//                case LoadCaseSubType.VariableDynamic:
//                    loadGroup.GroupType = LoadGroupType.Variable;
//                    break;
//                case LoadCaseSubType.VariablePrimaryEffect:
//                    loadGroup.GroupType = LoadGroupType.Variable;
//                    break;
//                case LoadCaseSubType.PermanentPrestressPretensioned:
//                    loadGroup.GroupType = LoadGroupType.Permanent;
//                    break;
//                case LoadCaseSubType.PermanentPrestressPrimary:
//                    loadGroup.GroupType = LoadGroupType.Permanent;
//                    break;
//                case LoadCaseSubType.PermanentSelfweightLocal:
//                    loadGroup.GroupType = LoadGroupType.Permanent;
//                    break;
//                default:
//                    break;
//            }

//            loadCase.LoadGroup = new ReferenceElement(loadGroup);
//            /*
//			for (int iL = 1; iL <= casePtr.Records.Count; iL++)
//			{
//				IRobotLoadRecord loadrecord = casePtr.Records.Get(iL);
//				switch (loadrecord.Type)
//				{
//					case IRobotLoadRecordType.I_LRT_BAR_DEAD:
//						//case IRobotLoadRecordType.I_LRT_DEAD:
//						break;
//					case IRobotLoadRecordType.I_LRT_BAR_DILATATION:
//						break;
//					case IRobotLoadRecordType.I_LRT_BAR_FORCE_CONCENTRATED:
//						break;
//					case IRobotLoadRecordType.I_LRT_BAR_FORCE_CONCENTRATED_MASS:
//						break;
//					case IRobotLoadRecordType.I_LRT_BAR_MOMENT_DISTRIBUTED:
//						break;
//					case IRobotLoadRecordType.I_LRT_BAR_THERMAL:
//						break;
//					case IRobotLoadRecordType.I_LRT_BAR_TRAPEZOIDALE:
//						break;
//					case IRobotLoadRecordType.I_LRT_BAR_TRAPEZOIDALE_MASS:
//						break;
//					case IRobotLoadRecordType.I_LRT_BAR_UNIFORM:
//						break;
//					case IRobotLoadRecordType.I_LRT_BAR_UNIFORM_MASS:
//						break;
//					case IRobotLoadRecordType.I_LRT_IN_3_POINTS:
//						break;
//					case IRobotLoadRecordType.I_LRT_IN_CONTOUR:
//						break;
//					case IRobotLoadRecordType.I_LRT_LINEAR:
//						break;
//					case IRobotLoadRecordType.I_LRT_LINEAR_3D:
//						break;
//					case IRobotLoadRecordType.I_LRT_LINEAR_ON_EDGES:
//						break;
//					case IRobotLoadRecordType.I_LRT_MASS_ACTIVATION:
//						break;
//					case IRobotLoadRecordType.I_LRT_MOBILE_DISTRIBUTED:
//						break;
//					case IRobotLoadRecordType.I_LRT_MOBILE_POINT_FORCE:
//						break;
//					case IRobotLoadRecordType.I_LRT_NODE_ACCELERATION:
//						break;
//					case IRobotLoadRecordType.I_LRT_NODE_AUXILIARY:
//						break;
//					case IRobotLoadRecordType.I_LRT_NODE_DISPLACEMENT:
//						break;
//					case IRobotLoadRecordType.I_LRT_NODE_FORCE:
//						break;
//					case IRobotLoadRecordType.I_LRT_NODE_FORCE_IN_POINT:
//						break;
//					case IRobotLoadRecordType.I_LRT_NODE_FORCE_MASS:
//						break;
//					case IRobotLoadRecordType.I_LRT_NODE_VELOCITY:
//						break;
//					//case IRobotLoadRecordType.I_LRT_POINT_AUXILIARY:
//					//	break;
//					case IRobotLoadRecordType.I_LRT_PRESSURE:
//						break;
//					case IRobotLoadRecordType.I_LRT_SPECTRUM_VALUE:
//						break;
//					case IRobotLoadRecordType.I_LRT_SURFACE_ON_OBJECT:
//						break;
//					//case IRobotLoadRecordType.I_LRT_THERMAL:
//					//	break;
//					case IRobotLoadRecordType.I_LRT_THERMAL_IN_3_POINTS:
//						break;
//					case IRobotLoadRecordType.I_LRT_UNIFORM:
//						break;
//					default:
//						break;
//				}
//			}
//			*/

//            //long loadType = 1;
//            //String loadTypeStr = _T("Standard");

//            //long specification = caseNature == IRobotCaseNature.I_CN_TEMPERATURE ? 1 : 0;
//            //String specificationStr = specification == 0 ? _T("Standard") : _T("Temperature");

//            //long duration = caseNature == IRobotCaseNature.I_CN_PERMANENT ? 0 : 2;
//            //String durationStr = duration == 0 ? _T("Long") : _T("Short");
//            openStructModel.AddObject(loadCase);
//            return loadCase;
//        }


//        public void CreateIDEAOpenModelResults(ref ETABSv17.cSapModel SapModel, OpenModel openStructModel, out OpenModelResult openStructModelR)
//        {
//            openStructModelR = new OpenModelResult();
//            ResultsForLoadCaseContainer(ref SapModel, openStructModel, openStructModelR);
//        }

//        public void ResultsForLoadCaseContainer(ref ETABSv17.cSapModel SapModel, OpenModel openStructModel, OpenModelResult openStructModelR)
//        {

//            //Debug.Assert(false, "ResultsForLoadCaseContainer");
//            openStructModelR.ResultOnMembers = new List<ResultOnMembers>();
//            FillInternalForces(ref SapModel, openStructModel, openStructModelR);
//            //if (Worker != null)
//            //{
//            //  Worker.ReportProgress(90, "Deformations");
//            //}
//            //FillDeformations(RobotProject, openStructModel, openStructModelR);
//        }

//        private bool GetNodes(OpenModel openStructModel, int pointId, Member1D mb, out WM.Point3D pA, out WM.Point3D pB)
//        {
//            Element1D el = openStructModel.Element1D.FirstOrDefault(c => c.Id == mb.Elements1D[0].Id);
//            LineSegment3D seg = openStructModel.LineSegment3D.FirstOrDefault(c => c.Id == el.Segment.Id);
//            if (seg.StartPoint.Id == pointId || seg.EndPoint.Id == pointId)
//            {
//                pA = new WM.Point3D();
//                pB = new WM.Point3D();
//                return false;
//            }
//            var npA = openStructModel.Point3D.FirstOrDefault(c => c.Id == seg.StartPoint.Id);
//            var npB = openStructModel.Point3D.FirstOrDefault(c => c.Id == seg.EndPoint.Id);
//            pA = new WM.Point3D(npA.X, npA.Y, npA.Z);
//            pB = new WM.Point3D(npB.X, npB.Y, npB.Z);
//            return true;
//        }

//        private WM.Point3D GetIntersection(CG.Plane3D src, WM.Point3D pA, WM.Point3D pB)
//        {
//            var l = new WM.Vector3D(pB.X - pA.X,
//                pB.Y - pA.Y,
//                pB.Z - pA.Z);
//            l.Normalize();

//            double d = WM.Vector3D.DotProduct(src.NormalVector, l);
//            if (Math.Abs(d) < 1e-9)
//            {
//                return new WM.Point3D(double.NaN, double.NaN, double.NaN);
//            }

//            WM.Point3D l0 = pA;

//            double dd = WM.Vector3D.DotProduct((src.PointOnPlane - l0), src.NormalVector) / d;
//            return (l0 + dd * l);
//        }

//        private void FillInternalForces(ref ETABSv17.cSapModel SapModel, OpenModel openStructModel, OpenModelResult openStructModelR)
//        {
//            //      //long rowId = 0;
//            //      IRobotBarForceServer forces = RobotProject.Structure.Results.Bars.Forces;

//            //      ResultOnMembers resIF = new ResultOnMembers();//ResultType.InternalForces);
//            //      //IRobotCollection Collection = GetSelectedBars();
//            //      RobotCaseCollection allcases = RobotProject.Structure.Cases.GetAll();
//            //      int num = allcases.Count;
//            //      List<int> myDictLC = new List<int>();
//            //      for (int i = 1; i <= num; i++)
//            //      {
//            //        var casePtr = allcases.Get(i);
//            //        if (casePtr != null && casePtr is IRobotSimpleCase)
//            //        {
//            //          myDictLC.Add(casePtr.Number);
//            //        }
//            //        casePtr = null;
//            //      }
//            //      allcases = null;
//            ResultOnMembers resIF = new ResultOnMembers();//ResultType.InternalForces);
//            IdeaRS.OpenModel.Geometry3D.Point3D connectionPoint = null;
//            if (openStructModel.ConnectionPoint.Any())
//            {
//                ConnectionPoint connectionPointdta = openStructModel.ConnectionPoint.FirstOrDefault();
//                connectionPoint = openStructModel.GetObject(connectionPointdta.Node) as IdeaRS.OpenModel.Geometry3D.Point3D;// openStructModel.Point3D.FirstOrDefault(c => c.Id == connectionPointdta.NodeId);
//            }

//            for (int ib = 0; ib < openStructModel.Member1D.Count; ib++)
//            {
//                //RobotBar barPtr = Collection.Get(ib) as RobotBar;
//                Member1D mb = openStructModel.Member1D[ib];
//                //if (barPtr != null)
//                {
//                    Debug.WriteLine(string.Format("Beam {0}", mb.Name));
//                    //Worker.ReportProgress(75, Properties.Resources.Internalforces + " - " + Properties.Resources.Bar + " " + (ib + 1).ToString());

//                    //double lenght = 10.0;//myDictMemberHelp[mb.Id].Length;

//                    //if (lenght > 1e-3)
//                    {
//                        double? absTempDist = null;
//                        if (connectionPoint != null)
//                        {
//                            WM.Point3D pA;
//                            WM.Point3D pB;
//                            bool ex = GetNodes(openStructModel, connectionPoint.Id, mb, out pA, out pB);
//                            if (ex)
//                            {
//                                CG.Plane3D tempPlane = new CG.Plane3D();
//                                tempPlane.PointOnPlane = new WM.Point3D(connectionPoint.X, connectionPoint.Y, connectionPoint.Z);
//                                WM.Vector3D segDirVect = pB - pA;
//                                segDirVect.Normalize();
//                                tempPlane.NormalVector = segDirVect;
//                                WM.Point3D pointOnSeg = GetIntersection(tempPlane, pA, pB);
//                                WM.Vector3D pointOnSegDirVect = pointOnSeg - pA;
//                                double projectionLength = WM.Vector3D.DotProduct(segDirVect, pointOnSegDirVect);
//                                absTempDist = Math.Abs(projectionLength);
//                            }
//                        }


//                        //int numPoints = 10;
//                        //List<ResultOnSection> mbS = new List<ResultOnSection>();
//                        //List<double> mbDx = new List<double>();
//                        //for (int ip = 0; ip <= numPoints; ip++) // řezy
//                        //{
//                        //  double dx = 1F / numPoints * ip;
//                        //  mbDx.Add(dx);
//                        //}
//                        //if (absTempDist != null)
//                        //{
//                        //  mbDx.Add(absTempDist.Value / lenght - 0.000001);
//                        //  mbDx.Add(absTempDist.Value / lenght + 0.000001);
//                        //}
//                        //mbDx.Sort();
//                        //for (int ip = 0; ip < mbDx.Count; ip++) // řezy
//                        //{
//                        //  ResultOnSection resSec = new ResultOnSection();
//                        //  resSec.Position = mbDx[ip];
//                        //  mbS.Add(resSec);
//                        //}
//                        eItemTypeElm ItemTypeElm = eItemTypeElm.ObjectElm;
//                        int NumberResults = 0;
//                        string[] Obj = null;
//                        double[] ObjSta = null;
//                        string[] Elm = null;
//                        double[] ElmSta = null;
//                        string[] LoadCase = null;
//                        string[] StepType = null;
//                        double[] StepNum = null;
//                        double[] P = null;
//                        double[] V2 = null;
//                        double[] V3 = null;
//                        double[] T = null;
//                        double[] M2 = null;
//                        double[] M3 = null;
//                        ResultOnMember resMember = new ResultOnMember(new Member() { Id = mb.Id }, ResultType.InternalForces);
//                        bool start = true;
//                        //SortedList<TDouble, int> resSection = new SortedList<TDouble, int>();
//                        //foreach (var lc in openStructModel.LoadCase)
//                        //{
//                        //	var ret = SapModel.Results.Setup.DeselectAllCasesAndCombosForOutput();
//                        //	ret = SapModel.Results.Setup.SetCaseSelectedForOutput(lc.Name);
//                        //	{
//                        //		SapModel.Results.FrameForce(myDictMemberHelp[ib].Name, ItemTypeElm, ref NumberResults, ref Obj, ref ObjSta, ref Elm, ref ElmSta, ref LoadCase, ref StepType, ref StepNum, ref P, ref V2, ref V3, ref T, ref M2, ref M3);
//                        //		//var lc = openStructModel.LoadCase.FirstOrDefault(c => c.Name == myDictMemberHelp[ib].Name);
//                        //		getSections(ref SapModel, myDictMemberHelp[ib].Name, resSection, mb, lc.Name, 10, 0, lc.Id, ref NumberResults, ref Obj, ref ObjSta, ref Elm, ref ElmSta, ref LoadCase, ref StepType, ref StepNum, ref P, ref V2, ref V3, ref T, ref M2, ref M3);
//                        //	}
//                        //	start = false;

//                        //}
//                        //foreach (var itemDx in resSection)
//                        //{
//                        //	ResultOnSection resSec;
//                        //	resSec = new ResultOnSection();
//                        //	resSec.Position = itemDx.Key.X;
//                        //	resMember.Results.Add(resSec);
//                        //}

//                        foreach (var lc in openStructModel.LoadCase)
//                        {
//                            var ret = SapModel.Results.Setup.DeselectAllCasesAndCombosForOutput();
//                            ret = SapModel.Results.Setup.SetCaseSelectedForOutput(lc.Name);
//                            {
//                                SapModel.Results.FrameForce(myDictMemberHelp[ib].Name, ItemTypeElm, ref NumberResults, ref Obj, ref ObjSta, ref Elm, ref ElmSta, ref LoadCase, ref StepType, ref StepNum, ref P, ref V2, ref V3, ref T, ref M2, ref M3);
//                                //var lc = openStructModel.LoadCase.FirstOrDefault(c => c.Name == myDictMemberHelp[ib].Name);
//                                getIfForces(ref SapModel, myDictMemberHelp[ib].Name, start, resMember.Results, /*resSection, */mb, lc.Name, 10, 0, lc.Id, ref NumberResults, ref Obj, ref ObjSta, ref Elm, ref ElmSta, ref LoadCase, ref StepType, ref StepNum, ref P, ref V2, ref V3, ref T, ref M2, ref M3);
//                            }
//                            start = false;

//                        }
//                        resIF.Members.Add(resMember);
//                    }
//                }
//            }
//            openStructModelR.ResultOnMembers.Add(resIF);
//        }

//        public class TDouble : IComparable<TDouble>
//        {
//            private double tst = 5e-4;
//            public TDouble(double x, double tst = 5e-4)
//            {
//                X = x;
//                this.tst = tst;
//            }
//            /// <summary>
//            /// X
//            /// </summary>
//            public double X { get; set; }

//            /// <summary>
//            /// CompareTo
//            /// </summary>
//            /// <param name="add"></param>
//            /// <returns></returns>
//            public int CompareTo(TDouble add)
//            {
//                if (Extension.IsEqual(X, add.X, tst))
//                {
//                    return 0;
//                }

//                if (X < add.X)
//                {
//                    return -1;
//                }
//                else
//                {
//                    return 1;
//                }
//            }
//        }

//        private void getSections(ref ETABSv17.cSapModel SapModel, string frmId, SortedList<TDouble, int> resSection, Member1D mb, string patter_case_combo, double lenght, double dx, int loadCaseNumber, ref int NumberResults, ref string[] Obj, ref double[] ObjSta, ref string[] Elm, ref double[] ElmSta, ref string[] LoadCase, ref string[] StepType, ref double[] StepNum, ref double[] P, ref double[] V2, ref double[] V3, ref double[] T, ref double[] M2, ref double[] M3)
//        {
//            int index = 0;
//            int endindex = 0;

//            if (NumberResults != 0)
//            {
//                index = Array.IndexOf(LoadCase, patter_case_combo);
//                endindex = Array.LastIndexOf(LoadCase, patter_case_combo);
//            }

//            if (NumberResults != 0)
//            {
//                double previoust = -10;
//                for (int j = index; j <= endindex; j++) // řez
//                {

//                    double t = (ObjSta[j] * UC(eUC.length)) / (ObjSta[endindex] * UC(eUC.length));
//                    //ResultOnSection resSec;
//                    TDouble add = new TDouble(t, 1e-7);

//                    if (t == previoust)
//                    {
//                        t += 0.000001;
//                    }
//                    if (resSection.ContainsKey(add) == false)
//                    {
//                        resSection.Add(add, j);
//                    }
//                    previoust = t;
//                }
//            }
//        }

//        private void getIfForces(ref ETABSv17.cSapModel SapModel, string frmId, bool start, List<ResultBase> res, /*SortedList<TDouble, int> resSection, */Member1D mb, string patter_case_combo, double lenght, double dx, int loadCaseNumber, ref int NumberResults, ref string[] Obj, ref double[] ObjSta, ref string[] Elm, ref double[] ElmSta, ref string[] LoadCase, ref string[] StepType, ref double[] StepNum, ref double[] P, ref double[] V2, ref double[] V3, ref double[] T, ref double[] M2, ref double[] M3)
//        {
//            int index = 0;
//            int endindex = 0;

//            if (NumberResults != 0)
//            {
//                index = Array.IndexOf(LoadCase, patter_case_combo);
//                endindex = Array.LastIndexOf(LoadCase, patter_case_combo);
//            }

//            string StartPoint = string.Empty;
//            string EndPoint = string.Empty;
//            int ret = SapModel.FrameObj.GetPoints(frmId, ref StartPoint, ref EndPoint);

//            Point3D ptB;
//            myPoint3D.TryGetValue(StartPoint, out ptB);
//            Point3D ptE;
//            myPoint3D.TryGetValue(EndPoint, out ptE);
//            double coeffMy = 1.0;
//            double coeffVz = -1.0;
//            //List<Tuple<double, ResultOfInternalForces>> resLcOut = new List<Tuple<double,ResultOfInternalForces>>();

//            if (NumberResults != 0)
//            {
//                double previoust = -10;
//                int inx = 0;
//                for (int j = index; j <= endindex; j++) // řez
//                {
//                    ResultOfInternalForces resLc;
//                    resLc = new ResultOfInternalForces();
//                    resLc.Loading = new ResultOfLoading() { Id = loadCaseNumber, LoadingType = LoadingType.LoadCase };
//                    resLc.Loading.Items.Add(new ResultOfLoadingItem() { Coefficient = 1.0 });

//                    double t = (ObjSta[j] * UC(eUC.length)) / (ObjSta[endindex] * UC(eUC.length));

//                    ResultOnSection resSec;
//                    if (start)
//                    {
//                        resSec = new ResultOnSection();
//                        resSec.Position = t;
//                        resSec.AbsoluteRelative = AbsoluteRelative.Relative;
//                        if (t == previoust)
//                        {
//                            resSec.Position += 0.000001;
//                        }
//                        res.Add(resSec);
//                    }
//                    else
//                    {
//                        if (inx < res.Count)
//                        {
//                            resSec = res[inx] as ResultOnSection;
//                        }
//                        else
//                        {
//                            continue;
//                        }
//                    }

//                    //System.Windows.Media.Media3D.Point3D pt = new System.Windows.Media.Media3D.Point3D(P[j], V3[j], V2[j]);
//                    //pt = mat.Transform(pt);

//                    //System.Windows.Media.Media3D.Point3D ptM = new System.Windows.Media.Media3D.Point3D(T[j], M2[j], M3[j]);
//                    //ptM = mat.Transform(ptM);

//                    //resLc.N = pt.X;
//                    //resLc.Qy = pt.Y;
//                    //resLc.Qz = pt.Z;
//                    //resLc.Mx = ptM.X;
//                    //resLc.My = ptM.Y;
//                    //resLc.Mz = ptM.Z;

//                    resLc.N = P[j] * UC(eUC.force);
//                    Debug.WriteLine(string.Format("Orig N {0} ; Idea {1}", P[j], resLc.N));
//                    resLc.Qy = V3[j] * UC(eUC.force);
//                    resLc.Qz = V2[j] * coeffVz * UC(eUC.force);
//                    resLc.Mx = T[j] * UC(eUC.force) * UC(eUC.length);
//                    resLc.My = M3[j] * coeffMy * UC(eUC.force) * UC(eUC.length);
//                    Debug.WriteLine(string.Format("Orig My {0} ; Idea {1}", M3[j], resLc.My));
//                    resLc.Mz = -M2[j] * UC(eUC.force) * UC(eUC.length); ;
//                    previoust = t;
//                    resSec.Results.Add(resLc);
//                    inx++;
//                }
//            }
//            //for (int inx = 0; inx < res.Count; inx++)
//            //{
//            //	ResultOnSection resSec = res[inx] as ResultOnSection;
//            //	ResultOfInternalForces resLc;
//            //	resLc = new ResultOfInternalForces();
//            //	resLc.Loading = new ResultOfLoading() { Id = loadCaseNumber, LoadingType = LoadingType.LoadCase };
//            //	resLc.Loading.Items.Add(new ResultOfLoadingItem() { Coefficient = 1.0 });
//            //	GetValuesInterpolate(resSec, resLcOut, ref resLc);
//            //	resSec.Results.Add(resLc);

//            //}
//        }

//        private void GetValuesInterpolate(ResultOnSection rezX, List<Tuple<double, ResultOfInternalForces>> resLcOut, ref ResultOfInternalForces resLc)
//        {
//            int count = resLcOut.Count;
//            int iDx = 0;
//            if (count < 1)
//            {
//                return;
//            }
//            while (resLcOut[iDx].Item1 < rezX.Position)
//            {
//                iDx++;
//                if (iDx == count)
//                {
//                    resLc.N = resLcOut[iDx].Item2.N;
//                    resLc.Qy = resLcOut[iDx].Item2.Qy;
//                    resLc.Qz = resLcOut[iDx].Item2.Qz;
//                    resLc.Mx = resLcOut[iDx].Item2.Mx;
//                    resLc.My = resLcOut[iDx].Item2.My;
//                    resLc.Mz = resLcOut[iDx].Item2.Mz;
//                    return;
//                }
//            }
//            double actdx = resLcOut[iDx].Item1;
//            if ((actdx == rezX.Position))
//            {
//                resLc.N = resLcOut[iDx].Item2.N;
//                resLc.Qy = resLcOut[iDx].Item2.Qy;
//                resLc.Qz = resLcOut[iDx].Item2.Qz;
//                resLc.Mx = resLcOut[iDx].Item2.Mx;
//                resLc.My = resLcOut[iDx].Item2.My;
//                resLc.Mz = resLcOut[iDx].Item2.Mz;
//                return;
//            }
//            if (iDx == 0)
//            {
//                iDx++;
//                if (count <= iDx)
//                {
//                    return;
//                }
//                actdx = resLcOut[iDx].Item1;
//            }
//            double dist = actdx - resLcOut[iDx - 1].Item1;
//            if (dist > 1e-6)
//            {
//                ResultOfInternalForces pVal1 = resLcOut[iDx].Item2;
//                ResultOfInternalForces pVal2 = resLcOut[iDx - 1].Item2;

//                double k2 = (rezX.Position - resLcOut[iDx - 1].Item1) / dist;
//                double k1 = 1 - k2;
//                //pVal = new double[pVal1.Count()];
//                resLc.N = (float)(k2 * pVal1.N + k1 * pVal2.N);
//                resLc.Qy = (float)(k2 * pVal1.Qy + k1 * pVal2.Qy);
//                resLc.Qz = (float)(k2 * pVal1.Qz + k1 * pVal2.Qz);
//                resLc.Mx = (float)(k2 * pVal1.Mx + k1 * pVal2.Mx);
//                resLc.My = (float)(k2 * pVal1.My + k1 * pVal2.My);
//                resLc.Mz = (float)(k2 * pVal1.Mz + k1 * pVal2.Mz);

//            }

//        }




//    }
//}
