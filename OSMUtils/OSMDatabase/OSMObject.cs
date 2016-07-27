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
	public abstract class OSMObject {
		private long _id;
		/// <summary>
		/// Gets or sets object ID
		/// </summary>
		public long ID {
			get {
				return _id;
			}
		}

		/// <summary>
		/// Intializes a new OSM object with the specified ID 
		/// </summary>
		/// <param name="id">Object ID.</param>
		protected OSMObject(long id) {
			_id = id;
			_tags = new OSMTagsCollection();
		}

		protected OSMTagsCollection _tags;
		/// <summary>
		/// Gets the collection of tags asociated with the current obejct
		/// </summary>
		public OSMTagsCollection Tags {
			get {
				return _tags;
			}
		}

		/// <summary>
		/// Returns a string representation the currenet object
		/// </summary>
		/// <returns>A string that represents the current object</returns>
		public override string ToString() {
			return _id.ToString();
		}

		/// <summary>
		/// Compares the current OSMObject with the specified object for equivalence.
		/// </summary>
		/// <param name="obj">The object to test for equivalence with the current object.</param>
		/// <returns>true if the OSMTag objects are equal, otherwise returns false.</returns>
		public override bool Equals(object obj) {
			if (obj == null)
				return false;

			if (obj is OSMObject) {
				OSMObject other = (OSMObject)obj;

				return _id.Equals(other._id);
			}
			else
				return base.Equals(obj);
		}

		/// <summary>
		/// Returns the hash code for the current object
		/// </summary>
		/// <returns>An integer hash code</returns>
		public override int GetHashCode() {
			return _id.GetHashCode();
		}


		//private DateTime _timestamp;
		///// <summary>
		///// Gets or sets timestamp 
		///// </summary>
		//public DateTime Timestamp {
		//  get { return _timestamp; }
		//  set { _timestamp = value; }
		//}


		//private int _version;

		//public int Version {
		//  get { return _version; }
		//}


		//private bool _visible;

		//public bool Visible {
		//  get { return _visible; }
		//  set { _visible = value; }
		//}

		//private string _user;

		//public string User {
		//  get { return _user; }
		//}

		//private int _userID;

		//public int UserID {
		//  get { return _userID; }
		//}

		//private int _changeset;

		//public int Changeset {
		//  get { return _changeset; }
		//}


	}
}
