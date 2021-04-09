// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using IdeaRS.OpenModel;
using IdeaRS.OpenModel.Result;

using IdeaStatiCa.Plugin;//V20

using System.IO;
using System.Reflection;
using KarambaIDEA.Core;
using Microsoft.Win32;
using System.Linq;
using System.Globalization;
using System.Windows.Threading;
using System.Collections.Generic;
using IdeaRS.OpenModel.Connection;

namespace KarambaIDEA.IDEA
{
    public class IdeaConnection
    {
        public OpenModelGenerator openModelGenerator;
        public Joint joint;
        public string filePath = "";
        public static string ideaStatiCaDir;
        bool windowWPF = true;//Mouse controls may freeze when true https://discourse.mcneel.com/t/mouse-click-not-working/73010/17
        //https://www.grasshopper3d.com/forum/topics/bug-grasshopper-partly-freezes-after-any-export-operation

        /// <summary>
        /// Constructor for an IdeaConnection based on a joint
        /// </summary>
        /// <param name="_joint">Joint object</param>
        public IdeaConnection(Joint _joint, bool Calculate = false)
        {
            try
            {
                //Find most recent version of IDEA StatiCa in registry
                RegistryKey staticaRoot = Registry.LocalMachine.OpenSubKey("SOFTWARE\\IDEAStatiCa");
                string[] SubKeyNames = staticaRoot.GetSubKeyNames();
                Dictionary<double?, string> versions = new Dictionary<double?, string>();
                foreach (string SubKeyName in SubKeyNames)
                {
                    versions.Add(double.Parse(SubKeyName, CultureInfo.InvariantCulture.NumberFormat), SubKeyName);
                }
                double[] staticaVersions = staticaRoot.GetSubKeyNames().Select(x => double.Parse(x, CultureInfo.InvariantCulture.NumberFormat)).OrderByDescending(x => x).ToArray();
                double? lastverion = staticaVersions.FirstOrDefault();
                string versionString = versions[lastverion];
                if (lastverion == null) { throw new ArgumentNullException("IDEA StatiCa installation cannot be found"); }
                string path = $@"{versionString.Replace(",", ".")}\IDEAStatiCa\Designer";
                ideaStatiCaDir = staticaRoot.OpenSubKey(path).GetValue("InstallDir64").ToString();
            }
            catch
            {
                throw new ArgumentNullException("IDEA StatiCa installation cannot be found");
            }

            ProgressWindow pop = new ProgressWindow();
            if (windowWPF)
            {                
                pop.Show();
                pop.AddMessage(string.Format("IDEA StatiCa installation was found in '{0}'", ideaStatiCaDir));
            }

            //1.set joint
            joint = _joint;

            //2.create folder for joint
            string folder = this.joint.project.projectFolderPath;
            filePath = Path.Combine(folder, this.joint.Name);
            if (!Directory.Exists(this.filePath))
            {
                Directory.CreateDirectory(this.filePath);
            }


            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(IdeaResolveEventHandler);

            openModelGenerator = new OpenModelGenerator();
            openModelGenerator.CreateOpenModel(joint, filePath);

            //3. create IOM and results
            OpenModel openModel = openModelGenerator.openModel;
            OpenModelResult openModelResult = openModelGenerator.openModelResult;
            if (windowWPF)
            {
                pop.AddMessage(string.Format("Creating Openmodel and OpenmodelResult for '{0}'", joint.Name));
            }
            
            
            if (joint.template!= null)
            {
                Templates.ApplyProgrammedIDEAtemplate(openModel , joint);
            }

            string iomFileName = Path.Combine(folder, joint.Name, "IOM.xml");
            string iomResFileName = Path.Combine(folder, joint.Name, "IOMresults.xml");
            if (windowWPF)
            {
                pop.AddMessage(string.Format("Saving Openmodel and OpenmodelResult to XML for '{0}'", joint.Name));
            }
            

            // save to the files
            openModel.SaveToXmlFile(iomFileName);
            openModelResult.SaveToXmlFile(iomResFileName);

            if (windowWPF)
            {
                pop.AddMessage(string.Format("Creating IDEA StatiCa File '{0}'", joint.Name));
            }

            string filename = joint.Name + ".ideaCon";       
            var fileConnFileNameFromLocal = Path.Combine(folder,joint.Name, filename);
            
            var calcFactory = new ConnHiddenClientFactory(ideaStatiCaDir);//V20
            //string newBoltAssemblyName = "M16 8.8";
            
            var client = calcFactory.Create();
            try
            {
                // it creates connection project from IOM 
                if (windowWPF){pop.AddMessage(string.Format("Joint '{0}' was saved to:\n {1}", joint.Name, fileConnFileNameFromLocal));}
                client.CreateConProjFromIOM(iomFileName, iomResFileName, fileConnFileNameFromLocal);

            }
            catch (Exception e)
            {
                if (windowWPF) { pop.AddMessage(string.Format("Creating of IDEA file Joint '{0}' failed", joint.Name)); }
                throw new Exception(string.Format("Error '{0}'", e.Message));
            }

            finally
            {
                if (client != null)
                {
                    client.Close();
                }
            }

            if (Calculate)//Currently not working
            {
                if (windowWPF) { pop.AddMessage(string.Format("Calculation Joint {0} start up", joint.Name)); }
                string path = IdeaConnection.ideaStatiCaDir;//path to idea
                string pathToFile = joint.JointFilePath;//ideafile path
                string newBoltAssemblyName = "M16 8.8";
                var calcFactory2 = new ConnHiddenClientFactory(path);
                ConnectionResultsData conRes = null;
                var client2 = calcFactory2.Create();
                try
                {
                    client2.OpenProject(pathToFile);


                    try
                    {

                        // get detail about idea connection project
                        var projInfo = client2.GetProjectInfo();

                        var connection = projInfo.Connections.FirstOrDefault();//Select first connection
                        if (joint.ideaTemplateLocation != null)
                        {
                            if (windowWPF) { pop.AddMessage(string.Format("Template found at:'{0}' ", joint.ideaTemplateLocation)); }
                            client2.AddBoltAssembly(newBoltAssemblyName);//??Here Martin
                            client2.ApplyTemplate(connection.Identifier, joint.ideaTemplateLocation, null);
                            client2.SaveAsProject(pathToFile);
                            if (windowWPF) { pop.AddMessage(string.Format("Template applied to Joint {0}", joint.Name)); }
                        }

                        if (windowWPF) { pop.AddMessage(string.Format("Calculating Joint {0}", joint.Name)); }
                        conRes = client2.Calculate(connection.Identifier);
                        client2.SaveAsProject(pathToFile);
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
                        client2.CloseProject();
                    }
                }
                finally
                {
                    if (client2 != null)
                    {
                        client2.Close();
                    }
                }
                if (conRes != null)
                {
                    SaveResultsSummary(joint, conRes);
                }
            }


            if (windowWPF)
            {
                pop.Close();
            }
            
        }

        

        private static Assembly IdeaResolveEventHandler(object sender, ResolveEventArgs args)
        {
            AssemblyName asmName = new AssemblyName(args.Name);
            string assemblyFileName = System.IO.Path.Combine(ideaStatiCaDir, asmName.Name + ".dll");
            if (System.IO.File.Exists(assemblyFileName))
            {
                return Assembly.LoadFile(assemblyFileName);
            }
            return args.RequestingAssembly;
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
    }
}
        