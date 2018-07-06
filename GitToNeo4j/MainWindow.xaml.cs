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

namespace GitToNeo4j
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel viewmodel = new ViewModel();

        public MainWindow()
        {
            InitializeComponent();
            this.viewmodel.StatusChanged += (arg) => this.StatusChanged(arg);
            this.viewmodel.ProgressChanged += (arg) => this.ProgressChanged(arg);
        }

        private void OnClone(object sender, RoutedEventArgs e)
        {            
            this.viewmodel.StartCloning(UrlInput.Text, LokalPath.Text);            
        }

        private void OnCommitsToDb(object sender, RoutedEventArgs e)
        {            
            this.viewmodel.CommitsToDb(new ViewModel.AnalysisOptions()
            {
                LocalPath = this.LokalPath.Text,
                AnalysisPath = this.LokalAnalysisPath.Text
            });
        }


        private void OnParseAst(object sender, RoutedEventArgs e)
        {
            this.viewmodel.ParseAst(new ViewModel.AnalysisOptions()
            {
                LocalPath = this.LokalPath.Text,
                AnalysisPath = this.LokalAnalysisPath.Text
            });

        }

        private void OnClear(object sender, RoutedEventArgs e)
        {
            this.viewmodel.Update(new ViewModel.AnalysisOptions()
            {
                LocalPath = this.LokalPath.Text,
                AnalysisPath = this.LokalAnalysisPath.Text
            });
            this.viewmodel.ClearDb();
        }

        private void StatusChanged(string arg)
        {
            // todo, check if this thread is the ui thread
            {
                Application.Current.Dispatcher.BeginInvoke(
                  DispatcherPriority.Background,
                  new Action(() => this.Output.Text += DateTime.Now.ToString("HH:mm:ss : ") + arg + "\n"));
            }
        }

        private void ProgressChanged(double arg)
        {
            // todo, check if this thread is the ui thread
            {
                Application.Current.Dispatcher.BeginInvoke(
                  DispatcherPriority.Background,
                  new Action(() =>
                  {
                      if (arg < 0)
                      {
                          this.SharedProgressBar.IsEnabled = false;
                          this.SharedProgressBar.Value = 0;
                      }
                      else
                      {
                          this.SharedProgressBar.IsEnabled = true;
                          this.SharedProgressBar.Value = arg;
                      }
                  }));
            }
        }
    }
}
