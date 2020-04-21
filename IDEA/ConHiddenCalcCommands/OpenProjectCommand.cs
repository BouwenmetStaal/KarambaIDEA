using IdeaStatiCa.ConnectionClient.Model;
using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace IdeaStatiCa.ConnectionClient.Commands
{
	public class OpenProjectCommand : ConnHiddenCalcCommandBase
	{
		public OpenProjectCommand(IConHiddenCalcModel model) : base (model)
		{
		}

		public override bool CanExecute(object parameter)
		{
			return (Model.IsIdea && !Model.IsService );
		}

		public override void Execute(object parameter)
		{
            string openFilePath = "";
			//openFileDialog.Filter = "IdeaConnection | *.ideacon";
            try
            {
                Debug.WriteLine("Creating the instance of IdeaRS.ConnectionService.Service.ConnectionSrv");

                var Service = Model.GetConnectionService();

                Debug.WriteLine("Opening the project file '{0}'", openFilePath);
                Service.OpenProject(openFilePath);

                var projectInfo = Service.GetProjectInfo();
                Model.SetConProjectData(projectInfo);

            }
            catch (Exception e)
            {
                Debug.Assert(false, e.Message);
                Model.SetStatusMessage(e.Message);
                Model.CloseConnectionService();
            }
        }
	}
}
