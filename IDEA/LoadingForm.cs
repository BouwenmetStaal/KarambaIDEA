using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GH = Grasshopper;

using System.Windows.Threading;



namespace KarambaIDEA.IDEA
{
	public class LoadingForm : Eto.Forms.Form
	{

		public string Message { get; set; }
		public KarambaIDEA.Core.Point Canvas_location { get; set; }

		public LoadingForm()
		{
			((Window)this).Resizable = false;
			((Window)this).Title = "KarambaIDEA";
			InitializeForm();
		}

		public LoadingForm(string message, KarambaIDEA.Core.Point canvas_location)
		{
			((Window)this).Resizable = false;
			((Window)this).Title = "KarambaIDEA";
			Message = message;
			Canvas_location = canvas_location;
			InitializeForm();
		}

		public void AddMessage(string text)
		{
			this.Message += text + Environment.NewLine;
			InitializeForm();
		}

		private void InitializeForm()
		{
			this.WindowStyle = WindowStyle.None;
			//var canvas_location = GH.Instances.ActiveCanvas.Viewport.ControlMidPoint;
			//var canvas_size = GH.Instances.ActiveCanvas.Size;
			this.Location = new Eto.Drawing.Point((int)Canvas_location.X, (int)Canvas_location.Y);
			//this.Location = new Eto.Drawing.Point(200, 200);
			this.Topmost = true;
			//this.Location = new Eto.Drawing.Point(canvas_location.X + canvas_size.Width/2 - 100, canvas_location.Y + canvas_size.Height / 2);
			
			Label label = new Label();
			label.Text = Message;


			TableLayout tableLayout = new TableLayout();
			tableLayout.Padding = new Padding(100, 200, 100, 200);
			tableLayout.Spacing = new Size(5, 5);

			tableLayout.Rows.Add(new TableRow(label, null));
			TableLayout control = tableLayout;
			base.Content = new TableLayout
			{
				Padding = new Padding(5),
				Spacing = new Size(5, 5),
				Rows =
			{
				new TableRow(control),
			}
			};
		}
	}
}
