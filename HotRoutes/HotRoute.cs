using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LK.OSMUtils.OSMDatabase;
using LK.MatchGPX2OSM;

namespace LK.HotRoutes
{
    public class HotRoute
    {
        protected List<Connection> _segments;

        public List<Connection> Segments
        {
            get
            {
                return _segments;
            }
        }

        public HotRoute(Connection start)
        {
            _segments.Add(start);
        }
    }
}