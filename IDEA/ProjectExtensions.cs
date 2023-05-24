using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdeaStatiCa.Plugin;
using IdeaRS.OpenModel;
using IdeaRS.OpenModel.Connection;
using IdeaRS.OpenModel.Result;
using KarambaIDEA.Core;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace KarambaIDEA.IDEA
{
    public static class IdeaProjectExtensions
    {
        public static string ExportToMobelBIM(this Project project, string path)
        {
            ModelBIM modelBIM = new ModelBIM();
            
            modelBIM.RequestedItems = RequestedItemsType.Connections;

            OpenModelGenerator IomGenerator = new OpenModelGenerator();

            IomGenerator.CreateOpenModelEntireModel(project);

            OpenModel model = IomGenerator.openModel;
            OpenModelResult results = IomGenerator.openModelResult;

            //Connection Items
            List<BIMItemId> items = new List<BIMItemId>(); 

            if (model != null)
            {
                modelBIM.Model = model;

                foreach (ConnectionPoint pt in model.ConnectionPoint)
                   items.Add(new BIMItemId() { Id = pt.Id, Type = BIMItemType.Node });

            }
            else
                return null;

            modelBIM.Results = results;

            //Member Items
            //List<BIMItemId> memberItems = new List<BIMItemId>();

            //foreach (int memberId in members)
            //    items.Add(new BIMItemId() { Id = memberId, Type = BIMItemType.Member });

            modelBIM.Items = items;
            modelBIM.Messages = new IdeaRS.OpenModel.Message.OpenMessages();

            XmlSerializer xs = new XmlSerializer(typeof(ModelBIM));

            using (Stream fs = new FileStream(path, FileMode.Create))
            {
                using (XmlTextWriter writer = new XmlTextWriter(fs, Encoding.Unicode))
                {
                    writer.Formatting = Formatting.Indented;
                    xs.Serialize(writer, modelBIM);
                }
            }

            return path;
        }
    }
}
