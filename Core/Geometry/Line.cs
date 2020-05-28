// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.Core
{
    public class Line
    {
        public int id;
        public Point start;
        public Point end;

        public Vector Vector
        {
            get
            {
                return new Vector(start, end);
            }
        }
        public double Length
        {
            get
            {
                return LengthLine(this); 
            }
        }
        public Line()
        {

        }

        public Line(int _id, Point _Start, Point _End)
        {
            this.id = _id;
            this.start = _Start;
            this.end = _End;

        }
        public Line(Point _Start, Point _End)
        {
           
            this.start = _Start;
            this.end = _End;

        }

        public static Line TranslateLineWithVector(Project _project, Line line, Vector translation)
        {
            Point newStart = Point.CreateNewOrExisting(_project,line.start.X + translation.X, line.start.Y + translation.Y, line.start.Z + translation.Z);
            Point newEnd = Point.CreateNewOrExisting(_project,line.end.X + translation.X, line.end.Y + translation.Y, line.end.Z + translation.Z);
            Line result = new Line(line.id, newStart, newEnd);
            return result;
        }
        public static Line FlipLine(Line line)
        {
            return new Line(line.id, line.end, line.start);
        }
        /// <summary>
        /// In this method the startpoint of the line is compared to the point. IF ther are equal the original line is returned. 
        /// If not the line will be flipped. Where startpoint and endpoint
        /// </summary>
        /// <param name="tol"></param>
        /// <param name="point"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static Line FlipLineIfPointNotEqualStartPoint(double tol, Point point, Line line)
        {
            if (Point.ArePointsEqual(tol, point, line.start) == true)
            {
                return line;
            }
            else
            {
                return new Line(line.id, line.end, line.start);
            }
        }
        public static int ShouldEccentricityBeAssumedPOSOrNEG(double tol, Point point, Line line)
        {
            if (Point.ArePointsEqual(tol, point, line.end) == true)
            {
                Line lijn2 = Line.FlipLineIfPointNotEqualStartPoint(tol, point, line);
                if (lijn2.Vector.Z > 0)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }

            }
            else
            {
                if (line.Vector.Z > 0)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }
        /// <summary>
        /// Move line to 0,0,0 based on given point
        /// </summary>
        /// <param name="point">centerpoint of joint</param>
        /// <param name="line">line to move</param>
        /// <returns></returns>
        public static Line MoveLineToOrigin(Point centerpoint,Line line)
        {
            Point start = Point.MovePointToOrigin(centerpoint, line.start);
            Point end = Point.MovePointToOrigin(centerpoint, line.end);
            Line newline = new Line(start, end);
            return newline;
        }
        public static double LengthLine(Line line)
        {
            Point st = line.start;
            Point end = line.end;
            double length = Math.Pow(end.X - st.X, 2) + Math.Pow(end.Y - st.Y, 2) + Math.Pow(end.Z - st.Z, 2);
            return Math.Sqrt(length);
        }
        public static List<Line> SplitLine(Line line, double length)
        {
            List<Line> lines = new List<Line>();
            if (line.Length > length)
            {
                
                Vector dir = line.Vector.Unitize();
                Point midPoint = Point.MovePointByVectorandLength(line.start, dir, length);
                lines.Add(new Line(line.start, midPoint));
                lines.Add(new Line(midPoint, line.end));
                
            }
            else
            {
                //NotImplementedException.();
            }
            return lines;
        }
        public static Point ExtendLine(Line line, double length, bool atStart)
        {
            Point point = new Point();
            if (atStart == true)
            {
                point = Point.MovePointByVectorandLength(line.start, line.Vector.FlipVector(), length);
            }
            else
            {
                point = Point.MovePointByVectorandLength(line.end, line.Vector, length);
            }
            return point;
        }
        public static void ModifyColumnBrepLine(List<BearingMember> bearlist, double maxBeamHeight)
        {
            BearingMember column = bearlist.First();
            if (bearlist.Count == 1)
            {
                if (column.isStartPoint == true)
                {

                    column.element.brepLine.start = KarambaIDEA.Core.Line.ExtendLine(column.element.Line, maxBeamHeight / 2000, true);
                }
                else
                {
                    column.element.brepLine.end = KarambaIDEA.Core.Line.ExtendLine(column.element.Line, maxBeamHeight / 2000, false);
                }
            }
        }
        public static void ModifyBeamBrepLine(BearingMember column, ConnectingMember con, double gap)
        {
            double length = column.element.crossSection.height / 2000 + gap / 1000;
            if (con.isStartPoint == true)
            {
                con.element.brepLine.start = KarambaIDEA.Core.Line.ExtendLine(con.element.Line, -length, true);
            }
            else
            {
                con.element.brepLine.end = KarambaIDEA.Core.Line.ExtendLine(con.element.Line, -length, false);
            }
        }
    }
}
