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
using System.Text;

namespace LK.GeoUtils.Geometry {
	/// <summary>
	/// Represents point on the Earth surface
	/// </summary>
	public struct PointGeo : IPointGeo {
		/// <summary>
		/// Gets or sets latitude of this point (north - positive value, south - negative value)
		/// </summary>
		public double Latitude { get; set; }

		/// <summary>
		/// Gets or sets the longitude of this point (east - positive value, west - negative value)
		/// </summary>
		public double Longitude { get; set; }

		/// <summary>
		/// Gets or sets the elevation aboce MSL in meters of this point
		/// </summary>
		public double Elevation { get; set; }

		/// <summary>
		/// Creates a new PointGeo with the specific latitude and longitude
		/// </summary>
		/// <param name="lat">The latitude of the point</param>
		/// <param name="lon">The longitude of the point</param>
		public PointGeo(double lat, double lon) : this() {
			Latitude = lat;
			Longitude = lon;
			Elevation = 0;
		}

		/// <summary>
		/// Creates a new PointGeo with the specific latitude, longitude and elevation
		/// </summary>
		/// <param name="lat">The latitude of the point</param>
		/// <param name="lon">The longitude of the point</param>
		/// <param name="elevation">The elevation above MSL of the point</param>
		public PointGeo(double lat, double lon, double elevation) : this() {
			Latitude = lat;
			Longitude = lon;
			Elevation = elevation;
		}

		/// <summary>
		/// Returns string representation of this point
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return string.Format("lat: {0:F5} lon: {1:F5}", Latitude, Longitude);
		}
	}
}
