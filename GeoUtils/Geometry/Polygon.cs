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

namespace LK.GeoUtils.Geometry {
	//TODO: Check if polygon is simple

	/// <summary>
	/// Represents simple 2D polygon
	/// </summary>
	/// <remarks>Doesn't support holes in the polygon.</remarks>
	public class Polygon<T> where T : IPointGeo, new() {
		List<IPointGeo> _vertices;

		/// <summary>
		/// Gets polygon vertices, that can be individually accessed by index.
		/// </summary>
		public IList<IPointGeo> Vertices {
			get {
				return _vertices;
			}
		}

		/// <summary>
		/// Gets polygon vertices count.
		/// </summary>
		public int VerticesCount {
			get {
				return _vertices.Count;
			}
		}

//		protected BBox _boundingBox;
		/// <summary>
		/// Gets the bounding box of this polygon
		/// </summary>
		public BBox BoundingBox {
			get {
				return new BBox(_vertices);
			}
		}

		/// <summary>
		/// Creates a new instance of the Polygon class and intitializes it with supplied data.
		/// </summary>
		/// <param name="vertices">Collection of vertices of the polygon.</param>
		public Polygon(IList<IPointGeo> vertices) {
			//_boundingBox = new BBox();
			_vertices = new List<IPointGeo>();

			foreach (var vertex in vertices) {
				AddVertex(vertex);
			}
		}

		/// <summary>
		/// Creates a new instance of the Polygon class.
		/// </summary>
		public Polygon() {
			//_boundingBox = new BBox();
			_vertices = new List<IPointGeo>();
		}

		/// <summary>
		/// Initializes a polygon with data from the sigle way
		/// </summary>
		/// <param name="way">The way used to initialize polygon</param>
		//void InitializeWithSingleWay(OSMWay way) {

		//}
		
		/// <summary>
		/// Adds a vertex to the polygon
		/// </summary>
		/// <param name="vertexId">Vertex id to be added</param>
		/// <exception cref="System.ArgumentException">Throws an ArgumentException if the polygon already contains given vertex.</exception>
		public void AddVertex(IPointGeo vertex) {
			if (_vertices.Contains(vertex)) {
				throw new ArgumentException(String.Format("Polygon already contains vertex {0}", vertex));
			}

			_vertices.Add(vertex);
		}

		/// <summary>
		/// Removes given vertex from the polygon. 
		/// </summary>
		/// <param name="vertex">The vertex to be removed</param>
		/// <returns>Returns true if the vertex was succefully removed, otherwise returns false.</returns>
		public bool RemoveVertex(IPointGeo vertex) {
			return _vertices.Remove(vertex);
		}

		/// <summary>
		/// Tests if the specific point is inside of this polygon.
		/// </summary>
		/// <param name="point">The point to be tested.</param>
		/// <returns>Returns true, if the point is inside this polygon, otherwise returns false.</returns>
		/// <remarks>If the point is on the bounadary, the result is undefined</remarks>
		public bool IsInside(IPointGeo point) {
			bool oddNodes = false;
			int j = _vertices.Count - 1;

			for (int i = 0; i < _vertices.Count; i++) {
				if (_vertices[i].Latitude <= point.Latitude && _vertices[j].Latitude >= point.Latitude
				 || _vertices[j].Latitude <= point.Latitude && _vertices[i].Latitude >= point.Latitude) {
					if (_vertices[i].Longitude + (point.Latitude - _vertices[i].Latitude) /
						 (_vertices[j].Latitude - _vertices[i].Latitude) *
						 (_vertices[j].Longitude - _vertices[i].Longitude) < point.Longitude) {
						oddNodes = !oddNodes;
					}
				}
				j = i;
			}

			return oddNodes;
		}
	}
}
