using LK.GeoUtils.Geometry;
using LK.OSMUtils.OSMDatabase;
using System;
using System.Collections.Generic;

namespace LK.TMatch
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
        public DateTime Start
        {
            get; set;
        }

        /// <summary>
        /// Gets bucket end time
        /// </summary>
        public DateTime End
        {
            get; set;
        }

        public Dictionary<OSMDB, HashSet<Connection>> Trajectories
        {
            get; set;
        }

        public List<Polyline<IPointGeo>> Paths
        {
            get; set;
        }

        public Bucket(string name, DateTime start, DateTime end)
        {
            Name = name;
            Start = start;
            End = end;
            Paths = new List<Polyline<IPointGeo>>();
            //Trajectories = new Dictionary<OSMDB, HashSet<Connection>>();
        }

        public Bucket()
        {
        }
    }
}