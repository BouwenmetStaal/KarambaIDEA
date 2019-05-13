using KarambaIDEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.IDEA
{
    public static class extention
    {
        public static BearingMember SecondOrDefault(this List<BearingMember> source)
        {
            if (source.Count() >= 1) { return source[1]; } else { return null; }
        }
        ////BearingMember b = bearingMembers.FirstOrDefault(x => x.ElementRAZ.id == bearingMembers.Min(y => y.ElementRAZ.id));
        //List<BearingMember> b = bearingMembers.OrderBy(x => x.ElementRAZ.id).ToList();
        //b.SecondOrDefault();
        //    for(int i=0;i<b.Count();i++)
        //    {
                
        //        BearingMember bb = b[i];
        //b.Add(bb);

            }
}

