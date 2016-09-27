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

using LK.GeoUtils.Geometry;

namespace LK.TMatch {
	/// <summary>
	/// Represents candidate point during map-matching
	/// </summary>
	public class CandidatePoint {
        /// <summary>
        /// Gets or sets ID of track associated with this candidate point
        /// </summary>
        public string TrackId { get; set;  }
        
		public IPointGeo MapPoint { get; set; }
		///// <summary>
		///// Gets or sets latitude of this point (north - positive value, south - negative value)
		///// </summary>
		//public double Latitude { get; set; }

		///// <summary>
		///// Gets or sets the longitude of this point (east - positive value, west - negative value)
		///// </summary>
		//public double Longitude { get; set; }

		/// <summary>
		/// Gets or sets the elevation above MSL in meters of this point
		/// </summary>
		public double Elevation { get; set; }

		/// <summary>
		/// Gets or sets ConnectionGeometry that represents the road on which this candidate point lies
		/// </summary>
		public ConnectionGeometry Road { get; set; }

		/// <summary>
		/// Gets or sets Segmen that represents the part of the road on which this candidate point lies
		/// </summary>
		public Segment<IPointGeo> RoadSegment { get; set; }

		/// <summary>
		/// Gets or sets Observation probability
		/// </summary>
		public double ObservationProbability { get; set; }

		/// <summary>
		/// Gets or sets layer in that this candidate point lies
		/// </summary>
		public CandidateGraphLayer Layer { get; set; }

		public List<CandidatesConnection> OutgoingConnections { get; private set; }
		public List<CandidatesConnection> IncomingConnections { get; private set; }

		/// <summary>
		/// Creates a new instance of the Candidate point
		/// </summary>
		public CandidatePoint() {
			OutgoingConnections = new List<CandidatesConnection>();
			IncomingConnections = new List<CandidatesConnection>();
		}

		private double _highestProbability = double.NegativeInfinity;
		/// <summary>
		/// Gets or sets highest probability for this candidate point during the candidates matching phase
		/// </summary>
		public double HighestProbability {
			get {
				return _highestProbability;
			}
			set {
				_highestProbability = value;
			}
		}

		/// <summary>
		/// Gets the CandidatePoint from the previous layer that's connection with this point has the highest probability
		/// </summary>
		public CandidatePoint HighesScoreParent;
	}
}
