using IdeaRS.OpenModel.Message;
using IdeaRS.OpenModel;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Text;
using System.Collections.Generic;
using IdeaRS.Connections.Data;

using System.Xml.Serialization;
using System.Linq;

using IdeaRS.OpenModel.Connection;


using System.Xml;
using System.Runtime.Serialization;
using KarambaIDEA.Core;
using KarambaIDEA.IDEA;

//using IdeaRS.ConnectionLink;

namespace TestCON
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow2
    {

        public MainWindow2()
        {

            Test();
        }


        /// <summary>
        /// FUNCTION PRIMAIRY FOR DEVELOPMENT.
        /// </summary>
        public void Test()
        {
            //1.create test joint
            TestFrameworkJoint2 fj = new TestFrameworkJoint2();
            Joint joint = fj.TestjointColumnA();
            joint.project.SetMinThroats(10);
            joint.project.SetWeldType();

            //2.Select template
            string templateFilePath = @"C:\Data\template.contemp";
            string templateFolder = @"C:\Data\TEMPLATES\";
            List<ConnectingMember> conmembers = joint.attachedMembers.OfType<ConnectingMember>().ToList();
            if (joint.IsContinues == false)
            {
                templateFilePath = templateFolder + "ended2members.contemp";
            }
            else
            {
                if (conmembers.Count == 1)
                {
                    templateFilePath = templateFolder + "continues1members.contemp";
                }
                if (conmembers.Count == 2)
                {
                    if (conmembers[0].ideaOperationID == 2)
                    {
                        templateFilePath = templateFolder + "continues2members.contemp";
                    }
                    else
                    {
                        templateFilePath = templateFolder + "kmirror.contemp";
                    }
                }
                if (conmembers.Count == 3)
                {
                    templateFolder = templateFolder + "continues3members.contemp";
                }
            }

            //3. create idea connection
            string path = @"C:\Data\";
            IdeaConnection ideaConnection = new IdeaConnection(joint, templateFilePath, path);

            //4.Modify template set unique weldsizes
            //4.1.in cornerjoints there are weldless workshop operations
            List<CutBeamByBeamData> cut = new List<CutBeamByBeamData>();
            ConnectionTemplate connectionTemplate1 = ideaConnection.connectionTemplateGenerator.connectionTemplate;
            foreach (var c in connectionTemplate1.Properties.Items)
            {
                if (c.Value.GetType() == typeof(CutBeamByBeamData))
                {
                    CutBeamByBeamData cutBeamByBeamData = (c.Value as CutBeamByBeamData);
                    if (cutBeamByBeamData.FlangesWeld != null)
                    {
                        cut.Add(cutBeamByBeamData);
                    }
                }
            }
            //4.2
            double flangesize = 0.001;
            double websize = 0.002;
            List<ConnectingMember> conlist2 = joint.attachedMembers.OfType<ConnectingMember>().ToList();
            foreach (ConnectingMember con in conlist2)
            {
                foreach (CutBeamByBeamData c in cut)
                {
                    string modifiedObject = c.ModifiedObjectPath;
                    string modifiedId = modifiedObject.Remove(0, 18).Remove(1, 1);
                    int modId = Convert.ToInt32(modifiedId);

                    if (con.ideaOperationID == modId)
                    {
                        if (con.flangeWeld.weldType == Weld.WeldType.Fillet)
                        {
                            c.FlangesWeld.WeldType = WeldTypeCode.Fillet;
                            c.Weld.WeldType = WeldTypeCode.Fillet;
                        }
                        else
                        {
                            c.FlangesWeld.WeldType = WeldTypeCode.DoubleFillet;
                            c.Weld.WeldType = WeldTypeCode.DoubleFillet;
                        }
                        c.FlangesWeld.Size = flangesize;
                        con.flangeWeld.size = flangesize;
                        c.Weld.Size = websize;
                        con.webWeld.size = websize;

                    }
                }
                flangesize = flangesize + 0.002;
                websize = websize + 0.002;
            }

            //5. run analysis
            ideaConnection.RunAnalysis();

            //6. save file
            string filePath = ideaConnection.filepath + "joint.ideaCon";
            ideaConnection.SaveIdeaConnectionProjectFile(filePath);

            //7. get project info 
            ConProjectInfo conProjectInfo = ideaConnection.GetConProjectInfo();

            //8. get connection data 
            IdeaRS.OpenModel.Connection.ConnectionData connectionData = ideaConnection.GetConnectionData();

            //match connectData weld id with unique weld size
            List<ConnectingMember> conm = joint.attachedMembers.OfType<ConnectingMember>().ToList();
            foreach (IdeaRS.OpenModel.Connection.WeldData weldData in connectionData.Welds)
            {
                foreach (ConnectingMember con in conm)
                {
                    if (weldData.Thickness == con.flangeWeld.size)
                    {
                        con.flangeWeld.Ids.Add(weldData.Id);
                    }
                    else
                    {
                        if (weldData.Thickness == con.webWeld.size)
                        {
                            con.webWeld.Ids.Add(weldData.Id);
                        }
                    }
                }
            }
            //9.Update templete to projectminimum throat
            ConnectionTemplate connectionTemplate3 = ideaConnection.connectionTemplateGenerator.connectionTemplate;
            foreach (var c in connectionTemplate3.Properties.Items)
            {
                if (c.Value.GetType() == typeof(CutBeamByBeamData))
                {
                    CutBeamByBeamData cutBeamByBeamData = (c.Value as CutBeamByBeamData);
                    if (cutBeamByBeamData.FlangesWeld != null)
                    {
                        cutBeamByBeamData.FlangesWeld.Size = joint.project.minthroat / 1000;
                        cutBeamByBeamData.Weld.Size = joint.project.minthroat / 1000;
                    }
                }
            }


            //10.runanlysis
            ideaConnection.RunAnalysis();

            //9. get results
            ConnectionResultsData connectionResultsData = ideaConnection.GetConnectionCheckResults();

            //save maximum unity check to welds
            foreach (ConnectingMember cm in conm)
            {
                double maxFlangeUC = 0;
                double maxWebUC = 0;
                foreach (int id in cm.flangeWeld.Ids)
                {
                    foreach (CheckResWeld res in connectionResultsData.ConnectionCheckRes[0].CheckResWeld)
                    {
                        if (res.Items.Contains(id))
                        {
                            maxFlangeUC = Math.Max(maxFlangeUC, res.UnityCheck);
                        }
                    }
                }
                cm.flangeWeld.unitycheck = maxFlangeUC;
                foreach (int id in cm.webWeld.Ids)
                {
                    foreach (CheckResWeld res in connectionResultsData.ConnectionCheckRes[0].CheckResWeld)
                    {
                        if (res.Items.Contains(id))
                        {
                            maxWebUC = Math.Max(maxWebUC, res.UnityCheck);
                        }
                    }
                }
                cm.webWeld.unitycheck = maxWebUC;
            }

            //check if uc above 100
        }
    }
}

