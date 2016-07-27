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

namespace LK.MatchGPX2OSM {
	static class Debug {
		public static void SaveCandidateIncomingConnections(CandidatePoint candidate, string filename) {
			int counter = -1;
			OSMDB result = new OSMDB();

			OSMNode osmCandidate = new OSMNode(counter--, candidate.MapPoint.Latitude, candidate.MapPoint.Longitude);
			osmCandidate.Tags.Add(new OSMTag("observation", candidate.ObservationProbability.ToString()));
			osmCandidate.Tags.Add(new OSMTag("time", candidate.Layer.TrackPoint.Time.ToString()));
			result.Nodes.Add(osmCandidate);
			foreach (var connection in candidate.IncomingConnections) {
				OSMNode from = new OSMNode(counter--, connection.From.MapPoint.Latitude, connection.From.MapPoint.Longitude);
				from.Tags.Add(new OSMTag("observation", connection.From.ObservationProbability.ToString()));
				from.Tags.Add(new OSMTag("time", connection.From.Layer.TrackPoint.Time.ToString()));
				result.Nodes.Add(from);

				OSMWay osmConnection = new OSMWay(counter--, new long[] { from.ID, osmCandidate.ID });
				osmConnection.Tags.Add(new OSMTag("transmission", connection.TransmissionProbability.ToString()));

				result.Ways.Add(osmConnection);
			}

			result.Save(filename);
		}
	}
}
