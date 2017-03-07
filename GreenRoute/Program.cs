using NDesk.Options;
using System;
using Google.OrTools.ConstraintSolver;
using System.Linq;
using System.Collections.Generic;
using LK.GPXUtils;

namespace LK.GreenRoute
{
    class Program
    {
        static void Main(string[] args)
        {

            string gpxPath = "";
            bool showHelp = false;
            

            OptionSet parameters = new OptionSet() {
                { "gpx=", "path to the map file",                                                    v => gpxPath = v},
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

            if (showHelp || string.IsNullOrEmpty(gpxPath))
            {
                ShowHelp(parameters);
                return;
            }

            HRDocument hr = new HRDocument();
            hr.Load(gpxPath);
            hr.Webster();

            GPXPoint bucketInfo = null;
            foreach (var track in hr.Tracks)
            {
                foreach (var seg in track.Segments)
                {
                    bucketInfo = seg.Nodes.ToList().Find(x => x.Id == 0);
                }
            }
            
            Solve(hr.getListProcessor(), hr.getJobTime(), hr.getJobId(), bucketInfo, hr.cycleTime);
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

        static void Solve(List<List<int>> machines, List<List<int>> processingTimes, List<List<int>> jobIds, GPXPoint bucketInfo, int cycleTime)
        {
            Console.WriteLine("\n---- Job shop Scheduling Program ----");
            JobShop jobShop = new JobShop(machines, processingTimes, jobIds, bucketInfo, cycleTime);
            jobShop.RunJobShopScheduling("Jobshop");
        }
    }

    
}