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
        RoadGraph roadGraph;

        public HashSet<HotRoute> Run(RoadGraph roadGraph, int eps, int minTraffic)
        {
            this.eps = eps;
            this.minTraffic = minTraffic;
            this.roadGraph = roadGraph;

            hotRoutes = new HashSet<HotRoute>();
            hotRouteStarts = new HashSet<Connection>();

            FindHotRouteStarts();

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
            var q = p.GetDirectlyTrafficDensityReachableNeighbors(minTraffic);
            if (q.Any())
            {
                //check if its a split
                foreach (var split in q)
                {
                    if (split.IsRouteTrafficDensityReachable(p))
                    {
                        var r_ = r;
                        r_.Segments.Add(split);
                        ExtendHotRoutes(r_);
                    }
                }
            }
            return r;
        }

        private void FindHotRouteStarts()
        {
            var candidates = roadGraph.Connections.Where(c => c.Traffic.Count() >= minTraffic);

            foreach (var ci in candidates)
            {
                var incoming = ci.From.Connections.Where(c => c.Traffic.Count() >= minTraffic && c.To == ci.From);
                var incomingAggreg = incoming.Select(c => c.Traffic.AsEnumerable()).Aggregate((a, b) => a.Union(b));

                if (ci.Traffic.Except(incomingAggreg).Count() >= minTraffic)
                {
                    hotRouteStarts.Add(ci);
                }
            }
        }
    }
}