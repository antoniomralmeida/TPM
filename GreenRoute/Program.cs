using NDesk.Options;
using System;
using Google.OrTools.ConstraintSolver;
using System.Linq;

namespace LK.GreenRoute
{
    class Program
    {
        static void Main(string[] args)
        {

            string osmPath = "";
            bool showHelp = false;

            Solve();

            OptionSet parameters = new OptionSet() {
                { "osm=", "path to the map file",                                                    v => osmPath = v},
                { "h|?|help",                                                                        v => showHelp = v != null},
            };

            try
            {
                parameters.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("TRoute: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `TRoute --help' for more information.");
                return;
            }

            if (showHelp || string.IsNullOrEmpty(osmPath))
            {
                ShowHelp(parameters);
                return;
            }

            HRDocument hr = new HRDocument();
            hr.Load(osmPath);
            hr.Webster();
        }

        /// <summary>
        /// Prints a help message
        /// </summary>
        /// <param name="p">The parameters accepted by this program</param>
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: TRoute [OPTIONS]+");
            Console.WriteLine("Outputs hot routes found");
            Console.WriteLine();

            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        private static void Solve()
        {
            Solver solver = new Solver("Zebra");

            int n = 5;

            //
            // Decision variables
            //

            // Colors
            IntVar red = solver.MakeIntVar(1, n, "red");
            IntVar green = solver.MakeIntVar(1, n, "green");
            IntVar yellow = solver.MakeIntVar(1, n, "yellow");
            IntVar blue = solver.MakeIntVar(1, n, "blue");
            IntVar ivory = solver.MakeIntVar(1, n, "ivory");

            // Nationality
            IntVar englishman = solver.MakeIntVar(1, n, "englishman");
            IntVar spaniard = solver.MakeIntVar(1, n, "spaniard");
            IntVar japanese = solver.MakeIntVar(1, n, "japanese");
            IntVar ukrainian = solver.MakeIntVar(1, n, "ukrainian");
            IntVar norwegian = solver.MakeIntVar(1, n, "norwegian");

            // Animal
            IntVar dog = solver.MakeIntVar(1, n, "dog");
            IntVar snails = solver.MakeIntVar(1, n, "snails");
            IntVar fox = solver.MakeIntVar(1, n, "fox");
            IntVar zebra = solver.MakeIntVar(1, n, "zebra");
            IntVar horse = solver.MakeIntVar(1, n, "horse");

            // Drink
            IntVar tea = solver.MakeIntVar(1, n, "tea");
            IntVar coffee = solver.MakeIntVar(1, n, "coffee");
            IntVar water = solver.MakeIntVar(1, n, "water");
            IntVar milk = solver.MakeIntVar(1, n, "milk");
            IntVar fruit_juice = solver.MakeIntVar(1, n, "fruit juice");

            // Smoke
            IntVar old_gold = solver.MakeIntVar(1, n, "old gold");
            IntVar kools = solver.MakeIntVar(1, n, "kools");
            IntVar chesterfields = solver.MakeIntVar(1, n, "chesterfields");
            IntVar lucky_strike = solver.MakeIntVar(1, n, "lucky strike");
            IntVar parliaments = solver.MakeIntVar(1, n, "parliaments");


            // for search
            IntVar[] all_vars =
              {parliaments, kools, chesterfields, lucky_strike, old_gold,
       englishman, spaniard, japanese, ukrainian, norwegian,
       dog, snails, fox, zebra, horse,
       tea, coffee, water, milk, fruit_juice,
       red, green, yellow, blue, ivory};

            //
            // Constraints
            //

            // Alldifferents
            solver.Add(new IntVar[]
                {red, green, yellow, blue, ivory}.AllDifferent());
            solver.Add(new IntVar[]
                {englishman, spaniard, japanese, ukrainian, norwegian}.AllDifferent());
            solver.Add(new IntVar[]
                {dog, snails, fox, zebra, horse}.AllDifferent());
            solver.Add(new IntVar[]
                {tea, coffee, water, milk, fruit_juice}.AllDifferent());
            solver.Add(new IntVar[]
                {parliaments, kools, chesterfields, lucky_strike, old_gold}.AllDifferent());

            //
            // The clues
            //
            solver.Add(englishman == red);
            solver.Add(spaniard == dog);
            solver.Add(coffee == green);
            solver.Add(ukrainian == tea);
            solver.Add(green == ivory + 1);
            solver.Add(old_gold == snails);
            solver.Add(kools == yellow);
            solver.Add(milk == 3);
            solver.Add(norwegian == 1);
            solver.Add((fox - chesterfields).Abs() == 1);
            solver.Add((horse - kools).Abs() == 1);
            solver.Add(lucky_strike == fruit_juice);
            solver.Add(japanese == parliaments);
            solver.Add((norwegian - blue).Abs() == 1);


            //
            // Search
            //
            DecisionBuilder db = solver.MakePhase(all_vars,
                                                  Solver.INT_VAR_DEFAULT,
                                                  Solver.INT_VALUE_DEFAULT);

            solver.NewSearch(db);

            IntVar[] p = { englishman, spaniard, japanese, ukrainian, norwegian };
            int[] ix = { 0, 1, 2, 3, 4 };
            while (solver.NextSolution())
            {
                int water_drinker = (from i in ix
                                     where p[i].Value() == water.Value()
                                     select i).First();
                int zebra_owner = (from i in ix
                                   where p[i].Value() == zebra.Value()
                                   select i).First();
                Console.WriteLine("The {0} drinks water.", p[water_drinker].ToString());
                Console.WriteLine("The {0} owns the zebra", p[zebra_owner].ToString());

            }

            Console.WriteLine("\nSolutions: {0}", solver.Solutions());
            Console.WriteLine("WallTime: {0}ms", solver.WallTime());
            Console.WriteLine("Failures: {0}", solver.Failures());
            Console.WriteLine("Branches: {0} ", solver.Branches());

            solver.EndSearch();

        }
    }

    
}