using IdeaRS.OpenModel.Connection;
using IdeaStatiCa.Plugin;
using System.Collections.Generic;
using IdeaRS.OpenModel.Geometry3D;

using Microsoft.Win32;
using KarambaIDEA.Core;
using System.Linq;
using System.Windows.Forms;
using System;
using System.IO;
using System.Globalization;

namespace KarambaIDEA.IDEA
{

#warning this class is now not required.

    /// <summary>
    /// Main view model of the example
    /// </summary>
    /// //public class HiddenCalculation : INotifyPropertyChanged, IConHiddenCalcModel
    /// 
    public class HiddenCalculationV20
    {
        //public static IdeaConnectionResult Calculate(ConnectionHiddenCheckClient client, string pathToFile, List<IIdeaModification> modifications, bool userFeedback)
        public static IdeaConnectionResult Calculate(string pathToFile, List<IdeaModification> modifications, bool userFeedback)
        {
            ProgressWindow pop = new ProgressWindow();
            if (userFeedback)
            {
                pop.Show();
                //pop.AddMessage(string.Format("Start calculation '{0}'", joint.Name));
                pop.AddMessage(string.Format("IDEA StatiCa installation was found in '{0}'", IdeaConnection.ideaStatiCaDir));
            }

            string path = IdeaStatiCaVersion.GetLatestVersionPath();
            
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

                    if (userFeedback)
                    {
                        //pop.AddMessage(string.Format("Calculation started: '{0}'", joint.Name));
                    }

                    //run through modification
                    foreach (IdeaModification mod in modifications)
                    {
                        mod.ModifyConnection(client, connection.Identifier);
                    }

                    conRes = client.Calculate(connection.Identifier);
#if (DEBUG)
                    client.SaveAsProject(pathToFile);
#endif
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
            if (userFeedback)
            {
                pop.Close();
            }

            return new IdeaConnectionResult(conRes);

        }

        public static void Calculate(Joint joint, bool userFeedback)
        {
            ProgressWindow pop = new ProgressWindow();
            if (userFeedback)
            {
                pop.Show();
                pop.AddMessage(string.Format("Start calculation '{0}'", joint.Name));
                pop.AddMessage(string.Format("IDEA StatiCa installation was found in '{0}'", IdeaConnection.ideaStatiCaDir));
            }

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

                    if (userFeedback)
                    {
                        pop.AddMessage(string.Format("Calculation started: '{0}'", joint.Name));
                    }
                    conRes = client.Calculate(connection.Identifier);
                    client.SaveAsProject(pathToFile);

                    //Export template file
                    string templatePath = Path.Combine(joint.project.projectFolderPath, joint.Name, "Template.xml");
                    client.ExportToTemplate(connection.Identifier, templatePath);//store template to location

                    //ConnectionTemplateGenerator connectionTemplateGenerator = new ConnectionTemplateGenerator(templatePath);
                                      
                                      

                    //example code for running multiple connectioin files
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
                SaveResults(joint, conRes);
            }
            if (userFeedback)
            {
                pop.Close();
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
        /// <summary>
        /// Save ResultSummary from IDEA StatiCa back into Core 
        /// </summary>
        /// <param name="joint">joint instance</param>
        /// <param name="cbfemResults">summary results retrieved from IDEA StatiCa</param>
        public static void SaveResults(Joint joint, ConnectionResultsData cbfemResults)
        {
            List<CheckResWeld> results = cbfemResults.ConnectionCheckRes[0].CheckResWeld;
            foreach (CheckResWeld w in results)
            {
                double idnumber = w.Id;
                double idnumer2 = w.Items[0];
            }
            double o1 = 0;
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

        public static Vector3D Unitize(this Vector3D vec)
        {
            vec.X = vec.X / vec.Length();
            vec.Y = vec.Y / vec.Length();
            vec.Z = vec.Z / vec.Length();
            return vec;
        }

        public static double Length(this Vector3D vec)
        {

            return Math.Sqrt(Math.Pow(vec.X, 2) + Math.Pow(vec.Y, 2) + Math.Pow(vec.Z, 2));
        }

        static public Vector3D VecScalMultiply(this Vector3D vec, double scalar)
        {
            Vector3D vector = new Vector3D();
            vector.X = vec.X * scalar;
            vector.Y = vec.Y * scalar;
            vector.Z = vec.Z * scalar;
            return vector;
        }

        public static Point3D MovePointVecAndLength(this Point3D point, Vector3D vec, double length)
        {
            vec.Unitize();
            Vector3D move = vec.VecScalMultiply(length);
            Point3D newpoint = new Point3D();
            newpoint.X = point.X + move.X;
            newpoint.Y = point.Y + move.Y;
            newpoint.Z = point.Z + move.Z;

            return newpoint;
        }
    }

    
}

