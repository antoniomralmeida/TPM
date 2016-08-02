using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LK.OSMUtils.OSMDatabase;
using LK.TMatch;

namespace LK.TRoute
{
    class FlowScan
    {
        HashSet<HotRoute> hotRoutes;
        HashSet<Connection> hotRouteStarts;
        int eps;
        int minTraffic;

        public HashSet<HotRoute> Run(RoadGraph roadGraph, int eps, int minTraffic)
        {
            this.eps = eps;
            this.minTraffic = minTraffic;

            hotRoutes = new HashSet<HotRoute>();
            hotRouteStarts = FindHotRouteStarts(roadGraph);

            foreach (var hrs in hotRouteStarts)
            {
                var r = new HotRoute(hrs);
                hotRoutes.Add(ExtendHotRoutes(r));
            }

            return hotRoutes;
        }

        public HotRoute ExtendHotRoutes(HotRoute r)
        {
            var p = r.Segments.Last();
            var Q = GetDirectlyTrafficDensityReachableNeighbors(p);
            if (Q.Any())
            {
                foreach (var split in Q)
                {
                    var r_ = r;
                    //append split edges to r_
                    ExtendHotRoutes(r_);
                }
            }
            return r;
        }

        private HashSet<Connection> FindHotRouteStarts(RoadGraph roadGraph)
        {
            HashSet<Connection> hotRouteStarts = new HashSet<Connection>();
            var x = roadGraph.Connections.Where(c => c.Traffic >= minTraffic);

            foreach (var px in x)
            {
                foreach (var c in px.To.Connections)
                {
                    //hotRouteStarts.Add(c);
                }
            }

            return hotRouteStarts;
        }

        public HashSet<Connection> GetDirectlyTrafficDensityReachableNeighbors(Connection r)
        {
            HashSet<Connection> directlyTrafficDensityReachableNeighbors = new HashSet<Connection>();

            foreach (var s in GetEpsNeighborhood(r))
            {
                if (r.Traffic + s.Traffic >= minTraffic)
                {
                    directlyTrafficDensityReachableNeighbors.Add(s);
                }
            }
            return directlyTrafficDensityReachableNeighbors;
        }

        public HashSet<Connection> GetEpsNeighborhood(Connection r)
        {
            HashSet<Connection> epsNeighborhood = new HashSet<Connection>();
            Queue<Connection> q = new Queue<Connection>();

            epsNeighborhood.Add(r);
            q.Enqueue(r);

            for (int i = 0; i < eps; i++)
            {
                var next = q.Dequeue();
                foreach (var s in next.To.Connections)
                {
                    epsNeighborhood.Add(s);
                    q.Enqueue(s);
                }
            }
            return epsNeighborhood;
        }

        public bool IsRouteTrafficDensityReachable(Connection r, Connection s)
        {

            throw new NotImplementedException();
        }
    }
}