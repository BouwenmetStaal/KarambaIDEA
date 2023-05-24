// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Drawing;
using System.Linq;
using System.Globalization;
using System.Windows.Threading;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

using IdeaRS.OpenModel;
using IdeaRS.OpenModel.Result;
using IdeaStatiCa.Plugin;


using KarambaIDEA.Core;
using Microsoft.Win32;

using System.Collections.Generic;
using IdeaRS.OpenModel.Connection;

namespace KarambaIDEA.IDEA
{
    public class KarambaIdeaJoint : Joint, IXmlOutput 
    {
        internal List<TemplateAssign> TemplateAssigns = new List<TemplateAssign>();

        //Added connection data.
        public List<Plate> plates = new List<Plate>();
        public List<Weld> welds = new List<Weld>();
        public List<Core.BoltGrid> boltGrids = new List<Core.BoltGrid>();

        //figure out a smart way to differentiate from base and updated XML when a template is assigned.
        private readonly OpenModel _openModel; //base openModel from the project
        private readonly OpenModelResult _openModelResult;

        public KarambaIdeaJoint(Joint joint) : base(joint) 
        {
            OpenModelGenerator openModelGenerator = new OpenModelGenerator();
            openModelGenerator.CreateOpenModel(joint, "");
            _openModel = openModelGenerator.openModel;
            _openModelResult = openModelGenerator.openModelResult;
        }

        public KarambaIdeaJoint(KarambaIdeaJoint joint) : this((Joint)joint)
        {
            TemplateAssigns = joint.TemplateAssigns;
        }

        public override string ToString()
        {
            return "KarambaIDEA Joint: " + id.ToString();
        }

        public void AddTemplateAssign(TemplateAssign templateAssign)
        {
            if (templateAssign is IdeaTemplateAssignCoded)
                addConnectionData((CodedTemplate)templateAssign.Template);
            else
                TemplateAssigns.Add(templateAssign);
        }

        /// <summary>
        /// Populates connection data to the joint based on a Coded Template
        /// </summary>
        /// <param name="template"></param>
        private void addConnectionData(CodedTemplate template)
        {
            template.AddConnectionDataToJoint(this);
        }

        /// <summary>
        /// Apply any user defined Connection Data to the Joint Open Model
        /// </summary>
        private void ApplyConnectionData()
        {
            
        }

        public void SaveOpenModelContainer(string filepath)
        {
            OpenModelContainer openModelContainer = new OpenModelContainer();
            openModelContainer.OpenModel = _openModel;
            openModelContainer.OpenModelResult = _openModelResult;

            //ToFinish
        }

        public void SaveOpenModel(string filepath)
        {
            _openModel.SaveToXmlFile(filepath);
        }

        public void SaveOpenModelResult(string filepath)
        {
            _openModelResult.SaveToXmlFile(filepath);
        }

        public string ToStringXML()
        {
            using (var stringwriter = new System.IO.StringWriter())
            {
                var serializer = new XmlSerializer(typeof(OpenModel));
                serializer.Serialize(stringwriter, this._openModel);
                return stringwriter.ToString();
            }
        }
    }
}
