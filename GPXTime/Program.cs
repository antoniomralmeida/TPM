using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using NDesk.Options;

using LK.GPXUtils;
using System.Xml;

namespace LK.GPXTime
{
    class Program
    {
        static void Main(string[] args)
        {
            string gpxPath = "";
            int samplingPeriod = 0;
            bool showHelp = false;
            bool filter = false;
            List<Double> tlist = new List<Double>();


            OptionSet parameters = new OptionSet() {
                { "gpx=",   "path to the GPX file to process or to the directory to process",               v => gpxPath = v},
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
                Console.Write("GPXTime: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `GPXTime --help' for more information.");
                return;
            }


            if (showHelp || string.IsNullOrEmpty(gpxPath))
            {
                ShowHelp(parameters);
                return;
            }


            // Process signle file
            if (File.Exists(gpxPath))
            {
                ProcessGPXFile(gpxPath, tlist, samplingPeriod, filter);
            }
            // Process all GPX in directory
            else if (Directory.Exists(gpxPath))
            {
                var files = Directory.GetFiles(gpxPath, "*.gpx");
                Console.WriteLine("Found {0} GPX file(s).", files.Length);

                foreach (var file in files)
                {
                    ProcessGPXFile(file, tlist, samplingPeriod, filter);
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("No GPX files found");
            }

            Double xMin, optimalBinWidth;
            List<int> f;


            List<Double> buckets = HistSample.CalculateOptimalBinWidth(tlist, out xMin, out optimalBinWidth, out f);


            System.Xml.XmlDocument doc = new XmlDocument();

            //(1) the xml declaration is recommended, but not mandatory
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            //(2) string.Empty makes cleaner code
            XmlElement element1 = doc.CreateElement(string.Empty, "time-config", string.Empty);
            doc.AppendChild(element1);
            XmlElement element2 = doc.CreateElement(string.Empty, "buckets", string.Empty);
            element1.AppendChild(element2);

            char i = 'A';

            for (int j = 0; j < (buckets.Count - 1); j++)
            {

                XmlElement element3 = doc.CreateElement(string.Empty, "bucket", string.Empty);
                element2.AppendChild(element3);

                XmlAttribute attribute = doc.CreateAttribute("name");
                attribute.Value = i.ToString();
                element3.Attributes.Append(attribute);


                XmlAttribute attribute2 = doc.CreateAttribute("start");
                attribute2.Value = DateTime.FromOADate(buckets[j]).ToString("HH:mm");
                element3.Attributes.Append(attribute2);

                XmlAttribute attribute3 = doc.CreateAttribute("end");
                attribute3.Value = DateTime.FromOADate(buckets[j + 1]).ToString("HH:mm");
                element3.Attributes.Append(attribute3);

                i++;

            }

            String fileout = Path.Combine(Path.GetDirectoryName(gpxPath), "GPXTime.xml");
            doc.Save(fileout);
            Console.WriteLine("\t\tDone.");


        }

        static void ProcessGPXFile(string path, List<Double> tlist, int samplingPeriod, bool filterOutput)
        {
            LK.GPXUtils.Filters.FrequencyFilter filter = new LK.GPXUtils.Filters.FrequencyFilter();

            Console.Write("Loading {0} ...", Path.GetFileName(path));
            GPXDocument gpx = new GPXDocument();
            gpx.Load(path);

            Console.WriteLine("[{0} track(s); {1} segment(s)]", gpx.Tracks.Count, gpx.Tracks.Sum(track => track.Segments.Count));
            for (int trackIndex = 0; trackIndex < gpx.Tracks.Count; trackIndex++)
            {
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


                        foreach (var t in toProcess.Nodes)
                        {
                            //    Console.WriteLine(t.Time);
                            tlist.Add((t.Time.Hour + t.Time.Minute / 60.0) / 24.0);
                        }
                        Console.WriteLine(".");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error: " + e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Prints a help message
        /// </summary>
        /// <param name="p">The parameters accepted by this program</param>
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: GPXTime [OPTIONS]+");
            Console.WriteLine("Matches GPX track to the OSM map");
            Console.WriteLine();

            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}