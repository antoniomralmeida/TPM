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

    public class OSMObjectCollection<T> : IEnumerable<T>  where T : OSMObject {
		protected Dictionary<long, T> _storage;

		public OSMObjectCollection() {
			_storage = new Dictionary<long, T>();
		}

		/// <summary>
		/// Gets the T object with the specified ID.
		/// </summary>
		/// <param name="key">ID of the object to get.</param>
		/// <returns>The T object with the specific ID.</returns>
		/// <exception cref="ArgumentException"></exception>
		public T this[long id] {
			get {
				try {
					return _storage[id];
				}
				catch (KeyNotFoundException e) {
					throw new ArgumentException(String.Format("The object with ID {0} wasn't found in the collection", id));
				}
			}
		}

		/// <summary>
		/// Gets the number of T objects in the collection
		/// </summary>
		public int Count {
			get {
				return _storage.Count;
			}
		}


		/// <summary>
		/// Adds an T object into the collection
		/// </summary>
		/// <param name="toAdd">The T object to be added to the collection</param>
		public void Add(T item) {
			if (item == null) {
				throw new ArgumentNullException("Can not add null to the collection");
			}

			_storage.Add(item.ID, item);
		}

		/// <summary>
		/// Removes the specific T object from the collection
		/// </summary>
		/// <param name="toRemove">The T object to remove from the collection</param>
		/// <returns>true if the object was removed from the collection, otherwise false</returns>
		public bool Remove(T toRemove) {
			return _storage.Remove(toRemove.ID);
		}

		/// <summary>
		/// Removes all objects from the collection
		/// </summary>
		public void RemoveAll() {
			_storage.Clear();
		}

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator() {
			return _storage.Values.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return _storage.Values.GetEnumerator();
		}

		#endregion
	}
}
