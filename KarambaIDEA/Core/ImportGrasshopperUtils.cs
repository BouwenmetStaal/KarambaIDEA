// Copyright (c) 2019 Rayaan Ajouz. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System.Collections.Generic;


namespace KarambaIDEA.Core
{
    public class ImportGrasshopperUtils
    {
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

        
    }
}
