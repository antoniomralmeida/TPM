//  Travel Time Analysis project
//  Copyright (C) 2010 Lukas Kabrt
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NDesk.Options;

using LK.OSMUtils.OSMDatabase;
using LK.GPXUtils;
using Utils.XMLUtils;

namespace LK.TMatch
{
    class Program
    {
        static void Main(string[] args)
        {

            DateTime span = DateTime.Now;

            string osmPath = "";
            string gpxPath = "";
            string xmlPath = "";
            string outputPath = ".";
            int samplingPeriod = 0;
            bool showHelp = false;
            bool filter = false;
            //List<OSMDB> OSMList = new List<OSMDB>();
            Dictionary<Bucket, List<OSMDB>> bucketDict = new Dictionary<Bucket, List<OSMDB>>();

            OptionSet parameters = new OptionSet() {
                { "osm=", "path to the routable map file",                                                  v => osmPath = v},
                { "gpx=", "path to the GPX file to process or to the directory to process",                 v => gpxPath = v},
                { "xml=", "path to the XML file with the time buckets",                                     v => xmlPath = v},
                { "o|output=", "path to the output directory",                                              v => outputPath = v},
                { "p|period=", "sampling period of the GPX file",                                           v => samplingPeriod = int.Parse(v)},
                { "f|filter", "enables output post processing",                                             v => filter = v != null},
                { "h|?|help",                                                                               v => showHelp = v != null},
            };

            try
            {
                parameters.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("TMatch: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `tmatch --help' for more information.");
                return;
            }


            if (showHelp || string.IsNullOrEmpty(osmPath) || string.IsNullOrEmpty(gpxPath) || string.IsNullOrEmpty(xmlPath) || string.IsNullOrEmpty(outputPath))
            {
                ShowHelp(parameters);
                return;
            }


            if (outputPath[outputPath.Length - 1] == '"')
            {
                outputPath = outputPath.Substring(0, outputPath.Length - 1);
            }


            Console.Write("Loading OSM file ...");
            OSMDB map = new OSMDB();
            map.Load(osmPath);
            Console.WriteLine("\t\t\tDone.");

            Console.Write("Building routable road graph ...");
            RoadGraph graph = new RoadGraph();
            graph.Build(map);
            Console.WriteLine("\tDone.");


            TMM processor = new TMM(graph);
            PathReconstructer reconstructor = new PathReconstructer(graph);

            XMLDocument xml = new XMLDocument();
            xml.Load(xmlPath);
            foreach (var b in xml.Buckets)
            {
                bucketDict.Add(b, new List<OSMDB>());
            }

            // Process single file
            if (File.Exists(gpxPath))
            {
                ProcessGPXFile(gpxPath, processor, reconstructor, outputPath, samplingPeriod, filter, bucketDict, xml.Buckets);
                //ProcessFinalOSM(bucketDict);
            }
            // Process all GPX in directory
            else if (Directory.Exists(gpxPath))
            {
                var files = Directory.GetFiles(gpxPath, "*.gpx");
                Console.WriteLine("Found {0} GPX file(s).", files.Length);

                foreach (var file in files)
                {
                    ProcessGPXFile(file, processor, reconstructor, outputPath, samplingPeriod, filter, bucketDict, xml.Buckets);
                    Console.WriteLine();
                }
                //ProcessFinalOSM(bucketDict);
            }
            else
            {
                Console.WriteLine("No GPX files found");
            }

            Console.WriteLine("\tDone.");
            Console.WriteLine("\tSpan=" + (DateTime.Now - span));

        }

        static void ProcessFinalOSM(Dictionary<Bucket, List<OSMDB>> bucketDict)
        {
            Dictionary<OSMWay, HashSet<long>> wayCount = new Dictionary<OSMWay, HashSet<long>>();

            foreach (var b in bucketDict)
            {
                if (b.Value.Any())
                {
                    OSMDB finalOsm = new OSMDB();
                    foreach (var osm in b.Value)
                    {
                        foreach (var node in osm.Nodes)
                            if (!finalOsm.Nodes.Contains(node))
                                finalOsm.Nodes.Add(node);

                        foreach (var way in osm.Ways)
                        {
                            if (!finalOsm.Ways.Contains(way))
                            {
                                finalOsm.Ways.Add(way);
                                //wayCount.Add(way, osm.);
                            }
                            //else wayCount[way].Add();
                        }
                    }

                    foreach (var way in finalOsm.Ways)
                        way.Tags.Add(new OSMTag("traffic", Convert.ToString(wayCount[way])));
                    finalOsm.Save("map" + b.Key.Name);
                }
            }
        }

        static void ProcessGPXFile(string path, TMM processor, PathReconstructer reconstructor, string outputPath, int samplingPeriod, 
            bool filterOutput, Dictionary<Bucket, List<OSMDB>> bucketDict, List<Bucket> buckets)
        {
            GPXUtils.Filters.FrequencyFilter filter = new GPXUtils.Filters.FrequencyFilter();

            Console.Write("Loading {0} ...", Path.GetFileName(path));
            GPXDocument gpx = new GPXDocument();
            gpx.Load(path);

            Console.WriteLine("[{0} track(s); {1} segment(s)]", gpx.Tracks.Count, gpx.Tracks.Sum(track => track.Segments.Count));
            for (int trackIndex = 0; trackIndex < gpx.Tracks.Count; trackIndex++)
            {
                Console.WriteLine(gpx.Tracks[trackIndex].Name);

                for (int segmentIndex = 0; segmentIndex < gpx.Tracks[trackIndex].Segments.Count; segmentIndex++)
                {
                    string name = string.IsNullOrEmpty(gpx.Tracks[trackIndex].Name) ? "t" + trackIndex.ToString() : gpx.Tracks[trackIndex].Name.Replace('\\', '-').Replace(":", "");
                    name += "_s" + segmentIndex.ToString();
                    Console.Write("\t" + name + " ");

                    try
                    {
                        GPXTrackSegment toProcess = gpx.Tracks[trackIndex].Segments[segmentIndex];
                        if (samplingPeriod > 0)
                            toProcess = filter.Filter(new TimeSpan(0, 0, samplingPeriod), toProcess);

                        if (toProcess.NodesCount > 1) {
                            var result = processor.Match(toProcess);
                            Console.Write(".");
                            
                            var reconstructedPath = reconstructor.Reconstruct(result);
                            Console.Write(".");

                            if (filterOutput)
                            {
                                reconstructor.FilterUturns(reconstructedPath, 100);
                            }
                            var pathOsm = reconstructor.SaveToOSM(reconstructedPath);
                            
                            //pathOsm.Save(Path.Combine(outputPath, Path.GetFileNameWithoutExtension(path) + "_" + name + ".osm"));
                            Console.WriteLine(".");

                            var overlapping = getOverlappingBuckets(toProcess, buckets);
                            foreach (var b in overlapping) {
                                bucketDict[b].Add(pathOsm);
                            }
                        } else {
                            throw new Exception(string.Format("Track segment discarded because number of nodes is less than 2."));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error: " + e.Message);
                    }
                }
            }
        }

        static List<Bucket> getOverlappingBuckets(GPXTrackSegment toProcess, List<Bucket> buckets)
        {
            List<Bucket> overlapping = new List<Bucket>();
            var start = toProcess.Nodes.First().Time.TimeOfDay;
            var end = toProcess.Nodes.Last().Time.TimeOfDay;

            foreach (var b in buckets)
            {
                if (start < b.End.TimeOfDay && end >= b.Start.TimeOfDay)
                {
                    overlapping.Add(b);
                }
            }
            return overlapping;
        }

        /// <summary>
        /// Prints a help message
        /// </summary>
        /// <param name="p">The parameters accepted by this program</param>
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: tmatch [OPTIONS]+");
            Console.WriteLine("Matches GPX track to the OSM map");
            Console.WriteLine();

            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}