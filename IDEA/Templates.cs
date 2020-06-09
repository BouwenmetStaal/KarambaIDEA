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

namespace KarambaIDEA.IDEA
{
    public class Templates
    {

        /// <summary>
        /// Predefined programmed idea templates can be added to the OpenModel
        /// </summary>
        /// <param name="openModel">open model</param>
        /// <param name="joint">joint instance</param>
        public static void ApplyProgrammedIDEAtemplate(OpenModel openModel, Joint joint)
        {
            //TODOL add templatefile location
            if (joint.template.workshopOperations == Template.WorkshopOperations.NoOperation||joint.template.workshopOperations == Template.WorkshopOperations.TemplateByFile)
            {

            }
            if (joint.template.workshopOperations == Template.WorkshopOperations.BoltedEndPlateConnection)
            {
                if (joint.template.plates.First() != null)
                {
                    double platethickness = joint.template.plates[0].thickness / 1000;
                    Templates.BoltedEndplateConnection(openModel, joint, platethickness);
                }

            }
            if (joint.template.workshopOperations == Template.WorkshopOperations.WeldAllMembers)
            {
                Templates.WeldAllMembers(openModel);
            }
        }

        #region: combined commands
        static public OpenModel BoltedEndplateConnection(OpenModel openModel, Joint joint, double tplate)
        {
            double w0 = joint.attachedMembers[0].element.crossSection.width / 1000;
            double h0 = joint.attachedMembers[0].element.crossSection.height / 1000;
            double w1 = joint.attachedMembers[1].element.crossSection.width / 1000;
            double h1 = joint.attachedMembers[1].element.crossSection.height / 1000;

            CreatePlateForBeam(openModel, joint, 0, w0, h0, tplate);
            CreatePlateForBeam(openModel, joint, 1, w1, h1, tplate);



            CutBeamByPlate(openModel, joint, 0, 0);
            CutBeamByPlate(openModel, joint, 1, 1);
            CreateBoltgrid(openModel, 0, 1, w0 - 0.1, h0 - 0.1);

            return openModel;
        }

        static public OpenModel WeldAllMembers(OpenModel openModel)
        {
            
            for (int i = 1; i < openModel.Connections[0].Beams.Count; i++)
            {
                CutBeamByBeam(openModel, 0, i);
                if (i > 1)
                {
                    CutBeamByBeam(openModel, 1, i);
                }
            }
            return openModel;
        }
        #endregion

        
        #region: primitive commands
        static public OpenModel CutBeamByBeam(OpenModel openModel, int cuttingobject, int modifiedObject)
        {

            // add cut
            if (openModel.Connections[0].CutBeamByBeams == null)
            {
                openModel.Connections[0].CutBeamByBeams = new List<IdeaRS.OpenModel.Connection.CutBeamByBeamData>();
            }
            openModel.Connections[0].CutBeamByBeams.Add(new IdeaRS.OpenModel.Connection.CutBeamByBeamData
            {

                CuttingObject = new ReferenceElement(openModel.Connections[0].Beams[cuttingobject]),
                ModifiedObject = new ReferenceElement(openModel.Connections[0].Beams[modifiedObject]),
                IsWeld = true,
            });
            return openModel;
        }
        static public OpenModel CutBeamByPlate(OpenModel openModel, Joint joint, int cuttingobject, int modifiedObject)
        {


            // add cut
            if (openModel.Connections[0].CutBeamByBeams == null)
            {
                openModel.Connections[0].CutBeamByBeams = new List<IdeaRS.OpenModel.Connection.CutBeamByBeamData>();

            }
            openModel.Connections[0].CutBeamByBeams.Add(new IdeaRS.OpenModel.Connection.CutBeamByBeamData
            {

                CuttingObject = new ReferenceElement(openModel.Connections[0].Plates[cuttingobject]),
                ModifiedObject = new ReferenceElement(openModel.Connections[0].Beams[modifiedObject]),
                IsWeld = true,
            });

            return openModel;
        }
        
        static public OpenModel CreatePlateForBeam(OpenModel openModel, Joint joint, int refBeam, double width, double height, double tplate)
        {
            joint.template.plates.Add(new Plate("Kopplaat", width, height, tplate));

            double w = 0.5 * width;
            double h = 0.5 * height;
            List<string> stringlist = new List<string>();
            stringlist.Add(Moveto(-w, -h));
            stringlist.Add(Lineto(w, -h));
            stringlist.Add(Lineto(w, h));
            stringlist.Add(Lineto(-w, h));
            stringlist.Add(Lineto(-w, -h));

            string region = Combine(stringlist);
            //thickness


            //string region = "M "+-w+" 0 L " + width + " 0 L " + width + " " + height + " L 0 " + height + " L 0 0";
            //geometry of plate descript by SVG path https://www.w3.org/TR/SVG/paths.html
            //helpfull online SVG generator https://mavo.io/demos/svgpath/
            region = region.Replace(",", ".");
            int pointId = openModel.ConnectionPoint[0].Id;
            Point3D point = openModel.Point3D.First(a => a.Id == pointId);

            if (openModel.Connections[0].Plates == null)
            {
                openModel.Connections[0].Plates = new List<IdeaRS.OpenModel.Connection.PlateData>();

            }
            int number = 1 + openModel.Connections[0].Plates.Count;
            BeamData beam = openModel.Connections[0].Beams[refBeam];
            Element ele = joint.project.elements.First(a => a.id == (beam.Id-1));
            AttachedMember at = joint.attachedMembers.First(a => a.element.id == (beam.Id - 1));

            
            CoordSystem coor = openModel.LineSegment3D[refBeam].LocalCoordinateSystem;//based on integer of linesegments not of plates
            var LocalCoordinateSystem = new CoordSystemByVector();

            double distanceXloc = tplate * 0.5;
            if (at.isStartPoint == false)
            {
                distanceXloc = -distanceXloc;
            }
            
            //TODO: make definition that creates plate based on the LCS of the reference beam
            Point movedPoint = Point.MovePointByVectorandLength(joint.centralNodeOfJoint, ele.Line.Vector, distanceXloc);

            openModel.Connections[0].Plates.Add(new IdeaRS.OpenModel.Connection.PlateData
            {
                Name = "P"+number,
                Thickness = tplate,
                
                
                Id = number,
                //openModel.GetMaxId(openModel.GetMaxId(PlateData ) +1), //unique identificator
                //openModel.GetMaxId(lineSegment2) + 1
                Material = openModel.MatSteel.First().Name,
                OriginalModelId = number.ToString(),//inique identificator from original model [string]
                Origin = new IdeaRS.OpenModel.Geometry3D.Point3D
                {
                    //TODO: include movement based on local x-axis. However, not yet available in current API
                    X =  movedPoint.X,
                    Y = movedPoint.Y,
                    Z = movedPoint.Z
                },
                //Y-axis is normal to plane of plate. 
                //Therefore, 
                //axixX = localY
                //axixY = localZ
                //axixZ = localX
                AxisX = new IdeaRS.OpenModel.Geometry3D.Vector3D
                {
                    X = ele.localCoordinateSystem.Y.X,
                    Y = ele.localCoordinateSystem.Y.Y,
                    Z = ele.localCoordinateSystem.Y.Z
                    
                },
                AxisY = new IdeaRS.OpenModel.Geometry3D.Vector3D
                {
                    X = ele.localCoordinateSystem.Z.X,
                    Y = ele.localCoordinateSystem.Z.Y,
                    Z = ele.localCoordinateSystem.Z.Z
                   
                },
                AxisZ = new IdeaRS.OpenModel.Geometry3D.Vector3D
                {
                    X = ele.localCoordinateSystem.X.X,
                    Y = ele.localCoordinateSystem.X.Y,
                    Z = ele.localCoordinateSystem.X.Z
                },
                Region = region ,
            });

            //(openModel.Connections[0].Plates ?? (openModel.Connections[0].Plates = new List<IdeaRS.OpenModel.Connection.PlateData>())).Add(plateData);


            return openModel;
        }
        static public OpenModel CreatePlate(OpenModel openModel, double height, double width, double moveX)
        {

            string region = "M 0 0 L " + width + " 0 L " + width + " " + height + " L 0 " + height + " L 0 0"; //geometry of plate descript by SVG path https://www.w3.org/TR/SVG/paths.html
            region = region.Replace(",", ".");
            int pointId = openModel.ConnectionPoint[0].Id;
            Point3D point = openModel.Point3D.First(a => a.Id == pointId);

            if (openModel.Connections[0].Plates == null)
            {
                openModel.Connections[0].Plates = new List<IdeaRS.OpenModel.Connection.PlateData>();

            }
            int number = 1 + openModel.Connections[0].Plates.Count;


            openModel.Connections[0].Plates.Add(new IdeaRS.OpenModel.Connection.PlateData
            {
                Name = "P" + number,
                Thickness = 0.02,
                Id = number,
                Material = openModel.MatSteel.First().Name,
                OriginalModelId = number.ToString(),//inique identificator from original model [string]
                Origin = new IdeaRS.OpenModel.Geometry3D.Point3D
                {

                    X = point.X + moveX,
                    Y = point.Y - 0.5 * width,
                    Z = point.Z - 0.5 * height
                },
                AxisX = new IdeaRS.OpenModel.Geometry3D.Vector3D
                {
                    X = 0,
                    Y = 1,
                    Z = 0
                },
                AxisY = new IdeaRS.OpenModel.Geometry3D.Vector3D
                {
                    X = 0,
                    Y = 0,
                    Z = 1
                },
                AxisZ = new IdeaRS.OpenModel.Geometry3D.Vector3D
                {
                    X = 1,
                    Y = 0,
                    Z = 0
                },
                Region = region,
            });
            return openModel;
        }
        static public OpenModel CreateBoltgrid(OpenModel openModel, int firstPlate, int secondPlate, double width, double height)
        {
            PlateData plateData = openModel.Connections[0].Plates[firstPlate];
            PlateData plateTwo = openModel.Connections[0].Plates[secondPlate];

            double w = 0.5 * width;
            double h = 0.5 * height;

            int pointId = openModel.ConnectionPoint[0].Id;
            Point3D point = openModel.Point3D.First(a => a.Id == pointId);


            if (openModel.Connections[0].BoltGrids == null)
            {
                openModel.Connections[0].BoltGrids = new List<IdeaRS.OpenModel.Connection.BoltGrid>();

            }
            int number = openModel.Connections[0].BoltGrids.Count;

            //openModel.Connections[0].Plates.Add(new IdeaRS.OpenModel.Connection.PlateData
            IdeaRS.OpenModel.Connection.BoltGrid boltGrid = new IdeaRS.OpenModel.Connection.BoltGrid()
            {
                Id = number+1,
                ConnectedPartIds = new List<string>(),
                Diameter = 0.016,
                HeadDiameter = 0.024,
                DiagonalHeadDiameter = 0.026,
                HeadHeight = 0.01,
                BoreHole = 0.018,
                TensileStressArea = 157,
                NutThickness = 0.013,
                AnchorLen = 0.05,
                Material = "8.8",
                Standard = "M 16",
            };

            boltGrid.Origin = new IdeaRS.OpenModel.Geometry3D.Point3D() { X = plateData.Origin.X, Y = plateData.Origin.Y, Z = plateData.Origin.Z };
            boltGrid.AxisX = new IdeaRS.OpenModel.Geometry3D.Vector3D() { X = plateData.AxisX.X, Y = plateData.AxisX.Y, Z = plateData.AxisX.Z };
            boltGrid.AxisY = new IdeaRS.OpenModel.Geometry3D.Vector3D() { X = plateData.AxisY.X, Y = plateData.AxisY.Y, Z = plateData.AxisY.Z };
            boltGrid.AxisZ = new IdeaRS.OpenModel.Geometry3D.Vector3D() { X = plateData.AxisZ.X, Y = plateData.AxisZ.Y, Z = plateData.AxisZ.Z };
            boltGrid.Positions = new List<IdeaRS.OpenModel.Geometry3D.Point3D>
            {
                new IdeaRS.OpenModel.Geometry3D.Point3D()
                {
                    X = point.X,
                    Y = point.Y-w,
                    Z = point.Z-h
                },
                new IdeaRS.OpenModel.Geometry3D.Point3D()
                {
                    X = point.X,
                    Y = point.Y+w,
                    Z = point.Z-h
                },
                new IdeaRS.OpenModel.Geometry3D.Point3D()
                {
                    X = point.X,
                    Y = point.Y-w,
                    Z = point.Z+h
                },
                new IdeaRS.OpenModel.Geometry3D.Point3D()
                {
                    X = point.X,
                    Y = point.Y+w,
                    Z = point.Z+h
                }
            };

            boltGrid.ConnectedPartIds = new List<string>() { plateTwo.OriginalModelId, plateData.OriginalModelId };

            openModel.Connections[0].BoltGrids.Add(boltGrid);

            //(openModel.Connections[0].BoltGrids ?? (openModel.Connections[0].BoltGrids = new List<IdeaRS.OpenModel.Connection.BoltGrid>())).Add(boltGrid);
            return openModel;
        }
        #endregion

        #region: SVG path commands
        /// <summary>
        /// SVG path code absolute move
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public string Moveto(double x, double y)
        {
            return string.Format("M {0} {1}", x, y);
        }
        /// <summary>
        /// SVG path code absolute line
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public string Lineto(double x, double y)
        {
            return string.Format("L {0} {1}", x, y);
        }
        static public string Combine(List<string> list)
        {
            return String.Join(" ", list.ToArray());
        }
        #endregion


    }
}
