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

using LK.GeoUtils.Geometry;

namespace LK.OSMUtils.OSMDatabase {
    [Serializable]
    /// <summary>
    /// Represents node in the OSM database.
    /// </summary>
    public class OSMNode : OSMObject, IPointGeo {
		private double _latitude;
		/// <summary>
		/// Gets or sets the latitude of the node
		/// </summary>
		public double Latitude {
			get { return _latitude; }
			set { _latitude = value; }
		}

		private double _longitude;
		/// <summary>
		/// Gets or sets the longitude of the node
		/// </summary>
		public double Longitude {
			get { return _longitude; }
			set { _longitude = value; }
		}

		/// <summary>
		/// Creates a new OSMNode with the specified ID and coordinates
		/// </summary>
		/// <param name="id">The ID of the node</param>
		/// <param name="latitude">The latitude of the point</param>
		/// <param name="longitude">The longitude of the point</param>
		public OSMNode(long id, double latitude, double longitude)
			: base(id) {
				_latitude = latitude;
				_longitude = longitude;
		}

		/// <summary>
		/// Creates a new OSMNode, ID is set to the int.MinValue
		/// </summary>
		public OSMNode()
			: base(int.MinValue) {
		}

        #region IPointGeo Members

        /// <summary>
        /// Gets or sets the elevation of the node
        /// </summary>
        /// <remarks>Isn't supported right now, returns 0.</remarks>
        public double Elevation
        {
            get { return 0; }
            set {; }
        }

        #endregion
    }
}
