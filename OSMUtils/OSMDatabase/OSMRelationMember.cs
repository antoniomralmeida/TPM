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
	/// <summary>
	/// Represents a member of the OSM relation
	/// </summary>
	public class OSMRelationMember {
		private OSMRelationMemberType _type;
		/// <summary>
		/// Gets the type of the relation member
		/// </summary>
		public OSMRelationMemberType Type {
			get { return _type; }
		}

		private long _reference;
		/// <summary>
		/// Gets the reference of this relation member
		/// </summary>
		public long Reference {
			get { return _reference; }
		}

		private string _role;
		/// <summary>
		/// Gets or sets the role of the relation's member.
		/// </summary>
		public string Role {
			get { return _role; }
			set { _role = value; }
		}

		/// <summary>
		/// Creates a new OSMRelationMember object with the given value
		/// </summary>
		/// <param name="type">The type of this relation member</param>
		/// <param name="reference">The reference of this relation member </param>
		/// <param name="role">The role of this member. It can be null.</param>
		public OSMRelationMember(OSMRelationMemberType type, long reference, string role) {
			_type = type;
			_reference = reference;
			_role = role;
		}

	}
}
