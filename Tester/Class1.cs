using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using IdeaRS.Connections.Data;

//*
namespace Tester
{

    public class ConnectionTemplateGenerator
    {
        public ConnectionTemplate connectionTemplate = new ConnectionTemplate();

        /// <summary>
        /// Creates an ConnectionTemplateGenerator object with a ConnectionTemplate loaded from an xml file
        /// </summary>
        /// <param name="xmlFileName">the path to the xml file</param>
        public ConnectionTemplateGenerator(string xmlFileName)
        {
            this.LoadFromXmlFile(xmlFileName);
        }


        /// <summary>
        /// Loads an Connection Template object from a XML file.
        /// </summary>
        /// <param name="xmlFileName">the path to the xml file</param>
        /// <returns>Connection Template</returns>
        public ConnectionTemplate LoadFromXmlFile(string xmlFileName)
        {
            // since xml contains dictionairies xmlSerializer doesn not work.
            // use DataContractSerializer instead.https://theburningmonk.com/2010/05/net-tips-xml-serialize-or-deserialize-dictionary-in-csharp/
            // all unique operations should be referenced in list: CutBeamByBeamData , add if error
            //IdeaRS.Connections.Data.StiffenerData
            DataContractSerializer serializer = new DataContractSerializer(typeof(ConnectionTemplate), new List<Type>() { typeof(IdeaRS.Connections.Data.CutBeamData) });
            using (FileStream fileStream = new FileStream(xmlFileName, FileMode.Open))
            {
                XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas());
                this.connectionTemplate = (ConnectionTemplate)serializer.ReadObject(reader);
                reader.Close();
                fileStream.Close();
            }
            return connectionTemplate;
        }

        /// <summary>
        /// Saves the connection template object to a XML file
        /// </summary>
        /// <param name="xmlFileName">the path where to save the xml fil</param>
        public void SaveToXmlFile(string xmlFileName)
        {
            using (FileStream fileStream = File.Open(xmlFileName, FileMode.Create))
            {
                DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(ConnectionTemplate), new List<Type>() { typeof(IdeaRS.Connections.Data.CutBeamByBeamData) });
                XmlDictionaryWriter xmlWriter = XmlDictionaryWriter.CreateTextWriter(fileStream, Encoding.Unicode);
                xmlSerializer.WriteObject(xmlWriter, this.connectionTemplate);
                xmlWriter.Flush();
                xmlWriter.Close();
                fileStream.Close();
            }
        }

        /// <summary>
        /// Serializes the connection template object and returns it as a memory stream
        /// </summary>
        /// <returns>memory stream containing the connetion template serialized in xml format</returns>
        public MemoryStream SerializeToXMLMemoryStream()
        {
            MemoryStream xmlTemplateStream = new MemoryStream();
            DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(ConnectionTemplate), new List<Type>() { typeof(IdeaRS.Connections.Data.CutBeamByBeamData) });
            xmlSerializer.WriteObject(xmlTemplateStream, this.connectionTemplate);
            xmlTemplateStream.Flush();
            return xmlTemplateStream;
#warning To be tested
        }

        /// <summary>
        /// Updates template based on properties in the joint
        /// </summary>
        /// <param name="joint"></param>
        public void UpdateTemplate()
        {
            //1.Create list with only (WELD)workshop operations
            //List<CutBeamByBeamData> cut = new List<CutBeamByBeamData>();

            foreach (var c in connectionTemplate.Properties.Items)
            {
                
                if (c.Value.GetType() == typeof(IdeaRS.Connections.Data.CutBeamData))
                {
                    //IdeaRS.Connections.Data.StiffenerData;
                    //IdeaRS.Connections.Data.EndPlateData;
                    //IdeaRS.Connections.Data.ColumnWidenerData;

                    
                    CutBeamByBeamData cutBeamByBeamData = (c.Value as IdeaRS.Connections.Data.CutBeamData);
                    if (cutBeamByBeamData.FlangesWeld != null)
                    {
                        
                    }
                }
            }

        }
    }
}