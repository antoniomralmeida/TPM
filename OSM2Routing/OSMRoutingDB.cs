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

using LK.OSMUtils.OSMDatabase;
using LK.OSMUtils.OSMDataSource;
using System.IO;

namespace LK.OSM2Routing
{
    /// <summary>
    /// Represents a OSMDB that can save it's content in routing-friendly form
    /// </summary>
    public class OSMRoutingDB
    {
        OSMDB _storage;
        IEnumerable<RoadType> _acceptedRoads;

        Dictionary<long, long> _ways;
        Dictionary<long, List<long>> _usedNodes;
        /// <summary>
        /// Gets the used nodes and ways that contains them
        /// </summary>
        public Dictionary<long, List<long>> UsedNodes
        {
            get
            {
                return _usedNodes;
            }
        }

        /// <summary>
        /// Gets the collection of OSMNodes
        /// </summary>
        public OSMObjectCollection<OSMNode> Nodes
        {
            get
            {
                return _storage.Nodes;
            }
        }

        /// <summary>
        /// Gets the collection of OSMWays
        /// </summary>
        public OSMObjectCollection<OSMWay> Ways
        {
            get
            {
                return _storage.Ways;
            }
        }

        /// <summary>
        /// Creates a new instance of the OSMFIlteredDB
        /// </summary>
        public OSMRoutingDB()
        {
            _storage = new OSMDB();
            _usedNodes = new Dictionary<long, List<long>>();
            _ways = new Dictionary<long, long>();
        }

        /// <summary>
        /// Load OSM file, and filters out ways that do not match specific road types
        /// </summary>
        /// <param name="acceptedRoads">Accepted road types</param>
        /// <param name="path">Path to the OSM file</param>
        public void Load(IEnumerable<RoadType> acceptedRoads, string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                Load(acceptedRoads, fs);
            }
        }

        /// <summary>
        /// Load OSM file, and filters out ways that do not match specific road types
        /// </summary>
        /// <param name="acceptedRoads">Accepted road types</param>
        /// <param name="input">Stream with OSM file</param>
        public void Load(IEnumerable<RoadType> acceptedRoads, Stream input)
        {
            _acceptedRoads = acceptedRoads;

            OSMXmlDataReader reader = new OSMXmlDataReader();

            reader.WayRead += WayRead;
            reader.Read(input);

            reader.WayRead -= WayRead;
            reader.NodeRead += NodeRead;
            input.Seek(0, 0);
            reader.Read(input);
        }

        /// <summary>
        /// Callback function for OSMXmlDataReader, checks whether the way matches desired RoadTypes and adds the matched road into DB
        /// </summary>
        /// <param name="way">The way read form the OSM file</param>
        void WayRead(OSMWay way)
        {
            foreach (RoadType roadType in _acceptedRoads)
            {
                if (roadType.Match(way) && _ways.ContainsKey(way.ID) == false)
                {
                    ExtractUsedNodes(way);
                    _storage.Ways.Add(new OSMRoad(way, roadType));
                    _ways.Add(way.ID, way.ID);
                }
            }
        }

        /// <summary>
        /// Callback function for OSMXmlDataReader, checks whether node is used and adds the used node into DB
        /// </summary>
        /// <param name="node">The node read form the OSM file</param>
        void NodeRead(OSMNode node)
        {
            if (_usedNodes.ContainsKey(node.ID))
            {
                _storage.Nodes.Add(node);
            }
        }

        /// <summary>
        /// Exctract nodes from the way and adds them into UsedNodes list
        /// </summary>
        /// <param name="way"></param>
        void ExtractUsedNodes(OSMWay way)
        {
            foreach (long nodeID in way.Nodes)
            {
                if (_usedNodes.ContainsKey(nodeID) == false)
                {
                    _usedNodes.Add(nodeID, new List<long>());
                }

                _usedNodes[nodeID].Add(way.ID);
            }
        }

        /// <summary>
        /// Splits ways at road crossings, check for oneway roads and save results in OSMDB
        /// </summary>
        /// <returns>OSMDB object with road segments and used nodes</returns>
        public OSMDB BuildRoutableOSM()
        {
            OSMDB result = new OSMDB();
            int counter = -1;

            foreach (OSMRoad route in _storage.Ways)
            {
                OSMWay segment = new OSMWay(counter--);
                OSMTag wayIDTag = new OSMTag("way-id", route.ID.ToString());
                OSMTag speedTag = new OSMTag("speed", route.Speed.ToString());

                string wayAccessibility = route.IsAccessible() ? "yes" : "no";
                OSMTag wayAccessibilityTag = new OSMTag("accessible", wayAccessibility);

                string wayAccessibilityReverse = route.IsAccessibleReverse() ? "yes" : "no";
                OSMTag wayAccessibilityReverseTag = new OSMTag("accessible-reverse", wayAccessibilityReverse);

                for (int i = 0; i < route.Nodes.Count; i++)
                {
                    segment.Nodes.Add(route.Nodes[i]);

                    if ((UsedNodes[route.Nodes[i]].Count > 1) && (i > 0) && (i < (route.Nodes.Count - 1)))
                    {
                        segment.Tags.Add(wayIDTag);
                        segment.Tags.Add(speedTag);
                        segment.Tags.Add(wayAccessibilityTag);
                        segment.Tags.Add(wayAccessibilityReverseTag);

                        result.Ways.Add(segment);

                        segment = new OSMWay(counter--);
                        segment.Nodes.Add(route.Nodes[i]);
                    }
                }

                segment.Tags.Add(wayIDTag);
                segment.Tags.Add(speedTag);
                segment.Tags.Add(wayAccessibilityTag);
                segment.Tags.Add(wayAccessibilityReverseTag);
                result.Ways.Add(segment);
            }

            foreach (OSMNode node in _storage.Nodes)
            {
                OSMNode newNode = new OSMNode(node.ID, node.Latitude, node.Longitude);

                // preserve junction and highway tags on nodes
                if (node.Tags.ContainsTag("junction"))
                {
                    newNode.Tags.Add(node.Tags["junction"]);
                }
                if (node.Tags.ContainsTag("highway"))
                {
                    newNode.Tags.Add(node.Tags["highway"]);
                }

                if (_usedNodes[node.ID].Count > 1)
                {
                    newNode.Tags.Add(new OSMTag("crossroad", "yes"));
                }

                result.Nodes.Add(newNode);
            }

            return result;
        }
    }
}