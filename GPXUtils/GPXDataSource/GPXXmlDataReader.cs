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
using System.IO;
using System.Xml;

using LK.GPXUtils;

namespace LK.GPXUtils.GPXDataSource {
	/// <summary>
	/// Provides minimalistic implementation of IGPXDataReader
	/// </summary>
	/// <remarks>GPXXmlDataReader can process only tracks</remarks>
	public class GPXXmlDataReader : IGPXDataReader {
		XmlReader _xmlReader;
		
		/// <summary>
		/// Reads data from the gpx file
		/// </summary>
		/// <param name="osmFile">Path to the gpx file.</param>
		public void Read(string gpxFile) {
			using (FileStream fs = new FileStream(gpxFile, FileMode.Open)) {
				this.Read(fs);
			}
		}

		/// <summary>
		/// Reads data from a stream
		/// </summary>
		/// <param name="stream">The stram to read data from</param>
		public void Read(Stream stream) {

			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.IgnoreComments = true;
			xmlReaderSettings.IgnoreProcessingInstructions = true;
			xmlReaderSettings.IgnoreWhitespace = true;

			try {
				_xmlReader = XmlTextReader.Create(stream, xmlReaderSettings);

				_xmlReader.Read();
				while (false == _xmlReader.EOF) {


					switch (_xmlReader.NodeType) {
						case XmlNodeType.XmlDeclaration:
							_xmlReader.Read();
							continue;

						case XmlNodeType.Element:
							if (_xmlReader.Name != "gpx")
								throw new XmlException("Invalid xml root element. Expected <gpx>.");

							ReadGPXTag();
							return;

						default:
							throw new XmlException();
					}
				}
			}
			finally {
				_xmlReader.Close();
				_xmlReader = null;
			}
		}

		/// <summary>
		/// Reads content of the root gpx element
		/// </summary>
		private void ReadGPXTag() {
			_xmlReader.Read();

			while (_xmlReader.NodeType != XmlNodeType.EndElement) {
				switch (_xmlReader.Name) {
					case "wpt":
						ReadWaypoint();
						break;
					case "trk":
					  ReadTrack();
					  break;
					case "rte":
						ReadRoute();
						break;
					default:
						_xmlReader.Skip();
						break;
				}
			}
		}

		/// <summary>
		/// Reads a point from gpx document
		/// </summary>
		private GPXPoint ReadPoint() {
			// latitude attribute
			string lat = _xmlReader.GetAttribute("lat");
			
			if (string.IsNullOrEmpty(lat)) {
				throw new XmlException("Attribute 'lat' is missing.");
			}
			double pointLat = double.Parse(lat, System.Globalization.CultureInfo.InvariantCulture);

			// longitude attribute
			string lon = _xmlReader.GetAttribute("lon");

			if (string.IsNullOrEmpty(lon)) {
				throw new XmlException("Attribute 'lon' is missing.");
			}
			double pointLon = double.Parse(lon, System.Globalization.CultureInfo.InvariantCulture);

			GPXPoint parsedPoint = new GPXPoint(pointLat, pointLon);

			if (_xmlReader.IsEmptyElement == false) {
				_xmlReader.Read();

				while (_xmlReader.NodeType != XmlNodeType.EndElement) {
					if (_xmlReader.NodeType == XmlNodeType.Element) {
						switch(_xmlReader.Name) {
							case "ele":
								string ele = _xmlReader.ReadString();
								parsedPoint.Elevation = double.Parse(ele, System.Globalization.CultureInfo.InvariantCulture);
								_xmlReader.Skip();
								break;
							case "time":
								string time = _xmlReader.ReadString();
								parsedPoint.Time = DateTime.Parse(time, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal);
								
							//parsedPoint.Time = DateTime.ParseExact(time, "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal);
								_xmlReader.Skip();								
								break;
							case "name":
								parsedPoint.Name = _xmlReader.ReadString();
								_xmlReader.Skip();
								break;
							case "desc":
								parsedPoint.Description = _xmlReader.ReadString();
								_xmlReader.Skip();
								break;
							case "cmt":
								parsedPoint.Commenet = _xmlReader.ReadString();
								_xmlReader.Skip();
								break;
							default:
								_xmlReader.Skip();
								break;
						}
					}
					else {
						_xmlReader.Skip();
					}
				}
			}

			_xmlReader.Skip();
			return parsedPoint;
		}

		private void ReadWaypoint() {
			OnWaypointRead(ReadPoint());
		}

		/// <summary>
		/// Reads track from the gpx
		/// </summary>
		private void ReadTrack() {
			GPXTrack parsedTrack = new GPXTrack();

			if (_xmlReader.IsEmptyElement == false) {
				_xmlReader.Read();

				while (_xmlReader.NodeType != XmlNodeType.EndElement) {
					if (_xmlReader.NodeType == XmlNodeType.Element) {
						switch (_xmlReader.Name) {
							case "trkseg":
								parsedTrack.Segments.Add(ReadTrackSegment());
								break;
							case "name":
								parsedTrack.Name = _xmlReader.ReadString();
								_xmlReader.Skip();
								break;
							default:
								_xmlReader.Skip();
								break;
						}
					}
					else {
						_xmlReader.Skip();
					}
				}
			}

			_xmlReader.Skip();
			OnTrackRead(parsedTrack);
		}

		/// <summary>
		/// Reads route from the gpx
		/// </summary>
		private void ReadRoute() {
			GPXRoute parsedRoute = new GPXRoute();

			if (_xmlReader.IsEmptyElement == false) {
				_xmlReader.Read();

				while (_xmlReader.NodeType != XmlNodeType.EndElement) {
					if (_xmlReader.NodeType == XmlNodeType.Element) {
						switch (_xmlReader.Name) {
							case "rtept":
								parsedRoute.Nodes.Add(ReadPoint());
								break;
							case "name":
								parsedRoute.Name = _xmlReader.ReadString();
								_xmlReader.Skip();
								break;
							default:
								_xmlReader.Skip();
								break;
						}
					}
					else {
						_xmlReader.Skip();
					}
				}
			}

			_xmlReader.Skip();
			OnRouteRead(parsedRoute);
		}

		private GPXTrackSegment ReadTrackSegment() {
			GPXTrackSegment parsedSegment = new GPXTrackSegment();

			if (_xmlReader.IsEmptyElement == false) {
				_xmlReader.Read();

				while (_xmlReader.NodeType != XmlNodeType.EndElement) {
					if (_xmlReader.NodeType == XmlNodeType.Element) {
						switch (_xmlReader.Name) {
							case "trkpt":
								parsedSegment.Nodes.Add(ReadPoint());
								break;
							default:
								_xmlReader.Skip();
								break;
						}
					}
					else {
						_xmlReader.Skip();
					}
				}
			}

			_xmlReader.Skip();
			return parsedSegment;
		}

		#region event handling

		/// <summary>
		/// Occurs when a track is read from xml
		/// </summary>
		public event GPXTrackReadHandler TrackRead;

		/// <summary>
		/// Raises the TrackRead event
		/// </summary>
		/// <param name="node">The track read from the xml</param>
		protected void OnTrackRead(GPXTrack track) {
			GPXTrackReadHandler temp = TrackRead;
			if (temp != null) {
				temp(track);
			}
		}

		/// <summary>
		/// Occurs when a route is read from xml
		/// </summary>
		public event GPXRouteReadHandler RouteRead;

		/// <summary>
		/// Raises the RouteRead event
		/// </summary>
		/// <param name="node">The route read from the xml</param>
		protected void OnRouteRead(GPXRoute route) {
			GPXRouteReadHandler temp = RouteRead;
			if (temp != null) {
				temp(route);
			}
		}

		/// <summary>
		/// Occurs when a waypoint is read from xml
		/// </summary>
		public event GPXWaypointReadHandler WaypointRead;

		/// <summary>
		/// Raises the WaypointRead event
		/// </summary>
		/// <param name="node">The waypoint read from the xml</param>
		protected void OnWaypointRead(GPXPoint waypoint) {
			GPXWaypointReadHandler temp = WaypointRead;
			if (temp != null) {
				temp(waypoint);
			}
		}

		#endregion
	}
}
