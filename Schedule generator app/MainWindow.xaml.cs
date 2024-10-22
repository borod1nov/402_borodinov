using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using schedule_generator_lib;
using WPF__App;

namespace WPF_App
{
    public partial class MainWindow : Window
    {
        public ViewData? Parameters { get; set; }
        public bool Stop_button {  get; set; }
        public MainWindow()
        {
            InitializeComponent();

            this.Parameters = new ViewData(0, 0, 0);
            this.DataContext = Parameters;
        }

        private async void Run_button_click(object sender, RoutedEventArgs e)
        {
            RunButton.IsEnabled = false;
            int population_size = 100000;
            int num_of_mutations = 100000;
            int num_of_crossovers = 100000;
            int steps_of_algorithm = 60;

            BindingExpression be1 = BindingOperations.GetBindingExpression(TextBox1, TextBox.TextProperty);
            be1.UpdateSource(); 
            BindingExpression be2 = BindingOperations.GetBindingExpression(TextBox2, TextBox.TextProperty);
            be2.UpdateSource();
            BindingExpression be3 = BindingOperations.GetBindingExpression(TextBox3, TextBox.TextProperty);
            be3.UpdateSource();

            Schedule_generator sg = new Schedule_generator(Parameters!.N, Parameters.R, Parameters.S, population_size, num_of_mutations, num_of_crossovers);

            int k = 1;
            Stop_button = false;
            while ((k <= steps_of_algorithm) & (Stop_button == false))
            {
                sg.Do_one_iteration_parallel();
                Schedule result = sg.best_schedule;

                DataTable dt = new DataTable();
                int n_columns = Parameters.S;
                int n_rows = Parameters.R;
                var table = Schedule.Different_view(result);
                for (int j = 1; j <= n_columns; j++)
                    dt.Columns.Add(j.ToString(), typeof(string));
                
                for (int i = 0; i < n_rows; i++)
                {
                    DataRow dr = dt.NewRow();
                    for (int j = 0; j < n_columns; j++)
                        dr[j] = table[i, j];
                    dt.Rows.Add(dr);
                }
                await Task.Delay(100).ContinueWith(_ =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        DataGrid1.ItemsSource = dt.DefaultView;
                        TextBlock1.Text = sg.best_score.ToString();
                        TextBlock2.Text = k.ToString();
                    });
                });
                k++;
            }
            RunButton.IsEnabled = true;
        }

        private void Stop_button_click(object sender, RoutedEventArgs e)
        {
            Stop_button = true;
            MessageBox.Show("Алгоритм остановлен");
        }
    }

    public class ViewData
    {
        public int N { get; set; }
        public int R { get; set; }
        public int S { get; set; }

        public ViewData(int N, int R, int S)
        {
            this.N = N;
            this.R = R;
            this.S = S;
        }
    }
}