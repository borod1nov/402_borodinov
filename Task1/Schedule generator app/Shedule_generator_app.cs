﻿using shedule_generator_lib;
class Program
{
    static void Main(string[] args)
    {
        int N = 7; // Количество участников
        int R = 7; // Количество туров
        int S = 4; // Количество площадок

        int population_size = 10000;
        int num_of_mutations = 10000;
        int num_of_crossovers = 10000;
        int steps_of_algorithm = 80;
        bool CTRL_C_NOT_PRESSED = true;

        Schedule_generator sg = new Schedule_generator(N, R, S, population_size, num_of_mutations, num_of_crossovers);

        Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler);

        void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Interruption of schedule generator...");
            args.Cancel = true;
            CTRL_C_NOT_PRESSED = false;
        }

        
        for (int i = 0; i < steps_of_algorithm; i++)
        {
            if (!CTRL_C_NOT_PRESSED)
                break;
            Schedule result = sg.Generate_best_schedule();
            result.Print_schedule();
        }

    }
}