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

namespace LK.MatchGPX2OSM {
	/// <summary>
	/// Represents a directed connection (an edge in the roadgraph) between two nodes.
	/// </summary>
	public class Connection {
		/// <summary>
		/// Create a new connection with the specific end nodes and create relation among nodes and connection
		/// </summary>
		/// <param name="from">The node, where this connection starts</param>
		/// <param name="to">The noe where this connection ends</param>
		public Connection(Node from, Node to) {
			this.From = from;
			from.Connections.Add(this);

			this.To = to;
			to.Connections.Add(this);
		}

		/// <summary>
		/// Gets or sets node, where this connection starts
		/// </summary>
		public Node From { get; set; }

		/// <summary>
		/// Gets or set node wher this connection ends
		/// </summary>
		public Node To { get; set; }

		/// <summary>
		/// Gets or sets shape of the connection
		/// </summary>
		public ConnectionGeometry Geometry { get; set; }

		/// <summary>
		/// Gets or sets maximal speed on this connection
		/// </summary>
		public double Speed { get; set; }
    }
}
