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

using LK.GeoUtils.Geometry;

namespace LK.GeoUtils {
	public static class Topology {
		public class Vector3 {
			public double X { get; set; }
			public double Y { get; set; }
			public double Z { get; set; }

			public Vector3() {
			}

			public Vector3(IPointGeo sphericalPoint) {
				double lat = Math.PI / 2 - Calculations.ToRadians(sphericalPoint.Latitude);
				double lon = Calculations.ToRadians(sphericalPoint.Longitude);
				X = Math.Sin(lat) * Math.Cos(lon);
				Y = Math.Sin(lat) * Math.Sin(lon);
				Z = Math.Cos(lat);
			}

			public void Normalize() {
				double length = Math.Sqrt(X * X + Y * Y + Z * Z);

				X /= length;
				Y /= length;
				Z /= length;
			}

			public void Multiply(double factor) {
				X *= factor;
				Y *= factor;
				Z *= factor;
			}
			
			public PointGeo ToSpherical() {
				PointGeo result = new PointGeo();
				result.Latitude = Calculations.ToDegrees( Math.PI / 2 - Math.Acos(Z));
				result.Longitude = Calculations.ToDegrees(Math.Atan2(Y, X));
				return result;
			}
			
			public static Vector3 CrossProduct(Vector3 A, Vector3 B) {
				return new Vector3() { X = A.Y * B.Z - A.Z * B.Y, Y = A.Z * B.X - A.X * B.Z, Z = A.X * B.Y - A.Y * B.X };
			}
		}
	
		/// <summary>
		/// Projects the point to the specific line segment
		/// </summary>
		/// <param name="toProject">The point to be projected</param>
		/// <param name="projectTo">The segment, point will be projected to</param>
		/// <returns>the orthogonaly projected point that lies on the specific line segment</returns>
		/// <remarks>This function uses the sphere Earth approximation</remarks>
		public static IPointGeo ProjectPointSphere(IPointGeo toProject, Segment<IPointGeo> projectTo) {
			Vector3 a = new Vector3(projectTo.StartPoint);
			Vector3 b = new Vector3(projectTo.EndPoint);
			Vector3 c = new Vector3(toProject);

			Vector3 greatCircleN = Vector3.CrossProduct(a, b);
			Vector3 greatCircleCN = Vector3.CrossProduct(c, greatCircleN);
			Vector3 projected = Vector3.CrossProduct(greatCircleN, greatCircleCN);
			projected.Normalize();

			PointGeo result = projected.ToSpherical();

			double apDistance = Calculations.GetDistance2D(projectTo.StartPoint, result);
			double bpDistance = Calculations.GetDistance2D(projectTo.EndPoint, result);
			if (apDistance + bpDistance - Calculations.GetLength(projectTo) < 0.01) {
				return result;
			}
			else {
				return (Calculations.GetDistance2D(projectTo.StartPoint, toProject) < Calculations.GetDistance2D(projectTo.EndPoint, toProject)) ? projectTo.StartPoint : projectTo.EndPoint;
			}
		}

		/// <summary>
		/// Projects the point to the specific line segment, this function uses the flat earth aproximation
		/// </summary>
		/// <param name="toProject">The point to be projected</param>
		/// <param name="projectTo">The segment, point will be projected to</param>
		/// <returns>the orthogonaly projected point that lies on the specific line segment</returns>
		/// <remarks>This function uses the flat Earth approximation</remarks>
		public static IPointGeo ProjectPoint(IPointGeo toProject, Segment<IPointGeo> projectTo) {
		  double u = ((projectTo.EndPoint.Longitude - projectTo.StartPoint.Longitude) * (toProject.Longitude - projectTo.StartPoint.Longitude) +
		              (projectTo.EndPoint.Latitude - projectTo.StartPoint.Latitude) * (toProject.Latitude - projectTo.StartPoint.Latitude)) /
		              (Math.Pow(projectTo.EndPoint.Longitude - projectTo.StartPoint.Longitude, 2) + Math.Pow(projectTo.EndPoint.Latitude - projectTo.StartPoint.Latitude, 2));

			if (u <= 0)
				return projectTo.StartPoint;
			if (u >= 1)
				return projectTo.EndPoint;

		  double lon = projectTo.StartPoint.Longitude + u * (projectTo.EndPoint.Longitude - projectTo.StartPoint.Longitude);
		  double lat = projectTo.StartPoint.Latitude + u * (projectTo.EndPoint.Latitude - projectTo.StartPoint.Latitude);

		  return new PointGeo(lat, lon);
		}

		/// <summary>
		/// Projects the point to the Polyline
		/// </summary>
		/// <param name="point">The point to be projected</param>
		/// <param name="line">The polyline, point will be projected to</param>
		/// <returns>the orthogonaly projected point that lies on the Polyline</returns>
		public static IPointGeo ProjectPoint(IPointGeo point, IPolyline<IPointGeo> line) {
			double minDiatance = double.PositiveInfinity;
			IPointGeo closestPoint = null;

			foreach (var segment in line.Segments) {
				IPointGeo projected = ProjectPoint(point, segment);
				double distance = Calculations.GetDistance2D(point, projected);
				if (distance < minDiatance) {
					minDiatance = distance;
					closestPoint = projected;
				}
			}

			return closestPoint;
		}

		/// <summary>
		/// Projects the point to the Polyline and sets onSegment out paramater
		/// </summary>
		/// <param name="point">The point to project</param>
		/// <param name="line">The polyline, point will be projected to</param>
		/// <param name="onSegment">The Segment of the Polyline, on which the projected point lies</param>
		/// <returns>the orthogonaly projected point that lies on the Polyline</returns>
		public static IPointGeo ProjectPoint(IPointGeo point, IPolyline<IPointGeo> line, out Segment<IPointGeo> onSegment) {
			double minDistance = double.PositiveInfinity;
			IPointGeo closestPoint = null;
			onSegment = null;

			foreach (var segment in line.Segments) {
				IPointGeo projected = ProjectPoint(point, segment);
				double distance = Calculations.GetDistance2D(point, projected);
				if (distance < minDistance) {
					minDistance = distance;
					closestPoint = projected;
					onSegment = segment;
				}
			}

			return closestPoint;
		}

		/// <summary>
		/// Translates point in the specific direction by specific distance
		/// </summary>
		/// <param name="point">The point to be translated</param>
		/// <param name="bearing">Bearing from the original point</param>
		/// <param name="distance">Distance from the original point</param>
		/// <returns>the translated point</returns>
		public static PointGeo ProjectPoint(IPointGeo point, double bearing, double distance) {
			double lat = Math.Asin(Math.Sin(Calculations.ToRadians(point.Latitude))*Math.Cos(distance / Calculations.EarthRadius) + 
				                     Math.Cos(Calculations.ToRadians(point.Latitude))*Math.Sin(distance / Calculations.EarthRadius) * Math.Cos(Calculations.ToRadians(bearing)));

			double lon = Calculations.ToRadians(point.Longitude) + 
				           Math.Atan2(Math.Sin(Calculations.ToRadians(bearing)) * Math.Sin(distance/Calculations.EarthRadius) * Math.Cos(Calculations.ToRadians(point.Latitude)), 
                              Math.Cos(distance / Calculations.EarthRadius) - Math.Sin(Calculations.ToRadians(point.Latitude)) * Math.Sin(lat));

			return new PointGeo(Calculations.ToDegrees(lat), Calculations.ToDegrees(lon));
		}

		/// <summary>
		/// Tests whether two BBoxes have non-empty intersection 
		/// </summary>
		/// <param name="bbox1">First BBox</param>
		/// <param name="bbox2">Second BBox</param>
		/// <returns>true if bbox have non-empty intersection, otherwise returns false</returns>
		public static bool Intersects(BBox bbox1, BBox bbox2) {
			return !(bbox2.West > bbox1.East || bbox2.East < bbox1.West || bbox2.South > bbox1.North || bbox2.North < bbox1.South);
		}

		/// <summary>
		/// Returns nodes on the path between two points
		/// </summary>
		/// <param name="from">The start point</param>
		/// <param name="to">The end point</param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static IEnumerable<IPointGeo> GetNodesBetweenPoints(IPointGeo from, IPointGeo to, IPolyline<IPointGeo> path) {
			var segments = path.Segments;

			List<IPointGeo> result = new List<IPointGeo>();

			int fromIndex = -1;
			int toIndex = -1;
			for (int i = 0; i < segments.Count; i++) {
				if (Calculations.GetDistance2D(from, segments[i]) < Calculations.EpsLength) {
					if (fromIndex > -1 && toIndex > -1 && toIndex <= fromIndex)
						;
					else
						fromIndex = i;
				}
				if (Calculations.GetDistance2D(to, segments[i]) < Calculations.EpsLength) {
					if (fromIndex > -1 && toIndex > -1 && toIndex >= fromIndex)
						;
					else
						toIndex = i;
				}
			}
			if (fromIndex == -1 || toIndex == -1)
				return result;

			if (fromIndex == toIndex - 1) {
				result.Add(segments[fromIndex].EndPoint);
			}
			else if (fromIndex - 1 == toIndex) {
				result.Add(segments[toIndex].EndPoint);
			}
			else if (fromIndex < toIndex) {
				for (int i = fromIndex; i < toIndex; i++) {
					result.Add(segments[i].EndPoint);
				}
			}
			else if (toIndex < fromIndex) {
				for (int i = fromIndex; i > toIndex; i--) {
					result.Add(segments[i].StartPoint);
				}
			}

			return result;
		}
	}
}
