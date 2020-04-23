using IdeaRS.OpenModel.Connection;
using IdeaStatiCa.Plugin;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Input;

using KarambaIDEA.Core;
using IdeaStatiCa.ConnectionClient.Commands;

using IdeaStatiCa.ConnectionClient.Model;
using System.Diagnostics;
using System;
using System.Linq;

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
            string path = IdeaConnection.IdeaInstallDir;//path to idea
            string pathToFile = joint.JointFilePath;//ideafile path
            //string pathToTemplate = "";
            var calcFactory = new ConnHiddenClientFactory(path);
            ConnectionResultsData conRes=null;
            var client = calcFactory.Create();
            try
            {
                client.OpenProject(pathToFile);

                try
                {
                    
                    // get detail about idea connection project
                    var projInfo = client.GetProjectInfo();
                    var connection = projInfo.Connections.FirstOrDefault();

                    //client.ApplyTemplate(connection.Identifier, pathToTemplate, null);
                    //client.SaveAsProject(pathToFile);
                    conRes = client.Calculate(connection.Identifier);

                    //projInfo.Connections.Count()
                    if (projInfo != null && projInfo.Connections != null)
                    {
                        conRes = client.Calculate(Guid.Empty.ToString());
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
        public static void SaveResultsSummary(Joint joint, ConnectionResultsData cbfemResults)
        {
            List<CheckResSummary> results = cbfemResults.ConnectionCheckRes[0].CheckResSummary;
            joint.ResultsSummary = new ResultsSummary();

            //TODO:include message when singilarity occurs
            //TODO:include message when bolts and welds are conflicting

            if (results.Count == 4)//No bolts in connections. Hence, no results for bolts.
            {
                joint.ResultsSummary.analysis = results[0].CheckValue;
                joint.ResultsSummary.plates = results[1].CheckValue;
                joint.ResultsSummary.welds = results[2].CheckValue;
                joint.ResultsSummary.buckling = results[3].CheckValue;
            }
            else//Bolts in connection.
            {
                joint.ResultsSummary.analysis = results[0].CheckValue;
                joint.ResultsSummary.plates = results[1].CheckValue;
                joint.ResultsSummary.bolts = results[2].CheckValue;
                joint.ResultsSummary.welds = results[3].CheckValue;
                joint.ResultsSummary.buckling = results[4].CheckValue;
            }

            string message = string.Empty;
            foreach (var result in results)
            {
                message += result.Name + ": " + result.UnityCheckMessage + " ";
            }
            joint.ResultsSummary.summary = message;

        }




    }


    
}

