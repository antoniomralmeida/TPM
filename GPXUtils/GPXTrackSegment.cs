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
using LK.GeoUtils;

namespace LK.GPXUtils {
    /// <summary>
    /// Represents a list of GPXPoints which are logically connected in order.
    /// </summary>
    public class GPXTrackSegment : Polyline<GPXPoint>
    {
        /// <summary>
        /// Creates a new, empty instance of GPXTrackSegment 
        /// </summary>
        public GPXTrackSegment()
            : base()
        {

        }

        /// <summary>
		/// Gets or sets the traffic
		/// </summary>
		public long Id
        {
            get;
            set;
        }

        /// <summary>
		/// Gets or sets the traffic
		/// </summary>
		public HashSet<long> Traffic
        {
            get;
            set;
        }

        /// <summary>
		/// Gets or sets the average speed
		/// </summary>
		public double AvgSpeed
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new instance of GPXTrackSegment and initializes it with specific collection of GPXPoints
        /// </summary>
        /// <param name="points">Points that belong to the GPXTrackSegment</param>
        public GPXTrackSegment(IEnumerable<GPXPoint> points)
            : base()
        {

            foreach (var point in points)
            {
                _nodes.Add(point);
            }
        }

        /// <summary>
        /// Creates a new instance of GPXTrackSegment and initializes it with specific collection of GPXPoints
        /// </summary>
        /// <param name="points">Points that belong to the GPXTrackSegment</param>
        public GPXTrackSegment(IEnumerable<GPXPoint> points, double avgSpeed, long id)
            : base()
        {
            Id = id;
            AvgSpeed = avgSpeed;
            
            foreach (var point in points)
            {
                _nodes.Add(point);
            }
        }
    }
}
