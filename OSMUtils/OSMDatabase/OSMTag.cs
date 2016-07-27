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
	public class OSMTag {
		private string _key;
		/// <summary>
		/// Gets the key of the tag
		/// </summary>
		public string Key {
			get { return _key; }
		}

		private string _value;
		/// <summary>
		/// Gets or Sets the value of the tag
		/// </summary>
		public string Value {
			get { return _value; }
			set { _value = value; }
		}

		/// <summary>
		/// Creates a new OSMTag object with the given key and value
		/// </summary>
		/// <param name="key">The key of the tag</param>
		/// <param name="value">The value of the tag</param>
		public OSMTag(string key, string value) {
			if (string.IsNullOrEmpty(key)) {
				throw new ArgumentException("Parameter 'key' can't be null or the empty string");
			}

			if (value == null) {
				throw new ArgumentNullException("Parameter 'value' can't be null");
			}

			_key = key;
			_value = value;
		}

		/// <summary>
		/// Returns a string representation of the current object.
		/// </summary>
		/// <returns>A string that represent the current object</returns>
		public override string ToString() {
			return String.Format("{0} = {1}", _key, _value);
		}

		/// <summary>
		/// Compares the current OSMTag object with the specified object for equivalence.
		/// </summary>
		/// <param name="obj">The object to test for equivalence with the current OSMTag object.</param>
		/// <returns>true if the objects are equal, otherwise returns false.</returns>
		public override bool Equals(object obj) {
			if (obj == null)
				return false;

			if (obj is OSMTag) {
				OSMTag other = (OSMTag)obj;

				return _key.Equals(other._key) && _value.Equals(other._value);
			}
			else {
				return base.Equals(obj);
			}
		}

		/// <summary>
		/// Returns the hash code for the current object
		/// </summary>
		/// <returns>An integer hash code</returns>
		public override int GetHashCode() {
			return unchecked(_key.GetHashCode() * 83 +  _value.GetHashCode());
		}  
	}
}
