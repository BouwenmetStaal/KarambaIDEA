using System;
using System.Collections.Generic;
using System.Linq;


using IdeaRS.OpenModel.Connection;
using IdeaRS.Connections.Data;


using System.Diagnostics;
using System.IO;
using System.Reflection;


using System.Xml;
using System.Runtime.Serialization;

using System.Xml.Serialization;
using KarambaIDEA.Core;

namespace KarambaIDEA.IDEA
{
    public class IdeaConnection
    {
        public Lazy<dynamic> dynLinkLazy;

        public OpenModelGenerator openModelGenerator;
        public ConnectionTemplateGenerator connectionTemplateGenerator;

        public Joint joint;
        private Guid ideaConnectionIdentifier = Guid.Empty;


        public string filepath = "";


        /// <summary>
        /// Constructor for an IdeaConnection based on a joint
        /// </summary>
        /// <param name="_joint">Joint object</param>
        public IdeaConnection(Joint _joint, string templateFilePath, string path = @"C:\Data\")
        {
            //1.set joint
            joint = _joint;

            //TODO: make sure only one folder is created now two folders are created.

            //2.create folder
            string folder = this.joint.project.filepath;
            filepath = Path.Combine(folder, this.joint.Name);
            if (!Directory.Exists(this.filepath))
            {
                Directory.CreateDirectory(this.filepath);
            }


            //3.initialize connection with IdeaRS connectionlink, must be by lazy link
            //IdeaRS.ConnectionLink object will only be found during runtime by use of the lazylink
            this.dynLinkLazy = new Lazy<dynamic>(() =>
            {
                string ideaInstallDir = @"C:\Program Files\IDEAStatiCa\StatiCa9";
                string ideaConLinkFullPath = System.IO.Path.Combine(ideaInstallDir, "IdeaRS.ConnectionLink.dll");
                var conLinkAssembly = Assembly.LoadFrom(ideaConLinkFullPath);

                object obj = conLinkAssembly.CreateInstance("IdeaRS.ConnectionLink.ConnectionLink");
                Debug.Assert(obj != null);
                dynamic d = obj;
                return d;
            });

            //4.create openmodel.
            openModelGenerator = new OpenModelGenerator();
            string ideaIOMFileName = openModelGenerator.CreateOpenModelGenerator(joint, filepath);
            connectionTemplateGenerator = new ConnectionTemplateGenerator(templateFilePath);


            //5.contemp XMLfile  to instance of the ConnectionTemplate object
            DataContractSerializer serializer = new DataContractSerializer(typeof(IdeaRS.Connections.Data.ConnectionTemplate), new List<Type>() { typeof(CutBeamByBeamData) });
            using (FileStream fs1 = new FileStream(templateFilePath, FileMode.Open))
            {
                XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs1, new XmlDictionaryReaderQuotas());
                IdeaRS.Connections.Data.ConnectionTemplate connectionTemplate = (IdeaRS.Connections.Data.ConnectionTemplate)serializer.ReadObject(reader);
                reader.Close();
                fs1.Close();
            }
        }

        /// <summary>
        /// Dispose also the dynlink when this object is disposed 
        /// </summary>
        public void Dispose()
        {
            if (this.dynLinkLazy != null && this.dynLinkLazy.IsValueCreated && this.dynLinkLazy.Value != null)
            {
                this.dynLinkLazy.Value.Dispose();
                this.dynLinkLazy = null;
            }

        }

        /// <summary>
        /// Creates a memory stream containing the IdeaConImportSettings object serialized to xml
        /// </summary>
        /// <returns>Memorystream containing the IdeaConImportSettings object serialized to xml</returns>
        private MemoryStream GetIdeaImportSettingsStream()
        {
            //make settings
            IdeaConImportSettings ideaConImportSettings = new IdeaConImportSettings();
            ideaConImportSettings.UseWizard = false;   //show the wizard
            ideaConImportSettings.OnePageWizard = true; //show one page or multipage wizard
            ideaConImportSettings.DefaultBoltAssembly = "M12 4.6"; //is also default
            ideaConImportSettings.DesignCode = "ECEN"; //this is also default
            ideaConImportSettings.StartIdeaStaticaApp = true; //this is also default
            ideaConImportSettings.WaitForExit = true;  //this is also default

            //serialize importsettings to XML and memory stream
            MemoryStream importSettingsStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(importSettingsStream, new System.Text.UTF8Encoding());
            XmlSerializer serializer = new XmlSerializer(typeof(IdeaConImportSettings));
            serializer.Serialize(writer, ideaConImportSettings);
            importSettingsStream.Flush();
            importSettingsStream.Seek(0, SeekOrigin.Begin);

            return importSettingsStream;
        }


        /// <summary>
        /// Save Idea connection project to file
        /// </summary>
        /// <param name="filepath">path to save file to (.ideaCon)</param>
        public void SaveIdeaConnectionProjectFile(string filepath)
        {
            using (FileStream fileStream = File.Open(filepath, FileMode.Create))
            {
                this.dynLinkLazy.Value.WriteProjectData(fileStream);
                fileStream.Close();
            }
        }


        /// <summary>
        /// Retrieves connection project information 
        /// </summary>
        /// <returns>ConProjectInfo object</returns>
        public ConProjectInfo GetConProjectInfo()
        {
            //retrieves the temporary file name
            string filename = dynLinkLazy.Value.TempFileFullName;
            //retrieves projectinfo as xml            
            String projectinfoXML = dynLinkLazy.Value.GetConnectionProjectInfo(filename);
            //deserialize xml back to ConProjectInfo object 
            ConProjectInfo conProjectInfo = new XmlSerializer(typeof(ConProjectInfo)).Deserialize(new StringReader(projectinfoXML)) as ConProjectInfo;
            return conProjectInfo;
        }

        /// <summary>
        /// Get the connection data 
        /// </summary>
        /// <returns>the ConnectionData object</returns>
        public IdeaRS.OpenModel.Connection.ConnectionData GetConnectionData()
        {
            var val = dynLinkLazy.Value;
            Guid guid = this.GetConnectionIdentifier();

            // conProjectInfo = = new XmlSerializer(typeof(ConProjectInfo)).Deserialize(new StringReader(projectinfoXML)) as ConProjectInfo;
            //ConnectionInfo con = conProjectInfo.Connections.
            //Guid guid2 = new Guid()

            //retrieves connection data as xml + deserialize xml back to ConnectionData object
            try
            {
                var connectiondataXML = dynLinkLazy.Value.GetConnectionDataXML(guid);
                StringReader stringReader = new StringReader(connectiondataXML);
                IdeaRS.OpenModel.Connection.ConnectionData connectionData = new XmlSerializer(typeof(IdeaRS.OpenModel.Connection.ConnectionData)).Deserialize(stringReader) as IdeaRS.OpenModel.Connection.ConnectionData;
                return connectionData;
            }
            catch (Exception)
            {
                //hier moet je sqfe
                string filePath2 = this.filepath + "//" + joint.Name + "FAILjoint.ideaCon";
                this.SaveIdeaConnectionProjectFile(filePath2);
                return null;
            }
            
           
        }

        /// <summary>
        /// Get connection check results
        /// </summary>
        /// <returns>the ConnectionCheckRes object</returns>
        public ConnectionResultsData GetConnectionCheckResults()
        {
            Guid guid = this.GetConnectionIdentifier();
            var resultsdataXML = dynLinkLazy.Value.GetResultsData();
            ConnectionResultsData connectionResultsData = new XmlSerializer(typeof(ConnectionResultsData)).Deserialize(new StringReader(resultsdataXML)) as ConnectionResultsData;
            //ConnectionCheckRes connectionCheckRes = connectionResultsData.ConnectionCheckRes.FirstOrDefault(a => a.ConnectionID == this.GetConnectionIdentifier());
            return connectionResultsData;
        }


        /// <summary>
        /// Retrieves the connection identifier (GUID) based on similar joint id.
        /// </summary>
        /// <returns></returns>
        private Guid GetConnectionIdentifier()
        {
            // Retrieve the connection guid if not available yet, based on similar joint id.
            if (this.ideaConnectionIdentifier == null || this.ideaConnectionIdentifier == Guid.Empty)
            {
                ConProjectInfo conProjectInfo = this.GetConProjectInfo();
                string connectionIdentifier = conProjectInfo.Connections.FirstOrDefault(a => a.Name == this.joint.Name).Identifier;
                this.ideaConnectionIdentifier = new Guid(connectionIdentifier);
            }
            return this.ideaConnectionIdentifier;
        }

        
        /// <summary>
        /// Map the weld IDs and operation Ids corresponding to ids used by Idea
        /// </summary>
        public void MapWeldsIdsAndOperationIds()
        {
            // no direct relation between weld ID and operation is present.
            // therefore we will derive this relation by specificating of unique sizes for welds
            // as adjustment of the template
            //steps:
            // 0 temporary store welds sizes
            // 1 Set unique welds sizes
            // 2 Analyse
            // 3 match weld and operation
            // 4 reset weld sizes

            // 0+1 temporary store welds sizes and set unique weld sizes

            //4.Modify template set unique weldsizes
            //4.1.in cornerjoints there are weldless workshop operations
            List<CutBeamByBeamData> cut = new List<CutBeamByBeamData>();
            ConnectionTemplate connectionTemplate1 = this.connectionTemplateGenerator.connectionTemplate;
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
            double flangesize = 0.001;//flange odd
            double websize = 0.002;//web even
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
                        c.FlangesWeld.Size = flangesize;//m
                        con.flangeWeld.size = flangesize*1000;//m to mm
                        c.Weld.Size = websize;//m
                        con.webWeld.size = websize*1000;//m to mm

                    }
                }
                flangesize = flangesize + 0.002;//flange odd
                websize = websize + 0.002;//web evem
            }



            // 2 run analysis
            this.RunAnalysis();
            //8. get connection data 
            IdeaRS.OpenModel.Connection.ConnectionData connectionData = this.GetConnectionData();


            // 3 match weld and operaton:
            foreach (IdeaRS.OpenModel.Connection.WeldData weldData in connectionData.Welds)
            {
                foreach (ConnectingMember con in this.joint.attachedMembers.OfType<ConnectingMember>().ToList())
                {
                    if (weldData.Thickness == (con.flangeWeld.size)/1000)//compare m with mm
                    {
                        con.flangeWeld.Ids.Add(weldData.Id);
                    }
                    else
                    {
                        if (weldData.Thickness == (con.webWeld.size)/1000)//compare m with mm
                        {
                            con.webWeld.Ids.Add(weldData.Id);
                        }
                    }
                }
            }

            // 5 reset original weldsizes
            double startsize = this.joint.project.minthroat;
            foreach(ConnectingMember con in this.joint.attachedMembers.OfType<ConnectingMember>().ToList())
            {
                con.flangeWeld.size = startsize;
                con.webWeld.size = startsize;
            }
            this.connectionTemplateGenerator.UpdateTemplate(joint);

        }


        public void RunAnalysis()
        {
            //save openmodel to xml 
            string ideaIOM_path = this.filepath + "//"+joint.Name+"IOM";//  @"C:\Data\TEST09102018";+ "\\"+joint.Name+
            this.openModelGenerator.openModel.SaveToXmlFile(ideaIOM_path);
            //save open model results to xml 
            string ideaIOMresults_path = this.filepath + "//" + joint.Name + "IOMres";// ideaIOM_path + "r";
            this.openModelGenerator.openModelResult.SaveToXmlFile(ideaIOMresults_path);
            //save template to xml
            string template_path = this.filepath + "//" + joint.Name + "template2";// @"C:\Data\template2.contemp";
            this.connectionTemplateGenerator.SaveToXmlFile(template_path);

            using (FileStream iomStream = new FileStream(ideaIOM_path, FileMode.Open, FileAccess.Read))
            {
                using (FileStream resultsStream = new FileStream(ideaIOMresults_path, FileMode.Open, FileAccess.Read))
                {
                    using (FileStream requiredTemplateStream = new FileStream(template_path, FileMode.Open, FileAccess.Read))
                    {
                        //No start project
                        Stream existingConProjStream = null;
                        // idea import sttings
                        Stream importSettingsStream = this.GetIdeaImportSettingsStream();

                        bool closeInputStreams = true;
                        //run hidden analysis
                        if (dynLinkLazy.Value != null)
                        {
                            //RunHiddenAnalysis is part of the IdeaRS.ConnectionLink DLL
                            this.dynLinkLazy.Value.RunHiddenAnalysis(iomStream, resultsStream, existingConProjStream, requiredTemplateStream, importSettingsStream, closeInputStreams);
                        }
                    }
                }
            }
        }


        public void CheckConnectionWelds()
        {
            // check if all weld id's are set:
            if (!(this.joint.attachedMembers.OfType<ConnectingMember>().All(a => a.flangeWeld.Ids != null) && this.joint.attachedMembers.OfType<ConnectingMember>().All(a => a.webWeld.Ids!=null)))
            {
                //map weld ids and operations first
                this.MapWeldsIdsAndOperationIds();
            }

            // run analysis
            this.RunAnalysis();

            // retrieve unity checks 
            //welds
            ConnectionResultsData connectionResultsData = this.GetConnectionCheckResults();
            foreach (ConnectingMember cm in joint.attachedMembers.OfType<ConnectingMember>().ToList())
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
            //plates
            List<CheckResPlate> plates = connectionResultsData.ConnectionCheckRes[0].CheckResPlate;
            int listlength = 0;
            if (joint.attachedMembers.OfType<BearingMember>().First().ElementRAZ.crossSection.shape == Core.CrossSection.Shape.HollowSection)
            {
                joint.attachedMembers.OfType<BearingMember>().First().platefailure =  plates[0].CheckStatus;
                listlength += 1;
            }
            else
            {
                bool bfl = plates[0].CheckStatus;
                bool tfl = plates[1].CheckStatus;
                bool w = plates[2].CheckStatus;
                joint.attachedMembers.OfType<BearingMember>().First().platefailure = PlateFailure(bfl, tfl, w);
                listlength += 3;
            }
            for (int i=0; i < 5; i++)
            {
                foreach (ConnectingMember cm in joint.attachedMembers.OfType<ConnectingMember>().ToList())
                {
                    if (i == cm.ideaOperationID)
                    {
                        if (cm.ElementRAZ.crossSection.shape == Core.CrossSection.Shape.HollowSection)
                        {
                            cm.platefailure = plates[listlength].CheckStatus;
                        }
                        else
                        {
                            bool bfl = plates[listlength+0].CheckStatus;
                            bool tfl = plates[listlength+1].CheckStatus;
                            bool w = plates[listlength+2].CheckStatus;
                            cm.platefailure= PlateFailure(bfl, tfl, w);
                            listlength += 3;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If platefailure occurs in any plate of the beam element (topflange, bottomflange or web) return failure.
        /// </summary>
        /// <param name="bfl">bottomflange</param>
        /// <param name="tfl">topflange</param>
        /// <param name="w">web</param>
        /// <returns></returns>
        public static bool PlateFailure(bool bfl, bool tfl, bool w)
        {
            if(bfl == false || tfl == false || w == false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Increase weld dimensions of welds that fail with 1.0mm
        /// </summary>
        public void IncreaseFailingWelds()
        {
            foreach (ConnectingMember c in this.joint.attachedMembers.OfType<ConnectingMember>())
            {
                if (c.flangeWeld.unitycheck > 100.0)
                {
                    c.flangeWeld.size += 1.0;//mm
                    
                }
                    
                if (c.webWeld.unitycheck > 100.0)
                {
                    if (c.ElementRAZ.crossSection.shape == Core.CrossSection.Shape.HollowSection)//HS does not posses flangewelds
                    {
                        c.flangeWeld.size += 1.0;//mm
                        c.webWeld.size += 1.0;//mmm
                    }
                    else
                    {
                        c.webWeld.size += 1.0;//mmm
                    }
                    
                }
                    
            }
        }

        /// <summary>
        /// Optimize welds by increasing weld dimensions with 1mm per iteration for all failing welds ("walking uphill")
        /// </summary>
        /// <param name="maxiterations">the maximum number of iterations</param>
        public void OptimizeWelds(int maxiterations = 10)
        {
            ////excel set headers
            //int spacing = 7;
            ////Excel excel = new Excel(@"C:\Data\TEMPLATES\iterationLOG_template.xlsx", 1);
            //List<ConnectingMember> CMs = this.joint.attachedMembers.OfType<ConnectingMember>().ToList();
            //excel.WritetoCellstring(0, 0, "U.C.");
            //excel.WritetoCellstring(0, spacing, "Weldsize");
            //excel.WritetoCellstring(0, spacing + spacing, "PlateFailure");
            //int pos = 0;
            //string bearname = this.joint.attachedMembers.OfType<BearingMember>().First().ElementRAZ.crossSection.name;
            //excel.WritetoCellstring(1, spacing + spacing, "Bear "+bearname);
            //for (int i =0; i < CMs.Count; i++)
            //{
            //    string name =CMs[i].ElementRAZ.crossSection.name;
            //    excel.WritetoCellstring(1, pos, "Con" + i+" "+name);
            //    excel.WritetoCellstring(1, pos+spacing, "Con" + i + " " + name);
            //    excel.WritetoCellstring(1, 1+i + spacing + spacing, "Con" + i + " " + name);//plateHeaders
            //    excel.WritetoCellstring(2, pos, "Flange"+ "Con" + i);
            //    excel.WritetoCellstring(2, pos+spacing, "Flange"+ "Con" + i);
            //    pos += 1;
            //    excel.WritetoCellstring(2, pos, "Web" + "Con" + i);
            //    excel.WritetoCellstring(2, pos + spacing, "Web" + "Con" + i);
            //    pos += 1;
            //}




            for (int i = 1; i <= maxiterations; i++)
            {
                //check welds
                this.CheckConnectionWelds();

                //save iteration file
                string filePath2 = this.filepath + "//" + joint.Name + "joint"+i+".ideaCon";
                this.SaveIdeaConnectionProjectFile(filePath2);

                //update excel file
                //string bearfailure = "";
                //if (this.joint.attachedMembers.OfType<BearingMember>().First().platefailure == false)
                //{
                //    bearfailure = "Failure";
                //}
                //else
                //{
                //    bearfailure = "No Failure";
                //}
                //excel.WritetoCellstring(2 + i, spacing + spacing, bearfailure);
                List<ConnectingMember> CMs2 = this.joint.attachedMembers.OfType<ConnectingMember>().ToList();
                //int pos3 = 0;
                for (int b=0; b < CMs2.Count; b++)
                {
                    //double flangesize = CMs2[b].flangeWeld.size;
                    //double websize = CMs2[b].webWeld.size;
                    //double flangeUC = CMs2[b].flangeWeld.unitycheck;
                    //double webUC = CMs2[b].webWeld.unitycheck;
                    //string failure = "";
                    //if (CMs2[b].platefailure == false)
                    //{
                    //    failure = "Failure";
                    //}
                    //else
                    //{
                    //    failure = "No Failure";
                    //}

                    //excel.WritetoCell(2 + i, pos3, flangeUC);
                    //excel.WritetoCell(2 + i, pos3+spacing, flangesize);
                    //excel.WritetoCellstring(2 + i, 1+b + spacing+spacing, failure);
                    //pos3 += 1;
                    //excel.WritetoCell(2 + i, pos3, webUC);
                    //excel.WritetoCell(2 + i, pos3 + spacing, websize);
                    //pos3 += 1;
                }
                        


                //check if all <100.0
                double tresshold = 100.0;
                if (this.joint.attachedMembers.OfType<ConnectingMember>().All(a => a.flangeWeld.unitycheck < tresshold) && this.joint.attachedMembers.OfType<ConnectingMember>().All(a => a.webWeld.unitycheck < tresshold))
                {
                    //save excel file
                    //string excelpath = this.filepath + "\\" + "iterationLOG.xlsx";
                    //excel.SaveAs(excelpath);
                    //excel.Close();
                    //exit loop
                    break;
                }
                else
                {
                    //increase dimensions only if there will be 1 more iterations
                    if (i <= maxiterations - 1)
                    {
                        this.IncreaseFailingWelds();
                        this.connectionTemplateGenerator.UpdateTemplate(this.joint);
                    }
                    else
                    {
                        //save excel file
                        //string excelpath = this.filepath + "\\" + "iterationLOG.xlsx";
                        //excel.SaveAs(excelpath);
                        //excel.Close();
                    }
                }
            }
        }


    }
}
