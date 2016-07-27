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

using LK.MatchGPX2OSM;
using LK.GeoUtils;
using LK.OSMUtils.OSMDatabase;

namespace LK.MatchGPX2OSM {
	/// <summary>
	/// Represents class that is able to find path beteewn two points in the RoadGraph
	/// </summary>
	public class AstarPathfinder {
		protected RoadGraph _graph;
		PartialPathList _open;
		Dictionary<Node, PartialPath> _close;

		List<Connection> _temporaryConnections;
		/// <summary>
		/// Creates a new instance of the pathfinder 
		/// </summary>
		/// <param name="graph">RoadGraph object that represents roads network</param>
		public AstarPathfinder(RoadGraph graph) {
			_graph = graph;

			_open = new PartialPathList();
			_close = new Dictionary<Node, PartialPath>();
			_temporaryConnections = new List<Connection>();
		}

		/// <summary>
		/// Initializes internal properties before search
		/// </summary>
		/// <param name="from">The start point</param>
		/// <param name="to">The destination point</param>
		void Initialize(CandidatePoint from, CandidatePoint to) {
			_open.Clear();
			_close.Clear();

			// Add nodes reachable from the From point to the open list
			foreach (var connection in from.Road.Connections) {
				PartialPath path = new PartialPath() {End = connection.To, PathFromPrevious = connection.Geometry,
																							Length = Calculations.GetPathLength(from.MapPoint, connection.To.MapPoint, connection.Geometry),
																							EstimationToEnd = Calculations.GetDistance2D(connection.To.MapPoint, to.MapPoint)
				};

				if (_open.Contains(path)) {
					if (_open[path.End].Length > path.Length) {
						_open.Remove(_open[path.End]);
						_open.Add(path);
					}
				}
				else {
					_open.Add(path);
				}
			}

			_temporaryConnections.Clear();
			// Add temporary connections to the TO point
			foreach (var targetConnections in to.Road.Connections) {
				Connection destinationConnection = new Connection(targetConnections.From, new Node(to.MapPoint)) { Geometry = to.Road };
				_temporaryConnections.Add(destinationConnection);
			}
		}

		/// <summary>
		/// Removes temporary objects after search
		/// </summary>
		/// <param name="to">The destination point</param>
		void Finalize(CandidatePoint to) {
			foreach (var connection in _temporaryConnections) {
				connection.From.Connections.Remove(connection);
			}
			// Remove temporary connections
			//foreach (var targetConnections in to.Road.Connections) {
			//  var connection = targetConnections.From.Connections.Where(c => c.To.MapPoint == to.MapPoint).Single();
			//  targetConnections.From.Connections.Remove(connection);
			//}
		}
		
		/// <summary>
		/// Builds result path
		/// </summary>
		/// <param name="lastPathPart"></param>
		/// <param name="from"></param>
		/// <returns></returns>
		IList<PathSegment> BuildPath(PartialPath lastPathPart, CandidatePoint from) {
			List<PathSegment> result = new List<PathSegment>();

			while (lastPathPart.PreviousNode != null) {
				result.Add(new PathSegment() { From = lastPathPart.PreviousNode, To = lastPathPart.End, Road = lastPathPart.PathFromPrevious });
				lastPathPart = _close[lastPathPart.PreviousNode];
			}

			result.Add(new PathSegment() { From = new Node(from.MapPoint), To = lastPathPart.End, Road = lastPathPart.PathFromPrevious });
			result.Reverse();

			return result;
		}
		
		/// <summary>
		/// Finds path between From and To points
		/// </summary>
		/// <param name="from">The start point</param>
		/// <param name="to">The destination point</param>
		/// <param name="length">Length of the path in meters</param>
		/// <returns>Path as list of PathSegments beteewn two point. If path wasn't found returns null.</returns>
		public IList<PathSegment> FindPath(CandidatePoint from, CandidatePoint to, ref double length) {
			Initialize(from, to);
			
			while (_open.Count > 0) {
				PartialPath current = _open.RemoveTop();
				_close.Add(current.End, current);
	
				// Path found
				if(current.End.MapPoint == to.MapPoint) {
					length = current.Length;
					var result = BuildPath(current, from);
					Finalize(to);

					return result;
				}
				
				foreach (var link in current.End.Connections) {
					if (link.From != current.End) continue;

					double distance;
					if (link.To.MapPoint == to.MapPoint) 
						distance = current.Length + Calculations.GetPathLength(current.End.MapPoint, to.MapPoint, link.Geometry);
					else
						distance = current.Length + link.Geometry.Length;

					if (_open.Contains(link.To)) {
						// Update previously found path in the open list (if this one is shorter)
						PartialPath p = _open[link.To];
						if (p.Length > distance) {
							p.PreviousNode = current.End;
							p.PathFromPrevious = link.Geometry;
							_open.Update(p, distance);
						}
					}
					else if (_close.ContainsKey(link.To)) {
						// Update previously found path in the close list (if this one is shorter)
						if (_close[link.To].Length > distance) {
							_close[link.To].Length = distance;
							_close[link.To].End = current.End;
							_close[link.To].PathFromPrevious = link.Geometry;
						}
					}
					else {
						// Expand path to new node
						PartialPath expanded = new PartialPath() {
							Length = distance,
							EstimationToEnd = Calculations.GetDistance2D(link.To.MapPoint, to.MapPoint),
							                                         End = link.To, PreviousNode = current.End, PathFromPrevious = link.Geometry };
						_open.Add(expanded);
					}
				}
			}

			Finalize(to);
			return null;
		}
	}
}
