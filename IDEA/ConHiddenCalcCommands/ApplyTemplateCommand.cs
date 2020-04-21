using IdeaStatiCa.ConnectionClient.Model;
using Microsoft.Win32;
using System.Windows.Forms;

namespace IdeaStatiCa.ConnectionClient.Commands
{
	public class ApplyTemplateCommand : ConnHiddenCalcCommandBase
	{
		public ApplyTemplateCommand(IConHiddenCalcModel model) : base(model)
		{
		}

		public override bool CanExecute(object parameter)
		{
			return (Model.IsIdea && Model.IsService && !IsCommandRunning);
		}

		public override void Execute(object parameter)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Idea Connection Template| *.contemp";
            /*
			if (openFileDialog.ShowDialog() == true)
			{
				var service = Model.GetConnectionService();
				var connection = (IConnectionId)parameter;

				var res = service.ApplyTemplate(connection.ConnectionId, openFileDialog.FileName, null);

				Model.SetStatusMessage(res);
			}
            */

		}
	}
}
