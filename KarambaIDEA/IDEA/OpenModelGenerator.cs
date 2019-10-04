// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using IdeaRS.OpenModel;
using IdeaRS.OpenModel.Connection;
using IdeaRS.OpenModel.CrossSection;
using IdeaRS.OpenModel.Geometry3D;
using IdeaRS.OpenModel.Loading;
using IdeaRS.OpenModel.Material;
using IdeaRS.OpenModel.Model;
using IdeaRS.OpenModel.Result;

using System;
using System.Collections.Generic;
using System.Linq;
using KarambaIDEA.Core;
using KarambaIDEA.IDEA;



namespace KarambaIDEA
{

    public class OpenModelGenerator
    {
        public OpenModel openModel = new OpenModel();
        public OpenModelResult openModelResult = new OpenModelResult() { ResultOnMembers = new List<ResultOnMembers>() };


		public string CreateOpenModelGenerator(Joint joint, string path)
        {
            //1.Set general project settings
            openModel.OriginSettings = new OriginSettings();
            openModel.OriginSettings.CrossSectionConversionTable = CrossSectionConversionTable.NoUsed;
            openModel.OriginSettings.ProjectName = joint.project.projectName;
            openModel.OriginSettings.Author = joint.project.author;
            openModel.OriginSettings.ProjectDescription = joint.project.projectName + "_connection:" + joint.id;
            openModel.OriginSettings.DateOfCreate = DateTime.Now;


			//NEW INICILI
			openModel.Connections.Add(new ConnectionData());
			openModel.Connections[0].Beams = new List<BeamData>();

			//2.add all relevant materials:
			List<KarambaIDEA.Core.MaterialSteel> materials = joint.attachedMembers.Select(a => a.ElementRAZ.crossSection.material).Distinct().ToList();
            foreach (KarambaIDEA.Core.MaterialSteel m in materials)
            {
                this.AddMaterialSteelToOpenModel(m as MaterialSteel);
            }

            //3.add all reelvant cross sections:
            List<KarambaIDEA.Core.CrossSection> crossSections = joint.attachedMembers.Select(a => a.ElementRAZ.crossSection).Distinct().ToList();
            foreach (KarambaIDEA.Core.CrossSection c in crossSections)
            {
                this.AddCrossSectionToOpenModel(c);
            }

            //4.add all relevant nodes:
            List<KarambaIDEA.Core.PointRAZ> startpoints = joint.attachedMembers.Select(a => a.ideaLine.Start).ToList();
            List<KarambaIDEA.Core.PointRAZ> endpoints = joint.attachedMembers.Select(a => a.ideaLine.End).ToList();
            List<KarambaIDEA.Core.PointRAZ> points = startpoints.Union(endpoints).Distinct().ToList();
            foreach (KarambaIDEA.Core.PointRAZ p in points)
            {
                this.AddPointsToOpenModel(p);
            }

            //5.create connection at the specified node:
            ConnectionPoint connectionPoint = new ConnectionPoint();
            Point3D point3D = openModel.Point3D.First(a => a.Id == joint.centralNodeOfJoint.id);
            connectionPoint.Node = new ReferenceElement(point3D);
            connectionPoint.Id = joint.id;
            connectionPoint.Name = "C" + joint.id;

            //6.create members
            List<BearingMember> bearingMembers = joint.attachedMembers.OfType<BearingMember>().ToList();
            List<ConnectingMember> connectingMembers = joint.attachedMembers.OfType<ConnectingMember>().ToList();
            if (bearingMembers.Count() == 1)
            {
                //line
                AddConnectedMember(bearingMembers.First(), connectionPoint);
            }
            else
            {
                //polyline
                AddConnectedMember(bearingMembers, connectionPoint);
            }
            foreach (ConnectingMember c in connectingMembers)
                AddConnectedMember(c, connectionPoint);
            openModel.AddObject(connectionPoint);

            //7.create loadcases
            foreach (LoadcaseRAZ loadcase in joint.project.loadcases)
            {
                AddLoadCaseToOpenModel(loadcase);
            }

			//8.create IOMresults
			CreateIDEAOpenModelResults(joint);

            ////9.save XML
            string strFil = path + "IOM";
            //openModel.SaveToXmlFile(strFil);
            //openModelResult.SaveToXmlFile(strFil + "result");
            return strFil;
        }

        /// <summary>
        /// Serializes the openmodel  object and returns it as a memory stream
        /// </summary>
        /// <returns>memory stream containing the openmodel  serialized in xml format</returns>



        private void AddMaterialSteelToOpenModel(MaterialSteel material)
        {

            MatSteelEc2 matOM = new MatSteelEc2();
            matOM.Id = material.id;
            matOM.Name = material.name;
            matOM.E = 210000000;
            matOM.Poisson = 0.3;
            matOM.UnitMass = 7870 / 9.81;
            matOM.fu = material.fu * Math.Pow(10, 6);// 430000000;
            matOM.fy = material.fy * Math.Pow(10, 6);// 275000000;
            matOM.fu40 = material.fu40 * Math.Pow(10, 6);// 410000000;
            matOM.fy40 = material.fy40 * Math.Pow(10, 6);// 255000000;
            matOM.SpecificHeat = 0.49;
            matOM.ThermalConductivity = 50.2;
            matOM.ThermalExpansion = 0.000012;
            matOM.G = matOM.E / (2 * (1 + matOM.Poisson));

            openModel.AddObject(matOM);
        }
        private void AddCrossSectionToOpenModel(KarambaIDEA.Core.CrossSection crossSection)
        {
            switch (crossSection.shape)
            {
                case KarambaIDEA.Core.CrossSection.Shape.ISection:
                    {
                        AddRolledCSS(crossSection);
                        return;
                    }
                case KarambaIDEA.Core.CrossSection.Shape.HollowSection:
                    {
                        AddHollowCSS(crossSection);
                        return;
                    }
                case KarambaIDEA.Core.CrossSection.Shape.CHSsection:
                    {
                        AddchsCSS(crossSection);
                        return;
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }
        private void AddHollowCSS(KarambaIDEA.Core.CrossSection crossSection)
        {
            CrossSectionParameter hollow = new CrossSectionParameter();
            hollow.Id = crossSection.id;
            MatSteel material = openModel.MatSteel.First(a => a.Id == crossSection.material.id);
            hollow.Material = new ReferenceElement(material);
            hollow.Name = crossSection.name;
            double height = crossSection.height / 1000;
            double width = crossSection.width / 1000;
            double tweb = crossSection.thicknessWeb / 1000;
            double tflange = crossSection.thicknessFlange / 1000;
            double radius = crossSection.radius / 1000;
            //CrossSectionFactory.FillCssRectangleHollow(hollow, width, height, tweb, tweb, tflange, tflange);
            CrossSectionFactory.FillCssSteelRectangularHollow(hollow, height, width, tweb, tweb, 2 * tweb, tflange);
            //height
            //width
            //thickness
            //innerradius
            //outerradius
            //unkown
            //CrossSectionFactory.FillCssSteelChannel(hollow, height, width, tweb, tflange, radius, radius, 0);

            openModel.AddObject(hollow);


        }
        private void AddRolledCSS(KarambaIDEA.Core.CrossSection crossSection)
        {
            CrossSectionParameter crossSectionParameter = new CrossSectionParameter();
            crossSectionParameter.Id = crossSection.id;
            //find related material:
            MatSteel material = openModel.MatSteel.First(a => a.Id == crossSection.material.id);
            crossSectionParameter.Material = new ReferenceElement(material);
            //set cross section type
            crossSectionParameter.CrossSectionType = CrossSectionType.RolledI;
            crossSectionParameter.Name = crossSection.name;
            crossSectionParameter.Parameters.Add(new ParameterString() { Name = "UniqueName", Value = crossSection.name });
            openModel.AddObject(crossSectionParameter);
        }
        private void AddchsCSS(KarambaIDEA.Core.CrossSection crossSection)
        {
            CrossSectionParameter chs = new CrossSectionParameter();
            chs.Id = crossSection.id;
            MatSteel material = openModel.MatSteel.First(a => a.Id == crossSection.material.id);
            chs.Material = new ReferenceElement(material);
            chs.Name = crossSection.name;
            double height = crossSection.height / 1000/2; //adjust to radius
            double width = crossSection.width / 1000;
            double tweb = crossSection.thicknessWeb / 1000;
            double tflange = crossSection.thicknessFlange / 1000;
            double radius = crossSection.radius / 1000;
            //CrossSectionFactory.FillCssRectangleHollow(hollow, width, height, tweb, tweb, tflange, tflange);
            CrossSectionFactory.FillOHollow(chs, height, tweb);
            //height
            //width
            //thickness
            //innerradius
            //outerradius
            //unkown
            //CrossSectionFactory.FillCssSteelChannel(hollow, height, width, tweb, tflange, radius, radius, 0);

            openModel.AddObject(chs);
        }
        private void AddPointsToOpenModel(PointRAZ point)
        {
            Point3D p = new Point3D();
            p.X = point.X;
            p.Y = point.Y;
            p.Z = point.Z;
            p.Id = point.id;//IDEA COUNTs FROM ONE //the ID is a unique ID from the Grasshopper project
            p.Name = "N" + p.Id;

            openModel.AddObject(p);
        }
        private void AddConnectedMember(List<BearingMember> bearingMembers, ConnectionPoint connectionPoint)
        {
            PolyLine3D polyLine3D = new PolyLine3D();
            polyLine3D.Id = openModel.GetMaxId(polyLine3D) + 1;
            openModel.AddObject(polyLine3D);


           
            Point3D pA = new Point3D();
            Point3D pB = new Point3D();
            Point3D pB2 = new Point3D();
            Point3D pC = new Point3D();

            pA = openModel.Point3D.First(a => a.Id == bearingMembers[0].ElementRAZ.line.Start.id);
            pB = openModel.Point3D.First(a => a.Id == bearingMembers[0].ElementRAZ.line.End.id);
            pB2 = openModel.Point3D.First(a => a.Id == bearingMembers[1].ElementRAZ.line.Start.id);
            pC = openModel.Point3D.First(a => a.Id == bearingMembers[1].ElementRAZ.line.End.id);

            List<Point3D> points = new List<Point3D>() { pA, pB, pB2, pC };

            Point3D pointA = pA; //Endpoint of first member
            Point3D pointB = pB; //Startpoint of first member
            Point3D pointC = pC; //Endpoint of second member


            LineSegment3D lineSegment1 = new LineSegment3D();
            lineSegment1.Id = openModel.GetMaxId(lineSegment1) + 1;
            openModel.AddObject(lineSegment1);
            lineSegment1.StartPoint = new ReferenceElement(pointA);
            lineSegment1.EndPoint = new ReferenceElement(pointB);
            polyLine3D.Segments.Add(new ReferenceElement(lineSegment1));

            SetLCS(bearingMembers[0], lineSegment1);

            LineSegment3D lineSegment2 = new LineSegment3D();
            lineSegment2.Id = openModel.GetMaxId(lineSegment2) + 1;
            openModel.AddObject(lineSegment2);
            lineSegment2.StartPoint = new ReferenceElement(pointB);
            lineSegment2.EndPoint = new ReferenceElement(pointC);
            polyLine3D.Segments.Add(new ReferenceElement(lineSegment2));
            
            SetLCS(bearingMembers[1], lineSegment2);

            //create elements
            Element1D el1 = new Element1D();
            //el1.Id = openModel.GetMaxId(el1) + 1; 

            el1.Id = bearingMembers[0].ElementRAZ.id + 1; //Use of Id from Grasshopper Model + Plus One

            el1.Name = "E" + el1.Id.ToString();
            el1.Segment = new ReferenceElement(lineSegment1);
            IdeaRS.OpenModel.CrossSection.CrossSection crossSection = openModel.CrossSection.First(a => a.Id == bearingMembers[0].ElementRAZ.crossSection.id);
            el1.CrossSectionBegin = new ReferenceElement(crossSection);
            el1.CrossSectionEnd = new ReferenceElement(crossSection);
            //el1.RotationRx = bearingMembers[0].ElementRAZ.rotationLCS;
            openModel.AddObject(el1);

            Element1D el2 = new Element1D();
            //el2.Id = openModel.GetMaxId(el2) + 1;

            el2.Id = bearingMembers[1].ElementRAZ.id + 1; //Use of Id from Grasshopper Model + Plus One

            el2.Name = "E" + el2.Id.ToString();
            el2.Segment = new ReferenceElement(lineSegment2);
            el2.CrossSectionBegin = new ReferenceElement(crossSection);
            el2.CrossSectionEnd = new ReferenceElement(crossSection);
            //el2.RotationRx = bearingMembers[1].ElementRAZ.rotationLCS;
            openModel.AddObject(el2);

            //create member
            Member1D member1D = new Member1D();
            member1D.Id = openModel.GetMaxId(member1D) + 1;
            member1D.Name = "Member" + member1D.Id.ToString();
            member1D.Elements1D.Add(new ReferenceElement(el1));
            member1D.Elements1D.Add(new ReferenceElement(el2));
            openModel.Member1D.Add(member1D);

			BeamData beamData = new BeamData
			{
				Id = member1D.Id,
				OriginalModelId = member1D.Id.ToString(),
				IsAdded = false,
				MirrorY = false,
				RefLineInCenterOfGravity = false,
			};
			openModel.Connections[0].Beams.Add(beamData);


			//create connected member
			ConnectedMember connectedMember = new ConnectedMember();
            connectedMember.Id = member1D.Id;
            connectedMember.MemberId = new ReferenceElement(member1D);
            connectionPoint.ConnectedMembers.Add(connectedMember);
        }
        private void AddConnectedMember(AttachedMember attachedMember, ConnectionPoint connectionPoint)
        {
            PolyLine3D polyLine3D = new PolyLine3D();
            polyLine3D.Id = openModel.GetMaxId(polyLine3D) + 1;
            openModel.AddObject(polyLine3D);

            //endpoints
            Point3D pA = openModel.Point3D.First(a => a.Id == attachedMember.ideaLine.Start.id);
            Point3D pB = openModel.Point3D.First(a => a.Id == attachedMember.ideaLine.End.id);

            //create line segment
            LineSegment3D lineSegment = new LineSegment3D();
            lineSegment.Id = openModel.GetMaxId(lineSegment) + 1;
            lineSegment.StartPoint = new ReferenceElement(pA);
            lineSegment.EndPoint = new ReferenceElement(pB);
            openModel.AddObject(lineSegment);
            polyLine3D.Segments.Add(new ReferenceElement(lineSegment));

            SetLCS(attachedMember, lineSegment);
            

            //create element
            Element1D element1D = new Element1D();
            //element1D.Id = openModel.GetMaxId(element1D) + 1;

            element1D.Id = attachedMember.ElementRAZ.id + 1; //Use of Id from Grasshopper Model + Plus One

            element1D.Name = "Element " + element1D.Id.ToString();
            element1D.Segment = new ReferenceElement(lineSegment);

            IdeaRS.OpenModel.CrossSection.CrossSection crossSection = openModel.CrossSection.First(a => a.Id == attachedMember.ElementRAZ.crossSection.id);
            element1D.CrossSectionBegin = new ReferenceElement(crossSection);
            element1D.CrossSectionEnd = new ReferenceElement(crossSection);
            //element1D.RotationRx = attachedMember.ElementRAZ.rotationLCS;

            if (attachedMember is ConnectingMember)
            {
                ConnectingMember connectingMember = attachedMember as ConnectingMember;
                double eccentricty = new double();
                if (connectingMember.localEccentricity == -0)
                {
                    eccentricty = 0;
                }
                else
                {
                    eccentricty = connectingMember.localEccentricity;
                }
                element1D.EccentricityBeginZ = eccentricty;
                element1D.EccentricityEndZ = eccentricty;

            }

            openModel.AddObject(element1D);

            //create member
            Member1D member1D = new Member1D();
            member1D.Id = openModel.GetMaxId(member1D) + 1;
            member1D.Name = "Member " + member1D.Id.ToString();
            member1D.Elements1D.Add(new ReferenceElement(element1D));
            openModel.Member1D.Add(member1D);

			IdeaRS.OpenModel.Connection.BeamData beam1Data = new IdeaRS.OpenModel.Connection.BeamData
			{
				Id = member1D.Id,
				OriginalModelId = member1D.Id.ToString(),
				IsAdded = false,
				MirrorY = false,
				RefLineInCenterOfGravity = false,
			};
			openModel.Connections[0].Beams.Add(beam1Data);


			//create connected member
			ConnectedMember connectedMember = new ConnectedMember();
            connectedMember.Id = member1D.Id;
            connectedMember.MemberId = new ReferenceElement(member1D);
			connectionPoint.ConnectedMembers.Add(connectedMember);

		}
		private void AddLoadCaseToOpenModel(KarambaIDEA.Core.LoadcaseRAZ _loadCaseRAZ)
        {
            LoadCase loadCase = new LoadCase();
            loadCase.Name = _loadCaseRAZ.name;
            loadCase.Id = _loadCaseRAZ.id;
            loadCase.Type = LoadCaseSubType.PermanentStandard;

            LoadGroupEC loadGroup = null;

            loadGroup = new LoadGroupEC();
            loadGroup.Id = _loadCaseRAZ.id;
            loadGroup.Name = "LG"+ _loadCaseRAZ.id;
            loadGroup.GammaQ = 1.5;
            loadGroup.Psi0 = 0.7;
            loadGroup.Psi1 = 0.5;
            loadGroup.Psi2 = 0.3;
            loadGroup.GammaGInf = 1.0;
            loadGroup.GammaGSup = 1.35;
            loadGroup.Dzeta = 0.85;
            openModel.AddObject(loadGroup);

            loadCase.LoadGroup = new ReferenceElement(loadGroup);


            openModel.AddObject(loadCase);

            CombiInputEC combi = new CombiInputEC();
            combi.Name = "CO" + _loadCaseRAZ.id;
            combi.TypeCombiEC = TypeOfCombiEC.ULS;
            combi.TypeCalculationCombi = TypeCalculationCombiEC.Linear;
            combi.Items = new List<CombiItem>();
            CombiItem it = new CombiItem();
            it.Id = 1;
            it.LoadCase = new ReferenceElement(loadCase);
            it.Coeff = 1.0;
            combi.Items.Add(it);
            openModel.AddObject(combi);
        }
        private void CreateIDEAOpenModelResults(Joint joint)
        {
            Project project = joint.project;
            openModelResult.ResultOnMembers = new List<ResultOnMembers>();
            ResultOnMembers resultIF = new ResultOnMembers();
            for (int ibeam = 0; ibeam < openModel.Member1D.Count; ibeam++)
            {
                //Continues Chord consist out of one member
                Member1D mb = openModel.Member1D[ibeam];
                //Loop is needed in case of a continuous member
                for (int iele = 0; iele < mb.Elements1D.Count; iele++)
                {
                    //Continouos chord consists out of two elements
                    Element1D elem = openModel.Element1D.First(a => a.Id == mb.Elements1D[iele].Id);//wordt hier de link met het verkeerde element gelegd?
                    //word de verkeerde id toegekent?

                    //results on members are constant in the framework
                    ResultOnMember resMember = new ResultOnMember(new Member() { Id = elem.Id, MemberType = MemberType.Element1D }, ResultType.InternalForces);
                    int numPoints = 1;
                    for (int ip = 0; ip <= numPoints; ip++)
                    {
                        ResultOnSection resSec = new ResultOnSection();
                        resSec.AbsoluteRelative = AbsoluteRelative.Relative;
                        resSec.Position = (double)ip / (double)numPoints;
                        //iterate over loadcases
                        int count = openModel.LoadCase.Count;
                        for (int i = 1; i <= count; i++)
                        {
                            ResultOfInternalForces resLoadCase = new ResultOfInternalForces();
                            int loadCaseNumber = i;
                            resLoadCase.Loading = new ResultOfLoading() { Id = loadCaseNumber, LoadingType = LoadingType.LoadCase };
                            resLoadCase.Loading.Items.Add(new ResultOfLoadingItem() { Coefficient = 1.0 });


                            //Check if Startpoint is equal to centerpoint
                            int GrassId = elem.Id - 1;//Element1D.Id - 1 == ElementRAZ.id
                            int GrassLCId = i - 1;//Loadcase grasshopper starts at 0, Loadcase IDEA starts at 1.

                            List<BearingMember> BM = joint.attachedMembers.OfType<BearingMember>().ToList();

                            //Find the element to check: isStartpoint true or false
                            AttachedMember attached = joint.attachedMembers.Find(a => a.ElementRAZ.id == GrassId);
                            if (attached.isStartPoint == true) //
                            {
                                SetStartLoads(1, joint, GrassLCId, GrassId, resLoadCase, resSec);
                                

                            }
                            else//isEndPoint
                            {
                                
                                if (attached is BearingMember && BM.Count == 2)
                                {
                                    SetEndLoads(1, joint, GrassLCId, GrassId, resLoadCase, resSec);
                                }
                                else
                                {
                                    SetEndLoads(-1, joint, GrassLCId, GrassId, resLoadCase, resSec);
                                }
                            }
                        }
                        resMember.Results.Add(resSec);
                    }
                    resultIF.Members.Add(resMember);

                }
            }
            openModelResult.ResultOnMembers.Add(resultIF);
        }

        public void SetStartLoads(int sign,Joint joint, int GrassLCId, int GrassId, ResultOfInternalForces resLoadCase, ResultOnSection resSec)
        {
            //Pick Startloads
            //API to IDEA UI, My, Vy and Vz are plotted negatively
            double N0 = 1000 * joint.project.loadcases[GrassLCId].loadsPerLineRAZs[GrassId].startLoads.N;
            double My0 = (-1) * 1000 * joint.project.loadcases[GrassLCId].loadsPerLineRAZs[GrassId].startLoads.My;
            double Vz0 = (-1) * 1000 * joint.project.loadcases[GrassLCId].loadsPerLineRAZs[GrassId].startLoads.Vz;
            double Vy0 = (-1) * 1000 * joint.project.loadcases[GrassLCId].loadsPerLineRAZs[GrassId].startLoads.Vy;
            double Mz0 = 1000 * joint.project.loadcases[GrassLCId].loadsPerLineRAZs[GrassId].startLoads.Mz;
            double Mt0 = 1000 * joint.project.loadcases[GrassLCId].loadsPerLineRAZs[GrassId].startLoads.Mt;

            //From Karamba3D to Framework
            double N =  N0  * sign;
            double My = My0 * sign;
            double Vz = Vz0 * sign;
            double Vy = Vy0 * sign;
            double Mz = Mz0 * sign;
            double Mt = Mt0 * sign;

            resLoadCase.N = N;
            resLoadCase.My = My;
            resLoadCase.Qz = Vz;
            resLoadCase.Qy = Vy;
            resLoadCase.Mz = Mz;
            resLoadCase.Mx = Mt;

            resSec.Results.Add(resLoadCase);
        }
        public void SetEndLoads(int sign, Joint joint, int GrassLCId, int GrassId, ResultOfInternalForces resLoadCase, ResultOnSection resSec)
        {
            //Pick EndLoads
            //API to IDEA UI, My, Vy and Vz are plotted negatively
            double N0 = 1000 * joint.project.loadcases[GrassLCId].loadsPerLineRAZs[GrassId].endLoads.N;
            double My0 = (-1) * 1000 * joint.project.loadcases[GrassLCId].loadsPerLineRAZs[GrassId].endLoads.My;
            double Vz0 = (-1) * 1000 * joint.project.loadcases[GrassLCId].loadsPerLineRAZs[GrassId].endLoads.Vz;
            double Vy0 = (-1) * 1000 * joint.project.loadcases[GrassLCId].loadsPerLineRAZs[GrassId].endLoads.Vy;
            double Mz0 = 1000 * joint.project.loadcases[GrassLCId].loadsPerLineRAZs[GrassId].endLoads.Mz;
            double Mt0 = 1000 * joint.project.loadcases[GrassLCId].loadsPerLineRAZs[GrassId].endLoads.Mt;

            //From Karamba3D to Framework
            double N = N0 * sign;
            double My = My0 * sign;
            double Vz = Vz0 * sign;
            double Vy = Vy0 * sign;
            double Mz = Mz0 * sign;
            double Mt = Mt0 * sign;

            resLoadCase.N = N;
            resLoadCase.My = My;
            resLoadCase.Qz = Vz;
            resLoadCase.Qy = Vy;
            resLoadCase.Mz = Mz;
            resLoadCase.Mx = Mt;

            resSec.Results.Add(resLoadCase);
        }

        public void SetLCS(AttachedMember attachedMember, LineSegment3D lineSegment)
        {
            //Defining LCS for First lineSegment
            double xcor = attachedMember.ElementRAZ.line.vector.X;
            double ycor = attachedMember.ElementRAZ.line.vector.Y;
            double zcor = attachedMember.ElementRAZ.line.vector.Z;

            

            //Define LCS (local-y in XY plane) and unitize
            VectorRAZ vx = new VectorRAZ(xcor, ycor, zcor).Unitize();
            VectorRAZ vy = new VectorRAZ();
            VectorRAZ vz = new VectorRAZ();
            if (xcor == 0.0 && ycor == 0.0)
            {
                vy = new VectorRAZ(0.0, 1.0, 0.0).Unitize();
                vz = new VectorRAZ((-zcor), 0.0, (xcor)).Unitize();
            }
            else
            {
                vy = new VectorRAZ(-ycor, xcor, 0.0).Unitize();
                vz = new VectorRAZ((-zcor * xcor), (-zcor * ycor), ((xcor * xcor) + (ycor * ycor))).Unitize();
            }

            if (attachedMember.ElementRAZ.rotationLCS == 0.0)
            {

            }
            else
            {
                vy = VectorRAZ.RotateVector(vx, attachedMember.ElementRAZ.rotationLCS, vy);
                vz = VectorRAZ.RotateVector(vx, attachedMember.ElementRAZ.rotationLCS, vz);
            }

            var LocalCoordinateSystem = new CoordSystemByVector();
            LocalCoordinateSystem.VecX = new Vector3D() { X = vx.X, Y = vx.Y, Z = vx.Z };
            LocalCoordinateSystem.VecY = new Vector3D() { X = vy.X, Y = vy.Y, Z = vy.Z };
            LocalCoordinateSystem.VecZ = new Vector3D() { X = vz.X, Y = vz.Y, Z = vz.Z };

            lineSegment.LocalCoordinateSystem = LocalCoordinateSystem;
        }
        public void SetLCSwithRotation(AttachedMember attachedMember, LineSegment3D lineSegment)
        {
            //Explode x-vector
            double xcor = attachedMember.ElementRAZ.line.vector.X;
            double ycor = attachedMember.ElementRAZ.line.vector.Y;
            double zcor = attachedMember.ElementRAZ.line.vector.Z;

            VectorRAZ vx = new VectorRAZ(xcor, ycor, zcor).Unitize();

            double rotation = attachedMember.ElementRAZ.rotationLCS;

            //Explode z-vector
            double xcorZ = 0.0;
            double ycorZ = 0.0;
            double zcorZ = 0.0;

            VectorRAZ vz = new VectorRAZ(xcorZ, ycorZ, zcorZ).Unitize();

            //Create y-vector with cross-product
            VectorRAZ vy = new VectorRAZ();
            vz = new VectorRAZ((vz.Y*vx.Z-vx.Y*vz.Z), (-vz.X*vx.Z+vx.X*vz.Z), (vz.X*vx.Y-vx.X*vz.Y)).Unitize();
            
            //Set LCS
            var LocalCoordinateSystem = new CoordSystemByVector();
            LocalCoordinateSystem.VecX = new Vector3D() { X = vx.X, Y = vx.Y, Z = vx.Z };
            LocalCoordinateSystem.VecY = new Vector3D() { X = vy.X, Y = vy.Y, Z = vy.Z };
            LocalCoordinateSystem.VecZ = new Vector3D() { X = vz.X, Y = vz.Y, Z = vz.Z };

            lineSegment.LocalCoordinateSystem = LocalCoordinateSystem;
        }
    }
}