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

using LK.GeoUtils.Geometry;

namespace LK.GPXUtils {
	/// <summary>
	/// Represents a geographic point with optional elevation and time.
	/// </summary>
	public class GPXPoint : IPointGeo {


        public GPXPoint() {
            
        }
        /// <summary>
		/// Gets or sets the Id of the point
		/// </summary>
		public long Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the latitude of this point
        /// </summary>
        public double Latitude {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the longitude of this point
		/// </summary>
		public double Longitude {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the elevation of this point
		/// </summary>
		/// <remarks>Value in meters above mean sea level.</remarks>
		public double Elevation {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the time when this point was recorded
		/// </summary>
		public DateTime Time {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the name of this point
		/// </summary>
		public string Name {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the description of this point
		/// </summary>
		public string Description {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the Comment for this point
		/// </summary>
		public string Commenet {
			get;
			set;
		}

        /// <summary>
		/// Gets or sets the traffic signal of a point
		/// </summary>
		public Boolean TrafficSignal
        {
            get;
            set;
        }
        /// <summary>
		/// Creates a new instance of GPXPoint
		/// </summary>
		/// <param name="lat">The latitute of the point</param>
		/// <param name="lon">The longitude of the point</param>
		public GPXPoint(long id, double lat, double lon) : this()
        {
            Latitude = lat;
            Longitude = lon;
            Id = id;

            Elevation = 0;
            Time = DateTime.MinValue;
        }

        /// <summary>
		/// Creates a new instance of GPXPoint
		/// </summary>
		/// <param name="lat">The latitute of the point</param>
		/// <param name="lon">The longitude of the point</param>
		public GPXPoint(long id, double lat, double lon, Boolean trafficSignal) : this()
        {
            Latitude = lat;
            Longitude = lon;
            Id = id;
            TrafficSignal = trafficSignal;

            Elevation = 0;
            Time = DateTime.MinValue;
        }

        /// <summary>
        /// Creates a new instance of GPXPoint
        /// </summary>
        /// <param name="lat">The latitute of the point</param>
        /// <param name="lon">The longitude of the point</param>
        public GPXPoint(double lat, double lon) : this() {
			Latitude = lat;
			Longitude = lon;

			Elevation = 0;
			Time = DateTime.MinValue;
		}

		/// <summary>
		/// Creates a new instance of GPXPoint
		/// </summary>
		/// <param name="lat">The latitute of the point</param>
		/// <param name="lon">The longitude of the point</param>
		/// <param name="time">The time, when the point was recorded</param>
		public GPXPoint(double lat, double lon, DateTime time) : this() {
			Latitude = lat;
			Longitude = lon;
			Time = time;

			Elevation = 0;
		}

		/// <summary>
		/// Creates a new instance of GPXPoint
		/// </summary>
		/// <param name="lat">The latitute of the point</param>
		/// <param name="lon">The longitude of the point</param>
		/// <param name="time">The time, when the point was recorded</param>
		/// <param name="elevation">The elevation of the point</param>
		public GPXPoint(double lat, double lon, DateTime time, double elevation)
			: this() {
			Latitude = lat;
			Longitude = lon;
			Time = time;
			Elevation = elevation;
		}
	}
}
