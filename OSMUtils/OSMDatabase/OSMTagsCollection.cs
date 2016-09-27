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
    [Serializable]
    public class OSMTagsCollection : IEnumerable<OSMTag> {
		protected IList<OSMTag> _tags;

		public OSMTagsCollection() {
		}

		/// <summary>
		/// Gets the number of OSMTag objects in the collection
		/// </summary>
		public int Count {
			get {
				if (_tags == null) {
					return 0;
				}
				else {
					return _tags.Count;
				}
			}
		}

		/// <summary>
		/// Gets the OSMTag with the specified key.
		/// </summary>
		/// <param name="key">Key of the OSMTag to get.</param>
		/// <returns>The OSMTag object with the specified key.</returns>
		/// <exception cref="ArgumentException"></exception>
		public OSMTag this[string key] {
			get {
				return GetTagByKey(key);
			}
		}

		/// <summary>
		/// Adds an OSMTag into the collection
		/// </summary>
		/// <param name="toAdd">The OSMTag to be added to the collection</param>
		public void Add(OSMTag toAdd) {
			if (_tags == null) {
				_tags = new List<OSMTag>();
			}

			if (ContainsTag(toAdd.Key)) {
				throw new ArgumentException(String.Format("Collection already contains tag with key '{0}'.", toAdd.Key));
			}

			_tags.Add(toAdd);
		}

		/// <summary>
		/// Removes the specific OSMTag from the collection
		/// </summary>
		/// <param name="toRemove">The OSMTag to remove from the collection</param>
		/// <returns>true if the OSMTag was removed from the collection, otherwise false</returns>
		public bool Remove(OSMTag toRemove) {
			if (_tags == null) {
				return false;
			}
	
			return _tags.Remove(toRemove);
		}

		/// <summary>
		/// Removes all OSMTags from the collection
		/// </summary>
		public void RemoveAll() {
			if (_tags == null) {
				return;
			}

			_tags.Clear();
		}

		/// <summary>
		/// Returns the OSMTag with the given key
		/// </summary>
		/// <param name="key">The key of the OSMTag object</param>
		/// <returns>the OSMTag object with given key. If the tag isn't found, an exception is thrown.</returns>
		protected OSMTag GetTagByKey(string key) {
			if(_tags == null || _tags.Count == 0)
				throw new ArgumentException(String.Format("Collection doesn't contain tag with key '{0}'.", key));

			foreach (OSMTag tag in _tags) {
				if (tag.Key == key) {
					return tag;
				}
			}

			throw new ArgumentException(String.Format("Collection doesn't contain tag with key '{0}'.", key));			
		}

		/// <summary>
		/// Tests whether the collection contains a tag with the given key
		/// </summary>
		/// <param name="key">The key to be tested</param>
		/// <returns>true if collection contains a tag with given key, otherwise returns false.</returns>
		public bool ContainsTag(string key) {
			if (_tags == null || _tags.Count == 0) {
				return false;
			}

			foreach (OSMTag tag in _tags) {
				if (tag.Key == key) {
					return true;
				}
			}

			return false;
		}


		#region IEnumerable<OSMTag> Members

		public IEnumerator<OSMTag> GetEnumerator() {
			if (_tags == null) {
				yield break;
			}
			else {
				foreach (OSMTag tag in _tags) {
					yield return tag;
				}
			}
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			if (_tags == null) {
				yield break;
			}
			else {
				foreach (OSMTag tag in _tags) {
					yield return tag;
				}
			}
		}

		#endregion
	}
}
