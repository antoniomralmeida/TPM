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
	public delegate void GPXTrackReadHandler(GPXTrack track);
	public delegate void GPXRouteReadHandler(GPXRoute route);
	public delegate void GPXWaypointReadHandler(GPXPoint waypoint);

	public interface IGPXDataReader {
		/// <summary>
		/// Occurs when a track is read from the datasource
		/// </summary>
		event GPXTrackReadHandler TrackRead;

		/// <summary>
		/// Occurs when a route is read from the datasource
		/// </summary>
		event GPXRouteReadHandler RouteRead;

		/// <summary>
		/// Occurs when a waypoint is read from the datasource
		/// </summary>
		event GPXWaypointReadHandler WaypointRead;
	}
}
