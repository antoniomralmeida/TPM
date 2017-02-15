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

using System.Xml;
using System.IO;

namespace LK.GPXUtils.GPXDataSource {
	public class GPXXmlDataWriter : IGPXDataWriter, IDisposable {
		private bool _closed;
		protected XmlWriter _xmlWriter;

		/// <summary>
		/// Creates a new GPXXmlDataWriter, which writes GPX data into the specific file.
		/// </summary>
		/// <param name="filename">Path to the file</param>
		public GPXXmlDataWriter(string filename) {
			XmlWriterSettings writerSetting = new XmlWriterSettings();
			writerSetting.Indent = true;

			_xmlWriter = XmlTextWriter.Create(new StreamWriter(filename, false, new UTF8Encoding(false)), writerSetting);

			WriteGPXElement();
		}

		/// <summary>
		/// Creates a new OXMXmlDataWriter, which writes OSM entities into the specific stream.
		/// </summary>
		/// <param name="stream"></param>
		public GPXXmlDataWriter(Stream stream) {
			XmlWriterSettings writerSetting = new XmlWriterSettings();
			writerSetting.Indent = true;

			_xmlWriter = XmlTextWriter.Create(new StreamWriter(stream, new UTF8Encoding(false)), writerSetting);

			WriteGPXElement();
		}

		/// <summary>
		/// Closes GPXXmlDataWrites, no more elements can be written after calling this method.
		/// </summary>
		public void Close() {
			_xmlWriter.WriteEndElement();
			_xmlWriter.Close();
			_closed = true;
		}

		/// <summary>
		/// Writes the root element to the output
		/// </summary>
		private void WriteGPXElement() {
			_xmlWriter.WriteStartElement("gpx");
			_xmlWriter.WriteAttributeString("version", "1.1");
			_xmlWriter.WriteAttributeString("creator", "GPXUtils");
		}

		/// <summary>
		/// Writes the specific waypoint to the output
		/// </summary>
		/// <param name="waypoint">The waypoint to be written</param>
		public void WriteWaypoint(GPXPoint waypoint) {
			WritePointData(waypoint, "wpt");
		}

		/// <summary>
		/// Writes point data to the specific tag to the output
		/// </summary>
		/// <param name="point">The point that's data should be written</param>
		/// <param name="tag">The name of the output tag</param>
		/// <example>
		///   //writes waypoint
		///		WritePointData(waypoint, "wpt");
		/// </example>
		protected void WritePointData(GPXPoint point, string tag) {
			_xmlWriter.WriteStartElement(tag);
            // writing id
            _xmlWriter.WriteAttributeString("id", point.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));

            _xmlWriter.WriteAttributeString("lat", point.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture));
			_xmlWriter.WriteAttributeString("lon", point.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture));

            if (point.Elevation != 0) {
				_xmlWriter.WriteElementString("ele", point.Elevation.ToString(System.Globalization.CultureInfo.InvariantCulture));
			}

			if (point.Time != DateTime.MinValue) {
			    _xmlWriter.WriteElementString("time", point.Time.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"));			
			}

			if (string.IsNullOrEmpty(point.Name) == false) {
				_xmlWriter.WriteElementString("name", point.Name);
			}

			if (string.IsNullOrEmpty(point.Description) == false) {
				_xmlWriter.WriteElementString("desc", point.Description);
			}

			if (string.IsNullOrEmpty(point.Commenet) == false) {
				_xmlWriter.WriteElementString("cmt", point.Commenet);
			}

            if (point.TrafficSignal != false)
            {
                _xmlWriter.WriteAttributeString("traffic_signal", point.TrafficSignal.ToString());
            }
			_xmlWriter.WriteEndElement();
		}

		/// <summary>
		/// Writes the specific route to the output
		/// </summary>
		/// <param name="route">The route to be written</param>
		public void WriteRoute(GPXRoute route) {
			_xmlWriter.WriteStartElement("rte");

			if (string.IsNullOrEmpty(route.Name) == false) {
				_xmlWriter.WriteElementString("name", route.Name);
			}

			foreach (GPXPoint point in route.Nodes) {
				WritePointData(point, "rtept");
			}

			_xmlWriter.WriteEndElement();
		}

		/// <summary>
		/// Writes the specific track to the output
		/// </summary>
		/// <param name="track">The track to be written</param>
		public void WriteTrack(GPXTrack track) {
			_xmlWriter.WriteStartElement("trk");

			if (string.IsNullOrEmpty(track.Name) == false) {
				_xmlWriter.WriteElementString("name", track.Name);
			}
            
			foreach (GPXTrackSegment segment in track.Segments) {
				_xmlWriter.WriteStartElement("trkseg");

                if (segment.Id != 0)
                {
                    _xmlWriter.WriteAttributeString("id", segment.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }

                // Traffic
                if (segment.Traffic != null)
                {
                    _xmlWriter.WriteStartElement("traffiseg");
                    _xmlWriter.WriteAttributeString("v", segment.Traffic.Count.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    _xmlWriter.WriteEndElement();
                }

                // Average Speed
                if (segment.AvgSpeed != 0)
                {
                    _xmlWriter.WriteStartElement("avgSpeed");
                    _xmlWriter.WriteAttributeString("v", segment.AvgSpeed.ToString("R"));
                    _xmlWriter.WriteEndElement();
                }

                // Route Speed
                if (segment.Speed != 0)
                {
                    _xmlWriter.WriteStartElement("speed");
                    _xmlWriter.WriteAttributeString("v", segment.Speed.ToString("R"));
                    _xmlWriter.WriteEndElement();
                }

                // Points
                foreach (GPXPoint point in segment.Nodes) {
					WritePointData(point, "trkpt");
				}

				_xmlWriter.WriteEndElement();
			}

			_xmlWriter.WriteEndElement();
		}

		#region IDisposable Members

		private bool _disposed = false;
		/// <summary>
		/// Implements IDisposable interface
		/// </summary>
		public void Dispose() {
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Closes OSMXmlDataWriter on Disposing
		/// </summary>
		/// <param name="disposing"></param>
		private void Dispose(bool disposing) {
			if (!this._disposed) {
				if (_closed == false) {
					Close();
				}

				_disposed = true;
			}
		}

		#endregion
	}
}
