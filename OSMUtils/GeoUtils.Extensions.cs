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

using LK.GeoUtils.Geometry;
using LK.OSMUtils.OSMDatabase;

namespace LK.GeoUtils {
	public static class Extensions {
		public static void AddWays(this Polygon<OSMNode> polygon, IList<OSMWay> ways, OSMDB db) {
			if (ways.Count == 1) {
				// Check if the created polygon is closed
				if (ways[0].Nodes.Count > 0 && ways[0].Nodes.First() != ways[0].Nodes.Last()) {
					throw new ArgumentException("Ways does not form a closed polygon");
				}

				for (int i = 0; i < ways[0].Nodes.Count - 1; i++) {
					polygon.AddVertex(db.Nodes[ways[0].Nodes[i]]);
				}
			}
			else {
				long lastVertexID = 0;

				if (ways[0].Nodes.First() == ways.Last().Nodes.First() || ways[0].Nodes.Last() == ways.Last().Nodes.First()) {
					lastVertexID = ways.Last().Nodes.First();
				}
				else {
					lastVertexID = ways.Last().Nodes.Last();
				}
				//// Check orientation of the first way
				//if (ways[0].Nodes.First() == ways[1].Nodes.First() || ways[0].Nodes.First() == ways[1].Nodes.First()) {
				//  for (int ii = ways[0].; ii < verticesToAdd.Count - 1; ii++) {
				//    AddVertex(verticesToAdd[ii]);
				//  }
				//}

				for (int i = 0; i < ways.Count; i++) {
					List<long> verticesToAdd = new List<long>();

					// Checks the way orienatation and picks nodes in correct order
					if (lastVertexID == ways[i].Nodes[0]) {
						verticesToAdd.AddRange(ways[i].Nodes);
					}
					else if (lastVertexID == ways[i].Nodes.Last()) {
						verticesToAdd.AddRange(ways[i].Nodes.Reverse());
					}
					else {
						throw new ArgumentException("Can not create polygon, ways aren't connected");
					}


					for (int ii = 0; ii < verticesToAdd.Count - 1; ii++) {
						polygon.AddVertex(db.Nodes[verticesToAdd[ii]]);
					}

					lastVertexID = verticesToAdd.Last();
				}

				// Check if the created polygon is closed
				if (polygon.VerticesCount > 0 && polygon.Vertices.First() != db.Nodes[lastVertexID]) {
					throw new ArgumentException("Ways does not form a closed polygon");
				}
			}
		}
	}
}
