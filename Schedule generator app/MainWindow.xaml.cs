using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using schedule_generator_lib;
using WPF__App;

namespace WPF_App
{
    public partial class MainWindow : Window
    {
        public ViewData? Parameters { get; set; }
        public bool Stop_button;
        public bool Load_button;
        public int population_size = 10000;
        public int num_of_mutations = 10000;
        public int num_of_crossovers = 10000;
        public int steps_of_algorithm = 60;

        public Schedule_generator? sg = null;
        public int n_iterations_made;
        public MainWindow()
        {
            InitializeComponent();

            this.Parameters = new ViewData(0, 0, 0);
            this.DataContext = Parameters;
            StopButton.IsEnabled = false;
            SaveButton.IsEnabled = false;
            CommandBinding customCommandBinding = new CommandBinding(SaveCommand, ExecutedSaveCommand, CanExecuteSaveCommand);
            this.CommandBindings.Add(customCommandBinding);
            SaveButton.Command = SaveCommand;
        }

        private async void Run_button_click(object sender, RoutedEventArgs e)
        {
            RunButton.IsEnabled = false;
            LoadButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            SaveButton.IsEnabled = false;

            BindingExpression be1 = BindingOperations.GetBindingExpression(TextBox1, TextBox.TextProperty);
            be1.UpdateSource(); 
            BindingExpression be2 = BindingOperations.GetBindingExpression(TextBox2, TextBox.TextProperty);
            be2.UpdateSource();
            BindingExpression be3 = BindingOperations.GetBindingExpression(TextBox3, TextBox.TextProperty);
            be3.UpdateSource();
            if ((Parameters!.N <= 0) | (Parameters.S <= 0) | (Parameters.R <= 0))
            {
                MessageBox.Show("Введите корректные параметры расписаний");
                RunButton.IsEnabled = true;
                LoadButton.IsEnabled = true;
                StopButton.IsEnabled = false;
                SaveButton.IsEnabled = false;
                return;
            }
            sg = new Schedule_generator(Parameters!.N, Parameters.R, Parameters.S, population_size, num_of_mutations, num_of_crossovers);

            n_iterations_made = 1;
            Stop_button = false;
            while ((n_iterations_made <= steps_of_algorithm) & (Stop_button == false))
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
                        TextBlock2.Text = n_iterations_made.ToString();
                    });
                });
                n_iterations_made++;
            }
            RunButton.IsEnabled = true;
            LoadButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            SaveButton.IsEnabled = true;
        }
        private async void Load_button_click(object sender, RoutedEventArgs e)
        {
            RunButton.IsEnabled = false;
            LoadButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            SaveButton.IsEnabled = false;
            using (var db = new ExperimentsContext())
            {
                var experiments_list = db.Experiments.Select(experiment => experiment.Name).ToList();
                if (experiments_list.Count == 0)
                {
                    MessageBox.Show("Сначала нужно провести эксперимент");
                    RunButton.IsEnabled = true;
                    LoadButton.IsEnabled = true;
                    StopButton.IsEnabled = false;
                    SaveButton.IsEnabled = false;
                    return;
                }
                open_window:
                Choose_experiment_window window = new Choose_experiment_window(experiments_list);
                window.Owner = this;
                if (window.ShowDialog() == true)
                {
                    Experiment exp = db.Experiments.Include(e => e.population).ToList().Single(e => e.Name == window.Experiment_name);

                    sg = new Schedule_generator(exp!.N, exp.R, exp.S, exp.population_size, exp.num_of_mutations, exp.num_of_crossovers);
                    for (int i = 0; i < exp.population!.Count; i++)
                    {
                        sg.population[i].table = JsonConvert.DeserializeObject<int[,]>(exp.population[i].table_in_json!)!;
                    }
                    n_iterations_made = exp.n_iterations_made;
                    Stop_button = false;
                    while ((n_iterations_made <= steps_of_algorithm) & (Stop_button == false))
                    {
                        sg.Do_one_iteration_parallel();
                        Schedule result = sg.best_schedule;

                        DataTable dt = new DataTable();
                        int n_columns = exp.S;
                        int n_rows = exp.R;
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
                                TextBlock2.Text = n_iterations_made.ToString();
                            });
                        });
                        n_iterations_made++;
                    }
                    RunButton.IsEnabled = true;
                    LoadButton.IsEnabled = true;
                    StopButton.IsEnabled = false;
                    SaveButton.IsEnabled = true;

                }
                else
                {
                    MessageBox.Show("Эксперимент не выбран");
                    goto open_window;
                }
            }
        }

        private void Stop_button_click(object sender, RoutedEventArgs e)
        {
            Stop_button = true;
            SaveButton.IsEnabled = true;
            MessageBox.Show("Алгоритм остановлен");
        }

        public static RoutedCommand SaveCommand = new RoutedCommand();
        private void ExecutedSaveCommand(object sender, ExecutedRoutedEventArgs e)
        {
        open_window:
            Enter_name_window window = new Enter_name_window();
            window.Owner = this;
            if (window.ShowDialog() == true)
            {
                if (window.Experiment_name != "")
                {
                    using (var db = new ExperimentsContext())
                    {
                        Experiment experiment = new Experiment
                        {
                            Name = window.Experiment_name,
                            n_iterations_made = this.n_iterations_made,
                            N = Parameters!.N,
                            R = Parameters.R,
                            S = Parameters.S,
                            population_size = this.population_size,
                            num_of_mutations = this.num_of_mutations,
                            num_of_crossovers = this.num_of_crossovers,
                        };
                        db.Experiments.Add(experiment);

                        foreach (Schedule schedule in sg!.population)
                        {
                            string json = JsonConvert.SerializeObject(schedule.table);
                            Schedule_table sch = new Schedule_table
                            {
                                Experiment = experiment,
                                table_in_json = json
                            };
                            db.Schedule_tables.Add(sch);
                        }
                        db.SaveChanges();
                        MessageBox.Show("Успешно сохранено");
                    }
                }
                else
                {
                    MessageBox.Show("Имя эксперимента не должно быть пустым");
                    goto open_window;
                }
            }
            else
            {
                MessageBox.Show("Имя эксперимента не введено");
                goto open_window;
            }
        }
        private void CanExecuteSaveCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
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
    class Experiment
    {
        public string? Name { get; set; }
        public int n_iterations_made { get; set; }
        public int N { get; set; }
        public int R { get; set; }
        public int S { get; set; }
        public int population_size { get; set; }
        public int num_of_mutations { get; set; }
        public int num_of_crossovers { get; set; }
        public List<Schedule_table>? population { get; set; } = new();
    }

    class Schedule_table
    {
        public int Id { get; set; }
        public string? table_in_json { get; set; }
        public Experiment? Experiment { get; set; }
    }

    class ExperimentsContext : DbContext
    {
        public DbSet<Experiment> Experiments { get; set; }
        public DbSet<Schedule_table> Schedule_tables { get; set; }
        public ExperimentsContext() => Database.EnsureCreated();
        protected override void OnConfiguring(DbContextOptionsBuilder o) => o.UseSqlite("Data Source=./../../../experiments.db");
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Experiment>().HasKey(exp => exp.Name);
        }
    }
}