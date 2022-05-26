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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;


using KarambaIDEA.Core;
using KarambaIDEA.IDEA.Parameters;
using KarambaIDEA.Core.JointTemplate;

namespace KarambaIDEA.IDEA
{
    public class IdeaTemplate : Template
    {
        private readonly string _name = "";
        private readonly string _filepath = "";
        private Dictionary<int, IIdeaParameter> _parameterCollection;

        //may want to desiaralise more info in the future.

        internal bool IsLoaded = false;


        public IdeaTemplate() { }
        public IdeaTemplate(string filepath)
        {
            _filepath = filepath;
        }

        public string FilePath { get { return _filepath; } }

        public override string ToString()
        {
            return "Joint Template: " + _filepath;
        }

        public void ReadTemplateFile()
        {
            using (Stream reader = new FileStream(_filepath, FileMode.Open))
            {
                using (XmlReader xmlReader = XmlReader.Create(reader))
                {
                    xmlReader.ReadToDescendant("Parameters");
                    _parameterCollection = new Dictionary<int, IIdeaParameter>();
                    XmlKeyValueListHelper.ReadKeyValueXml(xmlReader, _parameterCollection);
                }
            }
            IsLoaded = true;
        }

        public List<IIdeaParameter> GetParameters()
        {
            if (!IsLoaded)
                ReadTemplateFile();

            return _parameterCollection.Select(x => x.Value).ToList();
        }
    }

    public abstract class TemplateAssign
    {
        public Template Template = null;

        public TemplateAssign(Template template)
        {
            Template = template;
        }
    }

    public class IdeaTemplateAssignFull : TemplateAssign
    {
        public IdeaModifyConnectionParameters ParamModify;
        public IdeaTemplateAssignFull(IdeaTemplate template, IdeaModifyConnectionParameters paramModify) : base(template)
        {
            ParamModify = paramModify;
        }
    }

    public class IdeaTemplateAssignPartial : TemplateAssign
    {
        public IdeaModifyConnectionParameters ParamModify;
        public int SupportIndex = -1;
        public List<int> ConnectingMembers = new List<int>();

        public IdeaTemplateAssignPartial(IdeaTemplate template, int supportIndex, List<int> connectingMembers, IdeaModifyConnectionParameters paramModify) : base(template)
        {
            SupportIndex = supportIndex;
            ConnectingMembers = connectingMembers;
            ParamModify = paramModify;
        }
    }

    public class IdeaTemplateAssignCoded : TemplateAssign
    {
        public IdeaTemplateAssignCoded(Template template) : base (template) { }
    }

    internal class ParameterCollection : System.Collections.Generic.Dictionary<int, IIdeaParameter>, IXmlSerializable
    {
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            using (var subReader = reader.ReadSubtree())
            {
                XmlKeyValueListHelper.ReadKeyValueXml(subReader, this);
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlKeyValueListHelper.WriteKeyValueXml(writer, this);
        }
    }

    internal static class XmlKeyValueListHelper
    {
        public static void ReadKeyValueXml(System.Xml.XmlReader reader, ICollection<KeyValuePair<int, IIdeaParameter>> collection)
        {
            while (reader.Read())
            {
                while (!(String.Compare(reader.Name, "d2p1:KeyValueOfintParameterDatagVyOvWPW") == 0) || (reader.NodeType == XmlNodeType.EndElement))
                {
                    reader.Read();

                    //Break after all params are read.
                    if ((reader.Name == "Parameters") || (reader.Name == "ParametersModelLinks") || reader.ReadState == ReadState.EndOfFile)
                        return;
                }

                reader.ReadStartElement();

                if (reader.ReadToNextSibling("d2p1:Key"))
                {
                    int key = reader.ReadElementContentAsInt();

                    reader.ReadToDescendant("d2p1:Value");
                    reader.MoveToContent();
                    reader.ReadStartElement();

                    parameter param = new parameter();

                    if (reader.ReadToNextSibling("d4p1:Description"))
                        param.description = reader.ReadElementContentAsString();
                    if (reader.ReadToNextSibling("d4p1:Id"))
                        param.id = reader.ReadElementContentAsInt();
                    if (reader.ReadToNextSibling("d4p1:Identifier"))
                        param.identifier = reader.ReadElementContentAsString();
                    if (reader.ReadToNextSibling("d4p1:ParameterType"))
                        param.parameterType = reader.ReadElementContentAsString();
                    if (reader.ReadToNextSibling("d4p1:Value"))
                        param.value = reader.ReadElementContentAsString();

                    collection.Add(new KeyValuePair<int, IIdeaParameter>(key, IdeaParameterFactory.Create(param)));
                }
                else
                    break;
            }
        }

        public static void WriteKeyValueXml(System.Xml.XmlWriter writer, ICollection<KeyValuePair<int, IIdeaParameter>> collection)
        {
            throw new NotImplementedException();
        }
    }
}
