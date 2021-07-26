// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System.Collections.Generic;


namespace KarambaIDEA.Core
{
    public class ImportGrasshopperUtils
    {
        /// <summary>
        /// When strings are imported from Grasshopper, they may contain "\r\n". This method cleans strings that contain "\r\n" at their end
        /// </summary>
        /// <param name="list">list of strings imported from Grasshopper</param>
        /// <returns></returns>
        static public List<string> DeleteEnterCommandsInGHStrings(List<string> list)
        {
            List<string> newlist = new List<string>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].EndsWith(" \r\n"))
                {
                    list[i] = list[i].Replace(" \r\n", "");
                    list[i] = list[i].TrimEnd();
                    newlist.Add(list[i]);
                }
                else
                {
                    list[i] = list[i].TrimEnd();
                    newlist.Add(list[i]);
                }
            }
            return newlist;
        }

        static public Rhino.Geometry.Point3d CastPointToRhino(Core.Point point)
        {
            Rhino.Geometry.Point3d rhiPoint = new Rhino.Geometry.Point3d();
            rhiPoint.X = point.X;
            rhiPoint.Y = point.Y;
            rhiPoint.Z = point.Z;
            return rhiPoint;
        }

        static public Rhino.Geometry.Line CastLineToRhino(Core.Line line)
        {
            Rhino.Geometry.Line rhiLine = new Rhino.Geometry.Line();
            rhiLine.From = CastPointToRhino(line.start);
            rhiLine.To = CastPointToRhino(line.end);
            return rhiLine;
        }
        static public Rhino.Geometry.Vector3d CastVectorToRhino(Core.Vector vector)
        {
            Rhino.Geometry.Vector3d rhiVec = new Rhino.Geometry.Vector3d();
            rhiVec.X = vector.X;
            rhiVec.Y = vector.Y;
            rhiVec.Z = vector.Z;
            return rhiVec;
        }

        static public Core.Vector CastVectorToCore(Rhino.Geometry.Vector3d vec)
        {
            return new Vector(vec.X, vec.Y, vec.Z);
        }
    }
}
