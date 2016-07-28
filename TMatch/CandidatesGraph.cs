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
using LK.GPXUtils;

namespace LK.TMatch {
	/// <summary>
	/// Represents candidates graph
	/// </summary>
	public class CandidatesGraph {
		private List<CandidateGraphLayer> _layers;
		/// <summary>
		/// Gets the list of layers of the graph, where every layer coresponds with one GPX point
		/// </summary>
		public IList<CandidateGraphLayer> Layers {
			get {
				return _layers;
			}
		}

		/// <summary>
		/// Creates a new instance of candidates graph
		/// </summary>
		public CandidatesGraph() {
			_layers = new List<CandidateGraphLayer>();
		}

		/// <summary>
		/// Creates connections among candidate points in subsequent layers
		/// </summary>
		public void ConnectLayers() {
			for (int l = 0; l < _layers.Count - 1; l++) {
				for (int i = 0; i < _layers[l].Candidates.Count; i++) {
					for (int j = 0; j < _layers[l + 1].Candidates.Count; j++) {
						ConnectPoints(_layers[l].Candidates[i], _layers[l + 1].Candidates[j]);
					}
				}
			}
		}

		/// <summary>
		/// Creates connection between two points
		/// </summary>
		/// <param name="from">The point, where the connections starts</param>
		/// <param name="to">The point, where the connection ends</param>
		void ConnectPoints(CandidatePoint from, CandidatePoint to) {
			CandidatesConnection c = new CandidatesConnection() { From = from, To = to };
			from.OutgoingConnections.Add(c);
			to.IncomingConnections.Add(c);
		}

		/// <summary>
		/// Creates a new layer in the CandidatesGraph
		/// </summary>
		/// <param name="originalPoint">GPX track point</param>
		/// <param name="candidates">Candidate points for the original point</param>
		public void CreateLayer(GPXPoint originalPoint, IEnumerable<CandidatePoint> candidates) {
			CandidateGraphLayer result = new CandidateGraphLayer() { TrackPoint = originalPoint };
			result.Candidates.AddRange(candidates);

			foreach (var candidate in candidates) {
				candidate.Layer = result;
			}
			_layers.Add(result);
		}
	}
}
