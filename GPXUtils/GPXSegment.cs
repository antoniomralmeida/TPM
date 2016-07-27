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
	/// Represents a segment of the GPS track between two points
	/// </summary>
	public class GPXSegment : Segment<GPXPoint> {
		/// <summary>
		/// Creates a new segment with the specific start and end points
		/// </summary>
		/// <param name="start">The point, where segment starts</param>
		/// <param name="end">The point, where segment ends</param>
		public GPXSegment(GPXPoint start, GPXPoint end)
			: base(start, end) {
		}

		/// <summary>
		/// Gets the time of travel between the start and the end point 
		/// </summary>
		public TimeSpan TravelTime {
			get {
				return EndPoint.Time - StartPoint.Time;
			}
		}

		/// <summary>
		/// Gets the average speed on the segment in kph
		/// </summary>
		public double AverageSpeed {
			get {
				return Length / 1000 / TravelTime.TotalHours;
			}
		}
	}
}
