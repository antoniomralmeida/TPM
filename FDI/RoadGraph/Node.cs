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
using LK.GeoUtils.Geometry;

namespace LK.FDI {
    [Serializable]
	/// <summary>
	/// Represents a node in the routable road graph
	/// </summary>
	public class Node {
		/// <summary>
		/// Creates a new Node with the specific position
		/// </summary>
		/// <param name="mapPoint">The position of the node in geographic coordinates</param>
		public Node(IPointGeo mapPoint) {
			this._connections = new List<Connection>();
			this.MapPoint = mapPoint;
		}

		/// <summary>
		/// Creates a new Node
		/// </summary>
		public Node() {
			this._connections = new List<Connection>();
		}

		private List<Connection> _connections;
		/// <summary>
		/// Gets the collection of all connections going to or from this node
		/// </summary>
		public IList<Connection> Connections {
			get {
				return _connections;
			}
		}

		/// <summary>
		/// Gets or sets position of this node in geographic coordinates
		/// </summary>
		public IPointGeo MapPoint { get; set; }
	}
}
