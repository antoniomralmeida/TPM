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
	public interface IOSMDataWriter {
		/// <summary>
		/// Writes the specific OSMNode to the data source
		/// </summary>
		/// <param name="node">The OSMNode to be written.</param>
		void WriteNode(OSMNode node);

		/// <summary>
		/// Writes the specific OSMWay to the data source
		/// </summary>
		/// <param name="way">The OSMWay to be written.</param>
		void WriteWay(OSMWay way);

		/// <summary>
		/// Writes the specific OSMRelation to the data source
		/// </summary>
		/// <param name="relation">The OSMRelation to be written.</param>
		void WriteRelation(OSMRelation relation);
	}
}
