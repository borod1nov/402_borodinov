using System.Windows;

namespace WPF__App
{
    public partial class Choose_experiment_window : Window
    {
        public Choose_experiment_window(List<string?> experiments_list)
        {
            InitializeComponent();
            ExperimentsListBox.ItemsSource = experiments_list;
        }

        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        public string? Experiment_name 
        { 
            get { return ExperimentsListBox.SelectedItem as string; }
        }
    }
}
