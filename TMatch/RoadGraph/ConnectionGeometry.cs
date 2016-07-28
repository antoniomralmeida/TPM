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

namespace LK.TMatch {
	//Represents a shape of the connection
	public class ConnectionGeometry : Polyline<IPointGeo> {
		/// <summary>
		/// Gets or sets OSM ID of the original way
		/// </summary>
		public int WayID { get; set; }

		/// <summary>
		/// Gets the list of connections that shares this shape
		/// </summary>
		public IList<Connection> Connections { get; private set; }

		/// <summary>
		/// Creates a new Connection geometry object
		/// </summary>
		public ConnectionGeometry()
			: base() {
				Connections = new List<Connection>(2);
		}

		private BBox _bbox;
		/// <summary>
		/// Gets bounding box of this connection geometry
		/// </summary>
		/// <remarks>BBox is computed when it's accessed for the first time and it's cached</remarks>
		public BBox BBox {
			get {
				if (_bbox == null) {
					_bbox = new BBox(Nodes);
				}
				return _bbox;
			}
		}
	}
}
