using System.Windows;

namespace WPF__App
{
    public partial class Enter_name_window : Window
    {
        public Enter_name_window()
        {
            InitializeComponent();
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        public string Experiment_name
        {
            get { return NameBox.Text; }
        }
    }
}
