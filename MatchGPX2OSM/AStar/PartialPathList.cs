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

using LK.CommonLib.Collections;

namespace LK.MatchGPX2OSM {
	/// <summary>
	/// Represents a priority queue of the PartialPath, it is used as open list in the A* algorithm
	/// </summary>
	class PartialPathList : BinaryHeap<PartialPath> {
		Dictionary<Node, PartialPath> _paths;

		/// <summary>
		/// Creates a new instance of the PartialPathList
		/// </summary>
		public PartialPathList()
			: base() {
				_paths = new Dictionary<Node, PartialPath>();
		}

		/// <summary>
		/// Gets the PartialPath with the specified Position
		/// </summary>
		/// <param name="position">The Position of PartialPath</param>
		/// <returns></returns>
		public PartialPath this[Node position] {
			get {
				return _paths[position];
			}
		}

		/// <summary>
		/// Updates the PartialPath length
		/// </summary>
		/// <param name="item">The item to update</param>
		/// <param name="pathLength">The new length of the PartialPath</param>
		public void Update(PartialPath item, double pathLength) {
			base.Remove(item);

			item.Length = pathLength;
			base.Add(item);
		}
		
		/// <summary>
		/// Adds a new item to the List
		/// </summary>
		/// <param name="item">The item to add</param>
		public new void Add(PartialPath item) {
			_paths.Add(item.End, item);
			base.Add(item);
		}

		/// <summary>
		/// Removes the item from the List
		/// </summary>
		/// <param name="item">The item to remove</param>
		/// <returns>true if item was removed, otherwise returns false</returns>
		public new bool Remove(PartialPath item) {
			_paths.Remove(item.End);
			return base.Remove(item);
		}

		/// <summary>
		/// Removes the shortest PartialPath from the list
		/// </summary>
		/// <returns>The shortest PartialPath object from the list</returns>
		public new PartialPath RemoveTop() {
			PartialPath result = base.RemoveTop();
			_paths.Remove(result.End);

			return result;
		}

		/// <summary>
		/// Determinates whether List contains the specified item
		/// </summary>
		/// <param name="item">The item to check</param>
		/// <returns>true if item is presented in the List, otherwise returns false</returns>
		public new bool Contains(PartialPath item) {
			return _paths.ContainsKey(item.End);
		}

		/// <summary>
		/// Determinates whether List contains the item with the specified Position
		/// </summary>
		/// <param name="item">The Position of the item</param>
		/// <returns>true if item is presented in the List, otherwise returns false</returns>
		public bool Contains(Node position) {
			return _paths.ContainsKey(position);
		}

		/// <summary>
		/// Removes all objects from the List
		/// </summary>
		public new void Clear() {
			base.Clear();
			_paths.Clear();
		}
	}
}
