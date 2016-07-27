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
	/// Represents part of GPS trace between two points
	/// </summary>
	public class Segment<T> where T : IPointGeo {
		/// <summary>
		/// Gets the start point of this Segment
		/// </summary>
		public T StartPoint { get; private set; }
		
		/// <summary>
		/// Gets the end point of this Segment
		/// </summary>
		public T EndPoint { get; private set; }

		/// <summary>
		/// Creates a new segment with the specific start and end points
		/// </summary>
		/// <param name="start">The point, where segment starts</param>
		/// <param name="end">The point, where segment ends</param>
		public Segment(T start, T end)  {
			StartPoint = start;
			EndPoint = end;

			_length = Calculations.GetDistance2D(StartPoint, EndPoint);
		}

		private double _length;
		/// <summary>
		/// Gets length of the segment in meters
		/// </summary>
		public double Length {
			get {
				return _length;
			}
		}

		/// <summary>
		/// Determines whethet specified object is equal to the current Segment
		/// </summary>
		/// <param name="obj">The object to compare with the current Segment</param>
		/// <returns>true if obj is Segment and has the same StartPoint and EndPoint as this Segment, otherwise returns false</returns>
		public override bool Equals(object obj) {
			Segment<T> other = obj as Segment<T>;
			if (other != null) {
				return StartPoint.Equals(other.StartPoint) && EndPoint.Equals(other.EndPoint);
			}
			else
				return false;
		}

		/// <summary>
		/// Determines whethet specified object is equal to the current Segment
		/// </summary>
		/// <param name="obj">The object to compare with the current Segment</param>
		/// <returns>true if obj is Segment and has the same StartPoint and EndPoint as this Segment, otherwise returns false</returns>
		public bool Equals(Segment<T> other) {
			if (other != null) {
				return StartPoint.Equals(other.StartPoint) && EndPoint.Equals(other.EndPoint);
			}
			else
				return false;
		}

		/// <summary>
		/// Returns a hash code for the current Segment
		/// </summary>
		/// <returns>A hash code for the current Segment.</returns>
		public override int GetHashCode() {
			return unchecked(StartPoint.GetHashCode() + 31 * EndPoint.GetHashCode());
		}
	}
}
