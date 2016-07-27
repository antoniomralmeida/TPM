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

namespace LK.GeoUtils {
	public class GreatCircleDistanceCalculator : IDistanceCalculator {
		/// <summary>
		/// Calculates distance between 2 points on geoid surface (ignores points elevations)
		/// </summary>
		/// <param name="pt1">First point</param>
		/// <param name="pt2">Second point</param>
		/// <returns>distance between given points in meters</returns>
		public double Calculate2D(IPointGeo pt1, IPointGeo pt2) {
			const double earthRadius = 6371010.0;

			double dLat = ToRadians(pt2.Latitude - pt1.Latitude);
			double dLon = ToRadians(pt2.Longitude - pt1.Longitude);

			double a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) + Math.Cos(ToRadians(pt1.Latitude)) * Math.Cos(ToRadians(pt2.Latitude)) * Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0);
			double dAngle = 2 * Math.Asin(Math.Sqrt(a));

			return dAngle * earthRadius;
		}

		/// <summary>
		/// Convert angle from degrees to radians
		/// </summary>
		/// <param name="angle">Angle in degrees</param>
		/// <returns>angle in radians</returns>
		double ToRadians(double angle) {
			return angle * Math.PI / 180;
		}
	}
}
