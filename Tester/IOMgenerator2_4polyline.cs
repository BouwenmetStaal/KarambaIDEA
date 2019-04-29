using IdeaRS.OpenModel;
using IdeaRS.OpenModel.Connection;
using IdeaRS.OpenModel.CrossSection;
using IdeaRS.OpenModel.Geometry3D;
using IdeaRS.OpenModel.Loading;
using IdeaRS.OpenModel.Material;
using IdeaRS.OpenModel.Model;
using IdeaRS.OpenModel.Result;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConnectionLinkTestApp
{
    internal class IOMGenerator2_4polyline
    {
        private OpenModel openStructModel = null;
        private OpenModelResult openStructModelR = null;

        internal string CreateIOMTst()
        {



            CreateIDEAOpenModel();

            AddMaterialsToOpenModel();

            AddCrossSectionToOpenModel();

            AddNodesToOpenModel();

            CreateIDEAOpenModelConnection();



            AddLoadCasesToOpenModel();

            AddCombinationToOpenModel();

            CreateIDEAOpenModelResults();



            //RAZ: Filepath locatie toewijzen
            string strFil = @"C:\Data\test_k_joint";
            //if (result == true)

            openStructModel.SaveToXmlFile(strFil);
            openStructModelR.SaveToXmlFile(strFil + "r");
            //return saveFileDialog.FileName;

            return strFil;

        }

        private void CreateIDEAOpenModel()
        {
            openStructModel = new OpenModel();
            openStructModel.OriginSettings = new OriginSettings() { CrossSectionConversionTable = CrossSectionConversionTable.SCIA };
            openStructModel.OriginSettings.ProjectName = "TrussNode";
            openStructModel.OriginSettings.Author = "Author PGC";
            openStructModel.OriginSettings.ProjectDescription = "TestProjectCreatingTrussnodes";
            openStructModel.OriginSettings.DateOfCreate = DateTime.Now;


        }

        private void AddMaterialsToOpenModel()
        {
            MatSteelEc2 matOM = new MatSteelEc2();
            matOM.Id = openStructModel.GetMaxId(matOM) + 1;
            matOM.Name = "S275";
            matOM.E = 210000000;
            matOM.G = matOM.E / (2 * (1 + 0.3));
            matOM.Poisson = 0.3;
            matOM.UnitMass = 7850 / 9.81;
            matOM.fu = 430000000;
            matOM.fy = 275000000;
            matOM.fu40 = 410000000;
            matOM.fy40 = 255000000;
            matOM.SpecificHeat = 0.49;
            matOM.ThermalConductivity = 50.2;
            matOM.ThermalExpansion = 0.000012;

            openStructModel.AddObject(matOM);
        }

        private void AddCrossSectionToOpenModel()
        {
            AddWeldedI();
            AddRolledCSS();

        }


        private void AddRolledCSS()
        {
            CrossSectionParameter cssO = new CrossSectionParameter();
            cssO.Material = new ReferenceElement(openStructModel.MatSteel.First()); // I have only one material, I take the first one
            cssO.CrossSectionType = CrossSectionType.RolledI;
            string ideaName = "IPE200";
            cssO.Name = "IPE200";
            cssO.Parameters.Add(new ParameterString() { Name = "UniqueName", Value = ideaName });
            //Q: how do you know what the exact name of the parameter should be?

            openStructModel.AddObject(cssO);
        }

        private void AddWeldedI()
        {
            //Example of custom I section
            CrossSectionParameter cssWI = new CrossSectionParameter();
            cssWI.Id = 1;
            cssWI.Material = new ReferenceElement(openStructModel.MatSteel.First());  // I have only one material, I take the first one
            cssWI.Name = "I 400, welded";
            CrossSectionFactory.FillWeldedI(cssWI, 0.2, 0.4, 0.025, 0.020);
            openStructModel.AddObject(cssWI);


        }

        private void AddNodesToOpenModel()
        {
            Point3D pointA = new Point3D() { X = -3, Y = 0, Z = 0.0 };
            pointA.Name = "N1";
            pointA.Id = 1;
            openStructModel.AddObject(pointA);

            Point3D pointB = new Point3D() { X = 0, Y = 0, Z = 0 };
            pointB.Name = "N2";
            pointB.Id = 2;
            openStructModel.AddObject(pointB);

            Point3D pointC = new Point3D() { X = 3, Y = 0, Z = 0 };
            pointC.Name = "N3";
            pointC.Id = 3;
            openStructModel.AddObject(pointC);

            Point3D pointD = new Point3D() { X = -3, Y = 0, Z = 6 };
            pointD.Name = "N4";
            pointD.Id = 4;
            openStructModel.AddObject(pointD);

            Point3D pointE = new Point3D() { X = 3, Y = 0, Z = 3 };
            pointE.Name = "N5";
            pointE.Id = 5;
            openStructModel.AddObject(pointE);

        }

        private void CreateIDEAOpenModelConnection()
        {
            ConnectionPoint connection = new ConnectionPoint();
            connection.Node = new ReferenceElement(openStructModel.Point3D.First(n => n.Id == 2));
            connection.Id = 1;
            connection.Name = "Con " + "N3";

            ConnectedMember conMb1 = AddConnectedMember(1, 1, 1, 2, 3);
            ConnectedMember conMb2 = AddConnectedMember(2, 2, 2, 4);
            ConnectedMember conMb3 = AddConnectedMember(3, 2, 2, 5);
            connection.ConnectedMembers.Add(conMb1);
            connection.ConnectedMembers.Add(conMb2);
            connection.ConnectedMembers.Add(conMb3);

            openStructModel.AddObject(connection);
        }

        private ConnectedMember AddConnectedMember(int idMember, int idCss, int idPointA, int idPointB, int idPointC)
        {
            PolyLine3D polyLine3D = new PolyLine3D();
            polyLine3D.Id = idMember;
            openStructModel.AddObject(polyLine3D);

            LineSegment3D ls1 = new LineSegment3D();
            ls1.Id = openStructModel.GetMaxId(ls1) + 1;
            openStructModel.AddObject(ls1);
            ls1.StartPoint = new ReferenceElement(openStructModel.Point3D.First(c => c.Id == idPointA));
            ls1.EndPoint = new ReferenceElement(openStructModel.Point3D.First(c => c.Id == idPointB));
            polyLine3D.Segments.Add(new ReferenceElement(ls1));

            LineSegment3D ls2 = new LineSegment3D();
            ls2.Id = openStructModel.GetMaxId(ls2) + 1;
            openStructModel.AddObject(ls2);
            ls2.StartPoint = new ReferenceElement(openStructModel.Point3D.First(c => c.Id == idPointB));
            ls2.EndPoint = new ReferenceElement(openStructModel.Point3D.First(c => c.Id == idPointC));
            polyLine3D.Segments.Add(new ReferenceElement(ls2));


            Element1D el1 = new Element1D();
            el1.Id = openStructModel.GetMaxId(el1) + 1;
            el1.Name = "E" + el1.Id;
            el1.Segment = new ReferenceElement(ls1);
            el1.CrossSectionBegin = new ReferenceElement(openStructModel.CrossSection.First(c => c.Id == idCss));
            el1.CrossSectionEnd = new ReferenceElement(openStructModel.CrossSection.First(c => c.Id == idCss));
            openStructModel.AddObject(el1);

            Element1D el2 = new Element1D();
            el2.Id = openStructModel.GetMaxId(el2) + 1;
            el2.Name = "E" + el2.Id;
            el2.Segment = new ReferenceElement(ls2);
            el2.CrossSectionBegin = new ReferenceElement(openStructModel.CrossSection.First(c => c.Id == idCss));
            el2.CrossSectionEnd = new ReferenceElement(openStructModel.CrossSection.First(c => c.Id == idCss));
            openStructModel.AddObject(el2);

            Member1D mb = new Member1D();
            mb.Id = idMember;
            mb.Name = "B" + mb.Id;
            mb.Elements1D.Add(new ReferenceElement(el1));
            mb.Elements1D.Add(new ReferenceElement(el2));
            openStructModel.Member1D.Add(mb);

            ConnectedMember conMb = new ConnectedMember();
            conMb.Id = mb.Id;
            conMb.MemberId = new ReferenceElement(mb);
            return conMb;
        }

        private ConnectedMember AddConnectedMember(int idMember, int idCss, int idPointA, int idPointB)
        {
            PolyLine3D polyLine3D = new PolyLine3D();
            polyLine3D.Id = openStructModel.GetMaxId(polyLine3D) + 1;
            openStructModel.AddObject(polyLine3D);


            LineSegment3D ls = new LineSegment3D();
            ls.Id = openStructModel.GetMaxId(ls) + 1;
            ls.StartPoint = new ReferenceElement(openStructModel.Point3D.First(c => c.Id == idPointA));
            ls.EndPoint = new ReferenceElement(openStructModel.Point3D.First(c => c.Id == idPointB));
            openStructModel.AddObject(ls);
            polyLine3D.Segments.Add(new ReferenceElement(ls));

            Element1D el = new Element1D();
            el.Id = openStructModel.GetMaxId(el) + 1;
            el.Name = "E" + el.Id.ToString();
            el.Segment = new ReferenceElement(ls);
            el.CrossSectionBegin = new ReferenceElement(openStructModel.CrossSection.First(c => c.Id == idCss));
            el.CrossSectionEnd = new ReferenceElement(openStructModel.CrossSection.First(c => c.Id == idCss));
            openStructModel.AddObject(el);

            Member1D mb = new Member1D();
            mb.Id = idMember;
            mb.Name = "B" + mb.Id.ToString();
            mb.Elements1D.Add(new ReferenceElement(el));
            openStructModel.Member1D.Add(mb);

            ConnectedMember conMb = new ConnectedMember();
            conMb.Id = mb.Id;
            conMb.MemberId = new ReferenceElement(mb);
            return conMb;
        }

        private void AddLoadCasesToOpenModel()
        {
            LoadCase loadCase = new LoadCase();
            loadCase.Name = "LC1";
            loadCase.Id = 1;
            loadCase.Type = LoadCaseSubType.PermanentStandard;

            LoadGroupEC loadGroup = null;

            loadGroup = new LoadGroupEC();
            loadGroup.Id = 1;
            loadGroup.Name = "LG1";
            loadGroup.GammaQ = 1.5;
            loadGroup.Psi0 = 0.7;
            loadGroup.Psi1 = 0.5;
            loadGroup.Psi2 = 0.3;
            loadGroup.GammaGInf = 1.0;
            loadGroup.GammaGSup = 1.35;
            loadGroup.Dzeta = 0.85;
            openStructModel.AddObject(loadGroup);

            loadCase.LoadGroup = new ReferenceElement(loadGroup);

            openStructModel.AddObject(loadCase);
        }

        private void AddCombinationToOpenModel()
        {
            CombiInputEC combi = new CombiInputEC();
            combi.Name = "CO1";
            combi.TypeCombiEC = TypeOfCombiEC.ULS;
            combi.TypeCalculationCombi = TypeCalculationCombiEC.Linear;
            combi.Items = new List<CombiItem>();
            CombiItem it = new CombiItem();
            it.Id = 1;
            it.LoadCase = new ReferenceElement(openStructModel.LoadCase.First());
            it.Coeff = 1.0;
            combi.Items.Add(it);
            openStructModel.AddObject(combi);
        }

        private void CreateIDEAOpenModelResults()
        {
            openStructModelR = new OpenModelResult();
            openStructModelR.ResultOnMembers = new List<ResultOnMembers>();
            ResultOnMembers resIF = new ResultOnMembers();
            for (int ib = 0; ib < openStructModel.Member1D.Count; ib++)
            {
                Member1D mb = openStructModel.Member1D[ib];
                for (int iel = 0; iel < mb.Elements1D.Count; iel++)
                {
                    Element1D elem = openStructModel.Element1D.First(c => c.Id == mb.Elements1D[iel].Id);
                    ResultOnMember resMember = new ResultOnMember(new Member() { Id = elem.Id, MemberType = MemberType.Element1D }, ResultType.InternalForces);
                    int numPoints = 10;
                    for (int ip = 0; ip <= numPoints; ip++)
                    {
                        ResultOnSection resSec = new ResultOnSection();

                        resSec.AbsoluteRelative = AbsoluteRelative.Relative;
                        resSec.Position = (double)ip / (double)numPoints;
                        int count = openStructModel.LoadCase.Count;
                        for (int i = 1; i <= count; i++)
                        {
                            ResultOfInternalForces resLc = new ResultOfInternalForces();
                            int loadCaseNumber = i;
                            resLc.Loading = new ResultOfLoading() { Id = loadCaseNumber, LoadingType = LoadingType.LoadCase };
                            resLc.Loading.Items.Add(new ResultOfLoadingItem() { Coefficient = 1.0 });
                            resLc.N = 15000;
                            resLc.Qy = 2;
                            resLc.Qz = 3;
                            resLc.Mx = 4;
                            resLc.My = (ip + 1) * 21000;
                            resLc.Mz = 6;
                            resSec.Results.Add(resLc);
                        }

                        resMember.Results.Add(resSec);
                    }

                    resIF.Members.Add(resMember);
                }
            }

            openStructModelR.ResultOnMembers.Add(resIF);
        }
    }
}