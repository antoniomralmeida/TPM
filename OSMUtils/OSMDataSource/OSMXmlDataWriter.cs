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

namespace LK.OSMUtils.OSMDataSource
{
    public class OSMXmlDataWriter : IOSMDataWriter, IDisposable
    {
        private bool _closed;
        protected XmlWriter _xmlWriter;

        /// <summary>
        /// Creates a new OXMXmlDataWriter, which writes OSM entities into the specific file.
        /// </summary>
        /// <param name="filename">Path to the file</param>
        public OSMXmlDataWriter(string filename)
        {
            XmlWriterSettings writerSetting = new XmlWriterSettings();
            writerSetting.Indent = true;

            _xmlWriter = XmlTextWriter.Create(new StreamWriter(filename, false, new UTF8Encoding(false)), writerSetting);

            WriteOsmNode();
        }

        /// <summary>
        /// Creates a new OXMXmlDataWriter, which writes OSM entities into the specific stream.
        /// </summary>
        /// <param name="stream"></param>
        public OSMXmlDataWriter(Stream stream)
        {
            XmlWriterSettings writerSetting = new XmlWriterSettings();
            writerSetting.Indent = true;

            _xmlWriter = XmlTextWriter.Create(new StreamWriter(stream, new UTF8Encoding(false)), writerSetting);

            WriteOsmNode();
        }

        /// <summary>
        /// Closes OSMXmlDataWrites, no more elements can be written after calling this method.
        /// </summary>
        public void Close()
        {
            _xmlWriter.WriteEndElement();
            _xmlWriter.Close();
            _closed = true;
        }

        /// <summary>
        /// Writes the root element to the output
        /// </summary>
        protected void WriteOsmNode()
        {
            _xmlWriter.WriteStartElement("osm");
            _xmlWriter.WriteAttributeString("version", "0.5");
            _xmlWriter.WriteAttributeString("generator", "OSMUtils");
        }

        /// <summary>
        /// Writes OSMObject attributes to the output
        /// </summary>
        /// <param name="item">OSMObject thats attibutes to be written to the output.</param>
        protected void WriteOSMObjectAttributes(OSMObject item)
        {
            _xmlWriter.WriteAttributeString("id", item.ID.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            _xmlWriter.WriteAttributeString("visible", "true");
        }

        /// <summary>
        /// Writes OSMObject tags to the output
        /// </summary>
        /// <param name="item">OSMObject thats tags to be written to the output.</param>
        protected void WriteOSMObjectTags(OSMObject item)
        {
            foreach (OSMTag tag in item.Tags)
            {
                _xmlWriter.WriteStartElement("tag");

                _xmlWriter.WriteAttributeString("k", tag.Key);
                _xmlWriter.WriteAttributeString("v", tag.Value);

                _xmlWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Writes the specific OSMNode to the output
        /// </summary>
        /// <param name="node">The OSMNode to be written.</param>
        public void WriteNode(OSMNode node)
        {
            _xmlWriter.WriteStartElement("node");

            WriteOSMObjectAttributes(node);

            _xmlWriter.WriteAttributeString("lat", node.Latitude.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            _xmlWriter.WriteAttributeString("lon", node.Longitude.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));

            WriteOSMObjectTags(node);

            _xmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Writes the specific OSMWay to the output
        /// </summary>
        /// <param name="way">The OSMWay to be written.</param>
        public void WriteWay(OSMWay way)
        {
            _xmlWriter.WriteStartElement("way");

            WriteOSMObjectAttributes(way);

            foreach (var nodeRef in way.Nodes)
            {
                _xmlWriter.WriteStartElement("nd");
                _xmlWriter.WriteAttributeString("ref", nodeRef.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
                _xmlWriter.WriteEndElement();
            }

            WriteOSMObjectTags(way);

            _xmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Writes the specific OSMRelation to the output
        /// </summary>
        /// <param name="relation">The OSMRelation to be written.</param>
        public void WriteRelation(OSMRelation relation)
        {
            _xmlWriter.WriteStartElement("relation");

            WriteOSMObjectAttributes(relation);

            foreach (var member in relation.Members)
            {
                _xmlWriter.WriteStartElement("member");
                _xmlWriter.WriteAttributeString("type", member.Type.ToString());
                _xmlWriter.WriteAttributeString("ref", member.Reference.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
                _xmlWriter.WriteAttributeString("role", member.Role);
                _xmlWriter.WriteEndElement();
            }

            WriteOSMObjectTags(relation);

            _xmlWriter.WriteEndElement();
        }


        #region IDisposable Members

        private bool _disposed = false;
        /// <summary>
        /// Implements IDisposable interface
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Closes OSMXmlDataWriter on Disposing
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (_closed == false)
                {
                    Close();
                }

                _disposed = true;
            }
        }

        #endregion
    }
}