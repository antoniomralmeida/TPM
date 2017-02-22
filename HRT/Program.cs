using System;
using System.Collections.Generic;
using NDesk.Options;
using LK.OSMUtils.OSMDatabase;
using LK.FDI;
using LK.GPXUtils;
using System.Linq;

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

            // Getting all the nodes with traffic signals
            Dictionary<double, OSMNode> tagsNodes = new Dictionary<double, OSMNode>();
            foreach (var node in osmFile.Nodes)
            {
                foreach (var tag in node.Tags)
                {
                    if (tag.Value.Equals("traffic_signals"))
                    {
                        if (!tagsNodes.Keys.Contains(node.Latitude + node.Longitude))
                        {
                            tagsNodes.Add(node.Latitude + node.Longitude, node);
                        }
                        
                    }
                }
            }

            var hotRoutes = new FlowScan().Run(roadGraph, eps, minTraffic);

            // Saving GPX file of the Hot Route
            HashSet<GPXPoint> listPoints;
            HashSet<GPXTrackSegment> listSegments;
            GPXTrackSegment segTrack;

            List<GPXTrack> track = new List<GPXTrack>();
            GPXTrack tr;

            //Console.WriteLine(hotRoutes.Count);
            foreach (var hr in hotRoutes)
            {
                //Console.WriteLine("Number segs: " + hr.Segments.Count);
                listSegments = new HashSet<GPXTrackSegment>();

                foreach (var seg in hr.Segments)
                {
                    listPoints = new HashSet<GPXPoint>();

                    foreach (var segInner in seg.Geometry.Segments)
                    {
                        GPXPoint start;
                        if (tagsNodes.Keys.Contains(segInner.StartPoint.Latitude + segInner.StartPoint.Longitude))
                        {
                            OSMNode osmNode = tagsNodes[segInner.StartPoint.Latitude + segInner.StartPoint.Longitude];
                            start = new GPXPoint() { Id = osmNode.ID, Latitude = segInner.StartPoint.Latitude,
                                Longitude = segInner.StartPoint.Longitude, TrafficSignal = true };
                        } else
                        {
                            OSMNode osmNode = osmFile.Nodes.ToList().First(x => x.Latitude == segInner.StartPoint.Latitude &&
                                                                                x.Longitude == segInner.StartPoint.Longitude);
                            start = new GPXPoint() { Id = osmNode.ID, Latitude = segInner.StartPoint.Latitude,
                                Longitude = segInner.StartPoint.Longitude, TrafficSignal = false };
                        }

                        GPXPoint end;
                        if (tagsNodes.Keys.Contains(segInner.EndPoint.Latitude + segInner.EndPoint.Longitude))
                        {
                            OSMNode osmNode = tagsNodes[segInner.EndPoint.Latitude + segInner.EndPoint.Longitude];
                            end = new GPXPoint() { Id = osmNode.ID, Latitude = segInner.EndPoint.Latitude,
                                Longitude = segInner.EndPoint.Longitude, TrafficSignal = true };
                        } else
                        {
                            OSMNode osmNode = osmFile.Nodes.ToList().First(x => x.Latitude == segInner.EndPoint.Latitude && 
                                                                                x.Longitude == segInner.EndPoint.Longitude);
                            end = new GPXPoint() { Id = osmNode.ID, Latitude = segInner.EndPoint.Latitude,
                                Longitude = segInner.EndPoint.Longitude, TrafficSignal = false };
                        }

                        listPoints.Add(start);
                        listPoints.Add(end);
                    }
                    
                    segTrack = new GPXTrackSegment(listPoints, seg.AvgSpeed, seg.Speed, seg.Id);
                    // passing the traffic
                    segTrack.Traffic = seg.Traffic;
                    listSegments.Add(segTrack);
                }

                tr = new GPXTrack();
                tr.Segments.AddRange(listSegments);
                track.Add(tr);

            }

            // Bucket Information
            GPXPoint pBucket = new GPXPoint(0, 0, 0, false);
            var bucketInfo = osmFile.Nodes.ToList().Find(x => x.ID == 0);

            if (bucketInfo != null)
            {
                pBucket.StartBucket = TimeSpan.Parse(bucketInfo.Tags.First().Value);
                pBucket.EndBucket = TimeSpan.Parse(bucketInfo.Tags.Last().Value);
            }
            

            var gpx = new GPXDocument() { Tracks = track };
            gpx.Waypoints.Add(pBucket);
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