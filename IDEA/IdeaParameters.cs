using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdeaRS.OpenModel;

namespace KarambaIDEA
{

    public class IdeaParameter<T> where T : IdeaParameterValue
    {

        //IdeaRS.OpenModel.Parameters.WeldParam



    }

    public class IdeaParameterValue
    {
        public IdeaParameterValue() { }
    }





    public class Parameters
    {
        public Parameter[] Property1 { get; set; }
    }

    public class Parameter
    {
        public int id { get; set; }
        public string identifier { get; set; }
        public string description { get; set; }
        public string parameterType { get; set; }
        public ParameterValue value { get; set; }
    }

    public class ParameterValue { }

    public class AnchorValue : ParameterValue
    {
        public float length { get; set; }
        public int anchorTypeData { get; set; }
        public float anchorTypeSize { get; set; }
        public int coordinateSystem { get; set; }
        public object positions { get; set; }
        public Row[][] rows { get; set; }
        public Col[][] cols { get; set; }
        public object rowsNegative { get; set; }
        public object colsNegative { get; set; }
        public int rowsSymmetry { get; set; }
        public int colsSymmetry { get; set; }
        public int rowsPosition { get; set; }
        public int colsPosition { get; set; }
        public int rowsGridType { get; set; }
        public int colsGridType { get; set; }
        public int polarInput { get; set; }
        public int[] counts { get; set; }
        public Radius[][] radii { get; set; }
        public Angle[][] angles { get; set; }
        public int polarPosition { get; set; }
        public bool shearInThread { get; set; }
        public int boltInteraction { get; set; }
        public string name { get; set; }
        public string tableId { get; set; }
        public string itemId { get; set; }
    }

    public class Row
    {
        public float value { get; set; }
        public int count { get; set; }
    }

    public class Col
    {
        public float value { get; set; }
        public int count { get; set; }
    }

    public class Radius
    {
        public float value { get; set; }
        public int count { get; set; }
    }

    public class Angle
    {
        public float value { get; set; }
        public int count { get; set; }
    }


    public class WeldValue : ParameterValue
    {
        public int weldType { get; set; }
        public string weldMaterialName { get; set; }
        public float size { get; set; }
        public float beginOffset { get; set; }
        public float endOffset { get; set; }
        public float intermittentLength { get; set; }
        public float intermittentGap { get; set; }
    }

    public class StringValue : ParameterValue
    {
        public string weldType { get; set; }
        public string weldMaterialName { get; set; }
        public float size { get; set; }
        public float beginOffset { get; set; }
        public float endOffset { get; set; }
        public float intermittentLength { get; set; }
        public float intermittentGap { get; set; }
    }
}
