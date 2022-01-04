using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(100);
            dt.Tick += dtTicker;
            dt.Start();
        }

        private int increment = 0;

        private void dtTicker (object sender, EventArgs e)
        {
            increment++;
            TimerLabel.Content ="Elapsed time: " +increment.ToString()+" sec";
        }

        private void button_close_Click(object sender, RoutedEventArgs e)
        {
           
            this.Close();
        }

        public void AddMessage (string text)
        {
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
