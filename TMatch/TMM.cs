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

using LK.GeoUtils;
using LK.GeoUtils.Geometry;
using LK.GPXUtils;
using LK.OSMUtils.OSMDatabase;
using System.Collections;

namespace LK.TMatch
{
    public class TMM
    {
        public static int MaxCandidatesCount = 5;

        RoadGraph _graph;
        CandidatesGraph _candidatesGraph;
        AstarPathfinder _pathfinder;
        List<ConnectionGeometry> _trackCutout;
        internal OSMDB _db;

        /// <summary>
        /// Create a new instance of the STMatching class
        /// </summary>
        /// <param name="graph">The RoadGraph object that represents road network</param>
        public TMM(RoadGraph graph)
        {
            _graph = graph;
            _pathfinder = new AstarPathfinder(_graph);
            _trackCutout = new List<ConnectionGeometry>();
        }

        /// <summary>
        /// Matches the given GPX track to the map
        /// </summary>
        /// <param name="gpx">The GPS track log</param>
        /// <returns>List of the CandidatePoints that match GPS log the best</returns>
        public IList<CandidatePoint> Match(GPXTrackSegment gpx)
        {
            _candidatesGraph = new CandidatesGraph();

            CreateTrackCutout(gpx);

            //Find candidate points + ObservationProbability
            foreach (var gpxPoint in gpx.Nodes)
            {
                var candidates = FindCandidatePoints(gpxPoint);
                _candidatesGraph.CreateLayer(gpxPoint, candidates.OrderByDescending(c => c.ObservationProbability).Take(Math.Min(candidates.Count(), TMM.MaxCandidatesCount)));
            }

            // Calculate transmission probability
            _candidatesGraph.ConnectLayers();

            AssignTransmissionProbability();

            //Evaluates paths in the graph
            EvaluateGraph();

            //Extract result
            List<CandidatePoint> result = new List<CandidatePoint>();
            CandidatePoint current = _candidatesGraph.Layers[_candidatesGraph.Layers.Count - 1].Candidates.OrderByDescending(c => c.HighestProbability).FirstOrDefault();

            while (current != null)
            {
                result.Add(current);
                current = current.HighesScoreParent;
            }

            result.Reverse();
            return result;
        }

        /// <summary>
        /// Traverses through the CandidatesGraph and finds ancestor with the highest probability for every CandidatePoint in the graph
        /// </summary>
        void EvaluateGraph()
        {
            // Find matched sequence
            foreach (var candidate in _candidatesGraph.Layers[0].Candidates)
            {
                candidate.HighestProbability = candidate.ObservationProbability;
            }

            for (int i = 0; i < _candidatesGraph.Layers.Count - 1; i++)
            {
                foreach (var candidate in _candidatesGraph.Layers[i + 1].Candidates)
                {
                    foreach (var connection in candidate.IncomingConnections)
                    {
                        double score = connection.From.HighestProbability + candidate.ObservationProbability * connection.TransmissionProbability;

                        if (score > candidate.HighestProbability)
                        {
                            candidate.HighestProbability = score;
                            candidate.HighesScoreParent = connection.From;
                        }
                    }
                }

                if (_candidatesGraph.Layers[i + 1].Candidates.Max(c => c.HighestProbability) == double.NegativeInfinity)
                {
                    throw new Exception(string.Format("Can not find any connections between points {0} and {1}",
                                          _candidatesGraph.Layers[i].TrackPoint, _candidatesGraph.Layers[i + 1]));
                }
            }
        }

        void CreateTrackCutout(GPXTrackSegment track)
        {
            _trackCutout.Clear();

            BBox trackBBox = new BBox();
            foreach (var point in track.Nodes)
            {
                trackBBox.ExtendToCover(point);
            }
            trackBBox.Inflate(0.0015, 0.0015);

            foreach (var road in _graph.ConnectionGeometries)
            {
                if (Topology.Intersects(road.BBox, trackBBox))
                {
                    _trackCutout.Add(road);
                }
            }
        }

        /// <summary>
        /// Finds all candidates points for given GPS track point
        /// </summary>
        /// <param name="gpxPt">GPS point</param>
        /// <returns>Collection of points candidate points on road segments</returns>
        public IEnumerable<CandidatePoint> FindCandidatePoints(GPXPoint gpxPt)
        {
            List<CandidatePoint> result = new List<CandidatePoint>();
            BBox gpxBbox = new BBox(new IPointGeo[] { gpxPt });
            gpxBbox.Inflate(0.0007, 0.0011);

            foreach (var road in _trackCutout)
            {
                if (Topology.Intersects(gpxBbox, road.BBox))
                {
                    Segment<IPointGeo> roadSegment;
                    IPointGeo projectedPoint = Topology.ProjectPoint(gpxPt, road, out roadSegment);
                    result.Add(new CandidatePoint()
                    {
                        MapPoint = projectedPoint,
                        Road = road,
                        RoadSegment = roadSegment,
                        ObservationProbability = CalculateObservationProbability(gpxPt, projectedPoint)
                    });
                }
            }

            if (result.Count == 0)
            {
                throw new Exception(string.Format("Can not find any candidate point for {0}", gpxPt));
            }

            return result;
        }

        /// <summary>
        /// Calculates observation probability
        /// </summary>
        /// <param name="original">GPS track point</param>
        /// <param name="candidate">Candidate point</param>
        /// <returns>double representing probability that GPS track point corresponds with Candidate point</returns>
        double CalculateObservationProbability(GPXPoint original, IPointGeo candidate)
        {
            double sigma = 30;
            double distance = Calculations.GetDistance2D(original, candidate);
            return Math.Exp(-distance * distance / (2 * sigma * sigma)) / (sigma * Math.Sqrt(Math.PI * 2));
        }

        /// <summary>
        /// Assigns transmission probability to every connection in the graph
        /// </summary>
        void AssignTransmissionProbability()
        {
            foreach (var layer in _candidatesGraph.Layers)
            {
                foreach (var candidatePoint in layer.Candidates)
                {
                    foreach (var connection in candidatePoint.OutgoingConnections)
                    {
                        connection.TransmissionProbability = CalculateTransmissionProbability(connection);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates transmission probability for connection
        /// </summary>
        /// <param name="c">Connection</param>
        /// <returns>double value representing transmission probability</returns>
        double CalculateTransmissionProbability(CandidatesConnection c)
        {
            double gcd = Calculations.GetDistance2D(c.From.MapPoint, c.To.MapPoint);
            double shortestPath = FindShortestPath(c.From, c.To);

            if (gcd == 0 && shortestPath == 0)
                return 1;
            else
                return gcd / shortestPath;
        }

        /// <summary>
        /// Finds shortest path between two points along routes
        /// </summary>
        /// <param name="from">Start point</param>
        /// <param name="to">Destination point</param>
        /// <returns>length of the path in meters</returns>
        double FindShortestPath(CandidatePoint from, CandidatePoint to)
        {
            if (from.Road == to.Road)
            {
                return Calculations.GetPathLength(from.MapPoint, to.MapPoint, from.Road);
            }
            else
            {
                double length = double.PositiveInfinity;
                _pathfinder.FindPath(from, to, ref length);
                return length;
            }
        }
    }
}