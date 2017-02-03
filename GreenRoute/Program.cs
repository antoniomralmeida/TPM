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

        static void Solve()
        {
            Console.WriteLine("\n---- Job shop Scheduling Program ----");
            JobShop jobShop = new JobShop();
            jobShop.RunJobShopScheduling("Jobshop");
        }
    }

    
}