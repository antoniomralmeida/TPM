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

namespace LK.MatchGPX2OSM {
	class Program {
		static void Main(string[] args) {
			string osmPath = "";
			string gpxPath = "";
			string outputPath = ".";
			int samplingPeriod = 0;
			bool showHelp = false;
			bool filter = false;

			OptionSet parameters = new OptionSet() {
				{ "osm=", "path to the routable map file",																				v => osmPath = v},
				{ "gpx=",	"path to the GPX file to process or to the directory to process",				v => gpxPath = v},
				{ "o|output=", "path to the output directory",																		v => outputPath = v},
				{ "p|period=", "sampling period of the GPX file",																	v => samplingPeriod = int.Parse(v)},
				{ "f|filter", "enables output post processing",																		v => filter = v != null},
				{ "h|?|help",																																			v => showHelp = v != null},
			};

			try {
				parameters.Parse(args);
			}
			catch (OptionException e) {
				Console.Write("MatchGPX2OSM: ");
				Console.WriteLine(e.Message);
				Console.WriteLine("Try `matchgpx2osm --help' for more information.");
				return;
			}


			if (showHelp || string.IsNullOrEmpty(osmPath) || string.IsNullOrEmpty(gpxPath) || string.IsNullOrEmpty(outputPath)) {
				ShowHelp(parameters);
				return;
			}

			Console.Write("Loading OSM file ...");
			OSMDB map = new OSMDB();
			map.Load(osmPath);
			Console.WriteLine("\t\t\tDone.");

			Console.Write("Building routable road graph ...");
			RoadGraph graph = new RoadGraph();
			graph.Build(map);
			Console.WriteLine("\tDone.");


			STMatching processor = new STMatching(graph);
			PathReconstructer reconstructor = new PathReconstructer(graph);

			// Process signle file
			if (File.Exists(gpxPath)) {
				ProcessGPXFile(gpxPath, processor, reconstructor, outputPath, samplingPeriod, filter);
			}
			// Process all GPX in directory
			else if (Directory.Exists(gpxPath)) {
				var files = Directory.GetFiles(gpxPath, "*.gpx");
				Console.WriteLine("Found {0} GPX file(s).", files.Length);

				foreach (var file in files) {
					ProcessGPXFile(file, processor, reconstructor, outputPath, samplingPeriod, filter);
					Console.WriteLine();
				}
			}
			else {
				Console.WriteLine("No GPX files found");
			}
		}

		static void ProcessGPXFile(string path, STMatching processor, PathReconstructer reconstructor, string outputPath, int samplingPeriod, bool filterOutput) {
			GPXUtils.Filters.FrequencyFilter filter = new GPXUtils.Filters.FrequencyFilter();

			Console.Write("Loading {0} ...", Path.GetFileName(path));
			GPXDocument gpx = new GPXDocument();
			gpx.Load(path);

			Console.WriteLine("[{0} track(s); {1} segment(s)]", gpx.Tracks.Count, gpx.Tracks.Sum(track => track.Segments.Count));
			for (int trackIndex = 0; trackIndex < gpx.Tracks.Count; trackIndex++) {
				for (int segmentIndex = 0; segmentIndex < gpx.Tracks[trackIndex].Segments.Count; segmentIndex++) {
					string name = string.IsNullOrEmpty(gpx.Tracks[trackIndex].Name) ? "t" + trackIndex.ToString() : gpx.Tracks[trackIndex].Name.Replace('\\', '-').Replace(":","");
					name += "_s" + segmentIndex.ToString();
					Console.Write("\t" + name + " ");

					try {
						GPXTrackSegment toProcess = gpx.Tracks[trackIndex].Segments[segmentIndex];
						if (samplingPeriod > 0)
							toProcess = filter.Filter(new TimeSpan(0, 0, samplingPeriod), toProcess);

						var result = processor.Match(toProcess);
						Console.Write(".");

						var reconstructedPath = reconstructor.Reconstruct(result);
						Console.Write(".");

						if (filterOutput) {
							reconstructor.FilterUturns(reconstructedPath, 100);
						}
						var pathOsm = reconstructor.SaveToOSM(reconstructedPath);

						pathOsm.Save(Path.Combine(outputPath, Path.GetFileNameWithoutExtension(path) + "_" + name + ".osm"));
						Console.WriteLine(".");
					}
					catch (Exception e) {
						Console.WriteLine("Error: " + e.Message);
					}
				}
			}
		}

		/// <summary>
		/// Prints a help message
		/// </summary>
		/// <param name="p">The parameters accepted by this program</param>
		static void ShowHelp(OptionSet p) {
			Console.WriteLine("Usage: matchgpx2osm [OPTIONS]+");
			Console.WriteLine("Matches GPX track to the OSM map");
			Console.WriteLine();

			Console.WriteLine("Options:");
			p.WriteOptionDescriptions(Console.Out);
		}
	}
}
