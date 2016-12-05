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
    [Serializable]
	/// <summary>
	/// Represents a directed connection (an edge in the roadgraph) between two nodes.
	/// </summary>
	public class Connection {
		/// <summary>
		/// Create a new connection with the specific end nodes and create relation among nodes and connection
		/// </summary>
		/// <param name="from">The node, where this connection starts</param>
		/// <param name="to">The noe where this connection ends</param>
		public Connection(Node from, Node to) {
			this.From = from;
			from.Connections.Add(this);

			this.To = to;
            to.Connections.Add(this);

            this.Traffic = new HashSet<long>();
		}

        /// <summary>
        /// Gets or sets connection Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets node, where this connection starts
        /// </summary>
        public Node From { get; set; }

		/// <summary>
		/// Gets or set node wher this connection ends
		/// </summary>
		public Node To { get; set; }

		/// <summary>
		/// Gets or sets shape of the connection
		/// </summary>
		public ConnectionGeometry Geometry { get; set; }

		/// <summary>
		/// Gets or sets maximal speed on this connection
		/// </summary>
		public double Speed { get; set; }

        /// <summary>
        /// Gets or sets traffic on this connection
        /// </summary>
        public HashSet<long> Traffic { get; set; }

        /// <summary>
        /// Gets or sets eps-neighborhood of this connection
        /// </summary>
        public HashSet<Connection> epsNeighborhood { get; set; }

        /// <summary>
        /// Gets or sets directly traffic density-reachable neighbors of this connection
        /// </summary>
        public HashSet<Connection> directlyTrafficDensityReachableNeighbors { get; set; }

        public HashSet<Connection> GetEpsNeighborhood(int eps)
        {
            if (epsNeighborhood == null)
            {
                HashSet<Connection> epsNeighborhood = new HashSet<Connection>();
                Queue<Connection> q = new Queue<Connection>();
                
                q.Enqueue(this);

                for (int i = 0; i < eps; i++)
                {
                    var next = q.Dequeue();
                    foreach (var s in next.To.Connections.Where(c => c.From == next.To))
                    {
                        epsNeighborhood.Add(s);
                        q.Enqueue(s);
                    }
                }
                this.epsNeighborhood = epsNeighborhood;
            }
            return epsNeighborhood;
        }

        public HashSet<Connection> GetDirectlyTrafficDensityReachableNeighbors(int minTraffic, int eps)
        {
            if (directlyTrafficDensityReachableNeighbors == null)
            {
                HashSet<Connection> directlyTrafficDensityReachableNeighbors = new HashSet<Connection>();

                var epsN = this.GetEpsNeighborhood(eps);

                /*Console.WriteLine("this: " + this.From.MapPoint.Latitude + " " + this.From.MapPoint.Longitude + " " + this.To.MapPoint.Latitude + " " + this.To.MapPoint.Longitude);
                foreach(var s in epsN)
                    Console.WriteLine("others: " + s.From.MapPoint.Latitude + " " + s.From.MapPoint.Longitude + " " + s.To.MapPoint.Latitude + " " + s.To.MapPoint.Longitude);
                Console.WriteLine("----");*/

                foreach (var s in epsN)
                {
                    if (this.Traffic.Intersect(s.Traffic).Count() >= minTraffic)
                    {
                        directlyTrafficDensityReachableNeighbors.Add(s);
                    }
                }
                this.directlyTrafficDensityReachableNeighbors = directlyTrafficDensityReachableNeighbors;
            }
            return directlyTrafficDensityReachableNeighbors;
        }
    }
}
