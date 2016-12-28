using LK.GeoUtils.Geometry;
using LK.OSMUtils.OSMDatabase;
using System;
using System.Collections.Generic;

namespace LK.FDI
{
    public class Bucket
    {
        /// <summary>
        /// Gets bucket name
        /// </summary>
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Gets bucket start time
        /// </summary>
        public TimeSpan Start
        {
            get; set;
        }

        /// <summary>
        /// Gets bucket end time
        /// </summary>
        public TimeSpan End
        {
            get; set;
        }

        public Dictionary<OSMDB, HashSet<Connection>> Trajectories
        {
            get; set;
        }

        public Dictionary<String, List<Polyline<IPointGeo>>> Paths
        {
            get; set;
        }

        public Bucket(string name, TimeSpan start, TimeSpan end)
        {
            Name = name;
            Start = start;
            End = end;
            Paths = new Dictionary<String, List<Polyline<IPointGeo>>>();
        }

        public Bucket()
        {
        }
    }
}