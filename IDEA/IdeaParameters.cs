using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdeaRS.OpenModel;

using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace KarambaIDEA.IDEA.Parameters
{
    public class parameter : ICloneable
    {
        //This class is for the serialisation to JSON. 
        //Each Parameter Type holds this base parameter.  

        public int id { get; set; }
        public string identifier { get; set; }
        public string description { get; set; }
        public string parameterType { get; set; }
        public object value { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public object Clone()
        {
            return new parameter() { id = this.id, identifier = this.identifier, description = this.description, parameterType = this.parameterType, value = this.value };
        }
    }

    public interface IIdeaParameter : ICloneable
    {
    }

    public abstract class IdeaParameter<T> : IIdeaParameter
    {
        protected parameter _parameter;

        public IdeaParameter(parameter parameter) 
        {
            _parameter = parameter;
        }

        public object Clone()
        {
            return ((ICloneable)_parameter).Clone();
        }

        public abstract T GetValue();

        public void SetValue(T value)
        {
            _parameter.value = value.ToString();
        }

        public override string ToString()
        {
            return _parameter.ToString();
        }
    }

    public class IdeaParameterInt : IdeaParameter<int>
    {
        public IdeaParameterInt(parameter parameter) : base (parameter) { }

        public override int GetValue()
        {
            return int.Parse((string)_parameter.value);
        }
    }

    public class IdeaParameterFloat : IdeaParameter<double> 
    {
        public IdeaParameterFloat(parameter parameter) : base(parameter) { }

        public override double GetValue()
        {
            return double.Parse((string)_parameter.value);
        }
    }

    public class IdeaParameterString : IdeaParameter<string> 
    {
        public IdeaParameterString(parameter parameter) : base(parameter) { }

        public override string GetValue()
        {
            throw new NotImplementedException();
        }
    }

    public class IdeaParameterWeld : IdeaParameter<string>
    {
        public IdeaParameterWeld(parameter parameter) : base(parameter) { }

        public override string GetValue()
        {
            throw new NotImplementedException();
        }
    }

    public class IdeaParameterEnum : IdeaParameter<int>
    {
        public IdeaParameterEnum(parameter parameter) : base(parameter) { }

        public override int GetValue()
        {
            throw new NotImplementedException();
        }
    }

    public static class IdeaParameterFactory
    {
        public static IIdeaParameter Create(parameter parameter)
        {
            if (parameter.parameterType == "Enum")
                return new IdeaParameterEnum(parameter);
            else if (parameter.parameterType == "Float")
                return new IdeaParameterFloat(parameter);
            else if (parameter.parameterType == "Int")
                return new IdeaParameterInt(parameter);
            else
                return new IdeaParameterString(parameter);
        }
    }

    public static class ParameterSerialization
    {
        public static IIdeaParameter[] parametersFromTemplateXML(string templatePath)
        {
            //Read Template file.
            XDocument xdoc = XDocument.Load(templatePath);

            var parameters = xdoc.Descendants("Parameters");

            return new IIdeaParameter[] { };
        }

        public static IIdeaParameter[] parametersFromJSON(string parametersJSON)
        {
            //To do

            return new IIdeaParameter[] { };
        }
    }



    //Anchor parameter - To Do
    public class Anchor
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

    //Weld parameter - To Do
    public class Weld
    {
        public int weldType { get; set; }
        public string weldMaterialName { get; set; }
        public float size { get; set; }
        public float beginOffset { get; set; }
        public float endOffset { get; set; }
        public float intermittentLength { get; set; }
        public float intermittentGap { get; set; }
    }

}
