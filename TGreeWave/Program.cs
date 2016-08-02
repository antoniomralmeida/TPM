using System;
using NDesk.Options;

namespace LK.TGreeWave
{
    class Program
    {
        static void Main(string[] args)
        {
            string osmPath = "";
            int eps = -1;
            int minTraffic = -1;
            bool showHelp = false;

            OptionSet parameters = new OptionSet() {
                { "osm=", "path to the map file",                                                    v => osmPath = v},
                { "eps=",   "size of the eps-neighborhood to be considered (integer)",               v => eps = Convert.ToInt32(v)},
                { "minTraffic=", "minimum traffic considered (integer)",                             v => minTraffic = Convert.ToInt32(v)},
                { "h|?|help",                                                                        v => showHelp = v != null},
            };

            try
            {
                parameters.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("TGreenWave: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `TGreenWave --help' for more information.");
                return;
            }


            if (showHelp || string.IsNullOrEmpty(osmPath) || eps < 0 || minTraffic < 0)
            {
                ShowHelp(parameters);
                return;
            }
            
        }

        /// <summary>
        /// Prints a help message
        /// </summary>
        /// <param name="p">The parameters accepted by this program</param>
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: TGreenWave [OPTIONS]+");
            Console.WriteLine("Outputs hot routes found");
            Console.WriteLine();

            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
