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
