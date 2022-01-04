using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Windows.Threading;
using System.Threading;

namespace Tester
{
    class Test_Eto_forms : Eto.Forms.Form
	{
		public Test_Eto_forms()
		{
			// Set ClientSize instead of Size, as each platform has different window border sizes
			ClientSize = new Size(600, 400);

			// Title to show in the title bar
			Title = "Hello, Eto.Forms";

			// Content of the form
			Content = new Label { Text = "Some content", VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Center };
		}
	}

}


