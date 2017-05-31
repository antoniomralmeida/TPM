using LK.GPXUtils;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LK.TimeWindows
{  
    class DBScan
    {
        private class DBScanPoint
        {
            public bool visited;
            public double value;
            public int id;

            public DBScanPoint(double p)
            {
                this.id = 0; // Not classified
                this.visited = false;
                this.value = p;
            }
        }
        
        public static void dbscan(List<double> dbPoints, double eps, int minPoints)
        {
            List<List<DBScanPoint>> clusters = new List<List<DBScanPoint>>();
            List<DBScanPoint> neighborPoints;
            var points = dbPoints.Select(x => new DBScanPoint(x)).ToArray();
            int clusterCount = 0;
            
            for (int i = 0; i < points.Count(); i++)
            {
                var p = points[i];
                if (!p.visited)
                {
                    p.visited = true;
                    neighborPoints = findNeighbours(points, p, eps);
                    if (neighborPoints.Count < minPoints)
                    {
                        p.id = -1; // Noise point
                    }
                    else
                    {
                        clusterCount++;
                        clusters.Add(neighborPoints);
                        expandCluster(points, p, neighborPoints, clusterCount, eps, minPoints);
                    }
                }
            }
            Console.WriteLine("Testing Discretization by DBScan, numClusters:" + clusters.Count 
                            + ", SSE:" + sumOfSquareErrors(clusters));
        }

        private static void expandCluster(DBScanPoint[] points, DBScanPoint p, List<DBScanPoint> neighborPoints,
            int clusterCount, double eps, int minPoints)
        {
            p.id = clusterCount;
            List<DBScanPoint> neighbors;

            for (int i = 0; i < neighborPoints.Count; i++)
            {
                var pointNeighbor = neighborPoints[i];
                if (!neighborPoints[i].visited)
                {
                    neighborPoints[i].visited = true;
                    neighbors = findNeighbours(points, p, eps);
                    if (neighbors.Count >= minPoints)
                    {
                        neighborPoints.AddRange(neighbors);
                    }
                }
                if (pointNeighbor.id == 0) // not classified
                {
                    pointNeighbor.id = clusterCount;
                } 
            }
        }

        private static List<DBScanPoint> findNeighbours(DBScanPoint[] points, DBScanPoint p, double eps)
        {
            var neighborPoints = points.Where(x => Math.Abs(x.value - p.value) <= eps);
            return neighborPoints.ToList();
        }

        private static double sumOfSquareErrors(List<List<DBScanPoint>> clusters)
        {
            double[] points = new double [clusters.Count];

            for (int i = 0; i < clusters.Count; i++)
            {
                double meanCluster = 0;

                foreach (var p in clusters[i])
                {
                    meanCluster += p.value;
                }

                meanCluster = meanCluster / clusters[i].Count;

                double smallestSub = double.MaxValue;
                double point = 0;
                foreach (var p in clusters[i])
                {
                    double subtr = Math.Abs(p.value - meanCluster);
                    if (subtr < smallestSub)
                    {
                        smallestSub = subtr;
                        point = p.value;
                    }
                        
                }
                points[i] = point;
            }

            double meanClusters = 0;
            for (var i = 0; i < points.Count(); i++)
            {
                meanClusters += points[i];
            }

            return meanClusters / points.Count();
        }
    }
}
