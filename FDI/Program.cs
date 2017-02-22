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
using System.IO;
using NDesk.Options;
using Utils;
using LK.OSMUtils.OSMDatabase;
using LK.GPXUtils;
using LK.FDI.XMLUtils;
using LK.GeoUtils.Geometry;
using LK.GeoUtils;

namespace LK.FDI

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
                Console.Write("FDI: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `fdi --help' for more information.");
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

            TMM processor = new TMM(graph) { _db = map };
            PathReconstructer reconstructor = new PathReconstructer(graph) { _db = map };

            XMLDocument xml = new XMLDocument();
            xml.Load(xmlPath);
            var buckets = xml.Buckets;

            // Process single file
            if (File.Exists(gpxPath))
            {
                List<GPXTrack> result = new List<GPXTrack>();
                result.AddRange(getAllGpxTrackList(gpxPath));

                ProcessGPXFile(gpxPath, processor, reconstructor, outputPath, samplingPeriod, filter, buckets);
                GenerateOsmFiles(buckets, reconstructor, map, result);
                GenerateGpxFiles(buckets, gpxPath, 0);
            }
            // Process all GPX in directory
            else if (Directory.Exists(gpxPath))
            {
                var files = Directory.GetFiles(gpxPath, "*.gpx");
                List<GPXTrack> result = new List<GPXTrack>();

                Console.WriteLine("Found {0} GPX file(s).", files.Length);

                for (int i = 0; i < files.Length; i++)
                {
                    ProcessGPXFile(files[i], processor, reconstructor, outputPath, samplingPeriod, filter, buckets);
                    GenerateGpxFiles(buckets, gpxPath, i);
                    result.AddRange(getAllGpxTrackList(files[i]));

                    Console.WriteLine("NEW FILE BEING PROCESSED");
                }

                GenerateOsmFiles(buckets, reconstructor, map, result);
            }
            else
            {
                Console.WriteLine("No GPX files found");
            }

            Console.WriteLine("\tDone.");
            Console.WriteLine("\tSpan=" + (DateTime.Now - span));

        }

        static List<GPXTrack> getAllGpxTrackList(string file)
        {
            GPXDocument gpx = new GPXDocument();
            List<GPXTrack> gpxTrackList = new List<GPXTrack>();
            List<GPXTrackSegment> gpxTrackSegList;
            
            gpx.Load(file);

            foreach (var trk in gpx.Tracks)
            {
                gpxTrackSegList = new List<GPXTrackSegment>();

                // Sanatizing the data, some segments have zero nodes. 
                foreach (var seg in trk.Segments)
                {
                    if (seg.Nodes.Count != 0)
                    {
                        gpxTrackSegList.Add(seg);
                    }
                }

                // Clearing the olds
                trk.Segments.Clear();
                // Assigning the new ones. 
                trk.Segments.AddRange(gpxTrackSegList);

                if (trk.Segments.Count != 0)
                {
                    gpxTrackList.Add(trk);
                }
            }

            return gpxTrackList;
        }

        static void ProcessGPXFile(string path, TMM processor, PathReconstructer reconstructor, string outputPath, int samplingPeriod, 
            bool filterOutput, List<Bucket> buckets)
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

                            Console.WriteLine(".");

                            var trackId = gpx.Tracks[trackIndex].Name.Replace("trk_", "");
                            buckets = GetUpdatedBuckets(toProcess, reconstructedPath, buckets, trackId);

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

        static List<Bucket> GetUpdatedBuckets(GPXTrackSegment toProcess, List<Polyline<IPointGeo>> path, 
            List<Bucket> buckets, String trackId)
        {
            var start = toProcess.Nodes.First().Time.TimeOfDay;
            var end = toProcess.Nodes.Last().Time.TimeOfDay;
            
            foreach (var b in buckets)
            {
                if (start >= b.Start && end <= b.End)
                {
                    b.Paths.Add(trackId, path);
                }
            }
            return buckets;
        }

        static void GenerateOsmFiles(List<Bucket> buckets, PathReconstructer reconstructor, OSMDB map, List<GPXTrack> gpxTrackList)
        {
           
            foreach (var b in buckets)
            {
                if (b.Paths.Any())
                {
                    var mapCopy = ObjectCopier.Clone<OSMDB>(map);
                    List<Polyline<IPointGeo>> pathList = new List<Polyline<IPointGeo>>();

                    OSMNode bucketInfo = new OSMNode(0, 0, 0);
                    OSMTag start = new OSMTag("start", b.Start.ToString());
                    OSMTag end = new OSMTag("end", b.End.ToString());
                    bucketInfo.Tags.Add(start);
                    bucketInfo.Tags.Add(end);

                    foreach (var p in b.Paths)
                    {
                        var uniquePath = p.Value.GroupBy(x => new { x.Id }).Select(x => x.First());

                        foreach (var seg in uniquePath)
                        {
                            if (seg.Id != 0)
                            {
                                var matchingWay = mapCopy.Ways[seg.Id];
                                var avgSpeed = getAverageSpeed(p.Key, gpxTrackList);

                                if (avgSpeed != null)
                                {
                                    if (matchingWay.Tags.ContainsTag("avgSpeed"))
                                    {
                                        matchingWay.Tags["avgSpeed"].Value = avgSpeed;
                                    }
                                    else
                                    {
                                        matchingWay.Tags.Add(new OSMTag("avgSpeed", avgSpeed));
                                    }

                                }

                                if (matchingWay.Tags.ContainsTag("traffic"))
                                {
                                    matchingWay.Tags["traffic"].Value += "," + p.Key;
                                }
                                else
                                {
                                    matchingWay.Tags.Add(new OSMTag("traffic", p.Key));
                                }

                            }
                        }
                        pathList.AddRange(uniquePath);
                    }

                    //OSMDB resultMap = reconstructor.SaveToOSM(pathList);
                    //resultMap.Save("map" + b.Name + ".osm");

                    mapCopy.Nodes.Add(bucketInfo);
                    mapCopy.Save("map" + b.Name + ".osm");
                }
            }
        }

        // 
        // It gets as parameters the traffic, and a list of gpx tracks list.
        // Return the avarage speed of the specific traffic, if there are more
        // than one segment, it calculate the mean. 
        //
        static string getAverageSpeed(string traffic, List<GPXTrack> gpxTrackList)
        {
            double length;
            double avgSpeed = 0;
            TimeSpan intervalTime = new TimeSpan();
            
            foreach(var trk in gpxTrackList)
            {
                var name = trk.Name.Replace("trk_", "");
                // found the correct segment
                if (name.ToString() == traffic)
                {
                    foreach (var seg in trk.Segments)
                    {
                        length = 0;
                        for (int i = 0; i < seg.Nodes.Count; i++)
                        {
                            if (i + 1 < seg.Nodes.Count)
                            {
                                length += Calculations.GetDistance2D(seg.Nodes[i], seg.Nodes[i + 1]);
                            }
                        }

                        // meters to km
                        length = length / 1000;
                        if (seg.Nodes.Count != 0)
                        {
                            intervalTime = seg.Nodes.Last().Time - seg.Nodes.First().Time;
                        }
                        else
                        {
                            avgSpeed = 0;
                            return avgSpeed.ToString();
                        }

                        //Console.WriteLine("Length: " + length);
                        //Console.WriteLine("Total hours: " + intervalTime.TotalHours);
                        avgSpeed += (length / intervalTime.TotalHours);
                    }

                    avgSpeed = avgSpeed / trk.Segments.Count;
                    return avgSpeed.ToString();
                }
            }
                
            return null;
        }

        static void GenerateGpxFiles(List<Bucket> buckets, string file, int positionFile)
        {

            foreach (var b in buckets)
            {
                if (b.Paths.Any())
                {
                    // Saving the Map to GPX instead of OSM - START
                    var tracks = new List<GPXTrack>();

                    foreach (var p in b.Paths)
                    {
                        GPXTrack track = new GPXTrack(p.Key);
                        foreach (var t in p.Value)
                        {
                            List<GPXPoint> list = new List<GPXPoint>();

                            foreach (var s in t.Segments)
                            {
                                GPXPoint start = new GPXPoint() { Latitude = s.StartPoint.Latitude, Longitude = s.StartPoint.Longitude };
                                GPXPoint end = new GPXPoint() { Latitude = s.EndPoint.Latitude, Longitude = s.EndPoint.Longitude };

                                list.Add(start);
                                list.Add(end);
                            }

                            GPXTrackSegment gpxTrack = new GPXTrackSegment(list);
                            track.Segments.Add(gpxTrack);
                        }
                        tracks.Add(track);
                    }

                    var gpx = new GPXDocument() { Tracks = tracks };
                    gpx.Save("mapGpx" + positionFile + ".gpx");
                    // END
                }
            }
        }

        /// <summary>
        /// Prints a help message
        /// </summary>
        /// <param name="p">The parameters accepted by this program</param>
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: fdi [OPTIONS]+");
            Console.WriteLine("Flow Density Inference by matching");
            Console.WriteLine();

            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}