using schedule_generator_lib;

const int population_size = 10000;
const int num_of_mutations = 10000;
const int num_of_crossovers = 10000;

var builder = WebApplication.CreateBuilder();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/initial", (int N, int R, int S) =>
{
    Schedule_generator sg = new Schedule_generator(N, R, S, population_size, num_of_mutations, num_of_crossovers);
    return Results.Json(new Request_type_1(N, R, S, sg.best_score, sg.population));
});

app.MapPost("/next", (Request_type_1 data) =>
{
    Schedule_generator sg = new Schedule_generator(data.N, data.R, data.S, population_size, num_of_mutations, num_of_crossovers);
    for (int i = 0; i < population_size; i++)
    {
        sg.population[i].N = data.N;
        sg.population[i].R = data.R;
        sg.population[i].S = data.S;
        for (int j = 0; j < data.R; j++)
            for (int k = 0; k < data.N; k++)
                sg.population[i].table[j, k] = data.population![i][j][k];
    }
    sg.best_score = data.best_score;
    sg.Do_one_iteration_parallel();
    return Results.Json(new Request_type_2(data.N, data.R, data.S, sg.population, sg.best_schedule, sg.best_score));
});

app.Run();

public class Request_type_1
{
    public int N { get; set; }
    public int R { get; set; }
    public int S { get; set; }
    public double best_score { get; set; }
    public int[][][]? population { get; set; }
    public Request_type_1(int N, int R, int S, double best_score, List<Schedule> population)
    {
        this.N = N;
        this.R = R;
        this.S = S;
        this.best_score = best_score;
        this.population = new int[population.Count][][];
        for (int i = 0; i < population.Count; i++)
        {
            this.population[i] = new int[R][];
            for (int j = 0; j < R; j++)
            {
                this.population[i][j] = new int[N];
                for (int k = 0; k < N; k++)
                    this.population[i][j][k] = population[i].table[j, k];
            }
        }
    }
    public Request_type_1()
    {
        N = 0;
        R = 0;
        S = 0;
        best_score = 0;
        population = null;
    }
}
public class Request_type_2
{
    public int N { get; set; }
    public int R { get; set; }
    public int S { get; set; }
    public int[][][] population { get; set; }
    public int[][] best_schedule { get; set; }
    public double best_score { get; set; }
    public Request_type_2(int N, int R, int S, List<Schedule> population, Schedule best_schedule, double best_score)
    {
        this.N = N;
        this.R = R;
        this.S = S;

        this.population = new int[population.Count][][];
        for (int i = 0; i < population.Count; i++)
        {
            this.population[i] = new int[R][];
            for (int j = 0; j < R; j++)
            {
                this.population[i][j] = new int[N];
                for (int k = 0; k < N; k++)
                    this.population[i][j][k] = population[i].table[j, k];
            }
        }

        this.best_schedule = new int[R][];
        for (int j = 0; j < R; j++)
        {
            this.best_schedule[j] = new int[N];
            for (int k = 0; k < N; k++)
                this.best_schedule[j][k] = best_schedule.table[j, k];
        }

        this.best_score = best_score;
    }
    public Request_type_2()
    {
        N = 0;
        R = 0;
        S = 0;
        best_score = 0;
        best_schedule = null;
        population = null;
    }
}
public partial class Program { }