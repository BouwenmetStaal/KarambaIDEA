using IdeaStatiCa.ConnectionClient.Model;
using Microsoft.Win32;
using System.Windows.Forms;

namespace IdeaStatiCa.ConnectionClient.Commands
{
	public class SaveAsProjectCommand : ConnHiddenCalcCommandBase
	{
		public SaveAsProjectCommand(IConHiddenCalcModel model) : base(model)
		{
		}

		public override bool CanExecute(object parameter)
		{
			return Model.IsService;
		}

		public override void Execute(object parameter)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "IdeaConnection | *.ideacon";
            
			//if (saveFileDialog.ShowDialog() == true)
			{
				var service = Model.GetConnectionService();
				service.SaveAsProject(saveFileDialog.FileName);
			}
            
		}
	}
}
