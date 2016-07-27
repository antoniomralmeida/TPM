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

namespace LK.OSMUtils.OSMDatabase {
	/// <summary>
	/// Represents way in the OSM database.
	/// </summary>
	public class OSMWay : OSMObject {
		/// <summary>
		/// Creates a new OSMWay with the scpecific ID.
		/// </summary>
		/// <param name="id">ID of the OSMWay.</param>
		public OSMWay(long id)
			: base(id) {
				_nodes = new List<long>();
		}

		/// <summary>
		/// Creates a new OSMWay with the scpecific ID and list of nodes
		/// </summary>
		/// <param name="id">ID of the OSMWay.</param>
		/// <param name="nodes">Nodes of this OSMWay</param>
		public OSMWay(long id, IList<long> nodes)
			: base(id) {
			_nodes = new List<long>(nodes);
		}

		protected List<long> _nodes;
		/// <summary>
		/// Gets list of node IDs, that forms the way
		/// </summary>
		public IList<long> Nodes {
			get {
				return _nodes;
			}
		}

		/// <summary>
		/// Gets bool value indicating whether the way is closed.
		/// </summary>
		/// <remarks>A closed way must have at least 3 nodes.</remarks>
		public bool IsClosed {
			get {
				if (_nodes.Count < 3) {
					return false;
				}

				return _nodes.First() == _nodes.Last();
			}
		}
	}
}
