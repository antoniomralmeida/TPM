﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LK.OSMUtils.OSMDatabase;
using LK.TMatch;
using Utils;

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
            var q = p.GetDirectlyTrafficDensityReachableNeighbors(minTraffic, eps);

            /*if (r.Segments.Count < 10)
            //{
                Console.WriteLine("= HOT ROUTE =");
                foreach (var v in r.Segments)
                    Console.WriteLine(v.From.MapPoint.Latitude + " " + v.From.MapPoint.Longitude + " " + v.To.MapPoint.Latitude
                        + " " + v.To.MapPoint.Longitude);
                Console.WriteLine();
            //}*/

            /*if (r.Segments.Count < 10)
            //{
                Console.WriteLine("= NEIGHBORS =");
                foreach (var v in q)
                    Console.WriteLine(v.From.MapPoint.Latitude + " " + v.From.MapPoint.Longitude + " " + v.To.MapPoint.Latitude 
                        + " " + v.To.MapPoint.Longitude);
                Console.WriteLine();
            //}*/

            if (q.Any())
            {
                foreach (var split in q)
                {
                    /*Console.WriteLine("from lat: " + split.From.MapPoint.Latitude + ", from lng: " + split.From.MapPoint.Longitude);
                    Console.WriteLine("from lat: " + split.To.MapPoint.Latitude + ", from lng: " + split.To.MapPoint.Longitude);
                    Console.WriteLine();*/
                    if (IsRouteTrafficDensityReachable(r, split))
                    {
                        if (!r.Segments.Contains(split))
                        {
                            var r_ = r;
                            r_.Segments.Add(split);
                            ExtendHotRoutes(r_);
                        }
                    }
                }
            }
            return r;
        }

        public bool IsRouteTrafficDensityReachable(HotRoute r, Connection split)
        {
            var segments = new List<Connection>();
            segments.AddRange(r.Segments);
            segments.Add(split);
            int hrSize = segments.Count;

            if (hrSize < eps)
            {
                return true;
            }
            else
            {
                int i = hrSize;
                var s = segments[i-1];
                //Console.WriteLine(s.From.MapPoint.Latitude + " " + s.From.MapPoint.Longitude + " " + s.To.MapPoint.Latitude
                  //  + " " + s.To.MapPoint.Longitude);
                
                IEnumerable<long> intersected = segments[i-1].Traffic;
                for (int j = i-1; j > i-eps; j--)
                {
                    intersected = intersected.Intersect(segments[j-1].Traffic);
                }

                /*foreach (var v in intersected)
                    Console.Write(v + " ");
                Console.WriteLine();

                if (intersected.Count() < minTraffic)
                {
                    Console.WriteLine("false");
                    return false;
                }*/

                /*while (i >= eps)
                {
                    IEnumerable<long> intersected = segments[i-1].Traffic;
                    for (int j = i-1; j > i-eps; j--)
                    {
                        intersected = intersected.Intersect(segments[j-1].Traffic);
                    }

                    foreach (var v in intersected)
                        Console.Write(v + " ");
                    Console.WriteLine();
                    //Console.WriteLine(intersected.Count());

                    if (intersected.Count() < minTraffic)
                    {
                        Console.WriteLine("false");
                        return false;
                    }
                    i--;
                }*/
            }
            return true;
        }

        private void FindHotRouteStarts()
        {
            var candidates = roadGraph.Connections.Where(c => c.Traffic.Count() >= minTraffic);
            
            foreach (var ci in candidates)
            {
                var incoming = ci.From.Connections.Where(c => c.Traffic.Count() >= minTraffic && c.To == ci.From);

                if (incoming.Any())
                {
                    var incomingAggreg = incoming.Select(c => c.Traffic.AsEnumerable()).Aggregate((a, b) => a.Union(b));

                    if (ci.Traffic.Except(incomingAggreg).Count() >= minTraffic)
                    {
                        hotRouteStarts.Add(ci);
                    }
                }
            }
        }
    }
}