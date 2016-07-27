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

using LK.GPXUtils;

namespace LK.GPXUtils.GPXDataSource {
	public interface IGPXDataWriter {
		/// <summary>
		/// Writes specific GPXPoint to the storage
		/// </summary>
		/// <param name="waypoint">The waypoint to be written</param>
		void WriteWaypoint(GPXPoint waypoint);

		/// <summary>
		/// Writes specific GPXRoute to the storage
		/// </summary>
		/// <param name="route">The route to be written</param>
		void WriteRoute(GPXRoute route);

		/// <summary>
		/// Writes specific GPXTrack to the storage
		/// </summary>
		/// <param name="track">The track to be written</param>
		void WriteTrack(GPXTrack track);
	}
}
