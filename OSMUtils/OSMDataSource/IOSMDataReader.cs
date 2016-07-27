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

using LK.OSMUtils.OSMDatabase;

namespace LK.OSMUtils.OSMDataSource {
	public delegate void OSMNodeReadHandler(OSMNode node);
	public delegate void OSMWayReadHandler(OSMWay way);
	public delegate void OSMRelationReadHandler(OSMRelation relation);

	public interface IOSMDataReader {
		/// <summary>
		/// Occurs when an OSMNode is read from the datasource
		/// </summary>
		event OSMNodeReadHandler NodeRead;

		/// <summary>
		/// Occurs when an OSMWay is read from the datasource
		/// </summary>
		event OSMWayReadHandler WayRead;

		/// <summary>
		/// Occurs when an OSMRelation is read from the datasource
		/// </summary>
		event OSMRelationReadHandler RelationRead;
	}
}
