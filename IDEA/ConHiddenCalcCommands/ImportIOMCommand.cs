using IdeaStatiCa.ConnectionClient.Model;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace IdeaStatiCa.ConnectionClient.Commands
{
	public class ImportIOMCommand : ConnHiddenCalcCommandBase
	{
		public ImportIOMCommand(IConHiddenCalcModel model) : base(model)
		{
		}

		public override bool CanExecute(object parameter)
		{
			return (Model.IsIdea && !Model.IsService);
		}

		public override void Execute(object parameter)
		{
            Debug.WriteLine("Creating the instance of IdeaRS.ConnectionService.Service.ConnectionSrv");


            var service = Model.GetConnectionService();

            string iomFileName = "";
            string resultsFileName = Path.ChangeExtension("", ".xmlR");

            string tempProjectFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Path.GetRandomFileName());
            try
            {
                // create temporary idea connection project
                service.CreateConProjFromIOM(iomFileName, resultsFileName, tempProjectFileName);

                // open it
                Debug.WriteLine("Opening the project file '{0}'", tempProjectFileName);
                service.OpenProject(tempProjectFileName);

                var projectInfo = service.GetProjectInfo();
                Model.SetConProjectData(projectInfo);
            }
            catch (Exception e)
            {
                Debug.Assert(false, e.Message);
                Model.SetStatusMessage(e.Message);
                Model.CloseConnectionService();
            }
            finally
            {
                // delete temp file
                if (File.Exists(tempProjectFileName))
                {
                    File.Delete(tempProjectFileName);
                }
            }
        }
	}
}
