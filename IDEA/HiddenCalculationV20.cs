using IdeaRS.OpenModel.Connection;
using IdeaStatiCa.Plugin;
using System.Collections.Generic;


using KarambaIDEA.Core;
using System.Linq;
using System.Windows.Forms;

namespace KarambaIDEA.IDEA
{
    /// <summary>
    /// Main view model of the example
    /// </summary>
    /// //public class HiddenCalculation : INotifyPropertyChanged, IConHiddenCalcModel
    public class HiddenCalculationV20
    {
        public static void Calculate(Joint joint)
        {
            string path = IdeaConnection.ideaStatiCaDir;//path to idea
            string pathToFile = joint.JointFilePath;//ideafile path
            string newBoltAssemblyName = "M16 8.8";
            var calcFactory = new ConnHiddenClientFactory(path);
            ConnectionResultsData conRes = null;
            var client = calcFactory.Create();
            try
            {
                client.OpenProject(pathToFile);


                try
                {

                    // get detail about idea connection project
                    var projInfo = client.GetProjectInfo();

                    var connection = projInfo.Connections.FirstOrDefault();//Select first connection
                    if (joint.ideaTemplateLocation != null)
                    {
                        client.AddBoltAssembly(newBoltAssemblyName);//??Here Martin

                        client.ApplyTemplate(connection.Identifier, joint.ideaTemplateLocation, null);
                        client.SaveAsProject(pathToFile);
                    }


                    conRes = client.Calculate(connection.Identifier);

                    //projInfo.Connections.Count()
                    if (projInfo != null && projInfo.Connections != null)
                    {

                        /*
                        // iterate all connections in the project
                        foreach (var con in projInfo.Connections)
                        {
                            //Console.WriteLine(string.Format("Starting calculation of connection {0}", con.Identifier));

                            // calculate a get results for each connection in the project
                            var conRes = client.Calculate(con.Identifier);
                            //Console.WriteLine("Calculation is done");

                            // get the geometry of the connection
                            var connectionModel = client.GetConnectionModel(con.Identifier);
                        }
                        */
                    }
                }
                finally
                {
                    // Delete temps in case of a crash
                    client.CloseProject();
                }
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }
            }
            if (conRes != null)
            {
                SaveResultsSummary(joint, conRes);
            }


        }

        /// <summary>
        /// Save ResultSummary from IDEA StatiCa back into Core 
        /// </summary>
        /// <param name="joint">joint instance</param>
        /// <param name="cbfemResults">summary results retrieved from IDEA StatiCa</param>
        private static void SaveResultsSummary(Joint joint, ConnectionResultsData cbfemResults)
        {
            List<CheckResSummary> results = cbfemResults.ConnectionCheckRes[0].CheckResSummary;
            joint.ResultsSummary = new ResultsSummary();

            //TODO:include message when singilarity occurs
            //TODO:include message when bolts and welds are conflicting

            joint.ResultsSummary.analysis = results.GetResult("Analysis");
            joint.ResultsSummary.plates = results.GetResult("Plates");
            joint.ResultsSummary.bolts = results.GetResult("Bolts");
            joint.ResultsSummary.welds = results.GetResult("Welds");
            joint.ResultsSummary.buckling = results.GetResult("Buckling");

            string message = string.Empty;
            foreach (var result in results)
            {
                message += result.Name + ": " + result.UnityCheckMessage + " ";
            }
            joint.ResultsSummary.summary = message;

   
        }
    }
    public static class CalculationExtentions
    {
        public static double? GetResult(this List<CheckResSummary> source, string key  )
        {
            var boltResult = source.FirstOrDefault(x => x.Name == key);
            if (boltResult != null)
            { 
                return boltResult.CheckValue; 
            }
            return null;
        }
    }

    
}

