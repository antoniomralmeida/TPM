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
using LK.GeoUtils;
using LK.OSMUtils.OSMDatabase;
using LK.GeoUtils.Geometry;

namespace LK.TMatch {
	public class PathReconstructer {
		OSMDB _db;
		int _dbCounter;
		AstarPathfinder _pathfinder;
		Dictionary<IPointGeo, PointEx> _points;

		/// <summary>
		/// Creates a new instance of the PathReconstructer
		/// </summary>
		/// <param name="graph">The RoadGraph object with the road network that will be used in the reconstruction process</param>
		public PathReconstructer(RoadGraph graph) {
			_pathfinder = new AstarPathfinder(graph);
		}

		/// <summary>
		/// Represents part of the path of the result
		/// </summary>
		class PolylineID : Polyline<IPointGeo> {
			public int WayID { get; set; }
		}

		/// <summary>
		/// Represents point of the result track
		/// </summary>
		class PointEx : IPointGeo {
			public double Latitude { get; set; }
			public double Longitude { get; set; }
			public double Elevation { get; set; }
			public long NodeID { get; set; }
			public DateTime Time { get; set; }
			public bool Crossroad { get; set; }
		}

		/// <summary>
		/// Gets PointEx from the internal storage or creates a new one (for given point)
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		PointEx GetOrCreatePointEx(IPointGeo point, DateTime time) {
			if (_points.ContainsKey(point)) {
				if (_points[point].Time == DateTime.MinValue || _points[point].Time == time) {
					_points[point].Time = time;
					return _points[point];
				}
				else {
					PointEx result = new PointEx() { Latitude = point.Latitude, Longitude = point.Longitude, Elevation = point.Elevation, Time = time };
					_points[point] = result;

					OSMNode pointOSM = point as OSMNode;
					if (pointOSM != null) {
						result.NodeID = pointOSM.ID;
						if (pointOSM.Tags.ContainsTag("crossroad")) {
							result.Crossroad = true;
						}
					}
					return result;
				}
			}
			else {
				PointEx result = new PointEx() { Latitude = point.Latitude, Longitude = point.Longitude, Elevation = point.Elevation, Time = time};
				_points.Add(point, result);

				OSMNode pointOSM = point as OSMNode;
				if (pointOSM != null) {
					result.NodeID = pointOSM.ID;
					if (pointOSM.Tags.ContainsTag("crossroad")) {
						result.Crossroad = true;
					}
				}
				return result;
			}
		}

		/// <summary>
		/// Creates a new Polyline as part of the result
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="road"></param>
		/// <returns></returns>
		Polyline<IPointGeo> CreateLine(IPointGeo from, IPointGeo to, ConnectionGeometry road) {
			return CreateLine(from, to, DateTime.MinValue, DateTime.MinValue, road);
		}

		/// <summary>
		/// Creates a new Polyline as part of the result and assigns times for the start and end points
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="fromTime"></param>
		/// <param name="totime"></param>
		/// <param name="road"></param>
		/// <returns></returns>
		Polyline<IPointGeo> CreateLine(IPointGeo from, IPointGeo to, DateTime fromTime, DateTime totime, ConnectionGeometry road) {
			PolylineID line = new PolylineID() { WayID = road.WayID };

			PointEx toAdd = GetOrCreatePointEx(from, fromTime);
			line.Nodes.Add(toAdd);
            
			var points = Topology.GetNodesBetweenPoints(from, to, road);
			foreach (var point in points) {
				line.Nodes.Add(GetOrCreatePointEx(point, DateTime.MinValue));
			}

			toAdd = GetOrCreatePointEx(to, totime);
			line.Nodes.Add(toAdd);

			return line;
		}

		/// <summary>
		/// Recostrcucts Path from the list of candidate points
		/// </summary>
		/// <param name="matched">List of the candidate points</param>
		/// <returns>List of Polylines that represents matched path</returns>
		public List<Polyline<IPointGeo>> Reconstruct(IList<CandidatePoint> matched, HashSet<Connection> conSet) {
			_points = new Dictionary<IPointGeo, PointEx>();
			List<Polyline<IPointGeo>> result = new List<Polyline<IPointGeo>>();

			for (int i = 0; i < matched.Count - 1; i++) {
				ConnectionGeometry wayGeometry = null;
				if (Calculations.GetDistance2D(matched[i + 1].MapPoint, matched[i].Road) < Calculations.EpsLength)
					wayGeometry = matched[i].Road;
				else if (Calculations.GetDistance2D(matched[i].MapPoint, matched[i + 1].Road) < Calculations.EpsLength)
					wayGeometry = matched[i + 1].Road;

				// both points are on the same road segment
				if (wayGeometry != null) {
					result.Add(CreateLine(matched[i].MapPoint, matched[i + 1].MapPoint, matched[i].Layer.TrackPoint.Time, matched[i + 1].Layer.TrackPoint.Time, wayGeometry));
                    foreach (var c in wayGeometry.Connections)
                            conSet.Add(c);
                    
				}
				else {
					double length = double.PositiveInfinity;

					// find path between matched[i] and matched[i+1]
					var pathSegments = _pathfinder.FindPath(matched[i], matched[i + 1], ref length);

                    if (pathSegments == null) {
						throw new ArgumentException(string.Format("Can not find path between points {0} and {1}", matched[i].MapPoint, matched[i + 1].MapPoint));
					}
					if (pathSegments.Count > 1) {
						result.Add(CreateLine(pathSegments[0].From.MapPoint, pathSegments[0].To.MapPoint, matched[i].Layer.TrackPoint.Time, DateTime.MinValue, pathSegments[0].Road));

						for (int j = 1; j < pathSegments.Count - 1; j++) {
							result.Add(CreateLine(pathSegments[j].From.MapPoint, pathSegments[j].To.MapPoint, pathSegments[j].Road));
                        }

						result.Add(CreateLine(pathSegments[pathSegments.Count - 1].From.MapPoint, pathSegments[pathSegments.Count - 1].To.MapPoint, DateTime.MinValue, matched[i + 1].Layer.TrackPoint.Time, pathSegments[pathSegments.Count - 1].Road));
					}
					else {
						result.Add(CreateLine(pathSegments[0].From.MapPoint, pathSegments[0].To.MapPoint, matched[i].Layer.TrackPoint.Time, matched[i + 1].Layer.TrackPoint.Time, pathSegments[0].Road));
					}

                    foreach (var s in pathSegments)
                        foreach (var c in s.Road.Connections)
                            conSet.Add(c);
                }
			}
			return result;
		}

		/// <summary>
		/// Tests whether an uturn occured between previousLine and toTest
		/// </summary>
		/// <param name="previousLine"></param>
		/// <param name="toTest"></param>
		/// <returns></returns>
		bool IsUTurn(Polyline<IPointGeo> previousLine, Polyline<IPointGeo> toTest) {
			if (previousLine == null || toTest == null)
				return false;

			if (Calculations.GetLength(toTest) < Calculations.EpsLength)
				return false;

			Segment<IPointGeo> previousLineSegment = null;
			for (int i = previousLine.Segments.Count - 1; i >= 0; i--) {
				if (previousLine.Segments[i].Length > Calculations.EpsLength) {
					previousLineSegment = previousLine.Segments[i];
					break;
				}
			}

			Segment<IPointGeo> toTestSegment = null;
			for (int i = 0; i < toTest.Segments.Count; i++) {
				if (toTest.Segments[i].Length > Calculations.EpsLength) {
					toTestSegment = toTest.Segments[i];
					break;
				}
			}

			if (previousLineSegment != null && toTestSegment != null) {
				double firstBearing = Calculations.GetBearing(previousLineSegment);
				double secondBearing = Calculations.GetBearing(toTestSegment);

				return Math.Abs(Math.Abs(firstBearing - secondBearing) - 180) < 0.01;
			}

			return false;
		}

		/// <summary>
		/// Searches list of the Polylines from maxIndex back to the beggining of th list for the first polyline with non-zero length
		/// </summary>
		/// <param name="lines"></param>
		/// <param name="maxIndex"></param>
		/// <returns></returns>
		int GetPreviousNonZeroLengthLineIndex(IList<Polyline<IPointGeo>> lines, int maxIndex) {
			for (int i = maxIndex; i >= 0; i--) {
				if (lines[i].Length > Calculations.EpsLength)
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Filtes Uturns shorter then mamUTurnLength from the path
		/// </summary>
		/// <param name="path">List of the polylines that represents matched path</param>
		/// <param name="maxUTurnLength">Maximal length of the u-turn in meters</param>
		public void FilterUturns(IList<Polyline<IPointGeo>> path, double maxUTurnLength) {
			if (path == null || path.Count == 0)
				return;

			IPointGeo lastNonUTurn = path[0].Nodes[0];
			Polyline<IPointGeo> lastNonUTurnWay = path[0];

			int index = 0;
			while (index < path.Count) {
				Polyline<IPointGeo> current = path[index];

				if (current.Length < Calculations.EpsLength) {
					index++;
					continue;
				}

				if (current.Length > maxUTurnLength) {
					lastNonUTurn = current.Nodes.Last();
					lastNonUTurnWay = current;
					index++;
					continue;
				}

				int previousLineIndex = GetPreviousNonZeroLengthLineIndex(path, index - 1);

				if (previousLineIndex > -1 && IsUTurn(path[previousLineIndex], current)) {
					for (int i = previousLineIndex + 1; i < index; i++) {
						path[i].Nodes.Clear();
					}

					// forth and back to the same point
					if (Calculations.GetDistance2D(path[previousLineIndex].Nodes[0], current.Nodes[current.Nodes.Count - 1]) < Calculations.EpsLength) {
						IPointGeo from = path[previousLineIndex].Nodes[0];
						IPointGeo to = current.Nodes[current.Nodes.Count - 1];
						((PointEx)to).Time = DateTime.MinValue;

						path[previousLineIndex].Nodes.Clear();
						current.Nodes.Clear();
						current.Nodes.Add(from);
						current.Nodes.Add(to);
					}
					else {
						int currentSegmentIndex = 0;
						int lastSegmentIndex = path[previousLineIndex].Segments.Count - 1;

						int j = 0;
						while (j < current.Segments.Count &&
									 Calculations.GetDistance2D(current.Segments[j].EndPoint, path[previousLineIndex], ref lastSegmentIndex) < Calculations.EpsLength) {
							currentSegmentIndex = j;
							j++;
						}

						// delete the whole previous line
						if (lastSegmentIndex == 0) {
							IPointGeo from = path[previousLineIndex].Nodes[0];
							IPointGeo to = current.Segments[currentSegmentIndex].EndPoint;
							((PointEx)to).Time = DateTime.MinValue;

							while (current.Nodes[0] != to) {
								current.Nodes.RemoveAt(0);
							}
							current.Nodes.Insert(0, from);

							path[previousLineIndex].Nodes.Clear();
						}
						// delete the whole current line
						else if (currentSegmentIndex == current.Segments.Count - 1) {
							IPointGeo from = path[previousLineIndex].Segments[lastSegmentIndex].StartPoint;
							IPointGeo to = current.Nodes[current.Nodes.Count - 1];
							((PointEx)to).Time = DateTime.MinValue;

							while (path[previousLineIndex].Nodes[path[previousLineIndex].Nodes.Count - 1] != from) {
								path[previousLineIndex].Nodes.RemoveAt(path[previousLineIndex].Nodes.Count - 1);
							}

							path[previousLineIndex].Nodes.Add(to);
							current.Nodes.Clear();
						}
					}
				}
				else {
					index++;
				}
			}

			index = 0;
			while (index < path.Count) {
				if (path[index].Nodes.Count == 0)
					path.RemoveAt(index);
				else
					index++;
			}

			while (path.Count > 0 && ((PointEx)path[0].Nodes[0]).Time == DateTime.MinValue)
				path.RemoveAt(0);

			while (path.Count > 0 && ((PointEx)path.Last().Nodes.Last()).Time == DateTime.MinValue)
				path.RemoveAt(path.Count -1);
		}

		/// <summary>
		/// Saves path to the OSMDB
		/// </summary>
		/// <param name="path">The list of polylines that represent matched path</param>
		/// <returns>OSMDB with path converted to the OSM format</returns>
		public OSMDB SaveToOSM(IList<Polyline<IPointGeo>> path) {
			_db = new OSMDB();
			_dbCounter = -1;

			IPointGeo lastPoint = null;
			OSMNode node = null;

			foreach (var line in path) {
				if (line.Nodes.Count == 0)
					continue;

				if (line.Nodes.Count == 1)
					throw new Exception();
                
				OSMWay way = new OSMWay(_dbCounter--);
				way.Tags.Add(new OSMTag("way-id", ((PolylineID)line).WayID.ToString()));
				way.Tags.Add(new OSMTag("order", (_db.Ways.Count + 1).ToString()));
				_db.Ways.Add(way);
				foreach (var point in line.Nodes) {
					if (point != lastPoint) {
						lastPoint = point;
						PointEx pt = (PointEx)point;

						node = new OSMNode(_dbCounter--, pt.Latitude, pt.Longitude);
						if (pt.NodeID != 0) {
							node.Tags.Add(new OSMTag("node-id", pt.NodeID.ToString()));
						}
						if (pt.Time != DateTime.MinValue) {
							node.Tags.Add(new OSMTag("time", pt.Time.ToString()));
						}
						if (pt.Crossroad) {
							node.Tags.Add(new OSMTag("crossroad", "yes"));						
						}
						
						_db.Nodes.Add(node);
					}
					way.Nodes.Add(node.ID);
				}
			}

			return _db;
		}
	}
}
