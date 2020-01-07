using IdeaRS.OpenModel.Connection;
using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using KarambaIDEA.Core;

namespace KarambaIDEA.IDEA
{
    public class HiddenCalculation : INotifyPropertyChanged
    {
        #region private fields
        public event PropertyChangedEventHandler PropertyChanged;
        bool isIdea;
        object service;
        string statusMessage;
        string ideaStatiCaDir;
        Assembly conLinkAssembly;
        dynamic serviceDynamic;
        ObservableCollection<ConnectionVM> connections;
        string results;
        public Joint joint;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public HiddenCalculation(Joint _joint)
        {
            //1.set joint
            joint = _joint;
            //IdeaStatica needs to be initialized before running CBFEM -tr is done by creating the instance of 
            //IdeaRS.ConnectionLink.ConnectionLink it can be only once when an application starts.
            connections = new ObservableCollection<ConnectionVM>();
            ideaStatiCaDir = Properties.Settings.Default.IdeaInstallDir;
            if (Directory.Exists(ideaStatiCaDir))
            {
                string ideaConnectionFileName = Path.Combine(ideaStatiCaDir, "IdeaConnection.exe");
                if (File.Exists(ideaConnectionFileName))
                {
                    IsIdea = true;
                    StatusMessage = string.Format("IdeaStatiCa installation was found in '{0}'", ideaStatiCaDir);

                    string ideaConLinkFullPath = System.IO.Path.Combine(ideaStatiCaDir, "IdeaRS.ConnectionLink.dll");
                    conLinkAssembly = Assembly.LoadFrom(ideaConLinkFullPath);
                    object obj = conLinkAssembly.CreateInstance("IdeaRS.ConnectionLink.ConnectionLink");
                    dynamic d = obj;
                }
            }

            if (!IsIdea)
            {
                StatusMessage = string.Format("ERROR IdeaStatiCa doesn't exist in '{0}'", ideaStatiCaDir);
            }

            this.OpenAndCalculate(joint);
            
            //this.Calculate(filepath);
            /*
            OpenProjectCmd = new CustomCommand(this.CanOpen, this.Open);//RAZ: openen van IDEA bestand
            CloseProjectCmd = new CustomCommand(this.CanClose, this.Close);
            CalculateConnectionCmd = new CustomCommand(this.CanCalculate, this.Calculate); //RAZ: berekening wordt hier aangeropen

            ConnectionGeometryCmd = new CustomCommand(this.CanGetGeometry, this.GetGeometry);*/
        }
        #endregion
        /*
        #region Commands
        public CustomCommand OpenProjectCmd { get; set; }
        public CustomCommand CloseProjectCmd { get; set; }
        public CustomCommand CalculateConnectionCmd { get; set; }
        public CustomCommand ConnectionGeometryCmd { get; set; }
        #endregion
        */
        #region Properties
        public object Service
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

        public string Results
        {
            get => results;
            set
            {
                results = value;
                NotifyPropertyChanged("Results");
            }
        }

        #endregion

        #region Command handlers
        /// <summary>
        /// Is it possible to open a new project
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool CanOpen(object param)
        {
            return (IsIdea && Service == null);
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="filepath">full filepath of .IdeaCon file</param>
        public void OpenAndCalculate(Joint joint) //RAZ: open the IDEA-file
        {
            string filepath = joint.jointFilePath;
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "IdeaConnection | *.ideacon";
            if (filepath != null)
            {
                try
                {
                    //TODO: fix bug, script only works when IDEA StatiCa is opened by user.

                    Debug.WriteLine("Creating the instance of IdeaRS.ConnectionService.Service.ConnectionSrv");

                    // create the instance of the ConnectionSrv
                    Service = conLinkAssembly.CreateInstance("IdeaRS.ConnectionService.Service.ConnectionSrv");
                    serviceDynamic = Service;

                    // open idea connection project file
                    Debug.WriteLine("Opening the project file '{0}'", filepath);

                    // open idea connection project file
                    serviceDynamic.OpenIdeaConProjectFile(filepath, 0);

                    List<ConnectionVM> connectionsVm = GetConnectionViewModels();

                    this.Connections = new ObservableCollection<ConnectionVM>(connectionsVm);
                                       
                    // calculate all connections in the project
                    var projectData = serviceDynamic.ConDataContract;
                    //var con2 = projectData.Connections[0];

                    Guid connectionId = new Guid();
                    
                    foreach (var con in projectData.Connections.Values)
                    {
                        connectionId = (Guid)(con.Header.ConnectionID);
                    }
                   
                    object resData = serviceDynamic.CalculateProject(connectionId);
                    ConnectionResultsData cbfemResults = (ConnectionResultsData)resData;
                    //SaveResultsSummary(joint, cbfemResults);
                    
                    SaveResultsSummary(joint, cbfemResults);

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
        }

        public void SaveResultsSummary(Joint joint, ConnectionResultsData cbfemResults)
        {
            List<CheckResSummary> result = cbfemResults.ConnectionCheckRes[0].CheckResSummary;
            joint.ResultsSummary = new ResultsSummary();

            joint.ResultsSummary.analysis = result[0].CheckValue;
            joint.ResultsSummary.plates = result[1].CheckValue;
            joint.ResultsSummary.welds = result[2].CheckValue;
            joint.ResultsSummary.buckling = result[3].CheckValue;
            string message = string.Empty;
            foreach (var r in result)
            {
                message += r.Name + ": " + r.UnityCheckMessage + ", ";
            }
            joint.ResultsSummary.summary = message;

        }

        /// <summary>
        /// Is there a project to close ?
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool CanClose(object param)
        {
            return Service != null;
        }

        /// <summary>
        /// Close the current idea connection project
        /// </summary>
        /// <param name="param"></param>
        public void Close(object param)
        {
            if (serviceDynamic == null)
            {
                return;
            }

            serviceDynamic.CloseServices();

            serviceDynamic = null;
            Service = null;

            Results = string.Empty;
            Connections.Clear();
        }

        public bool CanCalculate(object param)
        {
            return (IsIdea && Service != null);
        }

        /// <summary>
        /// Run CBFEM and return brief results
        /// </summary>
        /// <param name="param">View model of the selected connection</param>
        public void Calculate(object param)
        {
            var res = string.Empty;
            Results = "Running CBFEM";
            try
            {
                var conVM = (ConnectionVM)param;

                //Running CBFEM and getting results
                
                
            }
            catch (Exception e)
            {
                StatusMessage = e.Message;
            }
        }

        public bool CanGetGeometry(object param)
        {
            return (IsIdea && Service != null);
        }

        /// <summary>
        /// Get geometry of th selected connection
        /// </summary>
        /// <param name="param">View model of the selected connection</param>
        public void GetGeometry(object param)
        {
            try
            {
                var conVM = (ConnectionVM)param;
                IdeaRS.OpenModel.Connection.ConnectionData conData = serviceDynamic.GetConnectionModel(conVM.ConnectionId);
                
            }
            catch (Exception e)
            {
                StatusMessage = e.Message;
            }
        }
        #endregion

        private List<ConnectionVM> GetConnectionViewModels()
        {
            List<ConnectionVM> connectionsVm = new List<ConnectionVM>();
            // get information obaout all aconections in the project
            var projectData = serviceDynamic.ConDataContract;
            foreach (var con in projectData.Connections.Values)
            {
                connectionsVm.Add(new ConnectionVM(con));
            }

            return connectionsVm;
        }

        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class ConnectionVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        dynamic dynamicItem;

        public ConnectionVM(object item)
        {
            this.dynamicItem = item;
        }

        public string Name
        {
            get => (string)(dynamicItem.Header.Name);
        }

        public Guid ConnectionId
        {
            get => (Guid)(dynamicItem.Header.ConnectionID);
        }

        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        
    }
}
