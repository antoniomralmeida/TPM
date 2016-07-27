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

using LK.GPXUtils;
using LK.GeoUtils;
using LK.GeoUtils.Geometry;
using LK.OSMUtils.OSMDatabase;

namespace LK.Analyzer {
	/// <summary>
	/// Represents stop in the trave time
	/// </summary>
	public struct Stop {
		public DateTime From;
		public DateTime To;

		public TimeSpan Length {
			get {
				return To - From;
			}
		}
	}

	/// <summary>
	/// Represents travel time between two points
	/// </summary>
	public class TravelTime {
		private SegmentInfo _segment;
		/// <summary>
		/// Gets the segment that is associated with this TravelTime object
		/// </summary>
		public SegmentInfo Segment {
			get { return _segment; }
		}

		/// <summary>
		/// Gets total travel time between Start and End point
		/// </summary>
		public TimeSpan TotalTravelTime {
			get {
				return TimeEnd - TimeStart;
			}
		}

		private DateTime _timeStart;
		/// <summary>
		/// Gets DateTime at the Start point
		/// </summary>
		public DateTime TimeStart {
			get {
				return _timeStart;
			}
		}

		private DateTime _timeEnd;
		/// <summary>
		/// Gets DateTime at the End point
		/// </summary>
		public DateTime TimeEnd {
			get {
				return _timeEnd;
			}
		}

		private List<Stop> _stops;
		/// <summary>
		/// Gets the list of stops from the TravelTime
		/// </summary>
		public IList<Stop> Stops {
			get {
				return _stops;
			}
		}

		/// <summary>
		/// Creates a new instance of the TravelTime object
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="points"></param>
		public TravelTime(SegmentInfo segment, DateTime start, DateTime end) {
			_segment = segment;
			_timeStart = start;
			_timeEnd = end;

			_stops = new List<Stop>();
		}

		/// <summary>
		/// Creates a new instance ot the TravelTime object
		/// </summary>
		/// <param name="segment"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="stops"></param>
		public TravelTime(SegmentInfo segment, DateTime start, DateTime end, IEnumerable<Stop> stops) {
			_segment = segment;
			_timeStart = start;
			_timeEnd = end;

			_stops = new List<Stop>();
			_stops.AddRange(stops);
		}

		/// <summary>
		/// Get length of the OSMWay in meters
		/// </summary>
		/// <param name="way"></param>
		/// <param name="db"></param>
		/// <returns></returns>
		static double GetLength(OSMWay way, OSMDB db) {
			double result = 0;
			for (int i = 0; i < way.Nodes.Count - 1; i++) {
				result += LK.GeoUtils.Calculations.GetDistance2D(db.Nodes[way.Nodes[i]], db.Nodes[way.Nodes[i + 1]]);
			}

			return result;
		}

		/// <summary>
		/// Estimates End of the travel time as an inerpolation between neighbour points
		/// </summary>
		/// <param name="db"></param>
		/// <param name="ways"></param>
		/// <param name="segmentIndex"></param>
		/// <returns></returns>
		static DateTime InterpolateEndTime(OSMDB db, IList<OSMWay> ways, int segmentIndex) {
			double lenghtBefore = 0;
			int i = segmentIndex;
			while (i >= 0 && db.Nodes[ways[i].Nodes[0]].Tags.ContainsTag("time") == false) {
				i--;
				lenghtBefore += GetLength(ways[i], db);
			}

			DateTime lastTime = DateTime.MinValue;
			if (i >= 0)
				lastTime = DateTime.Parse(db.Nodes[ways[i].Nodes[0]].Tags["time"].Value);
			else
				throw new ArgumentException("Can not find segment start time");

			double lengthAfter = 0;
			i = segmentIndex;
			while (i < ways.Count && db.Nodes[ways[i].Nodes.Last()].Tags.ContainsTag("time") == false) {
				i++;
				lengthAfter += GetLength(ways[i], db);
			}

			DateTime nextTime = DateTime.MinValue;
			if (i < ways.Count)
				nextTime = DateTime.Parse(db.Nodes[ways[i].Nodes.Last()].Tags["time"].Value);
			else
				throw new ArgumentException("Can not find segment end time");

			double miliseconds = (nextTime - lastTime).TotalMilliseconds;
			double totalLength = lenghtBefore + GetLength(ways[segmentIndex], db) + lengthAfter;

			if (totalLength > 0)
				return lastTime.AddMilliseconds(miliseconds * (lenghtBefore + GetLength(ways[segmentIndex], db)) / totalLength);
			else
				return lastTime.AddMilliseconds(miliseconds);
		}

		/// <summary>
		/// Estimates Start of the travel time as an inerpolation between neighbour points
		/// </summary>
		/// <param name="db"></param>
		/// <param name="ways"></param>
		/// <param name="segmentIndex"></param>
		/// <returns></returns>
		static DateTime InterpolateStartTime(OSMDB db, IList<OSMWay> ways, int segmentIndex) {
			double lenghtBefore = 0;
			int i = segmentIndex;
			while (i >= 0 && db.Nodes[ways[i].Nodes[0]].Tags.ContainsTag("time") == false) {
				i--;
				lenghtBefore += GetLength(ways[i], db);
			}

			DateTime lastTime = DateTime.MinValue;
			if (i >= 0)
				lastTime = DateTime.Parse(db.Nodes[ways[i].Nodes[0]].Tags["time"].Value);
			else
				throw new ArgumentException("Can not find segment start time");

			double lengthAfter = 0;
			i = segmentIndex;
			while (i < ways.Count && db.Nodes[ways[i].Nodes.Last()].Tags.ContainsTag("time") == false) {
				i++;
				lengthAfter += GetLength(ways[i], db);
			}

			DateTime nextTime = DateTime.MinValue;
			if (i < ways.Count)
				nextTime = DateTime.Parse(db.Nodes[ways[i].Nodes.Last()].Tags["time"].Value);
			else
				throw new ArgumentException("Can not find segment end time");

			double miliseconds = (nextTime - lastTime).TotalMilliseconds;
			double totalLength = lenghtBefore + GetLength(ways[segmentIndex], db) + lengthAfter;

			if (totalLength > 0)
				return lastTime.AddMilliseconds(miliseconds * lenghtBefore / totalLength);
			else
				return lastTime.AddMilliseconds(miliseconds);
		}

		/// <summary>
		/// Creates a list of travel times from the matched track
		/// </summary>
		/// <param name="track"></param>
		/// <returns></returns>
		public static IEnumerable<TravelTime> FromMatchedTrack(OSMDB track) {
			List<TravelTime> result = new List<TravelTime>();
			var orderedWays = track.Ways.OrderBy(way => int.Parse(way.Tags["order"].Value)).ToList();

			//Find start of the first segment
			int index = 0;
			while (index < orderedWays.Count && track.Nodes[orderedWays[index].Nodes[0]].Tags.ContainsTag("crossroad") == false)
				index++;

			while (index < orderedWays.Count) {
				int startNodeId = int.Parse(track.Nodes[orderedWays[index].Nodes[0]].Tags["node-id"].Value);
				DateTime segmentStartTime = DateTime.MinValue;
				if (track.Nodes[orderedWays[index].Nodes[0]].Tags.ContainsTag("time"))
					segmentStartTime = DateTime.Parse(track.Nodes[orderedWays[index].Nodes[0]].Tags["time"].Value);
				else
					segmentStartTime = InterpolateStartTime(track, orderedWays, index);

				List<GPXPoint> points = new List<GPXPoint>();
				points.Add(new GPXPoint(track.Nodes[orderedWays[index].Nodes[0]].Latitude, track.Nodes[orderedWays[index].Nodes[0]].Longitude, segmentStartTime));

				while (index < orderedWays.Count && track.Nodes[orderedWays[index].Nodes.Last()].Tags.ContainsTag("crossroad") == false) {
					if (track.Nodes[orderedWays[index].Nodes.Last()].Tags.ContainsTag("time")) {
						points.Add(new GPXPoint(track.Nodes[orderedWays[index].Nodes.Last()].Latitude, track.Nodes[orderedWays[index].Nodes.Last()].Longitude,
							DateTime.Parse(track.Nodes[orderedWays[index].Nodes.Last()].Tags["time"].Value)));
					}

					index++;
				}

				if (index < orderedWays.Count) {
					int endNodeId = int.Parse(track.Nodes[orderedWays[index].Nodes.Last()].Tags["node-id"].Value);

					DateTime segmentEndTime = DateTime.MinValue;
					if (track.Nodes[orderedWays[index].Nodes.Last()].Tags.ContainsTag("time"))
						segmentEndTime = DateTime.Parse(track.Nodes[orderedWays[index].Nodes.Last()].Tags["time"].Value);
					else
						segmentEndTime = InterpolateEndTime(track, orderedWays, index);

					points.Add(new GPXPoint(track.Nodes[orderedWays[index].Nodes.Last()].Latitude, track.Nodes[orderedWays[index].Nodes.Last()].Longitude, segmentEndTime));

					int wayId = int.Parse(orderedWays[index].Tags["way-id"].Value);
					SegmentInfo segment = new SegmentInfo() { NodeFromID = startNodeId, NodeToID = endNodeId, WayID = wayId };
					List<double> avgSpeeds = new List<double>();
					for (int i = 0; i < points.Count -1; i++) {
						avgSpeeds.Add(Calculations.GetDistance2D(points[i], points[i + 1]) / (points[i + 1].Time - points[i].Time).TotalSeconds);
					}

					TravelTime tt = new TravelTime(segment, segmentStartTime, segmentEndTime);
					int ii = 0;
					while (ii < avgSpeeds.Count) {
						if (avgSpeeds[ii] < 1.0) {
							Stop stop = new Stop() { From = points[ii].Time };
							while (ii < avgSpeeds.Count && avgSpeeds[ii] < 1.0)
								ii++;

							stop.To = points[ii].Time;
							tt.Stops.Add(stop);
						}
						ii++;
					}

					result.Add(tt);

					index++;
				}
			}

			return result;
		}
	}
}
