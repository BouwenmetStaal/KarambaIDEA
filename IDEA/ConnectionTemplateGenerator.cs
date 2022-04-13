using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using IdeaRS.Connections.Data;

using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using KarambaIDEA.Core;

namespace KarambaIDEA.IDEA
{

    //public class ConnectionTemplateGenerator
    //{
    //    private List<Type> _types = new List<Type>() { typeof(CutBeamData), typeof(StiffenerData), typeof(EndPlateData), typeof(ColumnWidenerData), typeof(GeneralPlateData), typeof(CutPlateData)};
    //    public ConnectionTemplate connectionTemplate = new ConnectionTemplate();

    //    /// <summary>
    //    /// Creates an ConnectionTemplateGenerator object with a ConnectionTemplate loaded from an xml file
    //    /// </summary>
    //    /// <param name="xmlFileName">the path to the xml file</param>
    //    public ConnectionTemplateGenerator(string xmlFileName)
    //    {
    //        this.LoadFromXmlFile(xmlFileName);
    //    }


    //    /// <summary>
    //    /// Loads an Connection Template object from a XML file.
    //    /// </summary>
    //    /// <param name="xmlFileName">the path to the xml file</param>
    //    /// <returns>Connection Template</returns>
    //    public ConnectionTemplate LoadFromXmlFile(string xmlFileName)
    //    {
    //        // since xml contains dictionairies xmlSerializer doesn not work.
    //        // use DataContractSerializer instead.https://theburningmonk.com/2010/05/net-tips-xml-serialize-or-deserialize-dictionary-in-csharp/
    //        // all unique operations should be referenced in list: CutBeamByBeamData , add if error
    //        List<Type> types = new List<Type>() { typeof(CutBeamData), typeof(StiffenerData), typeof(EndPlateData), typeof(ColumnWidenerData) };
    //        DataContractSerializer serializer = new DataContractSerializer(typeof(ConnectionTemplate),_types);


    //        using (FileStream fileStream = new FileStream(xmlFileName, FileMode.Open))
    //        {
    //            XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas());
    //            this.connectionTemplate = (ConnectionTemplate)serializer.ReadObject(reader);
    //            reader.Close();
    //            fileStream.Close();
    //        }
    //        return connectionTemplate;
    //    }

    //    /// <summary>
    //    /// Saves the connection template object to a XML file
    //    /// </summary>
    //    /// <param name="xmlFileName">the path where to save the xml fil</param>
    //    public void SaveToXmlFile(string xmlFileName)
    //    {
    //        using (FileStream fileStream = File.Open(xmlFileName, FileMode.Create))
    //        {
    //            DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(ConnectionTemplate), _types);
    //            XmlDictionaryWriter xmlWriter = XmlDictionaryWriter.CreateTextWriter(fileStream, Encoding.Unicode);
    //            xmlSerializer.WriteObject(xmlWriter, this.connectionTemplate);
    //            xmlWriter.Flush();
    //            xmlWriter.Close();
    //            fileStream.Close();
    //        }
    //    }


    //    /// <summary>
    //    /// Updates template based on properties in the joint
    //    /// </summary>
    //    /// <param name="joint"></param>
    //    public void UpdateTemplate()
    //    {
    //        //IdeaRS.Connections.Data.StiffenerData;
    //        //IdeaRS.Connections.Data.EndPlateData;
           

    //        //1.Create list with only (WELD)workshop operations
    //        List<CutBeamData> cut = new List<CutBeamData>();
    //        //List<Weld> cutBeamDatas = connectionTemplate.Properties.Items.OfType<CutBeamData>().ToList().Select(x => x.Weld);
    //        List<CutBeamData> cutBeamDatas = connectionTemplate.Properties.Items.OfType<CutBeamData>().ToList();
    //        double webweldSize = 0.001;
    //        double flangeweldSize = 0.002;
    //        foreach (var c in connectionTemplate.Properties.Items)
    //        {
    //            if (c.Value.GetType() == typeof(CutBeamData))
    //            {
                                      
    //                //Weld (IdeaRS.Connections.Data.WeldData){ Size [m], WeldType [DoubleFillet]}
    //                //Flangeweld (IdeaRS.Connections.Data.WeldData){ Size [m], WeldType [DoubleFillet]}
    //                WeldData webweld = ((CutBeamData)c.Value).Weld;
    //                WeldData flangeweld = ((CutBeamData)c.Value).FlangesWeld;
    //                webweld.Size = webweldSize;
    //                flangeweld.Size = flangeweldSize;
    //                webweldSize += 0.002;
    //                flangeweldSize += 0.002;

                  
    //            }
    //            if (c.Value.GetType() == typeof(StiffenerData))
    //            {
    //                //ShapeData (IdeaRS.Connections.Data.PlateEditorData){Thickness}
    //                //Weld (IdeaRS.Connections.Data.WeldData){ Size [m], WeldType [DoubleFillet]}
    //                WeldData weld = ((StiffenerData)c.Value).Weld;
    //                double platethickness = ((StiffenerData)c.Value).Thickness;
    //            }
    //            if (c.Value.GetType() == typeof(EndPlateData))
    //            {
    //                WeldData plateweld = ((EndPlateData)c.Value).PlateWeld;
    //                WeldData memberweld = ((EndPlateData)c.Value).MemberWeld;
    //                WeldData flangeweld = ((EndPlateData)c.Value).MemberFlangesWeld;
    //                double thickness = ((EndPlateData)c.Value).Thickness;
    //                //Data about the bolts missing?

    //            }
    //            if (c.Value.GetType() == typeof(ColumnWidenerData))
    //            {
    //               //single weld
    //                WeldData weld = ((ColumnWidenerData)c.Value).Weld;
    //                double platethickness = ((ColumnWidenerData)c.Value).Thickness;
    //            }
    //            if (c.Value.GetType() == typeof(GeneralPlateData))
    //            {
    //                double platethickness = ((GeneralPlateData)c.Value).Thickness;
    //            }
    //            if (c.Value.GetType() == typeof(CutPlateData))
    //            {
    //                //single weld
    //                WeldData weld = ((CutPlateData)c.Value).Weld;
    //            }
    //        }
    //    }

    //    IdeaRS.OpenModel.Connection.ConnectionData ConnectionData;
       
    //}
}