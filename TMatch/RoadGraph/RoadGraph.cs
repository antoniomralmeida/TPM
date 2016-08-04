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

using LK.OSMUtils.OSMDatabase;
using LK.OSMUtils.OSMDataSource;
using LK.GeoUtils.Geometry;

namespace LK.TMatch {
	//Represents routable road graph, every road segment is represented by directed Connection and every road crossing by Node
	public class RoadGraph {
		private List<Node> _nodes;
		/// <summary>
		/// Gets collection of all nodes in this graph
		/// </summary>
		public ICollection<Node> Nodes {
			get {
				return _nodes;
			}
		}

		private List<Connection> _connections;
		/// <summary>
		/// Gets collection of all edges in this graph
		/// </summary>
		public ICollection<Connection> Connections {
			get {
				return _connections;
			}
		}

		private List<ConnectionGeometry> _connectionGeometries;
		/// <summary>
		/// Gets collection of all shapes of the connections from this graph
		/// </summary>
		public ICollection<ConnectionGeometry> ConnectionGeometries {
			get {
				return _connectionGeometries;
			}
		}

		/// <summary>
		/// Creates a new RoadGraph
		/// </summary>
		public RoadGraph() {
			_nodes = new List<Node>();
			_connections = new List<Connection>();
			_connectionGeometries = new List<ConnectionGeometry>();
		}

		/// <summary>
		/// Builds road graph from map data
		/// </summary>
		/// <param name="map">OSMDB with preprocessed map data from OSM2Routing utility</param>
		public void Build(OSMDB map) {
			Dictionary<long, Node> usedNodes = new Dictionary<long, Node>();

			foreach (var segment in map.Ways) {
				Node start = GetOrCreateNode(segment.Nodes[0], usedNodes);
				try {
					start.MapPoint = map.Nodes[segment.Nodes[0]];
				}
				catch (ArgumentException) {
					continue; // If the start node was not found in the database, skip this path completely
				}

				Node end = GetOrCreateNode(segment.Nodes[segment.Nodes.Count - 1], usedNodes);
				try {
					end.MapPoint = map.Nodes[segment.Nodes[segment.Nodes.Count - 1]];
				}
				catch (ArgumentException) {
					continue; // If the end node was not found in the database, skip this path completely
				}

				double speed = double.Parse(segment.Tags["speed"].Value, System.Globalization.CultureInfo.InvariantCulture);
				int wayId = int.Parse(segment.Tags["way-id"].Value, System.Globalization.CultureInfo.InvariantCulture);

				ConnectionGeometry geometry = new ConnectionGeometry();
				geometry.WayID = wayId;
				foreach (var n in segment.Nodes) {
					try {
						OSMNode mapPoint = map.Nodes[n]; 
						geometry.Nodes.Add(mapPoint);
						//geometry.Nodes.Add(new PointGeo(mapPoint.Latitude, mapPoint.Longitude));
					}
					catch (ArgumentException) {
					continue; // If an intermediate node was not found in the database, skip just that node
					}
				}
				_connectionGeometries.Add(geometry);

				if (segment.Tags["accessible"].Value == "yes") {
                    Connection sc;
                    if (segment.Tags.ContainsTag("traffic"))
                    {
                        var traffic = new HashSet<long>(segment.Tags["traffic"].Value.Split(',').Select(x => long.Parse(x)));
                        sc = new Connection(start, end) { Speed = speed, Geometry = geometry, Traffic = traffic };
                    } else sc = new Connection(start, end) { Speed = speed, Geometry = geometry };
                    geometry.Connections.Add(sc);
					_connections.Add(sc);
				}

				if (segment.Tags["accessible-reverse"].Value == "yes") {
                    Connection sc;
                    if (segment.Tags.ContainsTag("traffic"))
                    {
                        var traffic = new HashSet<long>(segment.Tags["traffic"].Value.Split(',').Select(x => long.Parse(x)));
                        sc = new Connection(end, start) { Speed = speed, Geometry = geometry, Traffic = traffic };
                    }
                    else sc = new Connection(end, start) { Speed = speed, Geometry = geometry };
					geometry.Connections.Add(sc);
					_connections.Add(sc);
				}
			}
		}

		/// <summary>
		/// Gets Node with specific ID from the internal storage if available or creates a new one
		/// </summary>
		/// <param name="nodeId">The ID of the node</param>
		/// <returns>The node with specific ID</returns>
		private Node GetOrCreateNode(long nodeId, Dictionary<long, Node> usedNodes) {
			if (usedNodes.ContainsKey(nodeId) == false) {
				Node n = new Node();
				usedNodes.Add(nodeId, n);
				_nodes.Add(n);
				return n;
			}
			else {
				return usedNodes[nodeId];
			}
		}
	}
}
