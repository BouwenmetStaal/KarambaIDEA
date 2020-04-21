using IdeaStatiCa.ConnectionClient.Model;
using System;
using System.Threading.Tasks;

namespace IdeaStatiCa.ConnectionClient.Commands
{
	public class ConnectionGeometryCommand : ConnHiddenCalcCommandBase
	{
		public ConnectionGeometryCommand(IConHiddenCalcModel model) : base(model)
		{
		}

		public override bool CanExecute(object parameter)
		{
			return (Model.IsIdea && Model.IsService && !IsCommandRunning);
		}

		public override void Execute(object parameter)
		{
			var res = string.Empty;
			IsCommandRunning = true;
			Model.SetResults("Getting geometry of the connection");
			var calculationTask = Task.Run(() =>
			{
				try
				{
					var conVM = (IConnectionId)parameter;
					var Service = Model.GetConnectionService();

					IdeaRS.OpenModel.Connection.ConnectionData conData = Service.GetConnectionModel(conVM.ConnectionId);

					if (conData != null)
					{
						Model.SetResults(conData);
					}
					else
					{
						Model.SetResults("No data");
					}
				}
				catch (Exception e)
				{
					Model.SetStatusMessage(e.Message);
				}
				finally
				{
					IsCommandRunning = false;
				}
			});
		}
	}
}
