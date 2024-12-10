using System.Collections.Concurrent;
namespace schedule_generator_lib
{
    public class Schedule
    {
        public int[,] table;
        public int N;
        public int R;
        public int S;

        public Schedule(int N, int R, int S)
        {
            table = new int[R, N];
            this.N = N;
            this.R = R;
            this.S = S;

            for (int i = 0; i < R; i++)
                for (int j = 0; j < N; j++)
                    table[i, j] = 0;
        }

        public static Schedule Generate_random_schedule(int N, int R, int S)
        {
            Schedule result = new Schedule(N, R, S);
            for (int i = 0; i < R; i++)
            {
                int[] participants = new int[N];
                for (int j = 0; j < N; j++)
                    participants[j] = j;
                Random.Shared.Shuffle(participants);

                int[] places = new int[S];
                for (int j = 0; j < S; j++)
                    places[j] = j + 1;
                Random.Shared.Shuffle(places);

                for (int j = 0; j < N / 2 * 2; j = j + 2)
                {
                    int player1 = participants[j];
                    int player2 = participants[j + 1];
                    int place = places[j / 2];
                    result.table[i, player1] = place;
                    result.table[i, player2] = place;
                    if (N % 2 == 1)
                    {
                        int inactive_player = participants[N - 1];
                        result.table[i, inactive_player] = -1;
                    }
                }
            }
            return result;
        }
        public static Schedule Mutation(Schedule s1)
        {
            int N = s1.N;
            int R = s1.R;
            int S = s1.S;
            if (N < 4)
                return s1;
            Schedule result = new Schedule(N, R, S);
            Random rnd = new Random();
            int i = rnd.Next(R);
            for (int k = 0; k < R; k++)
                for (int j = 0; j < N; j++)
                    result.table[k, j] = s1.table[k, j];
            while (true)
            {
                int j1 = rnd.Next(N);
                int j2 = rnd.Next(N);
                if ((s1.table[i, j1] != -1) && (s1.table[i, j2] != -1) && (s1.table[i, j1] != s1.table[i, j2]))
                {
                    result.table[i, j1] = s1.table[i, j2];
                    result.table[i, j2] = s1.table[i, j1];
                    return result;
                }
            }
        }
        public static Schedule Crossover(Schedule s1, Schedule s2)
        {
            int N = s1.N;
            int R = s1.R;
            int S = s1.S;
            Schedule result = new Schedule(N, R, S);

            for (int i = 0; i < R / 2; i++)
                for (int j = 0; j < N; j++)
                    result.table[i, j] = s1.table[i, j];

            for (int i = R / 2; i < R; i++)
                for (int j = 0; j < N; j++)
                    result.table[i, j] = s2.table[i, j];

            return result;
        }
        public double Calculate_score()
        {
            int[] num_of_opponents_of_players = new int[N];
            for (int j = 0; j < N; j++)
            {
                int[] opponents_of_player = new int[R];
                for (int i = 0; i < R; i++)
                {
                    int place = table[i, j];
                    if (place == -1)
                        opponents_of_player[i] = -1;
                    else
                        for (int k = 0; k < N; k++)
                            if ((table[i, k] == table[i, j]) && (k != j))
                                opponents_of_player[i] = k;
                }
                num_of_opponents_of_players[j] = opponents_of_player.Where(elem => elem != -1).Distinct().Count();
            }
            double a_metric = num_of_opponents_of_players.Average();

            int[] num_of_places_of_players = new int[N];
            for (int j = 0; j < N; j++)
            {
                int[] places_of_player = new int[R];
                for (int i = 0; i < R; i++)
                    places_of_player[i] = table[i, j];
                num_of_places_of_players[j] = places_of_player.Where(elem => elem != -1).Distinct().Count();
            }
            double b_metric = num_of_places_of_players.Average();
            return a_metric * R + b_metric;
        }
        public static string[,] Different_view(Schedule s1)
        {
            int N = s1.N;
            int R = s1.R;
            int S = s1.S;
            string[,] result = new string[R, S];
            for (int i = 0; i < R; i++)
                for (int j = 0; j < S; j++)
                {
                    int first = 0;
                    int second = 0;
                    int count = 0;
                    int k = 0;
                    while ((k < N) & (count < 2))
                    {
                        if (s1.table[i, k] == j + 1)
                        {
                            if (count == 0)
                                first = k;
                            else if (count == 1)
                                second = k;
                            count++;
                        }
                        k++;
                    }
                    if (count == 0)
                        result[i, j] = "X";
                    else
                    {
                        first++;
                        second++;
                        result[i, j] = first.ToString() + " vs " + second.ToString();
                    }
                }
            return result;
        }
    }

    public class Schedule_generator
    {
        public int N;
        public int R;
        public int S;
        int population_size;
        int num_of_mutations;
        int num_of_crossovers;
        public List<Schedule> population;
        List<double> scores;
        public Schedule best_schedule;
        public double best_score;

        public Schedule_generator(int N, int R, int S, int population_size, int num_of_mutations, int num_of_crossovers)
        {
            this.N = N;
            this.R = R;
            this.S = S;
            this.population_size = population_size;
            this.num_of_mutations = num_of_mutations;
            this.num_of_crossovers = num_of_crossovers;

            population = new List<Schedule>();
            for (int i = 0; i < population_size; i++)
                population.Add(Schedule.Generate_random_schedule(N, R, S));
            scores = (new double[population_size + num_of_crossovers + num_of_mutations]).ToList();
            best_schedule = population[0];
            best_score = 0;
        }
        public void Add_children_parallel()
        {
            ConcurrentBag<Schedule> child_bag = new ConcurrentBag<Schedule>();
            Parallel.For(0, num_of_crossovers, _ =>
            {
                Random rnd = new Random();
                int parent1 = Random.Shared.Next(population_size);
                int parent2 = Random.Shared.Next(population_size);
                Schedule child = Schedule.Crossover(population[parent1], population[parent2]);
                child_bag.Add(child);
            });
            population.AddRange(child_bag);
        }
        public void Add_mutated_parallel()
        {
            ConcurrentBag<Schedule> mutated_bag = new ConcurrentBag<Schedule>();
            Parallel.For(0, num_of_mutations, _ =>
            {
                Random rnd = new Random();
                int to_be_mutated = Random.Shared.Next(population_size + num_of_crossovers);
                Schedule mutated = Schedule.Mutation(population[to_be_mutated]);
                mutated_bag.Add(mutated);
            });
            population.AddRange(mutated_bag);
        }
        public void Calculate_scores_parallel()
        {
            ConcurrentBag<double> scores_bag = new ConcurrentBag<double>();
            Parallel.For(0, population.Count, i =>
            {
                double score = population[i].Calculate_score();
                scores[i] = score;
            });
        }
        public List<Schedule> Tournament_selection_parallel()
        {
            ConcurrentBag<Schedule> result = new ConcurrentBag<Schedule>();
            Parallel.For(0, population_size, _ =>
            {
                int index1 = Random.Shared.Next(population.Count);
                int index2 = Random.Shared.Next(population.Count);
                int index3 = Random.Shared.Next(population.Count);

                int max_ind = index1;
                if (scores[index2] > scores[index1])
                    max_ind = index2;
                if (scores[index3] > scores[max_ind])
                    max_ind = index3;
                result.Add(population[max_ind]);
                if (scores[max_ind] > best_score)
                {
                    best_score = scores[max_ind];
                    best_schedule = population[max_ind];
                }
            });
            return result.ToList();
        }
        public void Do_one_iteration_parallel()
        {
            Add_children_parallel();
            Add_mutated_parallel();
            Calculate_scores_parallel();
            population = Tournament_selection_parallel();
        }
    }
}
