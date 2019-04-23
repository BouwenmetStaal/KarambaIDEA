using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.Core
{
    public class LineRAZ
    {
        public int id;
        public PointRAZ Start;
        public PointRAZ End;
        //public VectorRAZ vectorq;
        public VectorRAZ vector
        {
            get
            {
                return new VectorRAZ(Start, End);
            }
        }


        public LineRAZ()
        {

        }

        public LineRAZ(int _id, PointRAZ _Start, PointRAZ _End)
        {
            this.id = _id;
            this.Start = _Start;
            this.End = _End;

        }

        public static LineRAZ TranslateLineWithVector(Project _project, LineRAZ line, VectorRAZ translation)
        {
            PointRAZ newStart = PointRAZ.CreateNewOrExisting(_project,line.Start.X + translation.X, line.Start.Y + translation.Y, line.Start.Z + translation.Z);
            PointRAZ newEnd = PointRAZ.CreateNewOrExisting(_project,line.End.X + translation.X, line.End.Y + translation.Y, line.End.Z + translation.Z);
            LineRAZ result = new LineRAZ(line.id, newStart, newEnd);
            return result;
        }

        /// <summary>
        /// In this method the startpoint of the line is compared to the point. IF ther are equal the original line is returned. 
        /// If not the line will be flipped. Where startpoint and endpoint
        /// </summary>
        /// <param name="tol"></param>
        /// <param name="punt"></param>
        /// <param name="lijn"></param>
        /// <returns></returns>
        public static LineRAZ FlipLineIfPointNotEqualStartPoint(double tol, PointRAZ punt, LineRAZ lijn)
        {
            if (PointRAZ.ArePointsEqual(tol, punt, lijn.Start) == true)
            {
                return lijn;
            }
            else
            {
                return new LineRAZ(lijn.id, lijn.End, lijn.Start);
            }
        }

        public static LineRAZ FlipLine(LineRAZ lijn)
        {
            return new LineRAZ(lijn.id, lijn.End, lijn.Start);
        }


        public static int ShouldEccentricityBeAssumedPOSOrNEG(double tol, PointRAZ punt, LineRAZ lijn)
        {
            if (PointRAZ.ArePointsEqual(tol, punt, lijn.End) == true)
            {
                LineRAZ lijn2 = LineRAZ.FlipLineIfPointNotEqualStartPoint(tol, punt, lijn);
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
                if (lijn.vector.Z > 0)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }
    }
}
