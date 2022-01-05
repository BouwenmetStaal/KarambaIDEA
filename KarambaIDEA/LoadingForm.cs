using Eto.Drawing;
using Eto.Forms;
using GH = Grasshopper;

namespace KarambaIDEA
{
	public class LoadingForm : Eto.Forms.Form
	{
		public string Message { get; set; }

		public LoadingForm()
		{
			((Window)this).Resizable = false;
			((Window)this).Title = "KarambaIDEA";
			InitializeForm();
		}

		public LoadingForm(string message)
		{
			
			((Window)this).Resizable = false;
			((Window)this).Title = "KarambaIDEA";
			Message = message;
			InitializeForm();
		}
		

		private void InitializeForm()
		{
			this.WindowStyle = WindowStyle.None;
			//Rhino.UI.Dialogs.ShowTextDialog("blablalb", "blablalb");
			//GH.Instances.ActiveCanvas.
			var canvas_location = GH.Instances.ActiveCanvas.Viewport.ControlMidPoint;
			var canvas_size = GH.Instances.ActiveCanvas.Size;
			
			this.Location = new Eto.Drawing.Point((int)canvas_location.X, (int)canvas_location.Y);
			this.Topmost = true;
			
			//this.Location = new Eto.Drawing.Point(canvas_location.X + canvas_size.Width/2 - 100, canvas_location.Y + canvas_size.Height / 2);

			Label label = new Label();
			label.Text = Message;
			//label.Text = Message + (int)canvas_location.X +" " +(int)canvas_location.Y;
			

			TableLayout tableLayout = new TableLayout();
			tableLayout.Padding = new Padding(60, 60, 60, 60);// space around the table's sides
			//tableLayout.Padding = new Padding(canvas_size.Width/10, canvas_size.Height/10, canvas_size.Width/10, canvas_size.Height/10);
			tableLayout.Spacing = new Size(5, 5);// space between each cell

			tableLayout.Rows.Add(new TableRow(label, null));
			TableLayout control = tableLayout;
			base.Content = new TableLayout
			{
				Padding = new Padding(10),
				Spacing = new Size(5, 5),
				Rows =
			{
				new TableRow(control),
			}
			};
		}
	}
}
