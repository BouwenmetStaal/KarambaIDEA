using System;
using System.IO;
using System.Xml.Serialization;
using System.Linq;

using IdeaRS.OpenModel.Connection;
using IdeaRS.Connections.Data;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Text;

using System.Windows.Forms;
using WindowsFormsApp1;


namespace ConnectionLinkTestApp
{
    public class WeldMatch
    {
        public int weldId;
        public bool isFlange;
        public string operationName;
        public bool increase = false;
        public double initialsize;


        public WeldMatch(int _weldId, bool _isFlange, string _operationName, double _initialsize)
        {
            this.weldId = _weldId;
            this.isFlange = _isFlange;
            this.operationName = _operationName;
            this.initialsize = _initialsize;

        }
    }

    public partial class MainWindow
	{
        //RAZ:optellen
        public int tel = 0;
        public int max_iteration = 20;
        //StringBuilder csvcontent = new StringBuilder();
        //string csvpath = @"C:\Data\xyz.csv ";
        Excel excel = new Excel(@"C:\Data\WELDRESULTS_template.xlsx", 1);

        List<WeldMatch> weldMathes = new List<WeldMatch>();

        /// <summary>
        /// Calls method IdeaRS.ConnectionLink.ConnectionLink.OpenIdeaConnection to run Idea Connection
        /// </summary>
        /// <param name="iomFilePath">IOM filename - includes geometry of the connection (connected members)</param>
        /// <param name="templateFilePath">Idea connection template or connection project file name</param>
        /// <param name="isHiddenCalculation">Set true to run hidden calculation </param>
        private void OpenIOMInIdeaCon2(string iomFilePath, string templateFilePath, bool isHiddenCalculation, ConnectionTemplate connectionTemplate = null)
		{
            //reset weld update
            foreach (WeldMatch w in weldMathes)
            {
                w.increase = false;
            }

            FileStream fs = new FileStream(iomFilePath, FileMode.Open, FileAccess.Read);
			FileStream fsr = null;

			string filenameRes = iomFilePath + "R"; //nb:  PGC should be referenced by an input parameter instead of name + R ...
			if (System.IO.File.Exists(filenameRes))
			{
				fsr = new FileStream(filenameRes, FileMode.Open, FileAccess.Read);
			}

			FileStream connTemplateStream = null;
			FileStream connProjectStream = null;
			MemoryStream importSettingsStream = null;

			if (!string.IsNullOrEmpty(templateFilePath) && File.Exists(templateFilePath))
			{
				string fileExt = System.IO.Path.GetExtension(templateFilePath);
				bool isConProject = fileExt.Equals(".ideacon", StringComparison.InvariantCultureIgnoreCase);

				if (isConProject)
				{
					connProjectStream = new FileStream(templateFilePath, FileMode.Open, FileAccess.Read);
				}
				else
				{
					connTemplateStream = new FileStream(templateFilePath, FileMode.Open, FileAccess.Read);
				}
			}

 

            if (dynLinkLazy.Value != null)
            {
                //PGC: TEST importsettings and hiddenanalysis
                //IMPORT SETTINGS
                IdeaConImportSettings ideaConImportSettings = new IdeaConImportSettings();
                ideaConImportSettings.UseWizard = false;   //show the wizard
                ideaConImportSettings.OnePageWizard = true; //show one page or multipage wizard
                ideaConImportSettings.DefaultBoltAssembly = "M12 4.6"; //is also default
                ideaConImportSettings.DesignCode = "ECEN"; //this is also default
                ideaConImportSettings.StartIdeaStaticaApp = true; //this is also default
                ideaConImportSettings.WaitForExit = true;  //this is also default
                

                //serialize importsettings to XML and memory stream
                importSettingsStream = new MemoryStream(); 
                StreamWriter writer = new StreamWriter(importSettingsStream, new System.Text.UTF8Encoding());
                XmlSerializer serializer = new XmlSerializer(typeof(IdeaConImportSettings));
                serializer.Serialize(writer, ideaConImportSettings);
                importSettingsStream.Flush();
                importSettingsStream.Seek(0, SeekOrigin.Begin);

                //Hidden analysis
                // dynLinkLazy.Value.OpenIdeaConnection(fs, fsr, connProjectStream, connTemplateStream, importSettingsStream, true);
                dynLinkLazy.Value.RunHiddenAnalysis(fs, fsr, connProjectStream, connTemplateStream, importSettingsStream, true);
                
                //retrieves the file name
                string filename = dynLinkLazy.Value.TempFileFullName;

                







                ////retrieves projectinfo as xml + deserialize xml back to ConProjectInfo object
                //String projectinfoXML = dynLinkLazy.Value.GetConnectionProjectInfo(filename);
                //ConProjectInfo conProjectInfo = new XmlSerializer(typeof(ConProjectInfo)).Deserialize(new StringReader(projectinfoXML)) as ConProjectInfo;

                ////select first connection 
                
                //ConnectionInfo con = conProjectInfo.Connections.First();

                ////retrieves connection data as xml + deserialize xml back to ConnectionData object
                //var connectiondataXML= dynLinkLazy.Value.GetConnectionDataXML(new Guid(con.Identifier));
                
                ////ConnectionData connectionData= new XmlSerializer(typeof(ConnectionData)).Deserialize(new StringReader(connectiondataXML)) as ConnectionData;
                //IdeaRS.OpenModel.Connection.ConnectionData connectionData = new XmlSerializer(typeof(IdeaRS.OpenModel.Connection.ConnectionData)).Deserialize(new StringReader(connectiondataXML)) as IdeaRS.OpenModel.Connection.ConnectionData;

                //List<int> weldIds = connectionData.Welds.Select(a => a.Id).ToList();


                ////In eerste instantie heeft elke las een unieke dikte gekregen. Deze diktes zijn terug te vinden in "conncetionTemplate". 
                ////De lasdiktes in het project worden uit "connectionData" gehaald. Deze worden gematcht met de unieke lasdikten.
                ////Hieruit wordt bepaald of het een web-las of flens-las is. Daarna wordt het toegevoegd aan de lijst weldmatches.
                ////match weld thickness with corresponding operation:
                //bool templatechanged = false;
                //if (tel == 0)
                //{
                //    foreach (IdeaRS.OpenModel.Connection.WeldData weldData in connectionData.Welds)
                //    {
                //        foreach (CutBeamByBeamData c in connectionTemplate.Properties.Items.Values)
                //        {
                //            if (c.FlangesWeld.Size == weldData.Thickness)
                //            {
                //                //het is flange
                //                //het is dit element
                //                WeldMatch w = new WeldMatch(weldData.Id, true, c.Name, c.FlangesWeld.Size);
                //                weldMathes.Add(w);

                //            }
                //            else
                //            {
                //                if (c.Weld.Size == weldData.Thickness)
                //                {
                //                    //het is het web
                //                    //het is dit element
                //                    WeldMatch w = new WeldMatch(weldData.Id, false, c.Name, c.Weld.Size);
                //                    weldMathes.Add(w);
                //                }
                //            }
                //        }
                //    }

                //    templatechanged = true;
                //    //set start lassen
                //    foreach (var c in connectionTemplate.Properties.Items)
                //    {
                //        //check if type of operation is CutBeamByBeamData since all objects have other parameters
                //        if (c.Value.GetType() == typeof(CutBeamByBeamData))
                //        {
                //            //cast unknown object type to CutBeamByBeamData type
                //            CutBeamByBeamData cutBeamByBeamData = (c.Value as CutBeamByBeamData);
                //            cutBeamByBeamData.FlangesWeld.Size = 0.001;
                //            cutBeamByBeamData.Weld.Size = 0.001;
                //        }
                //    }
                //}
                //else
                //{
                //    //retrieves connection analysis results (governing unity checks) as xml + deserialize xml back to ConnectionResultsData object
                //    var resultsdataXML = dynLinkLazy.Value.GetResultsData();
                //    ConnectionResultsData connectionResultsData = new XmlSerializer(typeof(ConnectionResultsData)).Deserialize(new StringReader(resultsdataXML)) as ConnectionResultsData;

                //    int i = 0;
                //    foreach (int wId in weldIds)
                //    {

                //        //double maxuc = 0;
                //        //foreach (CheckResWeld res in connectionResultsData.ConnectionCheckRes[0].CheckResWeld)
                //        //{
                //        //    if (res.Items.Contains(conid))
                //        //    {
                //        //        maxuc = Math.Max(maxuc, res.UnityCheck);
                //        //    }
                //        //}
                        
                //        double maxuc = connectionResultsData.ConnectionCheckRes[0].CheckResWeld.Where(a => a.Items.Contains(wId)).Max(a => a.UnityCheck);

        

                //        //print maxuc to excel
                //        //create header with weldnames
             
                //        string weldname = weldMathes[i].operationName;
                //        string location;

                //        if (weldMathes[i].isFlange == true)
                //        {
                //            location = "Flange";
                //        }
                //        else
                //        {
                //            location = "Web";
                //        }

                //        excel.WritetoCellstring(0, i, "Utility, " + weldname + "," + location);
                //        excel.WritetoCell(tel, i, maxuc);
                //        i = i + 1; //column placing in excel


                //        if (maxuc > 100.0)
                //        {
                //            WeldMatch wmatch = weldMathes.First(a => a.weldId == wId);
                //            wmatch.increase = true;
                //        }
                //    }

                //    //weldsizes to excel
                //    int ic = i+1; //column placing in excel
                //    int ib = 0;
                //    int cutnumber = 1; //every cut consist out of a flangeweld and a webweld.
                //    //the list of throatthickness is shorter than the lists of results. Because in the list of results both flanges are displayed in case of single bevelcut.
                //    foreach (var c in connectionTemplate.Properties.Items)
                //    {
                //        //check if type of operation is CutBeamByBeamData since all objects have other parameters
                //        if (c.Value.GetType() == typeof(CutBeamByBeamData))
                //        {
                //            CutBeamByBeamData cutbeambybeamdata = c.Value as CutBeamByBeamData;

                //            if (weldMathes.Where(a => a.operationName == cutbeambybeamdata.Name && a.isFlange == true ).Count() > 0)
                //            {
                //                string weldname = "CUT "+ cutnumber;
                                
                //                excel.WritetoCellstring(0, ic, "throat, " + weldname +", Flange");

                //                ib = ib + 1;

                //                excel.WritetoCell(tel, ic,cutbeambybeamdata.FlangesWeld.Size);
                //                ic = ic + 1;
                                
                //            }
                //            if (weldMathes.Where(a => a.operationName == cutbeambybeamdata.Name && a.isFlange == false ).Count() > 0)
                //            {
                //                string weldname = "CUT " + cutnumber;

                //                excel.WritetoCellstring(0, ic, "throat, " + weldname+ ", Web");

                //                ib = ib + 1;

                //                excel.WritetoCell(tel, ic, cutbeambybeamdata.Weld.Size);
                //                ic = ic + 1;
                //            }

                //        }
                //        cutnumber = cutnumber + 1;
                //    }


                //    //increase welds that have increase ==true
                //    foreach (var c in connectionTemplate.Properties.Items)
                //    {
                //        //check if type of operation is CutBeamByBeamData since all objects have other parameters
                //        if (c.Value.GetType() == typeof(CutBeamByBeamData))
                //        {
                //            CutBeamByBeamData cutbeambybeamdata = c.Value as CutBeamByBeamData;

                //            if (weldMathes.Where(a => a.operationName == cutbeambybeamdata.Name && a.isFlange == true && a.increase == true).Count() > 0)
                //            {
                //                cutbeambybeamdata.FlangesWeld.Size = cutbeambybeamdata.FlangesWeld.Size + 0.001;
                //                templatechanged = true;
                                
                //            }
                //            if (weldMathes.Where(a => a.operationName == cutbeambybeamdata.Name && a.isFlange == false && a.increase == true).Count() > 0)
                //            {
                //                cutbeambybeamdata.Weld.Size = cutbeambybeamdata.Weld.Size + 0.001;
                //                templatechanged = true;
                                
                //            }

                //        }
                //    }

                    


                //}
                
                //if (templatechanged && tel<max_iteration)
                //{

                //    //create new contemp file
                   
                //    tel = tel + 1;
                //    //RAZ: save file of IDEA_connection [.ideacon]
                //    string filePath = @"C:\Data\\test_k_joint"+ tel +".ideaCon";
                //    using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                //    {
                //        dynLinkLazy.Value.WriteProjectData(fileStream);
                //        fileStream.Close();
                //    }


                //    string filename2 = @"C:\Data\template2.contemp";
                //    using (FileStream fs2 = File.Open(filename2, FileMode.Create))
                //    {
                //        DataContractSerializer serializer2 = new DataContractSerializer(typeof(ConnectionTemplate), new List<Type>() { typeof(CutBeamByBeamData) });
                //        XmlDictionaryWriter writer2 = XmlDictionaryWriter.CreateTextWriter(fs2, Encoding.Unicode);
                //        serializer2.WriteObject(writer2, connectionTemplate);
                //        writer2.Flush();
                //        writer2.Close();
                //        fs2.Close();
                //    }
                //    //MessageBox.Show("recalculate updated version");
                    
                //    this.OpenIOMInIdeaCon(iomFilePath, filename2, false, connectionTemplate);
                //}
                //else
                //{
                //    //MessageBox.Show("finsihed");
                //    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                //    dlg.DefaultExt = ".xml"; // Default file extension
                //    dlg.Filter = "Idea Connection Check results (.xml)|*.xml"; // Filter files by extension
                //    dlg.CheckPathExists = true;

                //    excel.SaveAs(@"C:\Data\WELDRESULTS2.xlsx");

                //    excel.Close();

                //    if (dlg.ShowDialog() != true)
                //    {
                //        return;
                //    }


                //    using (Stream outStream = dlg.OpenFile())
                //    {
                //        this.dynLinkLazy.Value.WriteConCheckResults(outStream);
                //        outStream.Close();
                //    }

                    

                //}
            }
        }
	}
}