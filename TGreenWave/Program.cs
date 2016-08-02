using System;

using Microsoft.SolverFoundation.Common;
using Microsoft.SolverFoundation.Solvers;


namespace LK.TGreeNWave
{
    class Program
    {
        static void Main(string[] args)
        {


            SimplexSolver solver = new SimplexSolver();

            int savid, vzvid;
            solver.AddVariable("Saudi Arabia", out savid);
            solver.SetBounds(savid, 0, 9000);
            solver.AddVariable("Venezuela", out vzvid);
            solver.SetBounds(vzvid, 0, 6000);

            int gasoline, jetfuel, machinelubricant, cost;
            solver.AddRow("gasoline", out gasoline);
            solver.AddRow("jetfuel", out jetfuel);
            solver.AddRow("machinelubricant", out machinelubricant);
            solver.AddRow("cost", out cost);

            solver.SetCoefficient(gasoline, savid, 0.3);
            solver.SetCoefficient(gasoline, vzvid, 0.4);
            solver.SetBounds(gasoline, 2000, Rational.PositiveInfinity);
            solver.SetCoefficient(jetfuel, savid, 0.4);
            solver.SetCoefficient(jetfuel, vzvid, 0.2);
            solver.SetBounds(jetfuel, 1500, Rational.PositiveInfinity);
            solver.SetCoefficient(machinelubricant, savid, 0.2);
            solver.SetCoefficient(machinelubricant, vzvid, 0.3);
            solver.SetBounds(machinelubricant, 500, Rational.PositiveInfinity);


            solver.SetCoefficient(cost, savid, 20);
            solver.SetCoefficient(cost, vzvid, 15);
            solver.AddGoal(cost, 1, true);

            solver.Solve(new SimplexSolverParams());

            Console.WriteLine("SA {0}, VZ {1}, Gasoline {2}, Jet Fuel {3}, Machine Lubricant {4}, Cost {5}",
                solver.GetValue(savid).ToDouble(),
                solver.GetValue(vzvid).ToDouble(),
                solver.GetValue(gasoline).ToDouble(),
                solver.GetValue(jetfuel).ToDouble(),
                solver.GetValue(machinelubricant).ToDouble(),
                solver.GetValue(cost).ToDouble());
            Console.ReadLine();

            /*      var s = new Simplex(
                    new[] { 10.2, 422.3, 6.91, 853 },
                    new[,] {
                {0.1, 0.5, 0.333333, 1},
                {30, 15, 19, 12},
                {1000, 6000, 4100, 9100},
                {50, 20, 21, 10},
                {4, 6, 19, 30}
                    },
                    new double[] { 2000, 1000, 1000000, 640, 432 }
                  );

                  var answer = s.maximize();
                  Console.WriteLine(answer.Item1);
                  Console.WriteLine(string.Join(", ", answer.Item2)); */
        }


    }
}