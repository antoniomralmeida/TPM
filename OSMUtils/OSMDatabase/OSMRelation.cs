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

namespace LK.OSMUtils.OSMDatabase {
	public class OSMRelation : OSMObject {
		/// <summary>
		/// Creates a new OSMRelation with the scpecific ID.
		/// </summary>
		/// <param name="id">ID of the OSMRelation.</param>
		public OSMRelation(int id)
			: base(id) {
				_members = new List<OSMRelationMember>();
		}

		protected List<OSMRelationMember> _members;
		/// <summary>
		/// Gets list of node IDs, that forms the way
		/// </summary>
		public IList<OSMRelationMember> Members {
			get {
				return _members;
			}
		}
	}
}
