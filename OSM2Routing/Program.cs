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

using NDesk.Options;

using LK.OSMUtils.OSMDatabase;

namespace LK.OSM2Routing {
	class Program {
		static void Main(string[] args) {
			string configPath = "";
			string osmPath = "";
			string outputPath = "";
			bool showHelp = false;

			OptionSet parameters = new OptionSet() {
				{ "osm=",					v => osmPath = v},
				{ "c|config=",			v => configPath = v},
				{ "o|output=",			v => outputPath = v},
				{ "h|?|help",				v => showHelp = v != null},
			};

			try {
				parameters.Parse(args);
			}
			catch (OptionException e) {
				Console.Write("OSM2Routing: ");
				Console.WriteLine(e.Message);
				Console.WriteLine("Try `osm2routing --help' for more information.");
				return;
			}

			if (showHelp || string.IsNullOrEmpty(osmPath) || string.IsNullOrEmpty(configPath) || string.IsNullOrEmpty(outputPath)) {
				ShowHelp(parameters);
				return;
			}

			try {
				Console.Write("Loading config file ...");
				RoutingConfig config = new RoutingConfig();
				config.Load(configPath);
				Console.WriteLine("\t\t\tDone.");

				Console.Write("Loading OSM file ...");
				OSMRoutingDB map = new OSMRoutingDB();
				map.Load(config.RoadTypes, osmPath);
				Console.WriteLine("\t\t\tDone.");

				Console.Write("Creating routable OSM file ...");
				OSMDB routableMap = map.BuildRoutableOSM();
				Console.WriteLine("\t\tDone.");

				Console.Write("Saving routable OSM file ...");
				routableMap.Save(outputPath);
				Console.WriteLine("\t\tDone.");
			}
			catch (Exception e) {
				Console.WriteLine("\nError: " + e.Message);
			}
		}

		/// <summary>
		/// Prints a help message
		/// </summary>
		/// <param name="p">The parameters accepted by this program</param>
		static void ShowHelp(OptionSet p) {
			Console.WriteLine("Usage: osm2routing [OPTIONS]+");
			Console.WriteLine("Converts OSM file into routing-friendly form");
			Console.WriteLine();

			Console.WriteLine("Options:");
			p.WriteOptionDescriptions(Console.Out);
		}
	}
}
