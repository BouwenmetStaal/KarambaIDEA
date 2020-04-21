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

namespace KarambaIDEA.IDEA
{
    /// <summary>
    /// Main view model of the example
    /// </summary>
    /// //public class HiddenCalculation : INotifyPropertyChanged, IConHiddenCalcModel
    public class HiddenCalculationV20 : INotifyPropertyChanged, IConHiddenCalcModel
    {
        #region private fields
        public event PropertyChangedEventHandler PropertyChanged;
        bool isIdea;
        string statusMessage;
        string ideaStatiCaDir;
        ObservableCollection<ConnectionVM> connections;
        string results;
        ConnHiddenClientFactory CalcFactory { get; set; }
        ConnectionHiddenCheckClient IdeaConnectionClient { get; set; }
        IConnHiddenCheck service;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public HiddenCalculationV20(Joint joint)
        {
            connections = new ObservableCollection<ConnectionVM>();
            //ideaStatiCaDir = IDEA.Properties.Settings.Default.IdeaInstallDir;
            ideaStatiCaDir = @"C:\Release_20_UT_x64_2020-04-20_23-28_20.0.139";
            if (Directory.Exists(ideaStatiCaDir))
            {
                string ideaConnectionFileName = Path.Combine(ideaStatiCaDir, "IdeaConnection.exe");
                if (File.Exists(ideaConnectionFileName))
                {
                    IsIdea = true;
                    StatusMessage = string.Format("IdeaStatiCa installation was found in '{0}'", ideaStatiCaDir);
                    CalcFactory = new ConnHiddenClientFactory(ideaStatiCaDir);
                }
            }

            if (!IsIdea)
            {
                StatusMessage = string.Format("ERROR IdeaStatiCa doesn't exist in '{0}'", ideaStatiCaDir);
            }

            //ConnectionResultsData cbfemResults = OpenAndCalculate(joint);//V10.1
            //SaveResultsSummary(joint, cbfemResults);//V10.1

            //Op basis van de ConnectionID wordt de verbinding gevonden en berekend.
            //In v20 is dit terug te vinden in de method Execute te vinden in ConnectionGeometryCommand


            
            OpenProjectCmd = new OpenProjectCommand(this);
            ImportIOMCmd = new ImportIOMCommand(this);
            CloseProjectCmd = new CloseProjectCommand(this);
            CalculateConnectionCmd = new CalculateConnectionCommand(this);
            ConnectionGeometryCmd = new ConnectionGeometryCommand(this);
            SaveAsProjectCmd = new SaveAsProjectCommand(this);
            ConnectionToTemplateCmd = new ConnectionToTemplateCommand(this);
            ApplyTemplateCmd = new ApplyTemplateCommand(this);
            
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="joint">joint contains a filepath, results are saved in joint</param>
        public ConnectionResultsData OpenAndCalculate(Joint joint) //RAZ: open the IDEA-file
        {
            ConnectionResultsData cbfemResults = new ConnectionResultsData();
            string filepath = joint.JointFilePath;
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "IdeaConnection | *.ideacon";
            /*
            if (filepath != null)
            {
                try
                {
                    //TODO: fix bug, script only works when IDEA StatiCa is opened by user.

                    Debug.WriteLine("Creating the instance of IdeaRS.ConnectionService.Service.ConnectionSrv");

                    // create the instance of the ConnectionSrv
                    //Service = conLinkAssembly.CreateInstance("IdeaRS.ConnectionService.Service.ConnectionSrv");
                    serviceDynamic = Service;

                    // open idea connection project file
                    Debug.WriteLine("Opening the project file '{0}'", filepath);

                    // open idea connection project file
                    serviceDynamic.OpenIdeaConProjectFile(filepath, 0);

                    //List<ConnectionVM> connectionsVm = GetConnectionViewModels();

                    //this.Connections = new ObservableCollection<ConnectionVM>(connectionsVm);

                    // calculate all connections in the project
                    var projectData = serviceDynamic.ConDataContract;
                    //var con2 = projectData.Connections[0];

                    Guid connectionId = new Guid();

                    foreach (var con in projectData.Connections.Values)
                    {
                        connectionId = (Guid)(con.Header.ConnectionID);
                    }

                    object resData = serviceDynamic.CalculateProject(connectionId);
                    cbfemResults = (ConnectionResultsData)resData;



                    //close file
                    serviceDynamic.CloseServices();
                    serviceDynamic = null;
                    Service = null;
                    Results = string.Empty;
                    Connections.Clear();
                }
                catch (Exception e)
                {
                    Debug.Assert(false, e.Message);
                    StatusMessage = e.Message;
                    if (Service != null)
                    {
                        ((IDisposable)Service)?.Dispose();
                        Service = null;
                    }
                }
            }
            */
            return cbfemResults;
        }

        /// <summary>
        /// Save ResultSummary from IDEA StatiCa back into Core 
        /// </summary>
        /// <param name="joint">joint instance</param>
        /// <param name="cbfemResults">summary results retrieved from IDEA StatiCa</param>
        public void SaveResultsSummary(Joint joint, ConnectionResultsData cbfemResults)
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

        #region Commands
        public ICommand OpenProjectCmd { get; set; }
        public ICommand ImportIOMCmd { get; set; }
        public ICommand CloseProjectCmd { get; set; }
        public ICommand CalculateConnectionCmd { get; set; }
        public ICommand ConnectionGeometryCmd { get; set; }
        public ICommand SaveAsProjectCmd { get; set; }
        public ICommand ConnectionToTemplateCmd { get; set; }
        public ICommand ApplyTemplateCmd { get; set; }
        #endregion

        #region IConHiddenCalcModel

        /// <summary>
        /// Indicate if the installation of IdeaStatiCa exits
        /// </summary>
        public bool IsIdea
        {
            get => isIdea;

            set
            {
                isIdea = value;
                NotifyPropertyChanged("IsIdea");
            }
        }

        public bool IsService
        {
            get => Service != null;
        }

        public string Results
        {
            get => results;
            set
            {
                results = value;
                NotifyPropertyChanged("Results");
            }
        }


        public IConnHiddenCheck GetConnectionService()
        {
            if (Service != null)
            {
                return Service;
            }

            IdeaConnectionClient = CalcFactory.Create();
            Service = IdeaConnectionClient;
            return Service;
        }

        public void CloseConnectionService()
        {
            if (Service == null)
            {
                return;
            }

            IdeaConnectionClient.CloseProject();
            IdeaConnectionClient.Close();
            IdeaConnectionClient = null;
            Service = null;

            Results = string.Empty;
            Connections.Clear();
        }

        public void SetStatusMessage(string msg)
        {
            /*
            Application.Current.Dispatcher.BeginInvoke(
             (ThreadStart)delegate
             {
                 this.StatusMessage = msg;
             });
             */
        }

        public void SetResults(object res)
        {
            /*
            //Application.Current.Dispatcher.BeginInvoke(
             //(ThreadStart)delegate
             {
                 var jsonSetting = new JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(), Culture = CultureInfo.InvariantCulture };

                 if (res is ConnectionResultsData cbfemResults)
                 {

                     var jsonFormating = Formatting.Indented;
                     this.Results = JsonConvert.SerializeObject(cbfemResults, jsonFormating, jsonSetting);
                 }
                 else if (res is IdeaRS.OpenModel.Connection.ConnectionData conData)
                 {
                     var jsonFormating = Formatting.Indented;
                     Results = JsonConvert.SerializeObject(conData, jsonFormating, jsonSetting);
                 }
                 else
                 {
                     this.Results = (res == null ? string.Empty : res.ToString());
                 }
             });
             */
        }

        public void SetConProjectData(ConProjectInfo projectData)
        {
            List<ConnectionVM> connectionsVm = new List<ConnectionVM>();
            // get information obaout all aconections in the project
            foreach (var con in projectData.Connections)
            {
                connectionsVm.Add(new ConnectionVM(con));
            }

            this.Connections = new ObservableCollection<ConnectionVM>(connectionsVm);
        }

        #endregion

        #region View model's properties and methods

        private IConnHiddenCheck Service
        {
            get => service;
            set
            {
                service = value;
                NotifyPropertyChanged("Service");
            }
        }

        /// <summary>
        /// The list of view models for all connections in the project
        /// </summary>
        public ObservableCollection<ConnectionVM> Connections
        {
            get => connections;
            set
            {
                connections = value;
                NotifyPropertyChanged("Connections");
            }
        }

        /// <summary>
        /// Notification in the status bar
        /// </summary>
        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                statusMessage = value;
                NotifyPropertyChanged("StatusMessage");
            }
        }

        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}