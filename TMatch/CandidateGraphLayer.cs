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

namespace LK.FDI {
	/// <summary>
	/// Represents set of the candidate points for the GPX point
	/// </summary>
	public class CandidateGraphLayer {
		/// <summary>
		/// Gets or sets point of the GPX track
		/// </summary>
		public GPXPoint TrackPoint { get; set; }

		/// <summary>
		/// Gets the collection of candidate points for the TrackPoint
		/// </summary>
		public List<CandidatePoint> Candidates { get; private set; }

		/// <summary>
		/// Creates a new instance of the CandidateGraphLayer
		/// </summary>
		public CandidateGraphLayer() {
			Candidates = new List<CandidatePoint>(TMM.MaxCandidatesCount);
		}
	}
}
