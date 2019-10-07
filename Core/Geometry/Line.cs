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
        public Point Start;
        public Point End;
        //public Vector vectorq;
        public Vector vector
        {
            get
            {
                return new Vector(Start, End);
            }
        }


        public Line()
        {

        }

        public Line(int _id, Point _Start, Point _End)
        {
            this.id = _id;
            this.Start = _Start;
            this.End = _End;

        }
        public Line(Point _Start, Point _End)
        {
           
            this.Start = _Start;
            this.End = _End;

        }

        public static Line TranslateLineWithVector(Project _project, Line line, Vector translation)
        {
            Point newStart = Point.CreateNewOrExisting(_project,line.Start.X + translation.X, line.Start.Y + translation.Y, line.Start.Z + translation.Z);
            Point newEnd = Point.CreateNewOrExisting(_project,line.End.X + translation.X, line.End.Y + translation.Y, line.End.Z + translation.Z);
            Line result = new Line(line.id, newStart, newEnd);
            return result;
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
            if (Point.ArePointsEqual(tol, point, line.Start) == true)
            {
                return line;
            }
            else
            {
                return new Line(line.id, line.End, line.Start);
            }
        }

        public static Line FlipLine(Line line)
        {
            return new Line(line.id, line.End, line.Start);
        }


        public static int ShouldEccentricityBeAssumedPOSOrNEG(double tol, Point point, Line line)
        {
            if (Point.ArePointsEqual(tol, point, line.End) == true)
            {
                Line lijn2 = Line.FlipLineIfPointNotEqualStartPoint(tol, point, line);
                if (lijn2.vector.Z > 0)
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
                if (line.vector.Z > 0)
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
            Point start = Point.MovePointToOrigin(centerpoint, line.Start);
            Point end = Point.MovePointToOrigin(centerpoint, line.End);
            Line newline = new Line(start, end);
            return newline;
        }
    }
}
