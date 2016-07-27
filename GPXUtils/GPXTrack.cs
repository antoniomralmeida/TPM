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

namespace LK.GPXUtils {
	/// <summary>
	/// Represents a track
	/// </summary>
	public class GPXTrack {
		/// <summary>
		/// Gets or sets the name of this track
		/// </summary>
		public string Name {
			get;
			set;
		}

		protected List<GPXTrackSegment> _segments;

		/// <summary>
		/// Gets the list of track segments that are part of this track
		/// </summary>
		public List<GPXTrackSegment> Segments {
			get {
				return _segments;
			}
		}

		/// <summary>
		/// Creates a new, empty instance of GPXTrack 
		/// </summary>
		public GPXTrack() {
			_segments = new List<GPXTrackSegment>();
		}

		/// <summary>
		/// Creates a new instance of GPXTrack with specific name
		/// </summary>
		/// <param name="name">The name of this GPXTrack</param>
		public GPXTrack(string name) {
			_segments = new List<GPXTrackSegment>();

			Name = name;
		}
	}
}
