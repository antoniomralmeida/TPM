using System;
using System.Collections.Generic;
using NDesk.Options;
using LK.OSMUtils.OSMDatabase;
using LK.FDI;
using LK.GPXUtils;

namespace LK.HRT
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
                Console.Write("TRoute: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `TRoute --help' for more information.");
                return;
            }
            
            if (showHelp || string.IsNullOrEmpty(osmPath) || eps < 0 || minTraffic < 0)
            {
                ShowHelp(parameters);
                return;
            }
            
            var osmFile = new OSMDB();
            osmFile.Load(osmPath);
            var roadGraph = new RoadGraph();
            roadGraph.Build(osmFile);
            
            var hotRoutes = new FlowScan().Run(roadGraph, eps, minTraffic);

            // Saving GPX file of the Hot Route
            HashSet<GPXPoint> listPoints;
            List<GPXTrackSegment> listSegments;
            GPXTrackSegment segTrack;

            List<GPXTrack> track = new List<GPXTrack>();
            GPXTrack tr;

            //Console.WriteLine(hotRoutes.Count);
            foreach (var hr in hotRoutes)
            {
                //Console.WriteLine("Number segs: " + hr.Segments.Count);
                listSegments = new List<GPXTrackSegment>();

                foreach (var seg in hr.Segments)
                {
                    listPoints = new HashSet<GPXPoint>();

                    foreach (var segInner in seg.Geometry.Segments)
                    {
                        GPXPoint start = new GPXPoint() { Id = seg.Id, Latitude = segInner.StartPoint.Latitude, Longitude = segInner.StartPoint.Longitude };
                        GPXPoint end = new GPXPoint() { Id = seg.Id, Latitude = segInner.EndPoint.Latitude, Longitude = segInner.EndPoint.Longitude };
                        listPoints.Add(start);
                        listPoints.Add(end);
                    }

                    segTrack = new GPXTrackSegment(listPoints, seg.AvgSpeed, seg.Id);
                    // passing the traffic
                    segTrack.Traffic = seg.Traffic;
                    listSegments.Add(segTrack);
                }

                tr = new GPXTrack();
                tr.Segments.AddRange(listSegments);
                track.Add(tr);

            }
            var gpx = new GPXDocument() { Tracks = track };
            gpx.Save("mapWithHotRoutes.gpx");

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
    }
}