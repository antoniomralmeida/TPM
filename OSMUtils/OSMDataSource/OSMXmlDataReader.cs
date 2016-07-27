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
using System.Xml;
using System.IO;

using LK.OSMUtils.OSMDatabase;
using System.Globalization;

namespace LK.OSMUtils.OSMDataSource {
	public class OSMXmlDataReader : IOSMDataReader {

		protected XmlReader _xmlReader;

		/// <summary>
		/// Reads data from the osm file
		/// </summary>
		/// <param name="osmFile">Path to the osm file.</param>
		public void Read(string osmFile) {
			using (FileStream fs = new FileStream(osmFile, FileMode.Open)) {
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
							if (_xmlReader.Name != "osm")
								throw new XmlException("Invalid xml root element. Expected <osm>.");

							ReadOsmTag();
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
		/// Reads the root element of the xml
		/// </summary>
		private void ReadOsmTag() {
			_xmlReader.Read();

			while (_xmlReader.NodeType != XmlNodeType.EndElement) {
				switch (_xmlReader.Name) {
					case "node":
						ReadNode();
						break;
					case "relation":
						ReadRelation();
						break;
					case "way":
						ReadWay();
						break;
					default:
						_xmlReader.Skip();
						break;
				}
			}
		}

		/// <summary>
		/// Reads an OSMTag from the XmlTextReader and reurn it.
		/// </summary>
		/// <returns>The OSMTag read form the XmlTextReader</returns>
		private OSMTag ReadTag() {
			string attK = _xmlReader.GetAttribute("k");
			if (attK == null)
				throw new XmlException("Attribute 'k' is missing.");

			string attV = _xmlReader.GetAttribute("v");
			if (attV == null)
				throw new XmlException("Attribute 'v' is missing.");

			_xmlReader.Skip();
			return new OSMTag(attK, attV);
		}

		private void ReadNode() {
			string attId = _xmlReader.GetAttribute("id");
			if (attId == null)
				throw new XmlException("Attribute 'id' is missing.");
			long nodeId = long.Parse(attId, System.Globalization.CultureInfo.InvariantCulture);

			string attLat = _xmlReader.GetAttribute("lat");
			if (attLat == null)
				throw new XmlException("Attribute 'lat' is missing.");
			double nodeLat = double.Parse(attLat, System.Globalization.CultureInfo.InvariantCulture);

			string attLon = _xmlReader.GetAttribute("lon");
			if (attLon == null)
				throw new XmlException("Attribute 'lon'is missing.");
			double nodeLon = double.Parse(attLon, System.Globalization.CultureInfo.InvariantCulture);

			OSMNode node = new OSMNode(nodeId, nodeLat, nodeLon);

			if (_xmlReader.IsEmptyElement == false) {
				_xmlReader.Read();

				while (_xmlReader.NodeType != XmlNodeType.EndElement) {
					if (_xmlReader.NodeType == XmlNodeType.Element && _xmlReader.Name == "tag") {
						node.Tags.Add(ReadTag());
					}
					else {
						_xmlReader.Skip();
					}
				}

			}

			OnNodeRead(node);
			_xmlReader.Skip();
		}

		private void ReadWay() {
			string attId = _xmlReader.GetAttribute("id");
			if (attId == null)
				throw new XmlException("Attribute 'id' is missing.");
			int wayId = int.Parse(attId, System.Globalization.CultureInfo.InvariantCulture);

			OSMWay way = new OSMWay(wayId);

			if (false == _xmlReader.IsEmptyElement) {
				_xmlReader.Read();

				while (_xmlReader.NodeType != XmlNodeType.EndElement) {
					switch (_xmlReader.NodeType) {
						case XmlNodeType.Element:
							switch (_xmlReader.Name) {
								case "nd":
									way.Nodes.Add(ReadWayNd());
									continue;
								case "tag":
									way.Tags.Add(ReadTag());
									continue;
								default:
									_xmlReader.Skip();
									continue;
							}
						default:
							_xmlReader.Skip();
							break;
					}
				}
			}

			OnWayRead(way);
			_xmlReader.Skip();
		}

		private long ReadWayNd() {
			string attRef = _xmlReader.GetAttribute("ref");
			if (String.IsNullOrEmpty(attRef))
				throw new XmlException("Attribute 'ref' is missing.");

			long nodeId = long.Parse(attRef, System.Globalization.CultureInfo.InvariantCulture);

			_xmlReader.Skip();

			return nodeId;
		}

		private void ReadRelation() {
			string attId = _xmlReader.GetAttribute("id");
			if (attId == null)
				throw new XmlException("Attribute 'id' is missing.");
			int relationId = int.Parse(attId, CultureInfo.InvariantCulture);

			OSMRelation relation = new OSMRelation(relationId);

			if (false == _xmlReader.IsEmptyElement) {
				_xmlReader.Read();

				while (_xmlReader.NodeType != XmlNodeType.EndElement) {
					switch (_xmlReader.NodeType) {
						case XmlNodeType.Element:
							switch (_xmlReader.Name) {
								case "member":
									 relation.Members.Add(ReadRelationMember());
									continue;
								case "tag":
									relation.Tags.Add(ReadTag());
									continue;
								default:
									_xmlReader.Skip();
									continue;
							}
						default:
							_xmlReader.Skip();
							break;
					}
				}
			}

			OnRelationRead(relation);
			_xmlReader.Skip();
		}

		private OSMRelationMember ReadRelationMember() {
			string attType = _xmlReader.GetAttribute("type");
			if (String.IsNullOrEmpty(attType))
				throw new XmlException("Attribute 'type' is missing.");

			string attRef = _xmlReader.GetAttribute("ref");
			if (String.IsNullOrEmpty(attRef))
				throw new XmlException("Attribute 'ref' is missing.");
			long refId = long.Parse(attRef, CultureInfo.InvariantCulture);

			string attRole = _xmlReader.GetAttribute("role");

			OSMRelationMemberType memberType;
			switch (attType) {
				case "way":
					memberType = OSMRelationMemberType.way;
					break;
				case "node":
					memberType = OSMRelationMemberType.node;
					break;
				case "relation":
					memberType = OSMRelationMemberType.relation;
					break;
				default:
					throw new XmlException("Unknown relation member type");
			}

		  _xmlReader.Skip();
			return new OSMRelationMember(memberType, refId, attRole);
		}

		#region event handling

		/// <summary>
		/// Occurs when an OSMNode is read from xml
		/// </summary>
		public event OSMNodeReadHandler NodeRead;

		/// <summary>
		/// Raises the NodeRead event
		/// </summary>
		/// <param name="node">The OSMNode read from the database</param>
		protected void OnNodeRead(OSMNode node) {
			OSMNodeReadHandler temp = NodeRead;
			if (temp != null) {
				temp(node);
			}
		}

		/// <summary>
		/// Occurs when an OSMWay is read from xml
		/// </summary>
		public event OSMWayReadHandler WayRead;

		/// <summary>
		/// Raises the WayRead event
		/// </summary>
		/// <param name="node">The OSMWay read from the database</param>
		protected void OnWayRead(OSMWay way) {
			OSMWayReadHandler temp = WayRead;
			if (temp != null) {
				temp(way);
			}
		}

		/// <summary>
		/// Occurs when an OSMRelation is read from xml
		/// </summary>
		public event OSMRelationReadHandler RelationRead;

		/// <summary>
		/// Raises the RelationRead event
		/// </summary>
		/// <param name="node">The OSMRelation read from the database</param>
		protected void OnRelationRead(OSMRelation relation) {
			OSMRelationReadHandler temp = RelationRead;
			if (temp != null) {
				temp(relation);
			}
		}

		#endregion
	}
}
