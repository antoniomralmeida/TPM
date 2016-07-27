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
	/// Represents a generic interface for polyline
	/// </summary>
	public interface IPolyline<T> where T : IPointGeo {
		/// <summary>
		/// Gets nodes of this polyline
		/// </summary>
		IList<T> Nodes { get; }

		/// <summary>
		/// Gets segments of this polyline
		/// </summary>
		IList<Segment<T>> Segments { get; }

		/// <summary>
		/// Gets the length of the IPolyline in meters
		/// </summary>
		double Length { get; }

		/// <summary>
		/// Gets nodes count
		/// </summary>
		int NodesCount { get; }
	}

	/// <summary>
	/// Represents a non-generic interface for polyline
	/// </summary>
	public interface IPolyline {
		/// <summary>
		/// Gets nodes of this polyline
		/// </summary>
		IList<IPointGeo> Nodes { get; }

		/// <summary>
		/// Gets nodes count
		/// </summary>
		int NodesCount { get; }
	}
}
