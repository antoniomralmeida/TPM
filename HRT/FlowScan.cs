using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LK.OSMUtils.OSMDatabase;
using Utils;
using LK.FDI;
using LK.GeoUtils;

namespace LK.HRT
{
    class FlowScan
    {
        HashSet<HotRoute> hotRoutes;
        HashSet<Connection> hotRouteStarts;
        int eps;
        int minTraffic;
        RoadGraph roadGraph;
        AstarPathfinder aStart;

        public HashSet<HotRoute> Run(RoadGraph roadGraph, int eps, int minTraffic)
        {
            this.eps = eps;
            this.minTraffic = minTraffic;
            this.roadGraph = roadGraph;

            hotRoutes = new HashSet<HotRoute>();
            hotRouteStarts = new HashSet<Connection>();

            //var test = roadGraph.Connections.Where(c => c.Traffic.Count() >= minTraffic);
            //Console.WriteLine(test.ToList().Count);

            this.aStart = new AstarPathfinder(this.roadGraph);
            FindHotRouteStarts();
            Console.WriteLine("Return FindHotRouteStars(): " + hotRouteStarts.Count);

            foreach (var hrs in hotRouteStarts)
            {
                var r = new HotRoute(hrs);
                ExtendHotRoutes(ref r);
                hotRoutes.Add(r);
            }
            
            return hotRoutes;
        }

        public void ExtendHotRoutes(ref HotRoute r)
        {
            var p = r.Segments.Last();
            //Console.WriteLine("P: " + p.Id);
            var q = p.GetDirectlyTrafficDensityReachableNeighbors(minTraffic, eps);
            //Console.WriteLine("Q: " + q.ToList().Count);
 
            if (q.Any())
            {
                foreach (var split in q)
                {
                    //Console.WriteLine("SPLIT ID: " + split.Id);
                    if (IsRouteTrafficDensityReachable(r, split))
                    {
                        if (!r.Segments.Contains(split))
                        {
                            
                            r.Segments.Add(split);
                            ExtendHotRoutes(ref r);
                        } else
                        {
                            return;
                        }
                    }
                }
            }
        }

        public bool IsRouteTrafficDensityReachable(HotRoute r, Connection split)
        {
            Connection last = r.Segments.Last();
            double len = Calculations.GetDistance2D(last.From.MapPoint, split.To.MapPoint);

            CandidatePoint from = new CandidatePoint();
            from.MapPoint = last.From.MapPoint;
            from.Road = last.Geometry;
            CandidatePoint to = new CandidatePoint();
            to.MapPoint = split.To.MapPoint;
            to.Road = split.Geometry;

            IList<PathSegment> result = this.aStart.FindPath(from, to, ref len);

            if (result.Count <= eps)
            {
                return true;
            }

            return false;
        }

        private void FindHotRouteStarts()
        {
            var candidates = roadGraph.Connections.Where(c => c.Traffic.Count() >= minTraffic);
            Console.WriteLine("NUMBER OF CADADIDATES: " + candidates.ToList().Count + "\n");

            foreach (var ci in candidates)
            {
                var incoming = ci.From.Connections.Where(c => c.Traffic.Count() >= minTraffic && c.To == ci.From && c.Id != ci.Id).Distinct();
                //Console.WriteLine("ID CADIDATE: " + ci.Id + " INCOMING: " + incoming.ToList().Count);
                //Console.WriteLine("LIST INCOMING");
                //foreach (var inc in incoming)
                //{
                //    Console.WriteLine("ID: " + inc.Id);
               // }

               // Console.WriteLine("\n");

                if (incoming.Any())
                {
                    var incomingAggreg = incoming.Select(c => c.Traffic.AsEnumerable()).Aggregate((a, b) => a.Union(b));
                    //var incomingAggreg = incoming.Select(c => c.Traffic.AsEnumerable()).Distinct();
                    //Console.WriteLine("INCOMING AGGREG: " + incomingAggreg.ToList().Count);
                    //Console.WriteLine("ci.Traffic.Except(incomingAggreg).Count():  " + ci.Traffic.Except(incomingAggreg).Count() + "\n");

                    if (ci.Traffic.Except(incomingAggreg).Count() >= minTraffic)
                    {
                        hotRouteStarts.Add(ci);
                    }

                    /*List<long> listAggreg = new List<long>();
                    foreach (var l in incomingAggreg)
                    {
                        listAggreg.AddRange(l);
                    }
                    var list = ci.Traffic.ToList();
                    if (list.Except(listAggreg).ToList().Count >= minTraffic)
                    {
                        hotRouteStarts.Add(ci);
                    }*/
                }

            }
        }
    }
}