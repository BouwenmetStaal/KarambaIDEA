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
    public class IdeaConnection_2
    {
        public int JointReference;
        private readonly string _filepath;
        private readonly string _projectName;

        private string _connectionNameRef;
        private bool _connectionIdSet = false;
        private string _connectionId;

        public IdeaConnectionResult Results = null;

        public IdeaServiceModel ServiceModel = KarambaIDEA.IDEA.IdeaServiceModel.Instance;

        public string FilePath { get { return _filepath; } }
        public string ConnectionNameRef { get { return String.IsNullOrEmpty(_connectionNameRef) ? "Default" : _connectionNameRef; } }

        public IdeaConnection_2(string refFilePath, int jointId, bool loadInfo, string conItemName = "")
        {
            JointReference = jointId;
            _filepath = refFilePath;
            _connectionNameRef = conItemName;

#warning implement loadInfo
        }

        public IdeaConnection_2(IdeaConnection_2 connection)
        {
            JointReference = connection.JointReference;
            _filepath = connection._filepath;
            _connectionId = connection._connectionId;
            _connectionNameRef = connection._connectionNameRef;
            _connectionIdSet = connection._connectionIdSet;
            _projectName = connection._projectName;
        }

        public IdeaConnection_2(string folder, KarambaIdeaJoint joint, bool userFeedback)
        {
            ProgressWindow pop = new ProgressWindow();

            string filePath = Path.Combine(folder, joint.Name);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            string iomFileName = Path.Combine(folder, joint.Name, "IOM.xml");
            string iomResFileName = Path.Combine(folder, joint.Name, "IOMresults.xml");

            if (userFeedback)
            {
                pop.AddMessage(string.Format("Saving Openmodel and OpenmodelResult to XML for '{0}'", joint.Name));
            }

            joint.SaveOpenModel(iomFileName);
            joint.SaveOpenModelResult(iomResFileName);

            if (userFeedback)
            {
                pop.AddMessage(string.Format("Creating IDEA StatiCa File '{0}'", joint.Name));
            }

            string filename = joint.Name + ".ideaCon";
            string filenameSaveAs = joint.Name + "_Modified" + ".ideaCon";
            var fileConnFileNameFromLocal = Path.Combine(folder, joint.Name, filename);

            if (userFeedback)
            {
                pop.AddMessage(string.Format("Joint '{0}' was saved to:\n {1}", joint.Name, fileConnFileNameFromLocal));
            }

            var client = ServiceModel.GetConnectionService();

            ConnectionFromIOM(iomFileName, iomResFileName, fileConnFileNameFromLocal, true, client);

            _filepath = fileConnFileNameFromLocal;
            JointReference = joint.id;
            SetConnectionId(client);

            if (joint.TemplateAssigns.Count > 0)
            {
                List<IdeaModification> listMods = new List<IdeaModification>();

                foreach (var templateAssign in joint.TemplateAssigns)
                {
                    if (templateAssign is IdeaTemplateAssignFull full)
                        listMods.Add(new IdeaModifyApplyTemplate_Full(full));
                    else if (templateAssign is IdeaTemplateAssignPartial partial)
                        listMods.Add(new IdeaModifyApplyTemplate_Partial(partial));
                }
                ModifyConnection(listMods, client);
            }

            var fileConnFileNameFromLocalSave = Path.Combine(folder, joint.Name, filenameSaveAs);
            client.SaveAsProject(fileConnFileNameFromLocalSave);

            _filepath = fileConnFileNameFromLocalSave;

            //Save(client);

            ServiceModel.CloseProject();

            if (userFeedback)                
                pop.Close();
        }

        public override string ToString()
        {
            return "Idea Connection: " + Path.GetFileName(_filepath) + " (" + ConnectionNameRef + ")" ;
        }

        private void SetConnectionId(ConnectionHiddenCheckClient client)
        {
            var projInfo = client.GetProjectInfo();

            if (!String.IsNullOrEmpty(_connectionNameRef))
                _connectionId = projInfo.Connections.Where(x => x.Name == _connectionNameRef).FirstOrDefault().Identifier;
            else
                _connectionId = projInfo.Connections.FirstOrDefault().Identifier;

            _connectionIdSet = true;
        }

        private static string ConnectionFromIOM(string iomModelPath, string IomResultPath, string conFilePath, bool keepOpen, ConnectionHiddenCheckClient client)
        {
            try
            {
                client.CreateConProjFromIOM(iomModelPath, IomResultPath, conFilePath);

                if (keepOpen)
                {
                    client.OpenProject(conFilePath);
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Error '{0}'", e.Message));
            }
            return conFilePath;
        }

        /// <summary>
        /// Opens the current Connection in the Conneciton HiddenCheckClient. If already open does nothing
        /// </summary>
        public void Open(ConnectionHiddenCheckClient client)
        {
            if (isOpen(client))
                return;
            else
                client.OpenProject(_filepath);

            if (!_connectionIdSet)
                SetConnectionId(client);
        }

        public void CloseProject(ConnectionHiddenCheckClient client)
        {
            client.CloseProject();
        }

        /// <summary>
        /// Checks whether the current connection is already open in the client.
        /// </summary>
        /// <returns></returns>
        private bool isOpen(ConnectionHiddenCheckClient client)
        {
#warning need to beable to see whether a project file is already opened. Need to ask if this is possible.
            //if (client.State == System.ServiceModel.CommunicationState.Opened)
            //{
            //    ConProjectInfo info = client.GetProjectInfo();
            //    if (info.Name == _projectName)
            //        return true;
            //}
            return false;
        }

        public void Save(ConnectionHiddenCheckClient client)
        {
            client.Save();
        }

        public void ModifyConnection(List<IdeaModification> modifications, ConnectionHiddenCheckClient client)
        {
            //order modifications
            modifications.OrderBy(x => x.ModifyHeirachy);

            foreach (var modification in modifications)
                modification.ModifyConnection(client, _connectionId);
        }

        public void CalculateConnection(List<IdeaModification> modifications, bool userFeedback)
        {
            ProgressWindow pop = new ProgressWindow();

            var client = ServiceModel.GetConnectionService();

            ConnectionResultsData conRes = null;

            Open(client);

            try
            {
                conRes = client.Calculate(_connectionId);
#if (DEBUG)
                //client.SaveAsProject(pathToFile);
#endif
                Results = new IdeaConnectionResult(conRes);
            }
            catch (Exception e)
            {
                if (userFeedback)
                {
                    pop.Close();
                }
                throw new Exception(string.Format("Error '{0}'", e.Message));
            }
            finally
            {
                // Delete temps in case of a crash
                client.CloseProject();
            }
        }
    }

#warning This class is no longer required.
    public class IdeaConnection
    {
        public OpenModelGenerator openModelGenerator; //TODO I think the Project should provide the OpenModel. i.e project.ExportJointtoIDEA(int Joint) 
        public Joint joint;
        public string filePath = "";
        public static string ideaStatiCaDir;
        //bool windowWPF = true;//Mouse controls may freeze when true https://discourse.mcneel.com/t/mouse-click-not-working/73010/17
        //https://www.grasshopper3d.com/forum/topics/bug-grasshopper-partly-freezes-after-any-export-operation

        /// <summary>
        /// Constructor for an IdeaConnection based on a joint
        /// </summary>
        /// <param name="_joint">Joint object</param>
        public IdeaConnection(Joint _joint, bool userFeedback, bool Calculate = false)
        {
            try
            {
#warning  duplicated code created in IdeaVersion.

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

            //LoadingForm form = new IDEA.LoadingForm("IDEA StatiCa started", p_canvas);
            //form.Show();
            //form.AddMessage(string.Format("IDEA StatiCa installation was found in '{0}'", ideaStatiCaDir));

            ProgressWindow pop = new ProgressWindow();
            if (userFeedback)
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
            if (userFeedback)
            {
                pop.AddMessage(string.Format("Creating Openmodel and OpenmodelResult for '{0}'", joint.Name));
            }

            if (joint.template != null)
            {
                IdeaTemplates.ApplyProgrammedIDEAtemplate(openModel, joint);
            }

            string iomFileName = Path.Combine(folder, joint.Name, "IOM.xml");
            string iomResFileName = Path.Combine(folder, joint.Name, "IOMresults.xml");
            if (userFeedback)
            {
                pop.AddMessage(string.Format("Saving Openmodel and OpenmodelResult to XML for '{0}'", joint.Name));
            }

            // save to the files
            openModel.SaveToXmlFile(iomFileName);
            openModelResult.SaveToXmlFile(iomResFileName);

            if (userFeedback)
            {
                pop.AddMessage(string.Format("Creating IDEA StatiCa File '{0}'", joint.Name));
            }

            string filename = joint.Name + ".ideaCon";
            var fileConnFileNameFromLocal = Path.Combine(folder, joint.Name, filename);
            string filename2 = joint.Name + "2.ideaCon";
            var fileConnFileNameFromLocal2 = Path.Combine(folder, joint.Name, filename2);

            var calcFactory = new ConnHiddenClientFactory(ideaStatiCaDir);//V20


#warning Need to enable Bolt assembly through IOM.
            //string newBoltAssemblyName = "M16 8.8";

            var client = calcFactory.Create();
            try
            {
                // it creates connection project from IOM 
                if (userFeedback) { pop.AddMessage(string.Format("Joint '{0}' was saved to:\n {1}", joint.Name, fileConnFileNameFromLocal)); }
                client.CreateConProjFromIOM(iomFileName, iomResFileName, fileConnFileNameFromLocal);


                //Apply template if template location is defined
                if (joint.ideaTemplateLocation != null)
                {
                    client.OpenProject(fileConnFileNameFromLocal);
                    var projInfo = client.GetProjectInfo();
                    string newBoltAssemblyName = "M16 8.8";
                    var connection = projInfo.Connections.FirstOrDefault();//Select first connection
                    if (userFeedback)
                    {
                        pop.AddMessage(string.Format("Template with path applied: '{0}'", joint.ideaTemplateLocation));
                    }
                    client.AddBoltAssembly(newBoltAssemblyName);

                    client.ApplyTemplate(connection.Identifier, joint.ideaTemplateLocation, null);
                    client.SaveAsProject(fileConnFileNameFromLocal);



                }
            }


            catch (Exception e)
            {
                if (userFeedback) { pop.AddMessage(string.Format("Creating of IDEA file Joint '{0}' failed", joint.Name)); }
                throw new Exception(string.Format("Error '{0}'", e.Message));
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
            return;
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
    }
}
        