using System;
using System.Collections.Generic;

using NDesk.Options;
using System.IO;

using LK.GPXUtils;

namespace LK.CSV2GPX
{
    class Program
    {
        static void Main(string[] args)
        {

            DateTime span = DateTime.Now;

            string csvPath = "";
            int samplingPeriod = 0;
            string outputPath = ".";
            bool showHelp = false;

            OptionSet parameters = new OptionSet() {
                { "csv=", "path to the CSV track file",                      v => csvPath = v},
                { "o|output=", "path to the output directory",               v => outputPath = v},
                { "p|period=", "sampling period of the CSV file",            v => samplingPeriod = int.Parse(v)},
                { "h|?|help",                                                v => showHelp = v != null},
            };

            try
            {
                parameters.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("CSV2GPX: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `csv2gpx --help' for more information.");
                return;
            }


            if (showHelp || string.IsNullOrEmpty(csvPath) || string.IsNullOrEmpty(outputPath))
            {
                ShowHelp(parameters);
                return;
            }


            // Process signle file
            if (File.Exists(csvPath))
            {
                ProcessCSVFile(csvPath, outputPath, samplingPeriod);
            }
            // Process all GPX in directory
            else if (Directory.Exists(csvPath))
            {
                var files = Directory.GetFiles(csvPath, "*.CSV");
                Console.WriteLine("Found {0} CSV file(s).", files.Length);

                foreach (var file in files)
                {
                    ProcessCSVFile(file, outputPath, samplingPeriod);
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("No CSV files found");
            }
            Console.WriteLine("\tDone.");

            Console.WriteLine("\tSpan=" + (DateTime.Now - span));

        }

        static void ProcessCSVFile(string path, string outputPath, int samplingPeriod)
        {
            double max_lat = -3.691978693, min_lat = -3.8881242275, max_lon = -38.4018707275, min_lon = -38.6374359131; //geo-bounding box for Fortaleza

            GPXDocument gpx = new GPXDocument();
            Console.Write("Parsing {0} ...", Path.GetFileName(path));

            string line;
            char[] seps = { ';', ',', '\t' };
            char sep = '.';
            String[] fields = { };
            long last_id = -1;

            int id_field = 0;
            int lat_field = 0;
            int lon_field = 0;
            int time_field = 0;
            int uf1_field = 0;
            int uf2_field = 0;
            int uf3_field = 0;
            int uf4_field = 0;
            GPXTrack trk = null;

            List<long> ids = new List<long>();
            Dictionary<String, long> tlist = new Dictionary<String, long>();

            if (samplingPeriod > 0)
            {

                DateTime dt = File.GetLastWriteTime(path);
                if ((DateTime.Now - dt).Days > samplingPeriod)
                {
                    Console.WriteLine("skiping");
                    return;
                }
            }

            // Read the file and display it line by line.
            System.IO.StreamReader csv = new System.IO.StreamReader(path);

            line = csv.ReadLine();
            if (line != null)
            {
                for (int i = 0; i < seps.Length; i++)
                {
                    if ((fields = line.Split(seps[i])).Length > 1)
                    {
                        sep = seps[i];
                        break;
                    }
                }
                if (sep == '.')
                    throw new Exception(string.Format("Can not find any known separators.{';',',','TAB'}"));

                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i] == "id")
                        id_field = i;
                    if (fields[i] == "lat")
                        lat_field = i;
                    if (fields[i] == "lon")
                        lon_field = i;
                    if (fields[i] == "time")
                        time_field = i;
                    if (fields[i] == "uf1")
                        uf1_field = i;
                    if (fields[i] == "uf2")
                        uf2_field = i;
                    if (fields[i] == "uf3")
                        uf3_field = i;
                    if (fields[i] == "uf4")
                        uf4_field = i;
                }
                if ((lat_field + lon_field) == 0)
                    throw new Exception(string.Format("Can not find header line.(id, lat, lon, time)"));

            }
            while ((line = csv.ReadLine()) != null)
            {
                fields = line.Split(sep);
                long id = Convert.ToInt64(fields[id_field]);
                if (id != last_id)
                {
                    if (ids.Find(item => item == id) == 0)
                        ids.Add(id);
                    last_id = id;
                    //break;
                }

                String key = fields[time_field].Substring(16, 5);

                if (tlist.ContainsKey(key))
                    tlist[key]++;
                else
                    tlist.Add(key, 1);
            }
            csv.Close();

            System.IO.StreamWriter log = new System.IO.StreamWriter(path + ".log");

            foreach (var pair in tlist)
                log.WriteLine(pair);
            log.Close();

            foreach (long next_id in ids)
            {
                Console.Write(next_id);

                int count = 0;

                csv = new System.IO.StreamReader(path);
                trk = new GPXTrack();
                GPXTrackSegment trkseg = new GPXTrackSegment();
                gpx.Tracks.Add(trk);
                trk.Name = "trk_" + next_id;
                line = csv.ReadLine();

                while ((line = csv.ReadLine()) != null)
                {
                    fields = line.Split(sep);
                    long id = Convert.ToInt64(fields[id_field]);
                    if (id == next_id)
                    {
                        if (!(fields[uf3_field] == "null" && fields[uf4_field] == "null"))
                        {
                            var lat = Double.Parse(fields[lat_field], System.Globalization.CultureInfo.InvariantCulture);
                            var lon = Double.Parse(fields[lon_field], System.Globalization.CultureInfo.InvariantCulture);

                            if (lat > min_lat && lat < max_lat && lon > min_lon && lon < max_lon)
                            {
                                GPXPoint p = new GPXPoint();

                                p.Latitude = lat;
                                p.Longitude = lon;
                                p.Time = DateTime.ParseExact(fields[time_field], "ddd MMM dd yyyy HH:mm:ss 'GMT-0300 (BRT)'", System.Globalization.CultureInfo.InvariantCulture);

                                trkseg.Nodes.Add(p);
                                count++;
                            }
                        }
                    }
                }
                trk.Segments.Add(trkseg);
                csv.Close();
                Console.WriteLine("[" + count + "]");
            }

            String fileout = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(path) + ".gpx");
            Console.WriteLine(fileout);
            gpx.Save(fileout);
            Console.WriteLine("\tDone.");

        }

        /// <summary>
        /// Prints a help message
        /// </summary>
        /// <param name="p">The parameters accepted by this program</param>
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: CSV2GPX [OPTIONS]+");
            Console.WriteLine("Matches CSV track to the GPX ");
            Console.WriteLine();

            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}