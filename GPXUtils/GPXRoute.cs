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

namespace LK.GPXUtils {
	/// <summary>
	/// Represents a route
	/// </summary>
	public class GPXRoute : Polyline<GPXPoint> {
		/// <summary>
		/// Gets or sets the name of this route
		/// </summary>
		public string Name {
			get;
			set;
		}

		/// <summary>
		/// Creates a new, empty instance of GPXRoute
		/// </summary>
		public GPXRoute() {
		}

		/// <summary>
		/// Creates a new instance of GPXRoute with specific name
		/// </summary>
		/// <param name="name">The name of this GPXRoute</param>
		public GPXRoute(string name) {
			Name = name;
		}
	}
}
