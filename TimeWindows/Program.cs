using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NDesk.Options;
using LK.GPXUtils;
using System.Xml;

namespace LK.TimeWindows
{
    class Program
    {

        static void Main(string[] args)
        {
            string gpxPath = "";
            int samplingPeriod = 0;
            DateTime span = DateTime.Now;
            bool showHelp = false;
            bool filter = false;
            List<Double> tlist = new List<Double>();
            List<Double> rre = new List<Double>();


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
                Console.Write("TimeWindows: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `TimeWindows --help' for more information.");
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

            int DataSetSize = tlist.Count();
            int numClusters = 2;
            double oldWithinss = double.MaxValue;
            double withinss;
            double[][] means;
            int[] count;
            int[] clustering;
            System.IO.StreamWriter csv;

            System.IO.StreamWriter log = new System.IO.StreamWriter("TimeWindows.log");

            Console.WriteLine("Sorting DataSetSize=" + DataSetSize);
            tlist.Sort();
            double[][] rawData = new double[DataSetSize][];
            for (int l = 0; l < DataSetSize; l++)
                rawData[l] = new double[] { tlist[l] };

            csv = new System.IO.StreamWriter("TimeWindows.csv");
            for (int n = 0; n < rawData.Length; n++)
                csv.WriteLine(rawData[n][0] );
            csv.Close();


            //Discretization Round Time
            
            Console.Write("Testing Discretization by Downscaling (Minute)");

            clustering = DiscTime.DiscretizationRoundTime(rawData, out numClusters, out means, out count, out withinss, "M");
            Console.WriteLine(",numClusters= "+ numClusters + ", withinss=" + withinss);
            rre.Add(withinss);
            log.WriteLine(numClusters + ";" + withinss);

            csv = new System.IO.StreamWriter("TimeWindows1-M.csv");
            for (int n=0;n<numClusters;n++)
                csv.WriteLine(means[n][0] + ";" + count[n]);
            csv.Close();


            Console.Write("Testing Discretization by Downscaling (Hour)");

            clustering = DiscTime.DiscretizationRoundTime(rawData, out numClusters, out means, out count, out withinss, "H");
            Console.WriteLine(",numClusters= " + numClusters + ", withinss=" + withinss);
            rre.Add(withinss);
            log.WriteLine(numClusters + ";" + withinss);

            csv = new System.IO.StreamWriter("TimeWindows1-H.csv");
            for (int n = 0; n < numClusters; n++)
                csv.WriteLine(means[n][0] + ";" + count[n]);
            csv.Close();


            //Discretization Histogram

            Console.Write("Testing Discretization by Histogram");
            clustering = HistSample.Histogram(rawData, out numClusters, out means, out count, out withinss);
            Console.WriteLine(",numClusters= " + numClusters + ", withinss=" + withinss);
            rre.Add( withinss);
            log.WriteLine(numClusters + ";" + withinss);

            csv = new System.IO.StreamWriter("TimeWindows2.csv");
            for (int n = 0; n < numClusters; n++)
                csv.WriteLine(means[n][0] + ";" + count[n]);
            csv.Close();

            Console.Write("Testing Discretization KMeans");

            //Sturges' formula
            int max_cluster;
            int min_cluster;

            Utils.Sturges(rawData.Length, out min_cluster, out max_cluster);
       
            double limWithinss = (2 * rre[0] + rre[1])/ 3;
            rre.Add(double.MaxValue);
            int c = min_cluster;
            
            do
            {
                Console.Write(".");
                clustering = KMeans.Cluster(rawData, c, out means, out count, out withinss,2); // this is it
                //Console.WriteLine(", withinss=" + withinss);
                rre.Add(withinss);
                double tax = (rre[rre.Count-1]- rre[rre.Count - 2]) / rre[rre.Count - 2];
                log.WriteLine(c + ";" + withinss);    
                if ((c > min_cluster && (tax > 0 || tax > -0.1))) // 10%   
                {             
                    numClusters = c - 1;
                    break;
                }
                if (withinss < limWithinss)
                {
                    numClusters = c;
                    break;
                }
                oldWithinss = withinss;
               
            } while (c++ <= max_cluster);
            
            log.Close();

            clustering = KMeans.Cluster(rawData, numClusters, out means, out count, out withinss, 3); // this is it
            Console.WriteLine(",numClusters= " + numClusters + ", withinss=" + withinss);

            // DBScan - Variables
            List<double> points = new List<double>();
            int row = 0, col = 0;
            for (row = 0; row < rawData.Length; row++)
            {
                for (col = 0; col < rawData[row].Length; col++)
                {
                    points.Add(rawData[row][col]);
                } 
            }
            
            // Starting DBScan 
            DBScan db = new DBScan();
            db.dbscan(rawData, points, 0.03, 100);

            csv = new System.IO.StreamWriter("TimeWindows3.csv");
            for (int n = 0; n < numClusters; n++)
                csv.WriteLine(means[n][0] + ";" + count[n]);
            csv.Close();

            List<Double> buckets = new List<double>();
            buckets.Add(0);
            int k = clustering[0];
            for (int j=1;j< clustering.Length;j++)
                if (clustering[j] != k)
                {
                    buckets.Add(rawData[j-1][0]);
                    k = clustering[j];
                }
            buckets.Add(0.99999);

           

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
                 attribute2.Value = DateTime.FromOADate(buckets[j]).ToString("HH:mm:ss");
                 element3.Attributes.Append(attribute2);

                 XmlAttribute attribute3 = doc.CreateAttribute("end");
                 attribute3.Value = DateTime.FromOADate(buckets[j + 1]).ToString("HH:mm:ss");
                 element3.Attributes.Append(attribute3);

                 i++;

             }

             String fileout = Path.Combine(Path.GetDirectoryName(gpxPath), "TimeWindows.xml");
             doc.Save(fileout);
             
            Console.WriteLine("\t\tDone.");
            Console.WriteLine("\tSpan=" + (DateTime.Now - span));

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
                    //Console.Write("\t" + name + " ");

                    try
                    {
                        GPXTrackSegment toProcess = gpx.Tracks[trackIndex].Segments[segmentIndex];
                        if (samplingPeriod > 0)
                            toProcess = filter.Filter(new TimeSpan(0, 0, samplingPeriod), toProcess);


                        foreach (var t in toProcess.Nodes)
                            tlist.Add(Utils.Time2Double(t.Time));
                        //Console.WriteLine(".");
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
            Console.WriteLine("Usage: TimeWindows [OPTIONS]+");
            Console.WriteLine("Matches GPX track to the OSM map");
            Console.WriteLine();

            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}