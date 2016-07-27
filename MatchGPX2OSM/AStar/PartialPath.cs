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

namespace LK.MatchGPX2OSM {
	/// <summary>
	/// Represents partial Path in the AStar pathfinder
	/// </summary>
	class PartialPath : IComparer<PartialPath>, IComparable {
		public Node End;
		public Node PreviousNode;
		public ConnectionGeometry PathFromPrevious;
		public double Length;
		public double EstimationToEnd;

		/// <summary>
		/// Determines whether Obj equals this PartialPath
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>true if Obj is PartialPath and CurrentPosition are the same, otherwise false</returns>
		public override bool Equals(object obj) {
			PartialPath other = obj as PartialPath;
			if (other != null) {
				return this.End.Equals(other.End);
			}
			else
				return false;
		}

		/// <summary>
		/// Determines whether Other equals this PartialPath
		/// </summary>
		/// <param name="other">The PartialPath to compare with the current PartialPath</param>
		/// <returns>true if CurrentPosition are the same, otherwise false</returns>
		public bool Equals(PartialPath other) {
				return this.End.Equals(other.End);
		}

		/// <summary>
		/// Returns a hash code for the current PartialPath
		/// </summary>
		/// <returns>A hash code for the current Segment.</returns>
		public override int GetHashCode() {
			return this.End.GetHashCode();
		}

		#region IComparer<Path> Members

		public int Compare(PartialPath x, PartialPath y) {
			double totalLength = x.Length + x.EstimationToEnd;

			return totalLength.CompareTo(y.Length + y.EstimationToEnd);
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object obj) {
			if (obj is PartialPath) {
				PartialPath other = (PartialPath)obj;
				return Compare(this, other);
			}
			return 0;
		}

		#endregion
	}
}
