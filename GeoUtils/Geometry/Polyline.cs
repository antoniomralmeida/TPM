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

using LK.CommonLib.Collections;

namespace LK.GeoUtils.Geometry {
	/// <summary>
	/// Represents a polyline defined by IPointGeo objects
	/// </summary>
	public class Polyline<T> : IPolyline<T> where T : IPointGeo  {
		protected ObservableList<T> _nodes;

		/// <summary>
		/// Gets the list of nodes of this polyline
		/// </summary>
		public IList<T> Nodes {
			get { return _nodes; }
		}

		/// <summary>
		/// Gets the list of segment of this polyline
		/// </summary>
		List<Segment<T>> _segments;
		public IList<Segment<T>> Segments {
			get {
				if (_segments == null) {
					_segments = GetSegments();
				}
				return _segments;
			}
		}

		/// <summary>
		/// Builds list of segments of this polyline
		/// </summary>
		/// <returns>List of segments of this polyline</returns>
		protected List<Segment<T>> GetSegments() {
			List<Segment<T>> result = new List<Segment<T>>();
			for (int i = 0; i < Nodes.Count -1; i++) {
				result.Add(new Segment<T>(Nodes[i], Nodes[i + 1]));
			}

			return result;
		}
	
		/// <summary>
		/// Gets nodes count
		/// </summary>
		public int NodesCount {
			get { return _nodes.Count; }
		}

		private double _length;
		/// <summary>
		/// Gets the length of the Polyline in meters
		/// </summary>
		public double Length {
			get {
				if (double.IsNaN(_length)) {
					_length = Calculations.GetLength((IPolyline<IPointGeo>)this);
				}

				return _length;
			}
		}

		protected void InvalidateComputedProperties(object sender) {
			_length = double.NaN;
			_segments = null;
		}

		/// <summary>
		/// Creates a new, empty instance of the polyline
		/// </summary>
		public Polyline() {
			_nodes = new ObservableList<T>();
			_nodes.ListContentChanged += new ListContentChangedHandler(InvalidateComputedProperties);

			InvalidateComputedProperties(null);
		}


	}
}
