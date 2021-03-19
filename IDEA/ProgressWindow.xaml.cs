using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KarambaIDEA.IDEA
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
        }
        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void AddMessage (string text)
        {
            //this.Dispatcher.Invoke(() =>
            //{
            //    Textbox1.Text = text;
            //});
            //Dispatcher.Invoke((Action)(() => Textbox1.Text = Textbox1.Text+"\n"+text));
            Dispatcher.Invoke(new Action(() =>
            {
                Textbox1.Text += text + Environment.NewLine;
            }), DispatcherPriority.Background);
            
        }

        private void Textbox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
